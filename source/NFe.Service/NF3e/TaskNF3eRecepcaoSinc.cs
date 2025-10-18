using NFe.Components;
using NFe.Exceptions;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Exceptions;

namespace NFe.Service
{
    public class TaskNF3eRecepcaoSinc : TaskAbst
    {
        public TaskNF3eRecepcaoSinc(string arquivo)
        {
            Servico = Servicos.NF3eAutorizacaoSinc;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var ler = new LerXML();
            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);

            try
            {
                ler.NF3e(ConteudoXML);

                var xmlNF3e = new Unimake.Business.DFe.Xml.NF3e.NF3e();
                xmlNF3e = xmlNF3e.LerXML<Unimake.Business.DFe.Xml.NF3e.NF3e>(ConteudoXML);

                if (xmlNF3e.InfNF3e.GRespTec == null)
                {
                    if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecCNPJ) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecEmail) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecTelefone) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecXContato))
                    {
                        xmlNF3e.InfNF3e.GRespTec = new Unimake.Business.DFe.Xml.NF3e.GRespTec
                        {
                            CNPJ = Empresas.Configuracoes[emp].RespTecCNPJ,
                            Email = Empresas.Configuracoes[emp].RespTecEmail,
                            Fone = Empresas.Configuracoes[emp].RespTecTelefone,
                            XContato = Empresas.Configuracoes[emp].RespTecXContato
                        };
                    }
                }

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.NF3e,
                    TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                };

                if (ConfiguracaoApp.Proxy)
                {
                    configuracao.HasProxy = true;
                    configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                    configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                    configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                }


                var autorizacaoSinc = new Unimake.Business.DFe.Servicos.NF3e.AutorizacaoSinc(xmlNF3e, configuracao);
                autorizacaoSinc.Executar();

                ConteudoXML = autorizacaoSinc.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "NF3e");

                vStrXmlRetorno = autorizacaoSinc.RetornoWSString;

                if (autorizacaoSinc.Result.CStat == 100)
                {
                    FinalizarNF3eSincrono(vStrXmlRetorno, emp);
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacaoSinc.Result.CStat.ToString("000") + "-" + autorizacaoSinc.Result.XMotivo.Trim(), "UNINFE - NF3e´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).RetornoXML, vStrXmlRetorno);

                if (File.Exists(NomeArquivoXML))
                {
                    File.Delete(NomeArquivoXML);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var arqXML = NomeArquivoXML;

                    if (File.Exists(arqEmProcessamento))
                    {
                        arqXML = arqEmProcessamento;

                        if (File.Exists(NomeArquivoXML))
                        {
                            TFunctions.MoveArqErro(NomeArquivoXML);
                        }
                    }

                    TFunctions.GravarArqErroServico(arqXML, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.ExtRetorno.ProRec_ERR, ex);
                }
                catch { }
            }
        }

        /// <summary>
        /// Salvar o arquivo da NF3e assinado na pasta EmProcessamento
        /// </summary>
        /// <param name="emp">Código da empresa</param>
        /// <param name="arqEmProcessamento">Onde será salvo o XML assinado</param>
        /// <param name="nomeTag">Nome da tag que abre o XML</param>
        protected void SalvarArquivoEmProcessamento(int emp, string arqEmProcessamento, string nomeTag)
        {
            Empresas.Configuracoes[emp].CriarSubPastaEnviado();

            var sw = File.CreateText(arqEmProcessamento);
            sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + ConteudoXML.GetElementsByTagName(nomeTag)[0].OuterXml);
            sw.Close();

            //if (File.Exists(arqEmProcessamento))
            //{
            //    File.Delete(NomeArquivoXML);
            //}
        }

        #endregion Execute

        #region FinalizarNF3eSincrono()

        /// <summary>
        /// Finalizar a NF3e no processo síncrono
        /// </summary>
        /// <param name="xmlRetorno">Conteúdo do XML retornado pela SEFAZ</param>
        /// <param name="emp">Código da empresa</param>
        private void FinalizarNF3eSincrono(string xmlRetorno, int emp)
        {
            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(xmlRetorno));

            var protNF3e = xml.GetElementsByTagName("protNF3e");

            var fluxoNFe = new FluxoNfe();

            FinalizarNF3e(protNF3e, fluxoNFe, emp, ConteudoXML);
        }

        #endregion FinalizarNF3eSincrono()

        #region FinalizarNF3e()

        private void FinalizarNF3e(XmlNodeList protNF3eList, FluxoNfe fluxoNFe, int emp, XmlDocument conteudoXmlNF3e)
        {
            var oLerXml = new LerXML();

            foreach (XmlNode protNF3eNode in protNF3eList)
            {
                var protNF3eElemento = (XmlElement)protNF3eNode;

                var strProtNF3e = protNF3eElemento.OuterXml;

                var infProtList = protNF3eElemento.GetElementsByTagName("infProt");

                foreach (XmlNode infProtNode in infProtList)
                {
                    var tirarFluxo = true;
                    var infProtElemento = (XmlElement)(infProtNode);

                    var strChaveNF3e = string.Empty;
                    var strStat = string.Empty;
                    var xMotivo = string.Empty;

                    if (infProtElemento.GetElementsByTagName(TpcnResources.chNF3e.ToString())[0] != null)
                    {
                        strChaveNF3e = "NF3e" + infProtElemento.GetElementsByTagName(TpcnResources.chNF3e.ToString())[0].InnerText;
                    }

                    if (infProtElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0] != null)
                    {
                        strStat = infProtElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;
                    }

                    if (infProtElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString())[0] != null)
                    {
                        xMotivo = infProtElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString())[0].InnerText;
                    }

                    // Definir o nome do arquivo da NFe e seu caminho
                    var strNomeArqNF3e = "";
                    strNomeArqNF3e = new FileInfo(NomeArquivoXML).Name;

                    if (string.IsNullOrEmpty(strNomeArqNF3e))
                    {
                        if (string.IsNullOrEmpty(strChaveNF3e))
                        {
                            oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                            throw new Exception("FinalizarNF3e(): Não pode obter o nome do arquivo");
                        }

                        strNomeArqNF3e = strChaveNF3e.Substring(4) + Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML;
                    }

                    var strArquivoNF3e = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqNF3e;

                    //Atualizar a Tag de status da NF3e no fluxo para que se ocorrer alguma falha na exclusão eu tenha esta campo para ter uma referencia em futuras consultas
                    fluxoNFe.AtualizarTag(strChaveNF3e, FluxoNfe.ElementoEditavel.cStat, strStat);

                    switch (strStat)
                    {
                        case "100": // NF3e autorizado

                            if (File.Exists(strArquivoNF3e))
                            {
                                var strArquivoNF3eProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" +
                                    Functions.ExtrairNomeArq(strNomeArqNF3e, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML) + Propriedade.ExtRetorno.ProcNF3e;

                                if (conteudoXmlNF3e == null)
                                {
                                    conteudoXmlNF3e = new XmlDocument();
                                    conteudoXmlNF3e.Load(strArquivoNF3e);
                                }

                                oLerXml.NF3e(conteudoXmlNF3e);

                                // Verifica se a -nf3e.xml existe na pasta de autorizados
                                var NF3eJaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML,
                                    Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML);

                                // Verifica se a -procNF3e.xml existe na pasta de autorizados
                                var procNF3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML,
                                    Propriedade.ExtRetorno.ProcNF3e);

                                if (!procNF3eJaNaAutorizada)
                                {
                                    if (!File.Exists(strArquivoNF3eProc))
                                    {
                                        oGerarXML.XmlDistNF3e(strArquivoNF3e, strProtNF3e, Propriedade.ExtRetorno.ProcNF3e, oLerXml.oDadosNfe.versao);
                                    }
                                }

                                if (!(procNF3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.ExtRetorno.ProcNF3e)))
                                {
                                    //Mover a nf3ePRoc da pasta de NF3e em processamento para a NF3e Autorizada
                                    //Para evitar falhar, tenho que mover primeiro o XML de distribuição (-procnf3e.xml) para
                                    //depois mover o da nf3e (-nf3e.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário,
                                    //assim sendo não inverta as posições. Wandrey 08/10/2009
                                    TFunctions.MoverArquivo(strArquivoNF3eProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                    // Atualizar a situação para que eu só mova o arquivo com final -nf3e.xml para a pasta autorizado se
                                    // a procnf3e já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                    // Isso vai dar uma maior segurança para não deixar sem gerar o -procNF3e.xml. Wandrey 13/12/2012
                                    procNF3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.ExtRetorno.ProcNF3e);
                                }

                                if (!NF3eJaAutorizada && procNF3eJaNaAutorizada)
                                {
                                    // Mover a NF3e da pasta de NF3e em processamento para NF3e Autorizada
                                    // Para evitar falhar, tenho que mover primeiro o XML de distribuição (-procnfe.xml) para
                                    // depois mover o da nf3e (-nf3e.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário.
                                    // assim sendo não inverta as posições. Wandrey 08/10/2009
                                    if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                    {
                                        TFunctions.MoverArquivo(strArquivoNF3e, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                    }
                                    else
                                    {
                                        TFunctions.MoverArquivo(strArquivoNF3e, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                    }
                                }

                                if (procNF3eJaNaAutorizada)
                                {
                                    try
                                    {
                                        var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                            PastaEnviados.Autorizados.ToString() + "\\" +
                                            Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                            Path.GetFileName(strArquivoNF3eProc);

                                        TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                    }
                                    catch (Exception ex)
                                    {
                                        Auxiliar.WriteLog("TaskNF3eRecepcaoSinc: " + ex.Message, false);
                                    }
                                }

                                // Vou verificar se estão os dois arquivos na pasta Autorizados, se tiver eu tiro do fluxo caso contrário não. Wandrey 13/02/2012
                                NF3eJaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML);
                                procNF3eJaNaAutorizada = oAux.EstaAutorizada(strArquivoNF3e, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NF3e).EnvioXML, Propriedade.ExtRetorno.ProcNF3e);

                                if (!procNF3eJaNaAutorizada || !NF3eJaAutorizada)
                                {
                                    tirarFluxo = false;
                                }
                            }

                            break;

                        default: // NF3e foi rejeitada
                                 // O Status da NF3e tem que ser maior que 1 ou deu algum erro na hora de ler o XML de retorno da consulta do recibo, sendo assim, vou mantar a nota no fluxo para consultar novamente.

                            if (Convert.ToInt32(strStat) >= 1)
                            {
                                // Mover o XML da NF3e a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoNF3e);

                                if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                                {
                                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(strStat).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - NF3e´s estão sendo rejeitados");
                                }
                            }
                            else
                            {
                                tirarFluxo = false;
                            }

                            break;
                    }

                    // Deletar a NF3e do arquivo de controle de fluxo
                    if (tirarFluxo)
                    {
                        fluxoNFe.ExcluirNfeFluxo(strChaveNF3e);
                    }

                    break;
                }
            }
        }

        #endregion FinalizarNF3e()
    }
}
