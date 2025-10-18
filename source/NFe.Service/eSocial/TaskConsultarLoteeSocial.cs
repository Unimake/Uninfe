using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;

namespace NFe.Service
{
    public class TaskConsultarLoteeSocial : TaskAbst
    {
        public TaskConsultarLoteeSocial(string arquivo)
        {
            Servico = Servicos.ConsultarLoteeSocial;

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
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).RetornoERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                //Serialização do xml
                var xmlESocial = new Unimake.Business.DFe.Xml.ESocial.ConsultarLoteEventos();
                xmlESocial = XMLUtility.Deserializar<Unimake.Business.DFe.Xml.ESocial.ConsultarLoteEventos>(ConteudoXML);

                //Configuração mínima para o consumo do serviço
                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.ESocial,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                };

                //Cria o serviço ConsultaLoteAssincrono
                var autorizacao = new Unimake.Business.DFe.Servicos.ESocial.ConsultaLoteAssincrono(xmlESocial, configuracao);
                autorizacao.Executar();

                //Recebe os retornos
                ConteudoXML = autorizacao.RetornoWSXML;
                vStrXmlRetorno = autorizacao.RetornoWSString;

                var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).EnvioXML;
                var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).RetornoXML;

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                GerarXMLDistribuicao(NomeArquivoXML, emp);

                ///
                /// grava o arquivo no FTP
                string filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  Functions.ExtrairNomeArq(NomeArquivoXML,
                                                  Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).RetornoXML);
                if (File.Exists(filenameFTP))
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_consloteevt).RetornoERR, ex);
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

        private void GerarXMLDistribuicao(string nomeArquivoXML, int emp)
        {
            XmlDocument arquivoRetornoConsultaLoteEventos = new XmlDocument();
            arquivoRetornoConsultaLoteEventos.LoadXml(vStrXmlRetorno);
            XmlNode eventoNode = null;
            StreamWriter swProc = null;
            var protocoloEnvio = ConteudoXML.GetElementsByTagName("protocoloEnvio")[0].InnerText;
            bool contemEventoComErro = false;
            var nomeArquivoProtocolo = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado, "EmProcessamento", $"{protocoloEnvio}.xml");

            XmlNode retornoProcessamentoLoteEventos = arquivoRetornoConsultaLoteEventos.GetElementsByTagName("retornoProcessamentoLoteEventos")[0];

            var codigoResposta = ((XmlElement)retornoProcessamentoLoteEventos).GetElementsByTagName("cdResposta")[0].InnerText;

            if (codigoResposta.Equals("201") && !String.IsNullOrEmpty(codigoResposta))
            {
                XmlNode retornoEventos = ((XmlElement)retornoProcessamentoLoteEventos).GetElementsByTagName("retornoEventos")[0];

                foreach (XmlNode retornoEvento in retornoEventos)
                {
                    var codigoRespostaEvento = ((XmlElement)retornoEvento).GetElementsByTagName("cdResposta")[0].InnerText;

                    if (codigoRespostaEvento.Equals("201") && !String.IsNullOrEmpty(codigoRespostaEvento))
                    {
                        var retornoEventoID = retornoEvento.Attributes.GetNamedItem("Id").Value;

                        XmlDocument arquivoLoteEventos = new XmlDocument();
                        arquivoLoteEventos.Load(nomeArquivoProtocolo);

                        XmlNode eventos = arquivoLoteEventos.GetElementsByTagName("eventos")[0];

                        foreach (XmlNode evento in eventos)
                        {
                            var eventoID = evento.Attributes.GetNamedItem("Id").Value;

                            if (retornoEventoID.Equals(eventoID))
                            {
                                eventoNode = ((XmlElement)evento).GetElementsByTagName("eSocial")[0];

                                XmlNode retEventos = ((XmlElement)retornoEvento).GetElementsByTagName("retornoEvento")[0];

                                var xmlDistribuicao = "<esocialProc>";
                                xmlDistribuicao += eventoNode.OuterXml;

                                foreach (XmlNode retEvento in retEventos)
                                {
                                    xmlDistribuicao += "<retornoEvento>";
                                    xmlDistribuicao += retEvento.OuterXml;
                                    xmlDistribuicao += "</retornoEvento>";
                                }

                                xmlDistribuicao += "</esocialProc>";

                                //Nome do arquivo de distribuição do eSocial
                                string nomeArqDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                    PastaEnviados.EmProcessamento.ToString() + "\\" +
                                    eventoID + Propriedade.ExtRetorno.ProceSocial;

                                //Gravar o XML em uma linha só (sem quebrar as tag´s linha a linha) ou dá erro na hora de
                                //validar o XML pelos Schemas. Wandrey/André 10/08/2018
                                swProc = File.CreateText(nomeArqDist);
                                swProc.Write(xmlDistribuicao);
                                swProc.Close();
                                swProc = null;

                                DateTime dataEvento = Convert.ToDateTime(eventoID.Substring(17, 4) + "-" + eventoID.Substring(21, 2) + "-" + eventoID.Substring(23, 2));

                                TFunctions.MoverArquivo(nomeArqDist, PastaEnviados.Autorizados, dataEvento);
                            }
                        }
                    }
                    else
                        contemEventoComErro = true;
                }

                if (contemEventoComErro)
                    TFunctions.MoveArqErro(nomeArquivoProtocolo);
                else
                    Functions.DeletarArquivo(nomeArquivoProtocolo);
            }
        }
    }
}