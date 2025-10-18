using NFe.Components;

namespace NFSe.Components
{
    public class SchemaXMLNFSe_AGILI
    {
        public static void CriarListaIDXML()
        {
            #region Gerar NFSe

            SchemaXML.InfSchemas.Add("NFSE-AGILI-GerarNfseEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\AGILI\\nfse_v_1.00.xsd",
                Descricao = "XML de Gerar NFSe",
                TagAssinatura = "ChaveDigital",
                TagAtributoId = "ChaveDigital",
                TargetNameSpace = "http://www.agili.com.br/nfse_v_1.00.xsd"
            });

            #endregion Gerar NFSe

            #region Cancelar NFSe

            SchemaXML.InfSchemas.Add("NFSE-AGILI-CancelarNfseEnvio", new InfSchema()
            {
                Tag = "CancelarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\AGILI\\nfse_v_1.00.xsd",
                Descricao = "XML de Cancelar NFSe",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://www.agili.com.br/nfse_v_1.00.xsd"
            });

            #endregion Cancelar NFSe

            #region Enviar Lote RPS

            SchemaXML.InfSchemas.Add("NFSE-AGILI-EnviarLoteRpsEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\AGILI\\nfse_v_1.00.xsd",
                Descricao = "XML de enviar Lote RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://www.agili.com.br/nfse_v_1.00.xsd"
            });

            #endregion Enviar Lote RPS

            #region Consultar Lote RPS

            SchemaXML.InfSchemas.Add("NFSE-AGILI-ConsultarLoteRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\AGILI\\nfse_v_1.00.xsd",
                Descricao = "XML de consultar Lote RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://www.agili.com.br/nfse_v_1.00.xsd"
            });

            #endregion Consultar Lote RPS

            #region Consultar NFSe por RPS

            SchemaXML.InfSchemas.Add("NFSE-AGILI-ConsultarNfsePorRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarNfsePorRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\AGILI\\nfse_v_1.00.xsd",
                Descricao = "XML de consultar NFSe por RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://www.agili.com.br/nfse_v_1.00.xsd"
            });

            #endregion Consultar NFSe por RPS

            #region Consultar NFSe por Faixa

            SchemaXML.InfSchemas.Add("NFSE-AGILI-ConsultarNfseFaixaEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseFaixaEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\AGILI\\nfse_v_1.00.xsd",
                Descricao = "XML de consultar NFSe por Faixa",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://www.agili.com.br/nfse_v_1.00.xsd"
            });

            #endregion Consultar NFSe por Faixa

            #region Consultar requerimento cancelamento

            SchemaXML.InfSchemas.Add("NFSE-AGILI-ConsultarRequerimentoCancelamentoEnvio", new InfSchema()
            {
                Tag = "ConsultarRequerimentoCancelamentoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\AGILI\\nfse_v_1.00.xsd",
                Descricao = "XML de consultar requerimento do cancelamento",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://www.agili.com.br/nfse_v_1.00.xsd"
            });

            #endregion Consultar requerimento cancelamento
        }
    }
}
