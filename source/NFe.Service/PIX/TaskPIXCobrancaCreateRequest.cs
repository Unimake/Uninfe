using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Servicos.PIX;

namespace NFe.Service
{
    public class TaskPIXCobrancaCreateRequest : TaskAbst
    {
        public TaskPIXCobrancaCreateRequest(string arquivo)
        {
            Servico = Servicos.PIXCobrancaCreateRequest;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).RetornoXML;
            var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

            try
            {
                if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].AppID) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].Secret))
                {
                    throw new Exception("Para utilizar o serviço de PIX é necessário configurar no UniNFe o AppID e Secret do eBank.");
                }

                ExecuteDLL(emp);
            }
            catch (Exception ex)
            {
                var lastException = ex.GetLastException();
                var traceId = ApiExceptionHelper.ExtrairTraceId(lastException);
                GerarXmlRetornoErro(pathXml, lastException.Message.Replace("\r\n", ""), traceId);
            }
            finally
            {
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                    // Se falhar a exclusao, o UniNFe tentara processar o arquivo novamente.
                }
            }
        }

        #region ExecuteDLL

        private void ExecuteDLL(int emp)
        {
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).RetornoXML;
            var file = Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + finalArqRetorno;

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
                using (var pixCobrancaCriar = new PixCobrancaCriar(ConteudoXML.OuterXml, configuracao))
                {
                    pixCobrancaCriar.Executar();

                    if (pixCobrancaCriar.Result.Status == 0)
                    {
                        var imageFormat = pixCobrancaCriar.Envio.QRCodeConfig?.ImageFormat ?? PixQrCodeImageFormat.PNG;
                        var pathQRCode = Path.Combine(
                            Empresas.Configuracoes[emp].PastaXmlRetorno,
                            file.Replace(".xml", "." + imageFormat.ToString().ToLowerInvariant()));

                        pixCobrancaCriar.GravarQRCode(pathQRCode);
                    }

                    vStrXmlRetorno = pixCobrancaCriar.RetornoWSString;

                    if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                    {
                        throw new Exception("A implementação do serviço PIX PixCobrancaCriar não retornou RetornoWSString. Atualize a DLL para fornecer o XML de retorno pronto.");
                    }
                    vStrXmlRetorno = AdicionarUniNFeVersaoAoRetorno(vStrXmlRetorno);

                    XmlRetorno(finalArqEnvio, finalArqRetorno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao executar DLL PIX PixCobrancaCriar: {ex.Message}", ex);
            }
        }

        #endregion ExecuteDLL

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
        private void GerarXmlRetornoErro(string path, string motivo, string traceId)
        {
            var oSettings = new XmlWriterSettings();
            var c = new UTF8Encoding(false);

            oSettings.Encoding = c;
            oSettings.Indent = true;
            oSettings.IndentChars = " ";
            oSettings.NewLineOnAttributes = false;
            oSettings.OmitXmlDeclaration = false;

            using (var oXmlGravar = XmlWriter.Create(path, oSettings))
            {
                oXmlGravar.WriteStartDocument();
                oXmlGravar.WriteStartElement("PIXCobrancaCreateResponse");
                oXmlGravar.WriteElementString("Status", "999");
                oXmlGravar.WriteElementString("Motivo", motivo);

                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    oXmlGravar.WriteElementString("TraceId", traceId);
                }

                oXmlGravar.WriteElementString("PixCopiaECola", string.Empty);
                oXmlGravar.WriteElementString("ImageQRCode", string.Empty);
                oXmlGravar.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
                oXmlGravar.WriteEndElement();
                oXmlGravar.WriteEndDocument();
            }
        }
    }
}


