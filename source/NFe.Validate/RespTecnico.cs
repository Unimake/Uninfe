using NFe.Components;
using System.Xml;

namespace NFe.Validate
{
    public class RespTecnico
    {
        public RespTecnico(string respTecCNPJ, string respTecXContato, string respTecEmail, string respTecTelefone, string respTecIdCSRT, string respTecCSRT)
        {
            RespTecCNPJ = respTecCNPJ;
            RespTecXContato = respTecXContato;
            RespTecEmail = respTecEmail;
            RespTecTelefone = respTecTelefone;
            RespTecIdCSRT = respTecIdCSRT;
            RespTecCSRT = respTecCSRT;
        }

        private string RespTecCNPJ { get; set; }
        private string RespTecXContato { get; set; }
        private string RespTecEmail { get; set; }
        private string RespTecTelefone { get; set; }
        private string RespTecIdCSRT { get; set; }
        private string RespTecCSRT { get; set; }

        public bool AdicionarResponsavelTecnico(XmlDocument conteudoXML)
        {
            XmlNode infDFe = null;
            var isNFe = false;
            var isCTe = false;
            var isMDFe = false;

            switch (conteudoXML.DocumentElement.Name)
            {
                case "NFe":
                    infDFe = conteudoXML.GetElementsByTagName("infNFe")[0];
                    isNFe = true;
                    break;

                case "CTe":
                case "CTeSimp":
                    infDFe = conteudoXML.GetElementsByTagName("infCte")[0];
                    isCTe = true;
                    break;

                case "MDFe":
                    infDFe = conteudoXML.GetElementsByTagName("infMDFe")[0];
                    isMDFe = true;
                    break;

                default:
                    break;
            }

            if (isNFe || isCTe || isMDFe)
            {
                var infRespTec = conteudoXML.GetElementsByTagName("infRespTec");

                if (infRespTec.Count == 0) //Não encontrou o grupo de tag <infRespTec>
                {
                    if (!string.IsNullOrEmpty(RespTecCNPJ) ||
                        !string.IsNullOrEmpty(RespTecXContato) ||
                        !string.IsNullOrEmpty(RespTecEmail) ||
                        !string.IsNullOrEmpty(RespTecTelefone) ||
                        !string.IsNullOrEmpty(RespTecIdCSRT) ||
                        !string.IsNullOrEmpty(RespTecCSRT))
                    {
                        var chaveDFe = infDFe.Attributes.GetNamedItem("Id").InnerText.Substring(3, 44);

                        var infRespTecnico = conteudoXML.CreateElement("infRespTec", infDFe.NamespaceURI);
                        XmlNode cnpj = conteudoXML.CreateElement("CNPJ", infDFe.NamespaceURI);
                        XmlNode contato = conteudoXML.CreateElement("xContato", infDFe.NamespaceURI);
                        XmlNode email = conteudoXML.CreateElement("email", infDFe.NamespaceURI);
                        XmlNode fone = conteudoXML.CreateElement("fone", infDFe.NamespaceURI);
                        XmlNode idCSRT = conteudoXML.CreateElement("idCSRT", infDFe.NamespaceURI);
                        XmlNode csrt = conteudoXML.CreateElement("hashCSRT", infDFe.NamespaceURI);

                        cnpj.InnerText = RespTecCNPJ;
                        contato.InnerText = RespTecXContato;
                        email.InnerText = RespTecEmail;
                        fone.InnerText = RespTecTelefone;

                        infRespTecnico.AppendChild(cnpj);
                        infRespTecnico.AppendChild(contato);
                        infRespTecnico.AppendChild(email);
                        infRespTecnico.AppendChild(fone);

                        if (!string.IsNullOrEmpty(RespTecIdCSRT) &&
                            !string.IsNullOrEmpty(RespTecCSRT) && isNFe)
                        {
                            idCSRT.InnerText = RespTecIdCSRT;
                            csrt.InnerText = GerarHashCSRT(RespTecCSRT, chaveDFe);

                            infRespTecnico.AppendChild(idCSRT);
                            infRespTecnico.AppendChild(csrt);
                        }

                        if (isNFe)
                        {
                            //Se existir a tag agropecuário, o responsável técnico tem que ser inserido antes deste grupo
                            XmlNode agropecuarioNode = conteudoXML.GetElementsByTagName("agropecuario")?.Item(0);

                            if (agropecuarioNode != null)
                            {
                                infDFe.InsertBefore(infRespTecnico, agropecuarioNode);
                            }
                            else
                            {
                                infDFe.AppendChild(infRespTecnico);
                            }
                        }
                        else
                        {
                            infDFe.AppendChild(infRespTecnico);
                        }

                        return true;
                    }
                }
                else if (isNFe)
                {
                    //Vou verificar se o hasCSRT já está convertido para Sha1Hash + 
                    var infRespTecElement = (XmlElement)infRespTec[0];
                    if (infRespTecElement.GetElementsByTagName("hashCSRT").Count >= 1)
                    {
                        var hashCSRT = conteudoXML.GetElementsByTagName("hashCSRT")[0].InnerText;
                        if (!Unimake.Business.DFe.Utility.Converter.IsSHA1Base64(hashCSRT))
                        {
                            conteudoXML.GetElementsByTagName("hashCSRT")[0].InnerText = GerarHashCSRT(hashCSRT, "");
                        }
                    }
                }
            }

            return false;
        }

        private string GerarHashCSRT(string csrt, string chaveDFe)
        {
            var result = Criptografia.GetSHA1HashData(csrt + chaveDFe);
            result = Functions.ToBase64Hex(result);

            return result;
        }

    }
}
