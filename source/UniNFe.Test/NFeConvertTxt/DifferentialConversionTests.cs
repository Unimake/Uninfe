using System.IO;
using System;
using System.Reflection;
using NFe.ConvertTxt.Generation;
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
                var novo = new NFeDFeXmlSerializer().Serializar(resultado.Nota).OuterXml;
                var diferenca = NFeConvertTxtXmlComparer.Comparar(legado, novo);
                Assert.True(diferenca == null, Path.GetFileName(arquivoOrigem) + ": " + diferenca);
            }
        }
    }
}
