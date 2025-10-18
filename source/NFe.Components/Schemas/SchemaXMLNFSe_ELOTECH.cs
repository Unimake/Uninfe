using NFe.Components;

namespace NFSe.Components
{
    public class SchemaXMLNFSe_ELOTECH
    {
        public static void CriarListaIDXML()
        {
            #region XML de Cancelamento de NFS-e

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-CancelarNfseEnvio", new InfSchema()
            {
                Tag = "CancelarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Cancelamento da NFS-e",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion XML de Cancelamento de NFS-e

            #region XML de Lote RPS

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-EnviarLoteRpsEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Lote RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion XML de Lote RPS

            #region XML de Lote RPS (Síncrono)

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-EnviarLoteRpsSincronoEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsSincronoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Lote RPS - Síncrono",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion XML de Lote RPS (Síncrono)

            #region XML de Substiuição de NFSe

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-SubstituirNfseEnvio", new InfSchema()
            {
                Tag = "SubstituirNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Substiuição de NFSe",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion XML de Substiuição de NFSe

            #region XML de Consulta Situação do Lote RPS

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-ConsultarSituacaoLoteRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarSituacaoLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Consulta da Situacao do Lote RPS",
                TagAssinatura = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion XML de Consulta Situação do Lote RPS

            #region XML de Consulta Lote RPS

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-ConsultarLoteRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarLoteRpsEnvio",                
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Consulta do Lote RPS",
                TagAssinatura = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion XML de Consulta Lote RPS

            #region Consulta NFSe por Rps

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-ConsultarNfseRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Consulta da NFSe por RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion Consulta NFSe por Rps

            #region Consulta NFSe por Serviço Prestado

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-ConsultarNfseServicoPrestadoEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseServicoPrestadoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Consulta da NFSe por Faixa",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion Consulta NFSe por Consulta NFSe por Serviço Prestado

            #region Consulta NFSe por Serviço Tomado

            SchemaXML.InfSchemas.Add("NFSE-ELOTECH-ConsultarNfseServicoTomadoEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseServicoTomadoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\ELOTECH\\nfse_v2_03.xsd",
                Descricao = "XML de Consulta da NFSe Servicos Tomados",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://shad.elotech.com.br/schemas/iss/nfse_v2_03.xsd"
            });

            #endregion Consulta NFSe por Serviço Tomado
        }
    }
}