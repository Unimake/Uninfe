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
            NormalizarMonofasiaLegada(xmlEsperado);
            NormalizarMonofasiaLegada(xmlAtual);
            NormalizarVNFTotZero(xmlEsperado);
            NormalizarVNFTotZero(xmlAtual);

            return CompararElemento(xmlEsperado.DocumentElement, xmlAtual.DocumentElement, string.Empty);
        }

        private static void NormalizarVNFTotZero(XmlDocument documento)
        {
            var elementos = documento.GetElementsByTagName("vNFTot")
                .OfType<XmlElement>()
                .ToList();
            foreach (var elemento in elementos)
            {
                if (decimal.TryParse(elemento.InnerText, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor) && valor == 0)
                {
                    elemento.ParentNode.RemoveChild(elemento);
                }
            }
        }

        private static void NormalizarMonofasiaLegada(XmlDocument documento)
        {
            var grupos = documento.GetElementsByTagName("gIBSCBSMono")
                .OfType<XmlElement>()
                .ToList();

            foreach (var grupo in grupos)
            {
                var filhos = ElementosFilhos(grupo);
                var padrao = filhos.FirstOrDefault(x => x.LocalName == "gMonoPadrao");
                var retencao = filhos.FirstOrDefault(x => x.LocalName == "gMonoReten");
                var retido = filhos.FirstOrDefault(x => x.LocalName == "gMonoRet");
                var diferimento = filhos.FirstOrDefault(x => x.LocalName == "gMonoDif");
                if (padrao == null && retencao == null && retido == null && diferimento == null)
                {
                    continue;
                }

                var referencia = filhos.FirstOrDefault(x => x.LocalName == "vTotIBSMonoItem" || x.LocalName == "vTotCBSMonoItem");
                RemoverSeExistir(grupo, padrao);
                RemoverSeExistir(grupo, retencao);
                RemoverSeExistir(grupo, retido);
                RemoverSeExistir(grupo, diferimento);

                var grupoIBS = CriarGrupoAdRem(
                    documento,
                    grupo.NamespaceURI,
                    "gIBSMonoAdRem",
                    padrao,
                    new[] { "qBCMono", "adRemIBS", "vIBSMono" },
                    retencao,
                    new[] { "qBCMonoReten", "adRemIBSReten", "vIBSMonoReten" },
                    retido,
                    "vIBSMonoRet");
                InserirAntes(grupo, grupoIBS, referencia);

                var grupoCBS = CriarGrupoAdRem(
                    documento,
                    grupo.NamespaceURI,
                    "gCBSMonoAdRem",
                    padrao,
                    new[] { "qBCMono", "adRemCBS", "vCBSMono" },
                    retencao,
                    new[] { "qBCMonoReten", "adRemCBSReten", "vCBSMonoReten" },
                    retido,
                    "vCBSMonoRet");
                InserirAntes(grupo, grupoCBS, referencia);
            }
        }

        private static XmlElement CriarGrupoAdRem(
            XmlDocument documento,
            string namespaceUri,
            string nomeGrupo,
            XmlElement padrao,
            string[] camposPadrao,
            XmlElement retencao,
            string[] camposRetencao,
            XmlElement retido,
            string campoRetido)
        {
            var incluirRetido = PossuiValorPositivo(retido, campoRetido);
            if (padrao == null && retencao == null && !incluirRetido)
            {
                return null;
            }

            var resultado = documento.CreateElement(nomeGrupo, namespaceUri);
            AdicionarSubgrupo(documento, resultado, "gMonoPadrao", padrao, camposPadrao);
            AdicionarSubgrupo(documento, resultado, "gMonoReten", retencao, camposRetencao);
            if (incluirRetido)
            {
                AdicionarSubgrupo(documento, resultado, "gMonoRet", retido, new[] { campoRetido });
            }

            return resultado;
        }

        private static void AdicionarSubgrupo(XmlDocument documento, XmlElement destino, string nome, XmlElement origem, IEnumerable<string> campos)
        {
            if (origem == null)
            {
                return;
            }

            var subgrupo = documento.CreateElement(nome, destino.NamespaceURI);
            foreach (var campo in campos)
            {
                var elemento = ElementosFilhos(origem).FirstOrDefault(x => x.LocalName == campo);
                if (elemento != null)
                {
                    subgrupo.AppendChild(documento.ImportNode(elemento, true));
                }
            }
            destino.AppendChild(subgrupo);
        }

        private static bool PossuiValorPositivo(XmlElement grupo, string campo)
        {
            var elemento = grupo == null ? null : ElementosFilhos(grupo).FirstOrDefault(x => x.LocalName == campo);
            return elemento != null &&
                decimal.TryParse(elemento.InnerText, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor) &&
                valor > 0;
        }

        private static void RemoverSeExistir(XmlElement pai, XmlElement filho)
        {
            if (filho != null)
            {
                pai.RemoveChild(filho);
            }
        }

        private static void InserirAntes(XmlElement pai, XmlElement filho, XmlElement referencia)
        {
            if (filho == null)
            {
                return;
            }

            if (referencia == null)
            {
                pai.AppendChild(filho);
            }
            else
            {
                pai.InsertBefore(filho, referencia);
            }
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
