using System;
using System.IO;
using System.Xml;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class NovaVersaoRegressionTests
    {
        [Fact]
        public void ConversorDaDllDeveProcessarArquivoQueConversorLegadoRejeita()
        {
            var arquivo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "Regressions", "novaVersao-nfe.txt");
            var fixture = new NFeConvertTxtFixture();

            using (var legado = fixture.Converter(arquivo))
            {
                Assert.False(legado.Sucesso);
                Assert.Contains("primeiro registro da nota deve ser o segmento A", legado.MensagemErro);
            }

            var conversaoNova = new Unimake.Business.DFe.Xml.NFe.NFeTxtConverter().Converter(arquivo);
            Assert.True(conversaoNova.Sucesso, conversaoNova.MensagemErro);

            var xml = new XmlDocument();
            xml.LoadXml(Assert.Single(conversaoNova.Documentos).Xml);
            Assert.Equal("9", xml.SelectSingleNode("//*[local-name()='dest']/*[local-name()='indIEDest']")?.InnerText);
            Assert.Equal(12, xml.SelectNodes("//*[local-name()='infNFe']/*[local-name()='det']").Count);
            Assert.Equal(12, xml.SelectNodes("//*[local-name()='det']/*[local-name()='imposto']/*[local-name()='IBSCBS']").Count);
        }
    }
}
