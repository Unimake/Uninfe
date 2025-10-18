using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.DARE;
using Unimake.Business.DFe.Xml.DARE;

namespace NFe.Service.DARE
{
    public class TaskRecepcaoDARE : TaskAbst
    {
        public TaskRecepcaoDARE(string arquivo)
        {
            Servico = Servicos.RecepcaoDARE;

            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).RetornoXML;

            try
            {
                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.DARE,
                    TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    Servico = Unimake.Business.DFe.Servicos.Servico.DAREEnvio,
                    SchemaVersao = "1.00",
                    ApiKey = Empresas.Configuracoes[emp].SenhaWS
                };

                if (ConteudoXML.OuterXml.Contains("DareLote"))
                {
                    var xmlRecepcaoDareLote = new DARELote();
                    xmlRecepcaoDareLote = xmlRecepcaoDareLote.LerXML<DARELote>(ConteudoXML);

                    var envioDareLote = new Unimake.Business.DFe.Servicos.DARE.EnvioDARELote(xmlRecepcaoDareLote, configuracao);
                    envioDareLote.Executar();

                    ConteudoXML = envioDareLote.ConteudoXMLAssinado;

                    vStrXmlRetorno = envioDareLote.RetornoWSString;

                    XmlRetorno(finalArqEnvio, finalArqRetorno);

                    if (vStrXmlRetorno.Contains("zipDownload"))
                    {
                        ExtrairZip(envioDareLote, emp);

                        GravarXmlDistribuicao(envioDareLote, emp);
                    }

                }
                else
                {
                    var xmlRecepcaoDARE = new Unimake.Business.DFe.Xml.DARE.DARE();
                    xmlRecepcaoDARE = xmlRecepcaoDARE.LerXML<Unimake.Business.DFe.Xml.DARE.DARE>(ConteudoXML);
                    var envioDare = new Unimake.Business.DFe.Servicos.DARE.EnvioDARE(xmlRecepcaoDARE, configuracao);

                    envioDare.Executar();

                    ConteudoXML = envioDare.ConteudoXMLAssinado;

                    vStrXmlRetorno = envioDare.RetornoWSString;

                    XmlRetorno(finalArqEnvio, finalArqRetorno);

                    if (vStrXmlRetorno.Contains("documentoImpressao"))
                    {
                        ExtrairPDF(envioDare, emp);

                        GravarXmlDistribuicao(envioDare, emp);
                    }
                }

                /// grava o arquivo no FTP
                var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                                                  Functions.ExtrairNomeArq(NomeArquivoXML,
                                                  Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).EnvioXML) + "\\" + Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).RetornoXML);

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
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).RetornoERR, ex);
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

        #region Private Methods

        private void ExtrairPDF(EnvioDARE envioDARE, int emp)
        {
            string arquivo = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).EnvioXML) + ".pdf";
            string nomePastaRetorno = Empresas.Configuracoes[emp].PastaXmlRetorno;

            if (!Directory.Exists(nomePastaRetorno))
            {
                Directory.CreateDirectory(nomePastaRetorno);
            }

            envioDARE.ExtrairPDF(nomePastaRetorno, arquivo);
        }

        private void ExtrairZip(EnvioDARELote envioDARELote, int emp)
        {
            string arquivo = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).EnvioXML) + ".zip";
            string nomePastaRetorno = Empresas.Configuracoes[emp].PastaXmlRetorno;

            if (!Directory.Exists(nomePastaRetorno))
            {
                Directory.CreateDirectory(nomePastaRetorno);
            }

            envioDARELote.ExtrairZip(nomePastaRetorno, arquivo);
        }

        private void GravarXmlDistribuicao(dynamic envioDARE, int emp)
        {
            string arquivo = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.RecepcaoDARE).EnvioXML) + "-dareproc.xml";
            string nomePastaAutorizados = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                    PastaEnviados.Autorizados.ToString() + "\\" +
                    Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(DateTime.Now);

            if (!Directory.Exists(nomePastaAutorizados))
            {
                Directory.CreateDirectory(nomePastaAutorizados);
            }

            envioDARE.GravarXmlDistribuicao(nomePastaAutorizados, arquivo);
        }

        #endregion Private Methods
    }
}
