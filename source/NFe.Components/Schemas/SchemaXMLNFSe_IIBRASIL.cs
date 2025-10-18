using NFe.Components;

namespace NFSe.Components
{
    public  class SchemaXMLNFSe_IIBRASIL
    {
        public static void CriarListaIDXML()
        {
            #region Gerar NFSe

            SchemaXML.InfSchemas.Add("NFSE-IIBRASIL-GerarNfseEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\IIBRASIL\\schema_nfse_v1_IIBR.xsd",
                Descricao = "XML de Gerar NFSe",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion Gerar NFSe

            #region ConsultarNfsePorRps

            SchemaXML.InfSchemas.Add("NFSE-IIBRASIL-ConsultarNfseRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\IIBRASIL\\schema_nfse_v1_IIBR.xsd",
                Descricao = "XML de Consultar NFSe por RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion ConsultarNfsePorRps
        }
    }
}
