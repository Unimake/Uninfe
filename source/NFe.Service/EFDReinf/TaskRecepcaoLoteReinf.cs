using NFe.Components;
using NFe.Settings;
using NFSe.Components;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.EFDReinf;

namespace NFe.Service
{
    public class TaskRecepcaoLoteReinf : TaskAbst
    {
        public TaskRecepcaoLoteReinf(string arquivo)
        {
            Servico = Servicos.RecepcaoLoteReinf;

            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);

            try
            {
                var xmlLoteReinf = new ReinfEnvioLoteEventos();

                xmlLoteReinf = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<ReinfEnvioLoteEventos>(ConteudoXML);

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.EFDReinf,
                    TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                };

                var autorizacaoLoteReinf = new Unimake.Business.DFe.Servicos.EFDReinf.RecepcionarLoteAssincrono(xmlLoteReinf, configuracao);
                autorizacaoLoteReinf.Executar();

                ConteudoXML = autorizacaoLoteReinf.ConteudoXMLAssinado;

                vStrXmlRetorno = autorizacaoLoteReinf.RetornoWSString;

                var xmlRetorno = autorizacaoLoteReinf.Result;

                MoverPastaProcessamento(xmlRetorno);

                var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_loteevt).EnvioXML;
                var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_loteevt).RetornoXML;

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                /// grava o arquivo no FTP
                var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  Functions.ExtrairNomeArq(NomeArquivoXML,
                                                  Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_loteevt).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_loteevt).RetornoXML);

                if (File.Exists(filenameFTP))
                {
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_loteevt).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_loteevt).RetornoERR, ex);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                    //Wandrey 31/08/2011
                }
            }
            finally
            {
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML de cancelamento de NFe, infelizmente
                    //não posso fazer mais nada, o UniNFe vai tentar mandar o arquivo novamente para o webservice, pois ainda não foi excluido.
                    //Wandrey 31/08/2011
                }
            }

        }

        #endregion Execute

        /// <summary>
        /// Gerar XML de distribuição do EFDReinf
        /// </summary>
        private void GerarXMLDistribuicao(XmlDocument conteudoXML, int emp)
        {
            var retornoLoteEventosArquivo = new XmlDocument();
            retornoLoteEventosArquivo.LoadXml(vStrXmlRetorno);
            XmlNode evtTotal = null;
            XmlNode eventoAprovado = null;
            StreamWriter swProc = null;

            try
            {
                var retornoLoteEventos = retornoLoteEventosArquivo.GetElementsByTagName("retornoLoteEventos")[0];
                var cdStatus = ((XmlElement)retornoLoteEventos).GetElementsByTagName("cdStatus")[0];

                if (cdStatus.InnerText.Equals("0"))
                {
                    var retonoEventos = retornoLoteEventosArquivo.GetElementsByTagName("retornoEventos")[0];

                    foreach (XmlNode retonoEvento in retonoEventos)
                    {
                        var cdRetorno = ((XmlElement)retonoEvento).GetElementsByTagName("cdRetorno")[0].InnerText;

                        if (cdRetorno.Equals("0"))
                        {
                            evtTotal = ((XmlElement)retonoEvento).GetElementsByTagName("Reinf")[0];

                            var retornoEventoID = ((XmlElement)retonoEvento).Attributes.GetNamedItem("id").Value;

                            var loteEventos = conteudoXML.GetElementsByTagName("loteEventos")[0];

                            foreach (XmlNode evento in loteEventos)
                            {
                                var eventoID = ((XmlElement)evento).Attributes.GetNamedItem("id").Value;

                                if (retornoEventoID.Equals(eventoID))
                                {
                                    var xmlDistribuicao = "<reinfProc>";
                                    eventoAprovado = ((XmlElement)evento).GetElementsByTagName("Reinf")[0];
                                    xmlDistribuicao += eventoAprovado.OuterXml;
                                    xmlDistribuicao += "<retornoEvento>";
                                    xmlDistribuicao += evtTotal.OuterXml;
                                    xmlDistribuicao += "</retornoEvento>";
                                    xmlDistribuicao += "</reinfProc>";

                                    //Nome do arquivo de distribuição do EFDReinf
                                    var nomeArqDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                        PastaEnviados.EmProcessamento.ToString() + "\\" +
                                        eventoID + Propriedade.ExtRetorno.ProcReinf;

                                    //Gravar o XML em uma linha só (sem quebrar as tag´s linha a linha) ou dá erro na hora de
                                    //validar o XML pelos Schemas. Wandrey/André 10/08/2018
                                    swProc = File.CreateText(nomeArqDist);
                                    swProc.Write(xmlDistribuicao);
                                    swProc.Close();
                                    swProc = null;

                                    var dataEvento = Convert.ToDateTime(eventoID.Substring(17, 4) + "-" + eventoID.Substring(21, 2) + "-" + eventoID.Substring(23, 2));

                                    TFunctions.MoverArquivo(nomeArqDist, PastaEnviados.Autorizados, dataEvento);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (swProc != null)
                {
                    swProc.Close();
                }
            }
        }

        private void MoverPastaProcessamento(ReinfRetornoLoteAssincrono xmlRetorno)
        {
            var cdResposta = xmlRetorno.RetornoLoteEventosAssincrono.Status.CdResposta.ToString();

            if (cdResposta.Equals("1") && !string.IsNullOrEmpty(cdResposta))
            {
                ConteudoXML.Save(NomeArquivoXML);

                var protocoloEnvio = xmlRetorno.RetornoLoteEventosAssincrono.DadosRecepcaoLote.ProtocoloEnvio.ToString();

                TFunctions.MoverArquivo(NomeArquivoXML, PastaEnviados.EmProcessamento, $"{protocoloEnvio}.xml");
            }
        }
    }
}