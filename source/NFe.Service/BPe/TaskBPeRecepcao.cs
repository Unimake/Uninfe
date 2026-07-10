using NFe.Components;
using NFe.Exceptions;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Exceptions;

namespace NFe.Service.BPe
{
    public class TaskBPeRecepcao : TaskAbst
    {
        public TaskBPeRecepcao(string arquivo)
        {
            Servico = Servicos.BPeAutorizacao;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var ler = new LerXML();
            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);
            Unimake.Business.DFe.Servicos.BPe.AutorizacaoBPe autorizacao = null;

            try
            {
                ler.BPe(ConteudoXML);

                var xmlBPe = new Unimake.Business.DFe.Xml.BPe.BPe();
                xmlBPe = xmlBPe.LerXML<Unimake.Business.DFe.Xml.BPe.BPe>(ConteudoXML);

                if (xmlBPe.InfBPe.InfRespTec == null)
                {
                    if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecCNPJ) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecEmail) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecTelefone) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecXContato))
                    {
                        xmlBPe.InfBPe.InfRespTec = new Unimake.Business.DFe.Xml.BPeTM.InfRespTec
                        {
                            CNPJ = Empresas.Configuracoes[emp].RespTecCNPJ,
                            Email = Empresas.Configuracoes[emp].RespTecEmail,
                            Fone = Empresas.Configuracoes[emp].RespTecTelefone,
                            XContato = Empresas.Configuracoes[emp].RespTecXContato
                        };
                    }
                }

                var configuracao = CriarConfiguracao(emp);

                autorizacao = new Unimake.Business.DFe.Servicos.BPe.AutorizacaoBPe(xmlBPe, configuracao);
                autorizacao.Executar();

                ConteudoXML = autorizacao.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "BPe");

                vStrXmlRetorno = autorizacao.RetornoWSString;

                if (autorizacao.Result.CStat == 100)
                {
                    FinalizarBPe(vStrXmlRetorno, emp, Propriedade.TipoEnvio.BPe, Propriedade.ExtRetorno.ProcBPe, "BPe");
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacao.Result.CStat.ToString("000") + "-" + autorizacao.Result.XMotivo.Trim(), "UNINFE - BPe´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.BPe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.BPe).RetornoXML, vStrXmlRetorno);

                if (File.Exists(NomeArquivoXML))
                {
                    File.Delete(NomeArquivoXML);
                }
            }
            catch (Exception ex)
            {
                GravarErroEnvio(arqEmProcessamento, Propriedade.TipoEnvio.BPe, ex);
            }
            finally
            {
                if (autorizacao != null)
                {
                    autorizacao.Dispose();
                }
            }
        }

        protected Configuracao CriarConfiguracao(int emp)
        {
            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                TipoDFe = TipoDFe.BPe,
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

            return configuracao;
        }

        protected void SalvarArquivoEmProcessamento(int emp, string arqEmProcessamento, string nomeTag)
        {
            Empresas.Configuracoes[emp].CriarSubPastaEnviado();

            var sw = File.CreateText(arqEmProcessamento);
            sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + ConteudoXML.GetElementsByTagName(nomeTag)[0].OuterXml);
            sw.Close();
        }

        protected void GravarErroEnvio(string arqEmProcessamento, Propriedade.TipoEnvio tipoEnvio, Exception ex)
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

                TFunctions.GravarArqErroServico(arqXML, Propriedade.Extensao(tipoEnvio).EnvioXML, Propriedade.ExtRetorno.ProRec_ERR, ex);
            }
            catch
            {
            }
        }

        protected void FinalizarBPe(string xmlRetorno, int emp, Propriedade.TipoEnvio tipoEnvio, string extensaoProc, string tagDocumento)
        {
            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(xmlRetorno));

            var protBPe = xml.GetElementsByTagName("protBPe");
            var fluxoNFe = new FluxoNfe();
            var oLerXml = new LerXML();

            foreach (XmlNode protBPeNode in protBPe)
            {
                var protBPeElemento = (XmlElement)protBPeNode;
                var strProtBPe = protBPeElemento.OuterXml;
                var infProtList = protBPeElemento.GetElementsByTagName("infProt");

                foreach (XmlNode infProtNode in infProtList)
                {
                    var tirarFluxo = true;
                    var infProtElemento = (XmlElement)infProtNode;

                    var strChaveBPe = string.Empty;
                    var strStat = string.Empty;
                    var xMotivo = string.Empty;

                    if (infProtElemento.GetElementsByTagName(TpcnResources.chBPe.ToString())[0] != null)
                    {
                        strChaveBPe = "BPe" + infProtElemento.GetElementsByTagName(TpcnResources.chBPe.ToString())[0].InnerText;
                    }

                    if (infProtElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0] != null)
                    {
                        strStat = infProtElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;
                    }

                    if (infProtElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString())[0] != null)
                    {
                        xMotivo = infProtElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString())[0].InnerText;
                    }

                    var strNomeArqBPe = new FileInfo(NomeArquivoXML).Name;

                    if (string.IsNullOrEmpty(strNomeArqBPe))
                    {
                        if (string.IsNullOrEmpty(strChaveBPe))
                        {
                            throw new Exception("FinalizarBPe(): Não pode obter o nome do arquivo");
                        }

                        strNomeArqBPe = strChaveBPe.Substring(3) + Propriedade.Extensao(tipoEnvio).EnvioXML;
                    }

                    var strArquivoBPe = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqBPe;

                    fluxoNFe.AtualizarTag(strChaveBPe, FluxoNfe.ElementoEditavel.cStat, strStat);

                    switch (strStat)
                    {
                        case "100":
                            if (File.Exists(strArquivoBPe))
                            {
                                var strArquivoBPeProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" +
                                    Functions.ExtrairNomeArq(strNomeArqBPe, Propriedade.Extensao(tipoEnvio).EnvioXML) + extensaoProc;

                                oLerXml.BPe(ConteudoXML);

                                var bpeJaAutorizado = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(tipoEnvio).EnvioXML,
                                    Propriedade.Extensao(tipoEnvio).EnvioXML);

                                var procBPeJaNaAutorizada = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(tipoEnvio).EnvioXML,
                                    extensaoProc);

                                if (!procBPeJaNaAutorizada && !File.Exists(strArquivoBPeProc))
                                {
                                    if (tagDocumento == "BPeTA")
                                    {
                                        oGerarXML.XmlDistBPeTA(strArquivoBPe, strProtBPe, extensaoProc, oLerXml.oDadosNfe.versao);
                                    }
                                    else if (tagDocumento == "BPeTM")
                                    {
                                        oGerarXML.XmlDistBPeTM(strArquivoBPe, strProtBPe, extensaoProc, oLerXml.oDadosNfe.versao);
                                    }
                                    else
                                    {
                                        oGerarXML.XmlDistBPe(strArquivoBPe, strProtBPe, extensaoProc, oLerXml.oDadosNfe.versao);
                                    }
                                }

                                if (!(procBPeJaNaAutorizada = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(tipoEnvio).EnvioXML, extensaoProc)))
                                {
                                    TFunctions.MoverArquivo(strArquivoBPeProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                    procBPeJaNaAutorizada = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(tipoEnvio).EnvioXML, extensaoProc);
                                }

                                if (!bpeJaAutorizado && procBPeJaNaAutorizada)
                                {
                                    if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                    {
                                        TFunctions.MoverArquivo(strArquivoBPe, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                    }
                                    else
                                    {
                                        TFunctions.MoverArquivo(strArquivoBPe, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                    }
                                }

                                bpeJaAutorizado = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(tipoEnvio).EnvioXML, Propriedade.Extensao(tipoEnvio).EnvioXML);
                                procBPeJaNaAutorizada = oAux.EstaAutorizada(strArquivoBPe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(tipoEnvio).EnvioXML, extensaoProc);

                                if (!procBPeJaNaAutorizada || !bpeJaAutorizado)
                                {
                                    tirarFluxo = false;
                                }
                            }

                            break;

                        default:
                            if (Convert.ToInt32(strStat) >= 1)
                            {
                                oAux.MoveArqErro(strArquivoBPe);

                                if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                                {
                                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(strStat).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - BPe´s estão sendo rejeitados");
                                }
                            }
                            else
                            {
                                tirarFluxo = false;
                            }

                            break;
                    }

                    if (tirarFluxo)
                    {
                        fluxoNFe.ExcluirNfeFluxo(strChaveBPe);
                    }

                    break;
                }
            }
        }
    }
}
