using NFe.Components;

namespace NFSe.Components
{
    public class SchemaXMLNFSe_GISSONLINE
    {
        public static void CriarListaIDXML()
        {
            #region RecepcionarLoteRps

            SchemaXML.InfSchemas.Add("NFSE-GISSONLINE-ns4:EnviarLoteRpsEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GISSONLINE\\enviar-lote-rps-envio-v2_04.xsd",
                Descricao = "XML de Recepcionar Lote de NFSe",
                TagAssinatura = "ns2:Rps",
                TagAtributoId = "ns2:InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "ns4:EnviarLoteRpsEnvio",
                TagLoteAtributoId = "ns4:LoteRps",
                TargetNameSpace = "http://www.giss.com.br/enviar-lote-rps-envio-v2_04.xsd"
            });

            #endregion RecepcionarLoteRps

            #region CancelarNfse

            SchemaXML.InfSchemas.Add("NFSE-GISSONLINE-ns4:CancelarNfseEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GISSONLINE\\cancelar-nfse-envio-v2_04.xsd",
                Descricao = "XML de Cancelamento de NFSe",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "ns4:Pedido",
                TagLoteAtributoId = "ns2:InfPedidoCancelamento",
                TargetNameSpace = "http://www.giss.com.br/cancelar-nfse-envio-v2_04.xsd"
            });

            #endregion CancelarNfse

            #region ConsultarLoteRps

            SchemaXML.InfSchemas.Add("NFSE-GISSONLINE-ns4:ConsultarLoteRpsEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GISSONLINE\\consultar-lote-rps-envio-v2_04.xsd",
                Descricao = "XML de Consulta lote RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "ns4:ConsultarLoteRpsEnvio",
                TagLoteAtributoId = "ns4:Protocolo",
                TargetNameSpace = "http://www.giss.com.br/consultar-lote-rps-envio-v2_04.xsd"
            });

            #endregion ConsultarLoteRps

            #region ConsultarNfsePorRps

            SchemaXML.InfSchemas.Add("NFSE-GISSONLINE-ns4:ConsultarNfseRpsEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GISSONLINE\\consultar-nfse-rps-envio-v2_04.xsd",
                Descricao = "XML de Consultar NFSe por RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "ns4:ConsultarNfseRpsEnvio",
                TagLoteAtributoId = "ns4:Prestador",
                TargetNameSpace = "http://www.giss.com.br/consultar-nfse-rps-envio-v2_04.xsd"
            });

            #endregion ConsultarNfsePorRps

            #region ConsultarNfseServicoPrestado

            SchemaXML.InfSchemas.Add("NFSE-GISSONLINE-ns4:ConsultarNfseServicoPrestadoEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GISSONLINE\\consultar-nfse-servico-prestado-envio-v2_04.xsd",
                Descricao = "XML de Consultar NFSe por Serviço Prestado",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "ns4:ConsultarNfseServicoPrestadoEnvio",
                TagLoteAtributoId = "ns4:Prestador",
                TargetNameSpace = "http://www.giss.com.br/consultar-nfse-servico-prestado-envio-v2_04.xsd"
            });

            #endregion ConsultarNfseServicoPrestado

            #region ConsultarNfseServicoTomado

            SchemaXML.InfSchemas.Add("NFSE-GISSONLINE-ns4:ConsultarNfseServicoTomadoEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\GISSONLINE\\consultar-nfse-servico-tomado-envio-v2_04.xsd",
                Descricao = "XML de Consultar NFSe por Serviço Tomado",
                TagAssinatura = "",
                TagAtributoId = "",
                TagLoteAssinatura = "ns4:ConsultarNfseServicoTomadoEnvio",
                TagLoteAtributoId = "ns4:Consulente",
                TargetNameSpace = "http://www.giss.com.br/consultar-nfse-servico-tomado-envio-v2_04.xsd"
            });

            #endregion ConsultarNfseServicoTomado
        }
    }
}
