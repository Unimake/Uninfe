using System.IO;
using System;
using System.Reflection;
using NFe.ConvertTxt;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class DifferentialConversionTests
    {
        private readonly NFeConvertTxtFixture fixture = new NFeConvertTxtFixture();

        [Theory]
        [MemberData(nameof(NFeConvertTxtFixture.ArquivosNFe400), MemberType = typeof(NFeConvertTxtFixture))]
        public void XmlDaDllDeveSerEquivalenteAoGeradorLegado(string arquivoOrigem, bool sucessoEsperado)
        {
            if (!sucessoEsperado) return;
            using (var resultado = fixture.Converter(arquivoOrigem))
            {
                Assert.True(resultado.Sucesso, resultado.MensagemErro);
                Assert.NotNull(resultado.Nota);
                var pastaLegado = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(pastaLegado);
                string legado;
                try
                {
                    var geradorLegado = new NFeW { cMensagemErro = string.Empty };
                    typeof(NFeW).GetMethod("GerarXmlLegado", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(geradorLegado, new object[] { resultado.Nota, pastaLegado, arquivoOrigem });
                    legado = File.ReadAllText(geradorLegado.cFileName);
                }
                finally
                {
                    if (Directory.Exists(pastaLegado)) Directory.Delete(pastaLegado, true);
                }
                var conversaoNova = new Unimake.Business.DFe.Xml.NFe.NFeTxtConverter().Converter(arquivoOrigem);
                Assert.True(conversaoNova.Sucesso, conversaoNova.MensagemErro);
                var novo = Assert.Single(conversaoNova.Documentos).Xml;
                var diferenca = NFeConvertTxtXmlComparer.Comparar(legado, novo);
                Assert.True(diferenca == null, Path.GetFileName(arquivoOrigem) + ": " + diferenca);
            }
        }
    }
}
