using NFe.Components;
using NFe.Exceptions;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Exceptions;

namespace NFe.Service.NFCom
{
    public class TaskNFComRecepcaoSinc : TaskAbst
    {
        public TaskNFComRecepcaoSinc(string arquivo)
        {
            Servico = Servicos.NFComAutorizacaoSinc;
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
                ler.NFCom(ConteudoXML);

                var xmlNFCom = new Unimake.Business.DFe.Xml.NFCom.NFCom();
                xmlNFCom = xmlNFCom.LerXML<Unimake.Business.DFe.Xml.NFCom.NFCom>(ConteudoXML);

                if (xmlNFCom.InfNFCom.GRespTec == null)
                {
                    if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecCNPJ) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecEmail) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecTelefone) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecXContato))
                    {
                        xmlNFCom.InfNFCom.GRespTec = new Unimake.Business.DFe.Xml.NFCom.GRespTec
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
                    TipoDFe = TipoDFe.NFCom,
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

                var autorizacaoSinc = new Unimake.Business.DFe.Servicos.NFCom.AutorizacaoSinc(xmlNFCom, configuracao);

                autorizacaoSinc.Executar();

                ConteudoXML = autorizacaoSinc.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "NFCom");

                vStrXmlRetorno = autorizacaoSinc.RetornoWSString;

                if (autorizacaoSinc.Result.CStat == 100)
                {
                    FinalizarNFComSincrono(vStrXmlRetorno, emp);
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacaoSinc.Result.CStat.ToString("000") + "-" + autorizacaoSinc.Result.XMotivo.Trim(), "UNINFE - NFCom´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).RetornoXML, vStrXmlRetorno);

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

                    TFunctions.GravarArqErroServico(arqXML, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.ExtRetorno.ProRec_ERR, ex);
                }
                catch { }
            }
        }

        /// <summary>
        /// Salvar o arquivo da NFCom assinado na pasta EmProcessamento
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

        #region FinalizarNFComSincrono()

        /// <summary>
        /// Finalizar a NFCom no processo síncrono
        /// </summary>
        /// <param name="xmlRetorno">Conteúdo do XML retornado pela SEFAZ</param>
        /// <param name="emp">Código da empresa</param>
        private void FinalizarNFComSincrono(string xmlRetorno, int emp)
        {
            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(xmlRetorno));

            var protNFCom = xml.GetElementsByTagName("protNFCom");
            var fluxoNFe = new FluxoNfe();

            FinalizarNFCom(protNFCom, fluxoNFe, emp, ConteudoXML);
        }

        #endregion FinalizarNFComSincrono()

        #region FinalizarNFCom()

        private void FinalizarNFCom(XmlNodeList protNFComList, FluxoNfe fluxoNFe, int emp, XmlDocument conteudoXmlNFCom)
        {
            var oLerXml = new LerXML();

            foreach (XmlNode protNFComNode in protNFComList)
            {
                var protNFComElemento = (XmlElement)protNFComNode;

                var strProtNFCom = protNFComElemento.OuterXml;

                var infProtList = protNFComElemento.GetElementsByTagName("infProt");

                foreach (XmlNode infProtNode in infProtList)
                {
                    var tirarFluxo = true;
                    var infProtElemento = (XmlElement)(infProtNode);

                    var strChaveNFCom = string.Empty;
                    var strStat = string.Empty;
                    var xMotivo = string.Empty;

                    if (infProtElemento.GetElementsByTagName(TpcnResources.chNFCom.ToString())[0] != null)
                    {
                        strChaveNFCom = "NFCom" + infProtElemento.GetElementsByTagName(TpcnResources.chNFCom.ToString())[0].InnerText;
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
                    var strNomeArqNFCom = "";
                    strNomeArqNFCom = new FileInfo(NomeArquivoXML).Name;

                    if (string.IsNullOrEmpty(strNomeArqNFCom))
                    {
                        if (string.IsNullOrEmpty(strChaveNFCom))
                        {
                            oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                            throw new Exception("FinalizarNFCom(): Não pode obter o nome do arquivo");
                        }

                        strNomeArqNFCom = strChaveNFCom.Substring(4) + Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML;
                    }

                    var strArquivoNFCom = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqNFCom;

                    //Atualizar a Tag de status da NFCom no fluxo para que se ocorrer alguma falha na exclusão eu tenha esta campo para ter uma referencia em futuras consultas
                    fluxoNFe.AtualizarTag(strChaveNFCom, FluxoNfe.ElementoEditavel.cStat, strStat);

                    switch (strStat)
                    {
                        case "100": // NFCom autorizado

                            if (File.Exists(strArquivoNFCom))
                            {
                                var strArquivoNFComProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" +
                                    Functions.ExtrairNomeArq(strNomeArqNFCom, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML) + Propriedade.ExtRetorno.ProcNFCom;

                                if (conteudoXmlNFCom == null)
                                {
                                    conteudoXmlNFCom = new XmlDocument();
                                    conteudoXmlNFCom.Load(strArquivoNFCom);
                                }

                                oLerXml.NFCom(conteudoXmlNFCom);

                                // Verifica se a -NFCom.xml existe na pasta de autorizados
                                var NFComJaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML,
                                    Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML);

                                // Verifica se a -procNFCom.xml existe na pasta de autorizados
                                var procNFComJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML,
                                    Propriedade.ExtRetorno.ProcNFCom);

                                if (!procNFComJaNaAutorizada)
                                {
                                    if (!File.Exists(strArquivoNFComProc))
                                    {
                                        oGerarXML.XmlDistNFCom(strArquivoNFCom, strProtNFCom, Propriedade.ExtRetorno.ProcNFCom, oLerXml.oDadosNfe.versao);
                                    }
                                }

                                if (!(procNFComJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.ExtRetorno.ProcNFCom)))
                                {
                                    //Mover a NFComPRoc da pasta de NFCom em processamento para a NFCom Autorizada
                                    //Para evitar falhar, tenho que mover primeiro o XML de distribuição (-procNFCom.xml) para
                                    //depois mover o da NFCom (-NFCom.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário,
                                    //assim sendo não inverta as posições. Wandrey 08/10/2009
                                    TFunctions.MoverArquivo(strArquivoNFComProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                    // Atualizar a situação para que eu só mova o arquivo com final -NFCom.xml para a pasta autorizado se
                                    // a procNFCom já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                    // Isso vai dar uma maior segurança para não deixar sem gerar o -procNFCom.xml. Wandrey 13/12/2012
                                    procNFComJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.ExtRetorno.ProcNFCom);
                                }

                                if (!NFComJaAutorizada && procNFComJaNaAutorizada)
                                {
                                    // Mover a NFCom da pasta de NFCom em processamento para NFCom Autorizada
                                    // Para evitar falhar, tenho que mover primeiro o XML de distribuição (-procnfe.xml) para
                                    // depois mover o da NFCom (-NFCom.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário.
                                    // assim sendo não inverta as posições. Wandrey 08/10/2009
                                    if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                    {
                                        TFunctions.MoverArquivo(strArquivoNFCom, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                    }
                                    else
                                    {
                                        TFunctions.MoverArquivo(strArquivoNFCom, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                    }
                                }

                                if (procNFComJaNaAutorizada)
                                {
                                    try
                                    {
                                        var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                            PastaEnviados.Autorizados.ToString() + "\\" +
                                            Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                            Path.GetFileName(strArquivoNFComProc);

                                        TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                    }
                                    catch (Exception ex)
                                    {
                                        Auxiliar.WriteLog("TaskNFComRecepcaoSinc: " + ex.Message, false);
                                    }
                                }

                                // Vou verificar se estão os dois arquivos na pasta Autorizados, se tiver eu tiro do fluxo caso contrário não. Wandrey 13/02/2012
                                NFComJaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML);
                                procNFComJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFCom, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.NFCom).EnvioXML, Propriedade.ExtRetorno.ProcNFCom);

                                if (!procNFComJaNaAutorizada || !NFComJaAutorizada)
                                {
                                    tirarFluxo = false;
                                }
                            }

                            break;

                        default: // NFCom foi rejeitada
                                 // O Status da NFCom tem que ser maior que 1 ou deu algum erro na hora de ler o XML de retorno da consulta do recibo, sendo assim, vou mantar a nota no fluxo para consultar novamente.

                            if (Convert.ToInt32(strStat) >= 1)
                            {
                                // Mover o XML da NFCom a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoNFCom);

                                if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                                {
                                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(strStat).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - NFCom´s estão sendo rejeitados");
                                }
                            }
                            else
                            {
                                tirarFluxo = false;
                            }

                            break;
                    }

                    // Deletar a NFCom do arquivo de controle de fluxo
                    if (tirarFluxo)
                    {
                        fluxoNFe.ExcluirNfeFluxo(strChaveNFCom);
                    }

                    break;
                }
            }
        }

        #endregion FinalizarNFCom()
    }
}
