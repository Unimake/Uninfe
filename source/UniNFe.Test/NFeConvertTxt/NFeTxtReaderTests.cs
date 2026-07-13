using System;
using System.IO;
using System.Linq;
using NFe.ConvertTxt;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    [Collection("NFeConvertTxt")]
    public sealed class NFeTxtReaderTests
    {
        [Fact]
        public void ArquivoSemCabecalhoDeveRetornarMensagemDeLayoutInvalido()
        {
            ExecutarComArquivo("A|4.00|NFe|", conversao =>
            {
                Assert.False(conversao.Sucesso);
                Assert.Contains("primeira linha", conversao.MensagemErro);
            });
        }

        [Fact]
        public void PrimeiroRegistroAposCabecalhoDeveSerSegmentoA()
        {
            ExecutarComArquivo("NOTAFISCAL|1|" + Environment.NewLine + "B|35|", conversao =>
            {
                Assert.False(conversao.Sucesso);
                Assert.Contains("segmento A", conversao.MensagemErro);
            });
        }

        [Fact]
        public void VersaoDiferenteDeQuatroDeveSerRejeitada()
        {
            ExecutarComArquivo("NOTAFISCAL|1|" + Environment.NewLine + "A|3.10|", conversao =>
            {
                Assert.False(conversao.Sucesso);
                Assert.Contains("versão 4.00", conversao.MensagemErro);
            });
        }

        [Fact]
        public void ArquivoComDuasNotasDeveGerarDoisXmls()
        {
            var primeiro = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "RTC", "NFE_Venda_00002.txt");
            var segundo = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures", "RTC", "NFe_Venda_para_o_Governo.txt");
            var conteudo = File.ReadAllLines(primeiro)
                .Concat(File.ReadAllLines(segundo).Skip(1));

            ExecutarComArquivo(string.Join(Environment.NewLine, conteudo), conversao =>
            {
                Assert.True(conversao.Sucesso, conversao.MensagemErro);
                Assert.Equal(2, conversao.ArquivosGerados);
            });
        }

        private static void ExecutarComArquivo(string conteudo, Action<ResultadoConversao> validar)
        {
            var pasta = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pasta);
            try
            {
                var arquivo = Path.Combine(pasta, "entrada.txt");
                var destino = Path.Combine(pasta, "xml");
                File.WriteAllText(arquivo, conteudo);

                var conversao = new ConversaoTXT();
                var sucesso = conversao.Converter(arquivo, destino);
                validar(new ResultadoConversao(sucesso, conversao.cMensagemErro, conversao.cRetorno.Count));
            }
            finally
            {
                if (Directory.Exists(pasta)) Directory.Delete(pasta, true);
            }
        }

        private sealed class ResultadoConversao
        {
            internal ResultadoConversao(bool sucesso, string mensagemErro, int arquivosGerados)
            {
                Sucesso = sucesso;
                MensagemErro = mensagemErro;
                ArquivosGerados = arquivosGerados;
            }

            internal bool Sucesso { get; }
            internal string MensagemErro { get; }
            internal int ArquivosGerados { get; }
        }
    }
}
