using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Unimake.Business.DFe.Xml.ESocial;

namespace NFe.Service
{
    public class TaskDownloadEventoseSocial : TaskAbst
    {
        public TaskDownloadEventoseSocial(string arquivo)
        {
            Servico = Servicos.DownloadEventoseSocial;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_downevt).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_downevt).RetornoXML;

            try
            {
                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.ESocial,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    Servico = Unimake.Business.DFe.Servicos.Servico.ESocialDownloadEvts,
                };

                if (ConteudoXML.OuterXml.Contains("solicDownloadEvtsPorId"))
                {
                    var xmlDownloadESocial = new Unimake.Business.DFe.Xml.ESocial.DownloadEventosPorID();
                    xmlDownloadESocial = XMLUtility.Deserializar<DownloadEventosPorID>(ConteudoXML);

                    var servicoDownloadESocial = new Unimake.Business.DFe.Servicos.ESocial.DownloadPorID(xmlDownloadESocial, configuracao);

                    servicoDownloadESocial.Executar();

                    ConteudoXML = servicoDownloadESocial.ConteudoXMLAssinado;

                    vStrXmlRetorno = servicoDownloadESocial.RetornoWSString;

                    servicoDownloadESocial.Dispose();
                }

                else if (ConteudoXML.OuterXml.Contains("solicDownloadEventosPorNrRecibo"))
                {
                    var xmlDownloadESocial = new Unimake.Business.DFe.Xml.ESocial.DownloadEventosPorNrRec();
                    xmlDownloadESocial = XMLUtility.Deserializar<DownloadEventosPorNrRec>(ConteudoXML);

                    var servicoDownloadESocial = new Unimake.Business.DFe.Servicos.ESocial.DownloadPorNrRec(xmlDownloadESocial, configuracao);

                    servicoDownloadESocial.Executar();

                    ConteudoXML = servicoDownloadESocial.ConteudoXMLAssinado;

                    vStrXmlRetorno = servicoDownloadESocial.RetornoWSString;

                    servicoDownloadESocial.Dispose();
                }

                XmlRetorno(finalArqEnvio, finalArqRetorno);
                
                /// grava o arquivo no FTP
                string filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  Functions.ExtrairNomeArq(NomeArquivoXML,
                                                  Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_downevt).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_downevt).RetornoXML);
                if (File.Exists(filenameFTP))
                    new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_downevt).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.eSocial_downevt).RetornoERR, ex);
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