using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.EBoleto;

namespace NFe.Service
{
    public class TaskBoletoRegistrar : TaskAbst
    {
        public TaskBoletoRegistrar(string arquivo)
        {
            Servico = Servicos.BoletoRegistrar;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).RetornoXML;
            var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

            try
            {
                if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].AppID) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].Secret))
                {
                    throw new Exception("Para utilizar o serviço do eBoleto é necessário configurar no UniNFe o AppID e Secret do eBank.");
                }

                ExecuteDLL(emp);
            }
            catch (Exception ex)
            {
                var lastException = ex.GetLastException();
                var traceId = ApiExceptionHelper.ExtrairTraceId(lastException);
                ApiExceptionHelper.GravarXmlRetornoEBoleto(pathXml, "BoletoRegistrarResponse", "999", lastException.Message.Replace("\r\n", ""), traceId);
            }
            finally
            {
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                }
            }
        }

        #region ExecuteDLL

        private void ExecuteDLL(int emp)
        {
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).RetornoXML;

            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo,
                AppId = Empresas.Configuracoes[emp].AppID,
                Secret = Empresas.Configuracoes[emp].Secret
            };

            try
            {
                using (var boleto = new BoletoRegistrar(ConteudoXML.OuterXml, configuracao))
                {
                    boleto.Executar();
                    vStrXmlRetorno = boleto.RetornoWSString;

                    if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                    {
                        throw new Exception("A implementação do serviço eBoleto BoletoRegistrar não retornou RetornoWSString. Atualize a DLL para fornecer o XML de retorno pronto.");
                    }

                    vStrXmlRetorno = AdicionarUniNFeVersaoAoRetorno(vStrXmlRetorno);

                    ExtrairPDFRetorno(emp, finalArqEnvio, finalArqRetorno);
                    XmlRetorno(finalArqEnvio, finalArqRetorno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao executar DLL eBoleto BoletoRegistrar: {ex.Message}", ex);
            }
        }

        #region ExtrairPDFRetorno

        /// <summary>
        /// Extrai o conteúdo da tag PdfContentBase64 do retorno do e-Boleto e grava o PDF na pasta de retorno.
        /// </summary>
        /// <param name="emp">Código da empresa</param>
        /// <param name="finalArqEnvio">Extensão final do arquivo de envio</param>
        /// <param name="finalArqRetorno">Extensão final do arquivo XML de retorno</param>
        public void ExtrairPDFRetorno(int emp, string finalArqEnvio, string finalArqRetorno)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                {
                    return;
                }

                var doc = new XmlDocument();
                doc.Load(Functions.StringXmlToStream(vStrXmlRetorno));

                var pdfContentSuccess = doc.GetElementsByTagName("PdfContentSuccess");
                if (pdfContentSuccess.Count > 0 && pdfContentSuccess[0].InnerText.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var pdfContentBase64 = doc.GetElementsByTagName("PdfContentBase64");
                if (pdfContentBase64.Count == 0 || string.IsNullOrWhiteSpace(pdfContentBase64[0].InnerText))
                {
                    return;
                }

                var arqPDF = Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + finalArqRetorno;
                arqPDF = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, arqPDF.Replace(".xml", ".pdf"));

                if (File.Exists(arqPDF))
                {
                    File.Delete(arqPDF);
                }

                File.WriteAllBytes(arqPDF, Convert.FromBase64String(pdfContentBase64[0].InnerText));

                var pdfPath = doc.GetElementsByTagName("PdfPath");
                XmlElement elementoPdfPath;
                if (pdfPath.Count > 0)
                {
                    elementoPdfPath = (XmlElement)pdfPath[0];
                }
                else
                {
                    elementoPdfPath = doc.CreateElement("PdfPath");
                    var root = doc.DocumentElement;
                    var proximoElemento = root?["PixPagamentoDetalhe"] ?? root?["QRCodeContent"] ?? root?["DLLVersao"] ?? root?["UniNFeVersao"];

                    if (proximoElemento != null)
                    {
                        root.InsertBefore(elementoPdfPath, proximoElemento);
                    }
                    else
                    {
                        root?.AppendChild(elementoPdfPath);
                    }
                }

                elementoPdfPath.InnerText = arqPDF;
                vStrXmlRetorno = doc.OuterXml;
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog("TaskBoletoRegistrar: Não foi possível extrair o PDF do retorno do e-Boleto. O XML de retorno do registro foi preservado. Erro: " + ex.GetAllMessages(), true);
            }
        }

        #endregion ExtrairPDFRetorno

        private string AdicionarUniNFeVersaoAoRetorno(string xmlRetorno)
        {
            if (string.IsNullOrWhiteSpace(xmlRetorno))
            {
                return xmlRetorno;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlRetorno);

            var root = xmlDoc.DocumentElement;
            if (root == null || root["UniNFeVersao"] != null)
            {
                return xmlRetorno;
            }

            var versaoNode = xmlDoc.CreateElement("UniNFeVersao");
            versaoNode.InnerText = Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-");
            root.AppendChild(versaoNode);

            return xmlDoc.OuterXml;
        }
        #endregion ExecuteDLL
    }
}
