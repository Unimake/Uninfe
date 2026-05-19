using NFe.Components;
using NFe.Exceptions;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Exceptions;

namespace NFe.Service.NFGas
{
    public class TaskNFGasRecepcaoSinc : TaskAbst
    {
        public TaskNFGasRecepcaoSinc(string arquivo)
        {
            Servico = Servicos.NFGasAutorizacaoSinc;
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
                ler.NFGas(ConteudoXML);

                var xmlNFGas = new Unimake.Business.DFe.Xml.NFGas.NFGas();
                xmlNFGas = xmlNFGas.LerXML<Unimake.Business.DFe.Xml.NFGas.NFGas>(ConteudoXML);

                if (xmlNFGas.InfNFGas.GRespTec == null)
                {
                    if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecCNPJ) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecEmail) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecTelefone) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecXContato))
                    {
                        xmlNFGas.InfNFGas.GRespTec = new Unimake.Business.DFe.Xml.NFGas.GRespTec
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
                    TipoDFe = TipoDFe.NFGas,
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

                var autorizacaoSinc = new Unimake.Business.DFe.Servicos.NFGas.AutorizacaoSinc(xmlNFGas, configuracao);

                autorizacaoSinc.Executar();

                ConteudoXML = autorizacaoSinc.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "NFGas");

                vStrXmlRetorno = autorizacaoSinc.RetornoWSString;

                if (autorizacaoSinc.Result.CStat == 100)
                {
                    FinalizarNFGasSincrono(vStrXmlRetorno, emp);
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacaoSinc.Result.CStat.ToString("000") + "-" + autorizacaoSinc.Result.XMotivo.Trim(), "UNINFE - NFGas´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).RetornoXML, vStrXmlRetorno);

                if (File.Exists(NomeArquivoXML))
                {
                    File.Delete(NomeArquivoXML);
                }

                autorizacaoSinc.Dispose();
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

                    TFunctions.GravarArqErroServico(arqXML, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.ExtRetorno.ProRec_ERR, ex);
                }
                catch { }
            }
        }

        /// <summary>
        /// Salvar o arquivo da NFGas assinado na pasta EmProcessamento
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

        #region FinalizarNFGasSincrono()

        /// <summary>
        /// Finalizar a NFGas no processo síncrono
        /// </summary>
        /// <param name="xmlRetorno">Conteúdo do XML retornado pela SEFAZ</param>
        /// <param name="emp">Código da empresa</param>
        private void FinalizarNFGasSincrono(string xmlRetorno, int emp)
        {
            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(xmlRetorno));

            var protNFGas = xml.GetElementsByTagName("protNFGas");
            var fluxoNFe = new FluxoNfe();

            FinalizarNFGas(protNFGas, fluxoNFe, emp, ConteudoXML);
        }

        #endregion FinalizarNFGasSincrono()

        #region FinalizarNFGas()

        private void FinalizarNFGas(XmlNodeList protNFGasList, FluxoNfe fluxoNFe, int emp, XmlDocument conteudoXmlNFGas)
        {
            var oLerXml = new LerXML();

            foreach (XmlNode protNFGasNode in protNFGasList)
            {
                var protNFGasElemento = (XmlElement)protNFGasNode;

                var strProtNFGas = protNFGasElemento.OuterXml;

                var infProtList = protNFGasElemento.GetElementsByTagName("infProt");

                foreach (XmlNode infProtNode in infProtList)
                {
                    var tirarFluxo = true;
                    var infProtElemento = (XmlElement)(infProtNode);

                    var strChaveNFGas = string.Empty;
                    var strStat = string.Empty;
                    var xMotivo = string.Empty;

                    if (infProtElemento.GetElementsByTagName(TpcnResources.chNFGas.ToString())[0] != null)
                    {
                        strChaveNFGas = "NFGas" + infProtElemento.GetElementsByTagName(TpcnResources.chNFGas.ToString())[0].InnerText;
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
                    var strNomeArqNFGas = "";
                    strNomeArqNFGas = new FileInfo(NomeArquivoXML).Name;

                    if (string.IsNullOrEmpty(strNomeArqNFGas))
                    {
                        if (string.IsNullOrEmpty(strChaveNFGas))
                        {
                            oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                            throw new Exception("FinalizarNFGas(): Não pode obter o nome do arquivo");
                        }

                        strNomeArqNFGas = strChaveNFGas.Substring(5) + Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML;
                    }

                    var strArquivoNFGas = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqNFGas;

                    //Atualizar a Tag de status da NFGas no fluxo para que se ocorrer alguma falha na exclusão eu tenha esta campo para ter uma referencia em futuras consultas
                    fluxoNFe.AtualizarTag(strChaveNFGas, FluxoNfe.ElementoEditavel.cStat, strStat);

                    switch (strStat)
                    {
                        case "100": // NFGas autorizado

                            if (File.Exists(strArquivoNFGas))
                            {
                                var strArquivoNFGasProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" +
                                    Functions.ExtrairNomeArq(strNomeArqNFGas, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML) + Propriedade.ExtRetorno.ProcNFGas;

                                if (conteudoXmlNFGas == null)
                                {
                                    conteudoXmlNFGas = new XmlDocument();
                                    conteudoXmlNFGas.Load(strArquivoNFGas);
                                }

                                oLerXml.NFGas(conteudoXmlNFGas);

                                // Verifica se a -NFGas.xml existe na pasta de autorizados
                                var NFGasJaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML,
                                    Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML);

                                // Verifica se a -procNFGas.xml existe na pasta de autorizados
                                var procNFGasJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML,
                                    Propriedade.ExtRetorno.ProcNFGas);

                                if (!procNFGasJaNaAutorizada)
                                {
                                    if (!File.Exists(strArquivoNFGasProc))
                                    {
                                        oGerarXML.XmlDistNFGas(strArquivoNFGas, strProtNFGas, Propriedade.ExtRetorno.ProcNFGas, oLerXml.oDadosNfe.versao);
                                    }
                                }

                                if (!(procNFGasJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.ExtRetorno.ProcNFGas)))
                                {
                                    //Mover a NFGasPRoc da pasta de NFGas em processamento para a NFGas Autorizada
                                    //Para evitar falhar, tenho que mover primeiro o XML de distribuição (-procNFGas.xml) para
                                    //depois mover o da NFGas (-NFGas.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário,
                                    //assim sendo não inverta as posições. Wandrey 08/10/2009
                                    TFunctions.MoverArquivo(strArquivoNFGasProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                    // Atualizar a situação para que eu só mova o arquivo com final -NFGas.xml para a pasta autorizado se
                                    // a procNFGas já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                    // Isso vai dar uma maior segurança para não deixar sem gerar o -procNFGas.xml. Wandrey 13/12/2012
                                    procNFGasJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.ExtRetorno.ProcNFGas);
                                }

                                if (!NFGasJaAutorizada && procNFGasJaNaAutorizada)
                                {
                                    // Mover a NFGas da pasta de NFGas em processamento para NFGas Autorizada
                                    // Para evitar falhar, tenho que mover primeiro o XML de distribuição (-procnfe.xml) para
                                    // depois mover o da NFGas (-NFGas.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário.
                                    // assim sendo não inverta as posições. Wandrey 08/10/2009
                                    if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                    {
                                        TFunctions.MoverArquivo(strArquivoNFGas, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                    }
                                    else
                                    {
                                        TFunctions.MoverArquivo(strArquivoNFGas, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                    }
                                }

                                if (procNFGasJaNaAutorizada)
                                {
                                    try
                                    {
                                        var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                            PastaEnviados.Autorizados.ToString() + "\\" +
                                            Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                            Path.GetFileName(strArquivoNFGasProc);

                                        TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                    }
                                    catch (Exception ex)
                                    {
                                        Auxiliar.WriteLog("TaskNFGasRecepcaoSinc: " + ex.Message, false);
                                    }
                                }

                                // Vou verificar se estão os dois arquivos na pasta Autorizados, se tiver eu tiro do fluxo caso contrário não. Wandrey 13/02/2012
                                NFGasJaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML);
                                procNFGasJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFGas, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFGas).EnvioXML, Propriedade.ExtRetorno.ProcNFGas);

                                if (!procNFGasJaNaAutorizada || !NFGasJaAutorizada)
                                {
                                    tirarFluxo = false;
                                }
                            }

                            break;

                        default: // NFGas foi rejeitada
                                 // O Status da NFGas tem que ser maior que 1 ou deu algum erro na hora de ler o XML de retorno da consulta do recibo, sendo assim, vou mantar a nota no fluxo para consultar novamente.

                            if (Convert.ToInt32(strStat) >= 1)
                            {
                                // Mover o XML da NFGas a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoNFGas);

                                if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                                {
                                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(strStat).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - NFGas´s estão sendo rejeitados");
                                }
                            }
                            else
                            {
                                tirarFluxo = false;
                            }

                            break;
                    }

                    // Deletar a NFGas do arquivo de controle de fluxo
                    if (tirarFluxo)
                    {
                        fluxoNFe.ExcluirNfeFluxo(strChaveNFGas);
                    }

                    break;
                }
            }
        }

        #endregion FinalizarNFGas()
    }
}
