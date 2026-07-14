using System;
using System.IO;
using System.Reflection;
using NFe.ConvertTxt;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class SecondRegressionTests
    {
        [Fact]
        public void NovoXmlDeveSerIgualAoLegado()
        {
            var arquivo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "Regressions", "0000042301054300027600113072026-NFE.txt");
            var fixture = new NFeConvertTxtFixture();
            using (var resultado = fixture.Converter(arquivo))
            {
                Assert.True(resultado.Sucesso, resultado.MensagemErro);
                var pasta = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(pasta);
                try
                {
                    var legadoGerador = new NFeW { cMensagemErro = string.Empty };
                    typeof(NFeW).GetMethod("GerarXmlLegado", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(legadoGerador, new object[] { resultado.Nota, pasta, arquivo });
                    var legado = File.ReadAllText(legadoGerador.cFileName);
                    var conversaoNova = new Unimake.Business.DFe.Xml.NFe.NFeTxtConverter().Converter(arquivo);
                    Assert.True(conversaoNova.Sucesso, conversaoNova.MensagemErro);
                    var novo = Assert.Single(conversaoNova.Documentos).Xml;
                    var diferenca = NFeConvertTxtXmlComparer.Comparar(legado, novo);
                    Assert.True(diferenca == null, diferenca);
                }
                finally { if (Directory.Exists(pasta)) Directory.Delete(pasta, true); }
            }
        }
    }
}
