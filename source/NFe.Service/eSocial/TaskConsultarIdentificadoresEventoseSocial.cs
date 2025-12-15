using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Unimake.Business.DFe.Xml.ESocial;

namespace NFe.Service
{
    public class TaskConsultarIdentificadoresEventoseSocial : TaskAbst
    {
        public TaskConsultarIdentificadoresEventoseSocial(string arquivo)
        {
            Servico = Servicos.ConsultarIdentificadoresEventoseSocial;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_considevt).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_considevt).RetornoXML;

            try
            {
                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.ESocial,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    Servico = Unimake.Business.DFe.Servicos.Servico.ESocialConsultaEvts,
                    ApiKey = Empresas.Configuracoes[emp].SenhaWS
                };

                if (ConteudoXML.OuterXml.Contains("consultaEvtsTrabalhador"))
                {
                    var xmlConsultaESocial = new Unimake.Business.DFe.Xml.ESocial.ConsultarEvtsTrabalhadorESocial();
                    xmlConsultaESocial = XMLUtility.Deserializar<ConsultarEvtsTrabalhadorESocial>(ConteudoXML);

                    var servicoConsultaESocial = new Unimake.Business.DFe.Servicos.ESocial.ConsultarEvtsTrabalhador(xmlConsultaESocial, configuracao);

                    servicoConsultaESocial.Executar();

                    ConteudoXML = servicoConsultaESocial.ConteudoXMLAssinado;

                    vStrXmlRetorno = servicoConsultaESocial.RetornoWSString;

                    servicoConsultaESocial.Dispose();
                }
                else if (ConteudoXML.OuterXml.Contains("consultaEvtsEmpregador"))
                {
                    var xmlConsultaESocial = new Unimake.Business.DFe.Xml.ESocial.ConsultarEvtsEmpregadorESocial();
                    xmlConsultaESocial = XMLUtility.Deserializar<ConsultarEvtsEmpregadorESocial>(ConteudoXML);

                    var servicoConsultaESocial = new Unimake.Business.DFe.Servicos.ESocial.ConsultarEvtsEmpregador(xmlConsultaESocial, configuracao);

                    servicoConsultaESocial.Executar();

                    ConteudoXML = servicoConsultaESocial.ConteudoXMLAssinado;

                    vStrXmlRetorno = servicoConsultaESocial.RetornoWSString;

                    servicoConsultaESocial.Dispose();
                }
                else if (ConteudoXML.OuterXml.Contains("consultaEvtsTabela"))
                {
                    var xmlConsultaESocial = new Unimake.Business.DFe.Xml.ESocial.ConsultarEvtsTabelaESocial();
                    xmlConsultaESocial = XMLUtility.Deserializar<ConsultarEvtsTabelaESocial>(ConteudoXML);

                    var servicoConsulta = new Unimake.Business.DFe.Servicos.ESocial.ConsultarEvtsTabela(xmlConsultaESocial, configuracao);

                    servicoConsulta.Executar();

                    ConteudoXML = servicoConsulta.ConteudoXMLAssinado;

                    vStrXmlRetorno = servicoConsulta.RetornoWSString;

                    servicoConsulta.Dispose();
                }

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                /// grava o arquivo no FTP
                string filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                          Functions.ExtrairNomeArq(NomeArquivoXML,
                                                          Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_considevt).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_considevt).RetornoXML);
                if (File.Exists(filenameFTP))
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_considevt).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_considevt).RetornoERR, ex);
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
    }
}