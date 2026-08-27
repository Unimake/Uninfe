using NFe.Service;
using System;
using System.IO;
using Xunit;

namespace UniNFe.Test.Autorizacao
{
    public class NumeroLoteRetornoTests
    {
        [Fact]
        public void LoteRecebidoProntoNaoDevePublicarArquivoNumeroLote()
        {
            var pastaRetorno = CriarPastaTemporaria();

            try
            {
                var publicou = TaskNFeRecepcao.PublicarArquivosNumeroLote(pastaRetorno, null, null, false);

                Assert.False(publicou);
                Assert.Empty(Directory.GetFiles(pastaRetorno));
            }
            finally
            {
                Directory.Delete(pastaRetorno, true);
            }
        }

        [Fact]
        public void ArquivosExistentesNoRetornoDevemSerSubstituidos()
        {
            var pastaTeste = CriarPastaTemporaria();
            var pastaTemporaria = Path.Combine(pastaTeste, "Temp");
            var pastaRetorno = Path.Combine(pastaTeste, "Retorno");
            Directory.CreateDirectory(pastaTemporaria);
            Directory.CreateDirectory(pastaRetorno);

            var arquivoTemporarioXML = Path.Combine(pastaTemporaria, "nota-num-lot.xml");
            var arquivoTemporarioTXT = Path.Combine(pastaTemporaria, "nota-num-lot.txt");
            var arquivoRetornoXML = Path.Combine(pastaRetorno, "nota-num-lot.xml");
            var arquivoRetornoTXT = Path.Combine(pastaRetorno, "nota-num-lot.txt");

            File.WriteAllText(arquivoTemporarioXML, "<DadosLoteNfe><NumeroLoteGerado>2</NumeroLoteGerado></DadosLoteNfe>");
            File.WriteAllText(arquivoTemporarioTXT, "2;");
            File.WriteAllText(arquivoRetornoXML, "<DadosLoteNfe><NumeroLoteGerado>1</NumeroLoteGerado></DadosLoteNfe>");
            File.WriteAllText(arquivoRetornoTXT, "1;");

            try
            {
                var publicou = TaskNFeRecepcao.PublicarArquivosNumeroLote(pastaRetorno, arquivoTemporarioXML, arquivoTemporarioTXT, true);

                Assert.True(publicou);
                Assert.Contains("NumeroLoteGerado>2", File.ReadAllText(arquivoRetornoXML));
                Assert.Equal("2;", File.ReadAllText(arquivoRetornoTXT));
                Assert.False(File.Exists(arquivoTemporarioXML));
                Assert.False(File.Exists(arquivoTemporarioTXT));
            }
            finally
            {
                Directory.Delete(pastaTeste, true);
            }
        }

        private static string CriarPastaTemporaria()
        {
            var pasta = Path.Combine(Path.GetTempPath(), "UniNFe.Test", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pasta);

            return pasta;
        }
    }
}
