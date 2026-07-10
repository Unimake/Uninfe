using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Globalization;

namespace UniNFe.Test.NFeConvertTxt
{
    internal static class NFeConvertTxtXmlComparer
    {
        public static string Comparar(string esperado, string atual)
        {
            var xmlEsperado = Carregar(esperado);
            var xmlAtual = Carregar(atual);

            return CompararElemento(xmlEsperado.DocumentElement, xmlAtual.DocumentElement, string.Empty);
        }

        private static XmlDocument Carregar(string xml)
        {
            var documento = new XmlDocument { PreserveWhitespace = false };
            documento.LoadXml(xml);
            return documento;
        }

        private static string CompararElemento(XmlElement esperado, XmlElement atual, string caminhoPai)
        {
            if (esperado == null || atual == null)
            {
                return $"{caminhoPai}: elemento esperado ou atual nao existe.";
            }

            var caminho = caminhoPai + "/" + esperado.LocalName;
            if (esperado.LocalName != atual.LocalName || esperado.NamespaceURI != atual.NamespaceURI)
            {
                return $"{caminho}: esperado '{esperado.Name}' no namespace '{esperado.NamespaceURI}', encontrado '{atual.Name}' no namespace '{atual.NamespaceURI}'.";
            }

            var diferencaAtributo = CompararAtributos(esperado, atual, caminho);
            if (diferencaAtributo != null)
            {
                return diferencaAtributo;
            }

            var filhosEsperados = ElementosFilhos(esperado);
            var filhosAtuais = ElementosFilhos(atual);
            if (filhosEsperados.Count != filhosAtuais.Count)
            {
                return $"{caminho}: esperados {filhosEsperados.Count} elementos filhos [{string.Join(",", filhosEsperados.Select(Descrever))}], encontrados {filhosAtuais.Count} [{string.Join(",", filhosAtuais.Select(Descrever))}].";
            }

            if (filhosEsperados.Count == 0 && !ValoresEquivalentes(esperado.InnerText, atual.InnerText))
            {
                return $"{caminho}: esperado valor '{esperado.InnerText}', encontrado '{atual.InnerText}'.";
            }

            for (var i = 0; i < filhosEsperados.Count; i++)
            {
                var diferenca = CompararElemento(filhosEsperados[i], filhosAtuais[i], caminho + $"[{i + 1}]");
                if (diferenca != null)
                {
                    return diferenca;
                }
            }

            return null;
        }

        private static string Descrever(XmlElement elemento) => ElementosFilhos(elemento).Count == 0 ? elemento.LocalName + "=" + elemento.InnerText : elemento.LocalName;

        private static bool ValoresEquivalentes(string esperado, string atual)
        {
            if (esperado == atual) return true;
            return decimal.TryParse(esperado, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeroEsperado) &&
                decimal.TryParse(atual, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeroAtual) &&
                numeroEsperado == numeroAtual;
        }

        private static string CompararAtributos(XmlElement esperado, XmlElement atual, string caminho)
        {
            var atributosEsperados = Atributos(esperado);
            var atributosAtuais = Atributos(atual);
            if (atributosEsperados.Count != atributosAtuais.Count)
            {
                return $"{caminho}: esperados {atributosEsperados.Count} atributos, encontrados {atributosAtuais.Count}.";
            }

            foreach (var atributo in atributosEsperados)
            {
                if (!atributosAtuais.TryGetValue(atributo.Key, out var valorAtual))
                {
                    return $"{caminho}: atributo '{atributo.Key}' nao encontrado.";
                }

                if (atributo.Value != valorAtual)
                {
                    return $"{caminho}/@{atributo.Key}: esperado '{atributo.Value}', encontrado '{valorAtual}'.";
                }
            }

            return null;
        }

        private static Dictionary<string, string> Atributos(XmlElement elemento) => elemento.Attributes
            .Cast<XmlAttribute>()
            .Where(x => x.Prefix != "xmlns" && x.Name != "xmlns")
            .ToDictionary(x => "{" + x.NamespaceURI + "}" + x.LocalName, x => x.Value);

        private static List<XmlElement> ElementosFilhos(XmlElement elemento) => elemento.ChildNodes
            .OfType<XmlElement>()
            .ToList();
    }
}
