using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.EFDReinf;

namespace NFe.Service.EFDReinf
{
    public class TaskConsultaLoteAssincronoReinf : TaskAbst
    {
        public TaskConsultaLoteAssincronoReinf(string arquivo)
        {
            Servico = Servicos.ConsultaLoteAssincReinf;

            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            int emp = Empresas.FindEmpresaByThread();

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_consloteevt).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_consloteevt).RetornoERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                var xmlConsultaLoteReinf = new ReinfConsultaLoteAssincrono();

                xmlConsultaLoteReinf = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<ReinfConsultaLoteAssincrono>(ConteudoXML);

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.EFDReinf,
                    TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                };

                var autorizacaoConsultaLoteReinf = new Unimake.Business.DFe.Servicos.EFDReinf.ConsultaLoteAssincrono(xmlConsultaLoteReinf, configuracao);
                autorizacaoConsultaLoteReinf.Executar();

                ConteudoXML = autorizacaoConsultaLoteReinf.ConteudoXMLAssinado;

                vStrXmlRetorno = autorizacaoConsultaLoteReinf.RetornoWSString;

                var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_consloteevt).EnvioXML;
                var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_consloteevt).RetornoXML;

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                GerarXMLDistribuicao(ConteudoXML, emp);

                ///
                /// grava o arquivo no FTP
                string filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  Functions.ExtrairNomeArq(NomeArquivoXML,
                                                  Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_consloteevt).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_consloteevt).RetornoXML);
                if (File.Exists(filenameFTP))
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_consloteevt).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.Reinf_consloteevt).RetornoERR, ex);
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

        /// <summary>
        /// Gerar XML de distribuição do EFDReinf
        /// </summary>
        private void GerarXMLDistribuicao(XmlDocument conteudoXML, int emp)
        {
            XmlDocument retornoLoteEventosArquivo = new XmlDocument();          
            retornoLoteEventosArquivo.LoadXml(vStrXmlRetorno);
            
            XmlNode evtTotal = null;
            XmlNode eventoAprovado = null;
            StreamWriter swProc = null;

            var protocoloEnvio = ConteudoXML.GetElementsByTagName("numeroProtocolo")[0].InnerText;
            bool contemEventoComErro = false;
            var nomeArquivoProtocolo = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado, "EmProcessamento", $"{protocoloEnvio}.xml");

            XmlNode retornoProcessamentoLoteEventos = retornoLoteEventosArquivo.GetElementsByTagName("retornoLoteEventosAssincrono")[0];

            var codigoResposta = ((XmlElement)retornoProcessamentoLoteEventos).GetElementsByTagName("cdResposta")[0].InnerText;

            if (codigoResposta.Equals("2") && !String.IsNullOrEmpty(codigoResposta))
            {
                XmlNode retornoEventos = ((XmlElement)retornoProcessamentoLoteEventos).GetElementsByTagName("retornoEventos")[0];

                foreach (XmlNode retornoEvento in retornoEventos)
                {
                    string cdRetorno = ((XmlElement)retornoEvento).GetElementsByTagName("cdRetorno")[0].InnerText;

                    if (cdRetorno.Equals("0") && !String.IsNullOrEmpty(cdRetorno))
                    {
                        evtTotal = ((XmlElement)retornoEvento).GetElementsByTagName("Reinf")[0];

                        string retornoEventoID = ((XmlElement)retornoEvento).Attributes.GetNamedItem("Id").Value;

                        if (File.Exists(nomeArquivoProtocolo))
                        {
                            XmlDocument arquivoLoteEventos = new XmlDocument();
                            arquivoLoteEventos.Load(nomeArquivoProtocolo);

                            XmlNode eventos = arquivoLoteEventos.GetElementsByTagName("eventos")[0];

                            foreach (XmlNode evento in eventos)
                            {
                                var eventoID = evento.Attributes.GetNamedItem("Id").Value;

                                if (retornoEventoID.Equals(eventoID))
                                {
                                    string xmlDistribuicao = "<reinfProc>";
                                    eventoAprovado = ((XmlElement)evento).GetElementsByTagName("Reinf")[0];
                                    xmlDistribuicao += eventoAprovado.OuterXml;
                                    xmlDistribuicao += "<retornoEvento>";
                                    xmlDistribuicao += evtTotal.OuterXml;
                                    xmlDistribuicao += "</retornoEvento>";
                                    xmlDistribuicao += "</reinfProc>";

                                    //Nome do arquivo de distribuição do EFDReinf
                                    string nomeArqDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                        PastaEnviados.EmProcessamento.ToString() + "\\" +
                                        eventoID + Propriedade.ExtRetorno.ProcReinf;

                                    //Gravar o XML em uma linha só (sem quebrar as tag´s linha a linha) ou dá erro na hora de
                                    //validar o XML pelos Schemas. Wandrey/André 10/08/2018
                                    swProc = File.CreateText(nomeArqDist);
                                    swProc.Write(xmlDistribuicao);
                                    swProc.Close();
                                    swProc = null;

                                    DateTime dataEvento = Convert.ToDateTime(eventoID.Substring(17, 4) + "-" + eventoID.Substring(21, 2) + "-" + eventoID.Substring(23, 2));

                                    TFunctions.MoverArquivo(nomeArqDist, PastaEnviados.Autorizados, dataEvento);

                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        contemEventoComErro = true;
                    }                        
                }
                if (contemEventoComErro)
                {
                    TFunctions.MoveArqErro(nomeArquivoProtocolo);
                }                    
                else
                {
                    Functions.DeletarArquivo(nomeArquivoProtocolo);
                }                   
            }
        }
    }
}