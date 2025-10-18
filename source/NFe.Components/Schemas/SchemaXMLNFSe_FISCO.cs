using NFe.Components;

namespace NFSe.Components
{
    internal class SchemaXMLNFSe_FISCO
    {
        public static void CriarListaIDXML()
        {
            #region Recepcionar Lote RPS
            SchemaXML.InfSchemas.Add("NFSE-FISCO-EnviarLoteRpsEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "EnviarLoteRpsEnvio",
                TagLoteAtributoId = "LoteRps",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Recepcionar Lote RPS Síncrono
            SchemaXML.InfSchemas.Add("NFSE-FISCO-EnviarLoteRpsSincronoEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsSincronoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "EnviarLoteRpsSincronoEnvio",
                TagAtributoId = "LoteRps",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Consultar Lote Rps
            SchemaXML.InfSchemas.Add("NFSE-FISCO-ConsultarLoteRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "ConsultarLoteRpsEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Consultar Nfse por Rps
            SchemaXML.InfSchemas.Add("NFSE-FISCO-ConsultarNfseRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "ConsultarNfseRpsEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Consultar Nfse por Faixa
            SchemaXML.InfSchemas.Add("NFSE-FISCO-ConsultarNfseFaixaEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseFaixaEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "ConsultarNfseFaixaEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Cancelar Nfse
            SchemaXML.InfSchemas.Add("NFSE-FISCO-CancelarNfseEnvio", new InfSchema()
            {
                Tag = "CancelarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "Pedido",
                TagAtributoId = "InfPedidoCancelamento",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Gerar Nfse
            SchemaXML.InfSchemas.Add("NFSE-FISCO-GerarNfseEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Consultar Nfse Servico Prestado
            SchemaXML.InfSchemas.Add("NFSE-FISCO-ConsultarNfseServicoPrestadoEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseServicoPrestadoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "ConsultarNfseServicoPrestadoEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Consultar Nfse Servico Tomado
            SchemaXML.InfSchemas.Add("NFSE-FISCO-ConsultarNfseServicoTomadoEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseServicoTomadoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura = "ConsultarNfseServicoTomadoEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

            #region Substituir Nfse
            SchemaXML.InfSchemas.Add("NFSE-FISCO-SubstituirNfseEnvio", new InfSchema()
            {
                Tag = "SubstituirNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                Descricao = "XML de Recepção de Lote RPS",
                TagAssinatura0 = "Pedido",
                TagAtributoId0 = "InfPedidoCancelamento",
                TagLoteAssinatura = "Rps",
                TagLoteAtributoId = "InfDeclaracaoPrestacaoServico",
                TagAssinatura = "SubstituirNfseEnvio",
                TagAtributoId = "SubstituicaoNfse",
                TargetNameSpace = "https://www.fisco.net.br/wsnfseabrasf/ServicosNFSEAbrasf.asmx"
            });
            #endregion

        }
    }
}
