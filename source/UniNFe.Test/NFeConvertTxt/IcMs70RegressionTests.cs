using System;
using System.IO;
using System.Reflection;
using System.Xml;
using NFe.ConvertTxt;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class IcMs70RegressionTests
    {
        [Fact]
        public void N09SemDesoneracaoDeveOmitirVicmsDesonNoLegadoENaDll()
        {
            var origem = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "Regressions",
                "001045277-icms70-sem-desoneracao-nfe.txt");
            var fixture = new NFeConvertTxtFixture();
            var pastaLegado = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pastaLegado);

            try
            {
                using (var leituraLegada = fixture.Converter(origem))
                {
                    Assert.True(leituraLegada.Sucesso, leituraLegada.MensagemErro);
                    var geradorLegado = new NFeW { cMensagemErro = string.Empty };
                    typeof(NFeW).GetMethod("GerarXmlLegado", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(geradorLegado, new object[] { leituraLegada.Nota, pastaLegado, origem });
                    var xmlLegado = new XmlDocument();
                    xmlLegado.Load(geradorLegado.cFileName);

                    var conversaoNova = new Unimake.Business.DFe.Xml.NFe.NFeTxtConverter().Converter(origem);
                    Assert.True(conversaoNova.Sucesso, conversaoNova.MensagemErro);
                    var xmlNovo = new XmlDocument();
                    xmlNovo.LoadXml(Assert.Single(conversaoNova.Documentos).Xml);

                    foreach (var xml in new[] { xmlLegado, xmlNovo })
                    {
                        var icms70 = xml.SelectSingleNode("//*[local-name()='ICMS70']");
                        Assert.NotNull(icms70);
                        Assert.Equal("70", icms70.SelectSingleNode("*[local-name()='CST']")?.InnerText);
                        Assert.Equal("46.85", icms70.SelectSingleNode("*[local-name()='vICMS']")?.InnerText);
                        Assert.Null(icms70.SelectSingleNode("*[local-name()='vICMSDeson']"));
                        Assert.Null(icms70.SelectSingleNode("*[local-name()='motDesICMS']"));
                    }

                    var diferenca = NFeConvertTxtXmlComparer.Comparar(xmlLegado.OuterXml, xmlNovo.OuterXml);
                    Assert.True(diferenca == null, diferenca);
                }
            }
            finally
            {
                if (Directory.Exists(pastaLegado)) Directory.Delete(pastaLegado, true);
            }
        }
    }
}
