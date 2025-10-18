using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NFe.Components;

namespace NFSe.Components
{
    public class SchemaXMLNFSe_DSF
    {
        public static void CriarListaIDXML()
        {
            #region 1.00
            #region XML de Cancelamento de NFS-e
            SchemaXML.InfSchemas.Add("NFSE-DSF-ns1:ReqCancelamentoNFSe", new InfSchema()
            {
                Tag = "ns1:ReqCancelamentoNFSe",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\ReqCancelamentoNFSe.xsd",
                Descricao = "XML de cancelamento de NFSe",
                TagAssinatura = "ns1:ReqCancelamentoNFSe",
                TagAtributoId = "Cabecalho",
                TargetNameSpace = "http://localhost:8080/WsNFe2/lote"
            });
            #endregion

            #region XML de Consulta de NFSe por Rps
            SchemaXML.InfSchemas.Add("NFSE-DSF-ns1:ReqConsultaNFSeRPS", new InfSchema()
            {
                Tag = "ns1:ReqConsultaNFSeRPS",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\ReqConsultaNFSeRPS.xsd",
                Descricao = "XML de Consulta de NFSe por Rps",
                TagAssinatura = "ns1:ReqConsultaNFSeRPS",
                TagAtributoId = "Lote",
                TargetNameSpace = "http://localhost:8080/WsNFe2/lote"
            });
            #endregion

            #region XML de Consulta de Lote RPS
            SchemaXML.InfSchemas.Add("NFSE-DSF-ns1:ReqConsultaLote", new InfSchema()
            {
                Tag = "ns1:ReqConsultaLote",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\ReqConsultaLote.xsd",
                Descricao = "XML de Consulta de Lote RPS",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://localhost:8080/WsNFe2/lote"
            });
            #endregion

            #region XML de Consulta de NFSe por Data
            SchemaXML.InfSchemas.Add("NFSE-DSF-ns1:ReqConsultaNotas", new InfSchema()
            {
                Tag = "ns1:ReqConsultaNotas",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\ReqConsultaNotas.xsd",
                Descricao = "XML de Consulta de NFSe por Data",
                TagAssinatura = "ns1:ReqConsultaNotas",
                TagAtributoId = "Cabecalho",
                TargetNameSpace = "http://localhost:8080/WsNFe2/lote"
            });
            #endregion

            #region XML de Consulta Situação do Lote RPS
            SchemaXML.InfSchemas.Add("NFSE-DSF-ns1:ConsultaSeqRps", new InfSchema()
            {
                Tag = "ns1:ConsultaSeqRps",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\ConsultaSeqRps.xsd",
                Descricao = "XML de Consulta da Situação do Lote RPS (Retorna número do Ultimo RPS)",
                TagAssinatura = "",
                TagAtributoId = "",
                TargetNameSpace = "http://localhost:8080/WsNFe2/lote"
            });
            #endregion

            #region XML de lote RPS
            SchemaXML.InfSchemas.Add("NFSE-DSF-ns1:ReqEnvioLoteRPS", new InfSchema()
            {
                Tag = "ns1:ReqEnvioLoteRPS",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\ReqEnvioLoteRPS.xsd",
                Descricao = "XML de Lote RPS",
                TagAssinatura = "ns1:ReqEnvioLoteRPS",
                TagAtributoId = "Lote",
                TargetNameSpace = "http://localhost:8080/WsNFe2/lote"
            });
            #endregion

            #endregion 1.00

            #region 2.04

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.04-CancelarNfseEnvio", new InfSchema()
            {
                Tag = "CancelarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.04\\nfse_v2-04.xsd",
                Descricao = "XML de cancelamento",
                TagAssinatura = "Pedido",
                TagAtributoId = "InfPedidoCancelamento",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.04-EnviarLoteRpsEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.04\\nfse_v2-04.xsd",
                Descricao = "XML de lote RPS",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "EnviarLoteRpsEnvio",
                TagLoteAtributoId = "LoteRps",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.04-EnviarLoteRpsSincronoEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsSincronoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.04\\nfse_v2-04.xsd",
                Descricao = "XML de lote RPS sincrono",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "EnviarLoteRpsSincronoEnvio",
                TagLoteAtributoId = "LoteRps",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.04-GerarNfseEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.04\\nfse_v2-04.xsd",
                Descricao = "XML de geração de NFSe",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.04-SubstituirNfseEnvio", new InfSchema()
            {
                Tag = "SubstituirNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.04\\nfse_v2-04.xsd",
                Descricao = "XML de substituição de NFSe",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "SubstituirNfseEnvio",
                TagLoteAtributoId = "SubstituicaoNfse",
                TagAssinatura0 = "Pedido",
                TagAtributoId0 = "InfPedidoCancelamento",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion 2.04

            #region 2.03

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-CancelarNfseEnvio", new InfSchema()
            {
                Tag = "CancelarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de cancelamento",
                TagAssinatura = "Pedido",
                TagAtributoId = "InfPedidoCancelamento",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-EnviarLoteRpsEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de lote RPS",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "EnviarLoteRpsEnvio",
                TagLoteAtributoId = "LoteRps",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-EnviarLoteRpsSincronoEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsSincronoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de lote RPS sincrono",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "EnviarLoteRpsSincronoEnvio",
                TagLoteAtributoId = "LoteRps",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-GerarNfseEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de geração de NFSe",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-SubstituirNfseEnvio", new InfSchema()
            {
                Tag = "SubstituirNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de substituição de NFSe",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "SubstituirNfseEnvio",
                TagLoteAtributoId = "SubstituicaoNfse",
                TagAssinatura0 = "Pedido",
                TagAtributoId0 = "InfPedidoCancelamento",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-ConsultarLoteRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de consulta lote RPS",
                TagAssinatura = "ConsultarLoteRpsEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-ConsultarNfseRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de consulta NFSe por RPS",
                TagAssinatura = "ConsultarNfseRpsEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-ConsultarNfseServicoPrestadoEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseServicoPrestadoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de consulta NFSe de serviço prestado",
                TagAssinatura = "ConsultarNfseServicoPrestadoEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-ConsultarNfseServicoTomadoEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseServicoTomadoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de consulta NFSe de serviço tomado",
                TagAssinatura = "ConsultarNfseServicoTomadoEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = ""
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-2.03-ConsultarNfseFaixaEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseFaixaEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\2.03\\nfse_v2-03_ima.xsd",
                Descricao = "XML de consulta de NFSe por faixa de numeração",
                TagAssinatura = "ConsultarNfseFaixaEnvio",
                TagAtributoId = "Prestador",
                TagLoteAssinatura = "",
                TagLoteAtributoId = "",
                TargetNameSpace = ""
            });

            #endregion 2.03

            #region 3.00
            #region São José dos Campos-SP

            SchemaXML.InfSchemas.Add("NFSE-DSF-3549904-ConsultarNfseEnvio", new InfSchema()
            {
                Tag = "ConsultarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\SJCSP\\nfse.xsd",
                Descricao = "XML de Consulta de NFSe por Data",
                TagAssinatura = "ConsultarNfseEnvio",
                TagAtributoId = "Prestador",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-3549904-ConsultarNfseRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarNfsePorRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\SJCSP\\nfse.xsd",
                Descricao = "XML de Consulta de NFSe por Rps",
                TagAssinatura = "ConsultarNfseRpsEnvio",
                TagAtributoId = "Prestador",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-3549904-ConsultarLoteRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\SJCSP\\nfse.xsd",
                Descricao = "XML de Consulta de Lote RPS",
                TagAssinatura = "ConsultarLoteRpsEnvio",
                TagAtributoId = "Protocolo",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-3549904-CancelarNfseEnvio", new InfSchema()
            {
                Tag = "CancelarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\SJCSP\\nfse.xsd",
                Descricao = "XML de Cancelamento da NFS-e",
                TagAssinatura = "CancelarNfseEnvio",
                TagAtributoId = "Pedido",
                TagLoteAssinatura = "Pedido",
                TagLoteAtributoId = "InfPedidoCancelamento",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-3549904-ConsultarSituacaoLoteRpsEnvio", new InfSchema()
            {
                Tag = "ConsultarSituacaoLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\SJCSP\\nfse.xsd",
                Descricao = "XML de Consulta da Situacao do Lote RPS",
                TagAssinatura = "ConsultarSituacaoLoteRpsEnvio",
                TagAtributoId = "Protocolo",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            SchemaXML.InfSchemas.Add("NFSE-DSF-3549904-EnviarLoteRpsEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\DSF\\SJCSP\\nfse.xsd",
                Descricao = "XML de Lote RPS",
                TagLoteAssinatura = "EnviarLoteRpsEnvio",
                TagLoteAtributoId = "LoteRps",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion

            #endregion 3.00

        }
    }
}
