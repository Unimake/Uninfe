using NFe.Components;
using NFe.Exceptions;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Exceptions;

namespace NFe.Service.DCe
{
    public class TaskDCeRecepcaoSinc : TaskAbst
    {
        public TaskDCeRecepcaoSinc(string arquivo)
        {
            Servico = Servicos.DCeAutorizacaoSinc;
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
                ler.DCe(ConteudoXML);

                var xmlDCe = new Unimake.Business.DFe.Xml.DCe.DCe();
                xmlDCe = xmlDCe.LerXML<Unimake.Business.DFe.Xml.DCe.DCe>(ConteudoXML);

                var configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = TipoDFe.DCe,
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

                var autorizacaoSinc = new Unimake.Business.DFe.Servicos.DCe.AutorizacaoSinc(xmlDCe, configuracao);

                autorizacaoSinc.Executar();

                ConteudoXML = autorizacaoSinc.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "DCe");

                vStrXmlRetorno = autorizacaoSinc.RetornoWSString;

                if (autorizacaoSinc.Result.CStat == 100)
                {
                    FinalizarDCeSincrono(vStrXmlRetorno, emp);
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacaoSinc.Result.CStat.ToString("000") + "-" + autorizacaoSinc.Result.XMotivo.Trim(), "UNINFE - DCe´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).RetornoXML, vStrXmlRetorno);

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

                    TFunctions.GravarArqErroServico(arqXML, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.ExtRetorno.ProRec_ERR, ex);
                }
                catch { }
            }
        }

        /// <summary>
        /// Salvar o arquivo da DCe assinado na pasta EmProcessamento
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

        #region FinalizarDCeSincrono()

        /// <summary>
        /// Finalizar a DCe no processo síncrono
        /// </summary>
        /// <param name="xmlRetorno">Conteúdo do XML retornado pela SEFAZ</param>
        /// <param name="emp">Código da empresa</param>
        private void FinalizarDCeSincrono(string xmlRetorno, int emp)
        {
            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(xmlRetorno));

            var protDCe = xml.GetElementsByTagName("protDCe");
            var fluxoNFe = new FluxoNfe();

            FinalizarDCe(protDCe, fluxoNFe, emp, ConteudoXML);
        }

        #endregion FinalizarDCeSincrono()

        #region FinalizarDCe()

        private void FinalizarDCe(XmlNodeList protDCeList, FluxoNfe fluxoNFe, int emp, XmlDocument conteudoXmlDCe)
        {
            var oLerXml = new LerXML();

            foreach (XmlNode protDCeNode in protDCeList)
            {
                var protDCeElemento = (XmlElement)protDCeNode;

                var strProtDCe = protDCeElemento.OuterXml;

                var infProtList = protDCeElemento.GetElementsByTagName("infProt");

                foreach (XmlNode infProtNode in infProtList)
                {
                    var tirarFluxo = true;
                    var infProtElemento = (XmlElement)(infProtNode);

                    var strChaveDCe = string.Empty;
                    var strStat = string.Empty;
                    var xMotivo = string.Empty;

                    if (infProtElemento.GetElementsByTagName(TpcnResources.cDC.ToString())[0] != null)
                    {
                        strChaveDCe = "DCe" + infProtElemento.GetElementsByTagName(TpcnResources.cDC.ToString())[0].InnerText;
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
                    var strNomeArqDCe = "";
                    strNomeArqDCe = new FileInfo(NomeArquivoXML).Name;

                    if (string.IsNullOrEmpty(strNomeArqDCe))
                    {
                        if (string.IsNullOrEmpty(strChaveDCe))
                        {
                            oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                            throw new Exception("FinalizarDCe(): Não pode obter o nome do arquivo");
                        }

                        strNomeArqDCe = strChaveDCe.Substring(4) + Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML;
                    }

                    var strArquivoDCe = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqDCe;

                    //Atualizar a Tag de status da DCe no fluxo para que se ocorrer alguma falha na exclusão eu tenha esta campo para ter uma referencia em futuras consultas
                    fluxoNFe.AtualizarTag(strChaveDCe, FluxoNfe.ElementoEditavel.cStat, strStat);

                    switch (strStat)
                    {
                        case "100": // DCe autorizado

                            if (File.Exists(strArquivoDCe))
                            {
                                var strArquivoDCeProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" +
                                    Functions.ExtrairNomeArq(strNomeArqDCe, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML) + Propriedade.ExtRetorno.ProcDCe;

                                if (conteudoXmlDCe == null)
                                {
                                    conteudoXmlDCe = new XmlDocument();
                                    conteudoXmlDCe.Load(strArquivoDCe);
                                }

                                oLerXml.DCe(conteudoXmlDCe);

                                // Verifica se a -DCe.xml existe na pasta de autorizados
                                var DCeJaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML,
                                    Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML);

                                // Verifica se a -procDCe.xml existe na pasta de autorizados
                                var procDCeJaNaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML,
                                    Propriedade.ExtRetorno.ProcDCe);

                                if (!procDCeJaNaAutorizada)
                                {
                                    if (!File.Exists(strArquivoDCeProc))
                                    {
                                        oGerarXML.XmlDistDCe(strArquivoDCe, strProtDCe, Propriedade.ExtRetorno.ProcDCe, oLerXml.oDadosNfe.versao);
                                    }
                                }

                                if (!(procDCeJaNaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.ExtRetorno.ProcDCe)))
                                {
                                    //Mover a DCePRoc da pasta de DCe em processamento para a DCe Autorizada
                                    //Para evitar falhar, tenho que mover primeiro o XML de distribuição (-procDCe.xml) para
                                    //depois mover o da DCe (-DCe.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário,
                                    //assim sendo não inverta as posições. Wandrey 08/10/2009
                                    TFunctions.MoverArquivo(strArquivoDCeProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                    // Atualizar a situação para que eu só mova o arquivo com final -DCe.xml para a pasta autorizado se
                                    // a procDCe já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                    // Isso vai dar uma maior segurança para não deixar sem gerar o -procDCe.xml. Wandrey 13/12/2012
                                    procDCeJaNaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.ExtRetorno.ProcDCe);
                                }

                                if (!DCeJaAutorizada && procDCeJaNaAutorizada)
                                {
                                    // Mover a DCe da pasta de DCe em processamento para DCe Autorizada
                                    // Para evitar falhar, tenho que mover primeiro o XML de distribuição (-procnfe.xml) para
                                    // depois mover o da DCe (-DCe.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário.
                                    // assim sendo não inverta as posições. Wandrey 08/10/2009
                                    if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                    {
                                        TFunctions.MoverArquivo(strArquivoDCe, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                    }
                                    else
                                    {
                                        TFunctions.MoverArquivo(strArquivoDCe, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                    }
                                }

                                if (procDCeJaNaAutorizada)
                                {
                                    try
                                    {
                                        var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                            PastaEnviados.Autorizados.ToString() + "\\" +
                                            Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                            Path.GetFileName(strArquivoDCeProc);

                                        TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                    }
                                    catch (Exception ex)
                                    {
                                        Auxiliar.WriteLog("TaskDCeRecepcaoSinc: " + ex.Message, false);
                                    }
                                }

                                // Vou verificar se estão os dois arquivos na pasta Autorizados, se tiver eu tiro do fluxo caso contrário não. Wandrey 13/02/2012
                                DCeJaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML);
                                procDCeJaNaAutorizada = oAux.EstaAutorizada(strArquivoDCe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.DCe).EnvioXML, Propriedade.ExtRetorno.ProcDCe);

                                if (!procDCeJaNaAutorizada || !DCeJaAutorizada)
                                {
                                    tirarFluxo = false;
                                }
                            }

                            break;

                        default: // DCe foi rejeitada
                                 // O Status da DCe tem que ser maior que 1 ou deu algum erro na hora de ler o XML de retorno da consulta do recibo, sendo assim, vou mantar a nota no fluxo para consultar novamente.

                            if (Convert.ToInt32(strStat) >= 1)
                            {
                                // Mover o XML da DCe a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoDCe);

                                if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                                {
                                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(strStat).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - DCe´s estão sendo rejeitados");
                                }
                            }
                            else
                            {
                                tirarFluxo = false;
                            }

                            break;
                    }

                    // Deletar a DCe do arquivo de controle de fluxo
                    if (tirarFluxo)
                    {
                        fluxoNFe.ExcluirNfeFluxo(strChaveDCe);
                    }

                    break;
                }
            }
        }

        #endregion FinalizarDCe()
    }
}
