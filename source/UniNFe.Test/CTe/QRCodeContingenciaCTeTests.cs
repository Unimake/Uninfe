using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Xunit;

namespace UniNFe.Test.CTe
{
    public class QRCodeContingenciaCTeTests
    {
        [Theory]
        [InlineData("CTe", TipoAmbiente.Producao, TipoEmissao.ContingenciaOfflineCTe)]
        [InlineData("CTe", TipoAmbiente.Homologacao, TipoEmissao.ContingenciaOfflineCTe)]
        [InlineData("CTeOS", TipoAmbiente.Producao, TipoEmissao.ContingenciaOfflineCTe)]
        [InlineData("CTeOS", TipoAmbiente.Homologacao, TipoEmissao.ContingenciaOfflineCTe)]
        [InlineData("CTeSimp", TipoAmbiente.Producao, TipoEmissao.ContingenciaOfflineCTe)]
        [InlineData("CTeSimp", TipoAmbiente.Homologacao, TipoEmissao.ContingenciaOfflineCTe)]
        [InlineData("CTe", TipoAmbiente.Producao, TipoEmissao.ContingenciaEPEC)]
        [InlineData("CTe", TipoAmbiente.Homologacao, TipoEmissao.ContingenciaEPEC)]
        [InlineData("CTeSimp", TipoAmbiente.Producao, TipoEmissao.ContingenciaEPEC)]
        [InlineData("CTeSimp", TipoAmbiente.Homologacao, TipoEmissao.ContingenciaEPEC)]
        public void PacoteGeraEValidaQRCodeDeContingenciaAssinado(string documento, TipoAmbiente ambiente, TipoEmissao emissao)
        {
            var xml = CriarXml(documento, ambiente, emissao);
            using (var rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                var pedido = new CertificateRequest("CN=CTe QRCode TESTE", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using (var certificado = pedido.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1)))
                {
                    var resultado = new ValidarEstruturaXML().ValidarServico(xml, new Configuracao
                    {
                        TipoDFe = TipoDFe.CTe,
                        CodigoUF = (int)UFBrasil.RO,
                        TipoAmbiente = ambiente,
                        CertificadoDigital = certificado
                    });
                    Assert.True(resultado.Validado, resultado.MensagemRetorno);
                    var chave = ((XmlElement)xml.GetElementsByTagName("infCte")[0]).GetAttribute("Id").Substring(3);
                    var qrCode = xml.GetElementsByTagName("qrCodCTe")[0].InnerText;
                    var parametros = qrCode.Split('?')[1].Split('&');
                    Assert.Equal(3, parametros.Length);
                    Assert.Equal("chCTe=" + chave, parametros[0]);
                    Assert.Equal("tpAmb=" + (int)ambiente, parametros[1]);
                    Assert.StartsWith("sign=", parametros[2]);
                    Assert.Equal(((int)emissao).ToString(), chave.Substring(34, 1));
                    var assinatura = Convert.FromBase64String(parametros[2].Substring(5));
                    using (var chavePublica = certificado.GetRSAPublicKey())
                    {
                        Assert.True(chavePublica.VerifyData(Encoding.UTF8.GetBytes(chave), assinatura, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));
                    }
                }
            }
        }

        private static XmlDocument CriarXml(string documento, TipoAmbiente ambiente, TipoEmissao emissao)
        {
            var texto = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "CTe", "Resources", documento + "_AtualizacaoSchemas.xml"));
            switch (documento)
            {
                case "CTe":
                    var cte = XMLUtility.Deserializar<Unimake.Business.DFe.Xml.CTe.CTe>(texto);
                    cte.InfCTe.Ide.TpEmis = emissao;
                    cte.InfCTe.Ide.TpAmb = ambiente;
                    cte.Signature = null;
                    return cte.GerarXML();
                case "CTeOS":
                    var cteOS = XMLUtility.Deserializar<Unimake.Business.DFe.Xml.CTeOS.CTeOS>(texto);
                    cteOS.InfCTe.Ide.TpEmis = emissao;
                    cteOS.InfCTe.Ide.TpAmb = ambiente;
                    cteOS.Signature = null;
                    return cteOS.GerarXML();
                case "CTeSimp":
                    var cteSimp = XMLUtility.Deserializar<Unimake.Business.DFe.Xml.CTeSimp.CTeSimp>(texto);
                    cteSimp.InfCTe.Ide.TpEmis = emissao;
                    cteSimp.InfCTe.Ide.TpAmb = ambiente;
                    cteSimp.Signature = null;
                    return cteSimp.GerarXML();
                default:
                    throw new ArgumentException("Documento inesperado.", nameof(documento));
            }
        }
    }
}
