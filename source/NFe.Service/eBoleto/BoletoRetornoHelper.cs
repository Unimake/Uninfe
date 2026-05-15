using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using NFe.Components;

namespace NFe.Service
{
    internal static class BoletoRetornoHelper
    {
        private static readonly string[] TraceNames =
        {
            "TraceId",
            "TraceID",
            "traceId",
            "traceID",
            "TraceIdentifier",
            "RequestId",
            "RequestID",
            "requestId",
            "requestID",
            "CorrelationId",
            "CorrelationID",
            "correlationId",
            "correlationID",
            "x-amzn-trace-id",
            "X-Amzn-Trace-Id"
        };

        public static string ExtrairTraceId(object origem)
        {
            return ExtrairTraceIdInterno(origem, new HashSet<int>(), 0) ?? string.Empty;
        }

        public static void GravarXmlRetorno(
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

        private static string ExtrairTraceIdInterno(object origem, HashSet<int> visitados, int nivel)
        {
            if (origem == null || nivel > 6)
            {
                return null;
            }

            if (origem is string textoOrigem)
            {
                return ExtrairTraceIdDaString(textoOrigem);
            }

            var idReferencia = RuntimeHelpers.GetHashCode(origem);
            if (!visitados.Add(idReferencia))
            {
                return null;
            }

            if (origem is Exception exception)
            {
                var traceDaMensagem = ExtrairTraceIdDaString(exception.Message);
                if (!string.IsNullOrWhiteSpace(traceDaMensagem))
                {
                    return traceDaMensagem;
                }

                var traceDaInnerException = ExtrairTraceIdInterno(exception.InnerException, visitados, nivel + 1);
                if (!string.IsNullOrWhiteSpace(traceDaInnerException))
                {
                    return traceDaInnerException;
                }
            }

            if (origem is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key != null && IsTraceName(entry.Key.ToString()))
                    {
                        var traceDaChave = entry.Value?.ToString();
                        if (!string.IsNullOrWhiteSpace(traceDaChave))
                        {
                            return traceDaChave.Trim();
                        }
                    }

                    var traceDaEntrada = ExtrairTraceIdInterno(entry.Value, visitados, nivel + 1);
                    if (!string.IsNullOrWhiteSpace(traceDaEntrada))
                    {
                        return traceDaEntrada;
                    }
                }
            }

            if (origem is IEnumerable enumerable && !(origem is string))
            {
                foreach (var item in enumerable)
                {
                    var traceDoItem = ExtrairTraceIdInterno(item, visitados, nivel + 1);
                    if (!string.IsNullOrWhiteSpace(traceDoItem))
                    {
                        return traceDoItem;
                    }
                }
            }

            foreach (var propriedade in origem.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!propriedade.CanRead || propriedade.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                object valor;
                try
                {
                    valor = propriedade.GetValue(origem, null);
                }
                catch
                {
                    continue;
                }

                if (valor == null)
                {
                    continue;
                }

                if (IsTraceName(propriedade.Name))
                {
                    var textoEncontrado = valor.ToString();
                    if (!string.IsNullOrWhiteSpace(textoEncontrado))
                    {
                        return textoEncontrado.Trim();
                    }
                }

                if (valor is string textoPropriedade)
                {
                    var traceDaString = ExtrairTraceIdDaString(textoPropriedade);
                    if (!string.IsNullOrWhiteSpace(traceDaString))
                    {
                        return traceDaString;
                    }
                    continue;
                }

                var traceDaPropriedade = ExtrairTraceIdInterno(valor, visitados, nivel + 1);
                if (!string.IsNullOrWhiteSpace(traceDaPropriedade))
                {
                    return traceDaPropriedade;
                }
            }

            return null;
        }

        private static bool IsTraceName(string nome)
        {
            foreach (var traceName in TraceNames)
            {
                if (string.Equals(nome, traceName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ExtrairTraceIdDaString(string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
            {
                return null;
            }

            var padroes = new[]
            {
                @"(?i)\btrace(?:id|identifier)?\b\s*[:=]\s*(?<valor>[^\s|;,\r\n]+)",
                @"(?i)\brequest(?:id)?\b\s*[:=]\s*(?<valor>[^\s|;,\r\n]+)",
                @"(?i)\bcorrelation(?:id)?\b\s*[:=]\s*(?<valor>[^\s|;,\r\n]+)",
                @"(?i)\bRoot=(?<valor>[^\s|;,\r\n]+)"
            };

            foreach (var padrao in padroes)
            {
                var match = Regex.Match(conteudo, padrao);
                if (match.Success)
                {
                    return match.Groups["valor"].Value.Trim();
                }
            }

            return null;
        }
    }
}
