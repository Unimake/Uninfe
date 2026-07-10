using NFe.ConvertTxt.Mapping;
using System.Xml;
using Unimake.Business.DFe.Utility;

namespace NFe.ConvertTxt.Generation
{
    public sealed class NFeDFeXmlSerializer
    {
        public XmlDocument Serializar(NFe nota)
        {
            var documento = XMLUtility.Serializar(new NFeDFeMapper().Mapear(nota));
            AjustarCompatibilidade(documento);
            return documento;
        }

        private static void AjustarCompatibilidade(XmlDocument documento)
        {
            var referencias = documento.SelectNodes("//*[local-name()='gPagAntecipado']/*[local-name()='refDFe']");
            foreach (XmlElement referencia in referencias)
            {
                var compativel = documento.CreateElement("refNFe", referencia.NamespaceURI);
                compativel.InnerText = referencia.InnerText;
                referencia.ParentNode.ReplaceChild(compativel, referencia);
            }

            RenomearElementos(documento, "//*[local-name()='IS']/*[local-name()='adRemIS']", "pISEspec");
            RemoverNaoPositivos(documento, "//*[local-name()='gIBSCBS']/*[local-name()='vIBS']");
        }

        private static void RemoverNaoPositivos(XmlDocument documento, string xpath)
        {
            var elementos = documento.SelectNodes(xpath);
            foreach (XmlElement elemento in elementos)
            {
                if (decimal.TryParse(elemento.InnerText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var valor) && valor < 0)
                {
                    elemento.ParentNode.RemoveChild(elemento);
                }
            }
        }

        private static void RenomearElementos(XmlDocument documento, string xpath, string nome)
        {
            var elementos = documento.SelectNodes(xpath);
            foreach (XmlElement elemento in elementos)
            {
                var novo = documento.CreateElement(nome, elemento.NamespaceURI);
                novo.InnerText = elemento.InnerText;
                elemento.ParentNode.ReplaceChild(novo, elemento);
            }
        }
    }
}
