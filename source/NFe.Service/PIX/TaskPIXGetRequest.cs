using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    public class TaskPIXGetRequest : TaskAbst
    {
        public TaskPIXGetRequest(string arquivo)
        {
            Servico = Servicos.PIXGetRequest;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var extEnvio = Propriedade.Extensao(TipoEnvioPIX).EnvioXML;
            var extRetorno = Propriedade.Extensao(TipoEnvioPIX).RetornoXML;
            var file = Functions.ExtrairNomeArq(NomeArquivoXML, extEnvio) + extRetorno;
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
            var finalArqEnvio = Propriedade.Extensao(TipoEnvioPIX).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(TipoEnvioPIX).RetornoXML;

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
                var assembly = typeof(Configuracao).Assembly;
                var pixType = assembly.GetType($"Unimake.Business.DFe.Servicos.PIX.{NomeServicoPIX}");
                if (pixType == null) throw new Exception($"A implementação do serviço PIX {NomeServicoPIX} não foi localizada na DLL.");

                var pixInstance = Activator.CreateInstance(pixType, new object[] { ConteudoXML.OuterXml, configuracao });
                var executarMethod = pixInstance.GetType().GetMethod("Executar");
                if (executarMethod == null) throw new Exception($"A implementação do serviço PIX {NomeServicoPIX} não expõe o método Executar().");
                executarMethod.Invoke(pixInstance, null);

                var retornoProp = pixInstance.GetType().GetProperty("RetornoWSString");
                if (retornoProp != null) vStrXmlRetorno = retornoProp.GetValue(pixInstance)?.ToString();

                if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                {
                    throw new Exception($"A implementação do serviço PIX {NomeServicoPIX} não retornou RetornoWSString. Atualize a DLL para fornecer o XML de retorno pronto.");
                }
                vStrXmlRetorno = AdicionarUniNFeVersaoAoRetorno(vStrXmlRetorno);

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                var disposeMethod = pixInstance.GetType().GetMethod("Dispose");
                disposeMethod?.Invoke(pixInstance, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao executar DLL PIX {NomeServicoPIX}: {ex.Message}", ex);
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

        private Propriedade.TipoEnvio TipoEnvioPIX =>
            Servico == Servicos.PIXGetRequest ? Propriedade.TipoEnvio.PIXGetRequest : Propriedade.TipoEnvio.PIXConsultaRequest;

        private string NomeServicoPIX =>
            Servico == Servicos.PIXGetRequest ? "PixConsultar" : "PixCobrancaConsultar";

        private string NomeElementoRetorno =>
            Servico == Servicos.PIXGetRequest ? "PIXGetResponse" : "PIXConsultaResponse";

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
                oXmlGravar.WriteStartElement(NomeElementoRetorno);
                oXmlGravar.WriteElementString("Status", "999");
                oXmlGravar.WriteElementString("Motivo", motivo);

                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    oXmlGravar.WriteElementString("TraceId", traceId);
                }

                oXmlGravar.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
                oXmlGravar.WriteEndElement();
                oXmlGravar.WriteEndDocument();
            }
        }
    }
}


