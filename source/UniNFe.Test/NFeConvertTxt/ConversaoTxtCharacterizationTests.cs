using System.IO;
using System;
using NFe.ConvertTxt;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class ConversaoTxtCharacterizationTests
    {
        private readonly NFeConvertTxtFixture fixture = new NFeConvertTxtFixture();

        [Theory]
        [MemberData(nameof(NFeConvertTxtFixture.ArquivosNFe400), MemberType = typeof(NFeConvertTxtFixture))]
        public void ConversaoDevePreservarResultadoCaracterizado(string arquivoOrigem, bool sucessoEsperado)
        {
            using (var resultado = fixture.Converter(arquivoOrigem))
            {
                Assert.True(resultado.Sucesso == sucessoEsperado, Path.GetFileName(arquivoOrigem));

                if (!sucessoEsperado) return;

                Assert.NotEmpty(resultado.Arquivos);
                foreach (var arquivo in resultado.Arquivos)
                {
                    Assert.True(File.Exists(arquivo.Caminho), arquivo.Caminho);
                    Assert.Equal(44, arquivo.Chave.Length);
                }
            }
        }

        [Fact]
        public void MesmaInstanciaDeveConverterArquivosConsecutivosSemReaproveitarEstado()
        {
            var pastaDestino = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pastaDestino);
            try
            {
                var conversao = new ConversaoTXT();
                var primeiro = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "RTC", "NFE_Venda_00002.txt");
                var segundo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "RTC", "NFe_Venda_para_o_Governo.txt");

                Assert.True(conversao.Converter(primeiro, pastaDestino), conversao.cMensagemErro);
                Assert.Single(conversao.cRetorno);
                Assert.True(conversao.Converter(segundo, pastaDestino), conversao.cMensagemErro);
                Assert.Single(conversao.cRetorno);
            }
            finally
            {
                if (Directory.Exists(pastaDestino)) Directory.Delete(pastaDestino, true);
            }
        }
    }
}
