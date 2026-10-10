using System;
using System.Globalization;
using System.IO;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Utility;
using Xunit;
using ModeloNFCom = Unimake.Business.DFe.Xml.NFCom.NFCom;

namespace UniNFe.Test.NFCom
{
    public class SchemasNFComNuGetTests
    {
        private const string NamespaceNFCom = "http://www.portalfiscal.inf.br/nfcom";

        private static ModeloNFCom LerModelo()
        {
            return XMLUtility.Deserializar<ModeloNFCom>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "NFCom", "Resources", "NFCom_AtualizacaoSchemas.xml")));
        }

        [Fact]
        public void PacotePreservaNovosCamposEValidaSchemaEmbutido()
        {
            var modelo = LerModelo();
            Assert.Equal(4106902, modelo.InfNFCom.Assinante.CMunPrinc);
            Assert.Equal(80.12345678m, modelo.InfNFCom.Det[0].Prod.VItemLiq);
            Assert.Equal(80.12345678m, modelo.InfNFCom.Det[0].Prod.VProdLiq);
            Assert.Equal(80.12d, modelo.InfNFCom.Total.VProdLiq);
            Assert.Equal(18.25d, modelo.InfNFCom.Det[0].Imposto.GICMSPrevistoPagtoAntecip.VICMSPrevisto);
            Assert.Equal(100.12d, modelo.InfNFCom.Det[0].GProcRef.GIBSCBS.VBC);
            Assert.Equal(2, modelo.InfNFCom.Assinante.TerminaisAdicionais.Count);
            var xml = modelo.GerarXML();
            Assert.Equal(xml.InnerText, XMLUtility.Deserializar<ModeloNFCom>(xml).GerarXML().InnerText);
            var validador = new ValidarSchema();
            validador.Validar(xml, "NFCom.nfcom_v1.00.xsd", NamespaceNFCom);
            Assert.True(validador.Success, validador.ErrorMessage);
        }

        [Theory]
        [InlineData("PR123456", true)]
        [InlineData("PR12345678", true)]
        [InlineData("PR1234567", false)]
        public void PacoteAplicaRestricaoDeBeneficioFiscal(string codigo, bool valido)
        {
            var modelo = LerModelo();
            modelo.InfNFCom.Det[0].Imposto = new Unimake.Business.DFe.Xml.NFCom.Imposto
            {
                ICMS40 = new Unimake.Business.DFe.Xml.NFCom.ICMS40 { CST = "40", VICMSDeson = 1, CBenef = codigo }
            };
            var validador = new ValidarSchema();
            validador.Validar(modelo.GerarXML(), "NFCom.nfcom_v1.00.xsd", NamespaceNFCom);
            Assert.True(validador.Success == valido, validador.ErrorMessage);
            if (!valido)
            {
                Assert.Contains("cBenef", validador.ErrorMessage);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("0.00")]
        public void PacoteDistingueAusenciaEZeroNosValoresLiquidos(string valor)
        {
            var modelo = LerModelo();
            decimal? item = valor == null ? (decimal?)null : decimal.Parse(valor, CultureInfo.InvariantCulture);
            modelo.InfNFCom.Det[0].Prod.VItemLiq = item;
            modelo.InfNFCom.Det[0].Prod.VProdLiq = item;
            modelo.InfNFCom.Total.VProdLiq = valor == null ? (double?)null : double.Parse(valor, CultureInfo.InvariantCulture);
            var xml = modelo.GerarXML();
            Assert.Equal(valor == null ? 0 : 1, xml.GetElementsByTagName("vItemLiq", NamespaceNFCom).Count);
            Assert.Equal(valor == null ? 0 : 2, xml.GetElementsByTagName("vProdLiq", NamespaceNFCom).Count);
            Assert.Equal(item, XMLUtility.Deserializar<ModeloNFCom>(xml).InfNFCom.Det[0].Prod.VItemLiq);
            var validador = new ValidarSchema();
            validador.Validar(xml, "NFCom.nfcom_v1.00.xsd", NamespaceNFCom);
            Assert.True(validador.Success, validador.ErrorMessage);
        }
    }
}