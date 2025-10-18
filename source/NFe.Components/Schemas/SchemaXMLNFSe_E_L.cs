using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NFe.Components;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NFSe.Components
{
    public class SchemaXMLNFSe_E_L
    {
        public static void CriarListaIDXML()
        {
            #region E&L 2.04

            #region Enviar Lote RPS

            SchemaXML.InfSchemas.Add("NFSE-EL-EnviarLoteRpsEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\EL\\nfse_v2-04.xsd",
                Descricao = "Recepcionar Lote RPS",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "EnviarLoteRpsEnvio",
                TagLoteAtributoId = "LoteRps",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion

            #region Enviar Lote RPS Sincrono

            SchemaXML.InfSchemas.Add("NFSE-EL-EnviarLoteRpsSincronoEnvio", new InfSchema()
            {
                Tag = "EnviarLoteRpsSincronoEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\EL\\nfse_v2-04.xsd",
                Descricao = "Recepcionar Lote RPS Sincrono",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TagLoteAssinatura = "EnviarLoteRpsSincronoEnvio",
                TagLoteAtributoId = "LoteRps",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion

            #region Cancelar NFSe

            SchemaXML.InfSchemas.Add("NFSE-EL-CancelarNfseEnvio", new InfSchema()
            {
                Tag = "CancelarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\EL\\nfse_v2-04.xsd",
                Descricao = "Cancelar nota fiscal de serviço",
                TagAssinatura = "Pedido",
                TagAtributoId = "InfPedidoCancelamento",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion

            #region Gerar NFSe

            SchemaXML.InfSchemas.Add("NFSE-EL-GerarNfseEnvio", new InfSchema()
            {
                Tag = "GerarNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\EL\\nfse_v2-04.xsd",
                Descricao = "Gerar Nota Fiscal de Serviço",
                TagAssinatura = "Rps",
                TagAtributoId = "InfDeclaracaoPrestacaoServico",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion

            #region Substituit NFSe

            SchemaXML.InfSchemas.Add("NFSE-EL-SubstituirNfseEnvio", new InfSchema()
            {
                Tag = "SubstituirNfseEnvio",
                ID = SchemaXML.InfSchemas.Count + 1,
                ArquivoXSD = "NFSe\\EL\\nfse_v2-04.xsd",
                Descricao = "Substituir nota fiscal de Serviço",
                TagAssinatura = "Pedido",
                TagAtributoId = "InfPedidoCancelamento",
                TagLoteAssinatura = "Rps",
                TagLoteAtributoId = "InfDeclaracaoPrestacaoServico",
                TagAssinatura0 = "SubstituirNfseEnvio",
                TagAtributoId0 = "SubstituicaoNfse",
                TargetNameSpace = "http://www.abrasf.org.br/nfse.xsd"
            });

            #endregion

            #endregion 
        }
    }
}
