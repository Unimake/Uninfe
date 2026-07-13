using System;
using System.IO;
using System.Reflection;
using NFe.ConvertTxt;
using NFe.ConvertTxt.Generation;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class IcMs40RegressionTests
    {
        private readonly NFeConvertTxtFixture fixture = new NFeConvertTxtFixture();

        [Fact]
        public void RotinaLegadaDeveGerarICMS40SemIndDeduzDesonQuandoNaoHaVICMSDeson()
        {
            using (var resultado = fixture.Converter(CaminhoFixture()))
            {
                Assert.True(resultado.Sucesso, resultado.MensagemErro);
                resultado.Nota.resptecnico.CNPJ = null;
                var xml = GerarXmlLegado(resultado.Nota, CaminhoFixture());
                Assert.Contains("<ICMS40>", xml);
                Assert.DoesNotContain("<indDeduzDeson>", xml);
            }
        }

        [Fact]
        public void NovaRotinaDeveSerEquivalenteAoLegadoNoICMS40()
        {
            using (var resultado = fixture.Converter(CaminhoFixture()))
            {
                Assert.True(resultado.Sucesso, resultado.MensagemErro);
                resultado.Nota.resptecnico.CNPJ = null;
                var legado = GerarXmlLegado(resultado.Nota, CaminhoFixture());
                var novo = new NFeDFeXmlSerializer().Serializar(resultado.Nota).OuterXml;
                Assert.Null(NFeConvertTxtXmlComparer.Comparar(legado, novo));
                Assert.DoesNotContain("<indDeduzDeson>", novo);
            }
        }

        private static string CaminhoFixture() => Path.Combine(
            AppContext.BaseDirectory,
            "NFeConvertTxt",
            "Fixtures",
            "Regressions",
            "NFe_000250887_07_43_31-nfe-orig.txt");

        private static string GerarXmlLegado(NFe.ConvertTxt.NFe nota, string arquivoOrigem)
        {
            var pasta = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pasta);
            try
            {
                var gerador = new NFeW { cMensagemErro = string.Empty };
                typeof(NFeW).GetMethod("GerarXmlLegado", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(gerador, new object[] { nota, pasta, arquivoOrigem });
                return File.ReadAllText(gerador.cFileName);
            }
            finally
            {
                if (Directory.Exists(pasta)) Directory.Delete(pasta, true);
            }
        }
    }
}
