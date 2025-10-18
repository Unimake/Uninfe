using NFe.Components;

namespace NFSe.Components
{
    public class SchemaXMLNFSe_GEISWEB
    {
        public static void CriarListaIDXML()
        {
            #region XML de lote RPS

            SchemaXML.InfSchemas.Add("NFSE-GEISWEB-EnviaLoteRPS", new InfSchema()
            {
                Tag = "EnviaLoteRPS",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GEISWEB\\envio_lote_rps.xsd",
                Descricao = "XML de Lote RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = ""
            });

            #endregion XML de lote RPS

            #region Consulta NFSe por Rps

            SchemaXML.InfSchemas.Add("NFSE-GEISWEB-ConsultaNota", new InfSchema()
            {
                Tag = "ConsultaNota",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GEISWEB\\consulta_nfse.xsd",
                Descricao = "XML de Consulta da NFSe por RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = ""
            });

            #endregion Consulta NFSe por Rps

            #region XML de Cancelamento de NFS-e

            SchemaXML.InfSchemas.Add("NFSE-GEISWEB-CancelaNota", new InfSchema()
            {
                Tag = "CancelaNota",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GEISWEB\\cancela_nfse.xsd",
                Descricao = "XML de Cancelamento da NFS-e",
                TagAssinatura = "CancelaNfse",
                TagAtributoId = "CnpjCpf",
                TargetNameSpace = ""
            });

            #endregion XML de Cancelamento de NFS-e

        }
    }
}