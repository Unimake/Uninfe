using NFe.Components;

namespace NFSe.Components
{
    public class SchemaXMLNFSe_GIAP
    {
        public static void CriarListaIDXML()
        {
            #region XML de lote RPS

            SchemaXML.InfSchemas.Add("NFSE-GIAP-notaFiscal", new InfSchema()
            {
                Tag = "notaFiscal",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "",
                Descricao = "XML de Gerar Nota Fiscal de Serviço",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-GIAP-cancelaNota", new InfSchema()
            {
                Tag = "cancelaNota",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "",
                Descricao = "XML de Cancelamento",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-GIAP-consulta", new InfSchema()
            {
                Tag = "consulta",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "",
                Descricao = "XML de Consulta",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = ""
            });

            #endregion XML de lote RPS
        }
    }
}