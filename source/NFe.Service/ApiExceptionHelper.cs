using System;
using System.Text.Json;
using System.Xml;
using NFe.Components;

namespace NFe.Service
{
    internal static class ApiExceptionHelper
    {

        public static string ExtrairTraceId(object origem)
        {
            try
            {
                string json = origem is Exception ex ? ex.Message : origem as string;
                if (string.IsNullOrWhiteSpace(json))
                    return string.Empty;

                var trimmed = json.TrimStart();
                if (!trimmed.StartsWith("{"))
                    return string.Empty;

                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return string.Empty;

                foreach (var prop in root.EnumerateObject())
                {
                    if (string.Equals(prop.Name, "traceId", StringComparison.OrdinalIgnoreCase) && prop.Value.ValueKind == JsonValueKind.String)
                        return prop.Value.GetString() ?? string.Empty;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void GravarXmlRetornoEBoleto(
            string path,
            string rootElement,
            string status,
            string motivo,
            string traceId,
            Action<XmlWriter> escreverConteudo = null)
        {
            var settings = new XmlWriterSettings
            {
                Encoding = new System.Text.UTF8Encoding(false),
                Indent = true,
                IndentChars = " ",
                NewLineOnAttributes = false,
                OmitXmlDeclaration = false
            };

            XmlWriter xmlWriter = null;

            try
            {
                xmlWriter = XmlWriter.Create(path, settings);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement(rootElement);
                xmlWriter.WriteElementString("Status", status);
                xmlWriter.WriteElementString("Motivo", motivo);

                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    xmlWriter.WriteElementString("TraceId", traceId);
                }

                escreverConteudo?.Invoke(xmlWriter);

                xmlWriter.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                xmlWriter.Close();
            }
            finally
            {
                if (xmlWriter != null && xmlWriter.WriteState != WriteState.Closed)
                {
                    xmlWriter.Close();
                }
            }
        }

        public static void GravarXmlRetornoUMessenger(
            XmlWriter xmlWriter,
            string status,
            string motivo,
            string returnMessageID = "",
            string messageID = "",
            string traceId = "")
        {
            xmlWriter.WriteStartElement("Mensagem");

            if (!string.IsNullOrWhiteSpace(messageID))
            {
                xmlWriter.WriteAttributeString("Id", messageID);
            }

            xmlWriter.WriteElementString("Status", status);
            xmlWriter.WriteElementString("Motivo", motivo);

            if (!string.IsNullOrWhiteSpace(returnMessageID) && status == "1")
            {
                xmlWriter.WriteElementString("messageID", returnMessageID);
            }

            if (!string.IsNullOrWhiteSpace(traceId))
            {
                xmlWriter.WriteElementString("TraceId", traceId);
            }

            xmlWriter.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
            xmlWriter.WriteEndElement(); //Mensagem
        }
    }
}
