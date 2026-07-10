using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UniNFe.Test.NFeConvertTxt
{
    public sealed class NFeConvertTxtFixture
    {
        public static IEnumerable<object[]> ArquivosNFe400()
        {
            var raiz = Path.Combine(AppContext.BaseDirectory, "NFeConvertTxt", "Fixtures");
            var conversoesComSucesso = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CST_SEM_CLASSTRIB_SEM_NotaCredito03Retorno_SemImpostoIBSCBS.txt",
                "NFE_Devolucao_00003.txt",
                "NFe_ReformaTributaria_1_prod-nfe.txt",
                "NFe_ReformaTributaria_3_prods-nfe.txt",
                "NFe_Reforma_Tributaria-nfe.txt",
                "NFe_Reforma_Tributaria_Monofasica-nfe.txt",
                "NFE_Venda_00002.txt",
                "NFe_Venda_para_o_Governo.txt"
            };

            return Directory
                .GetFiles(raiz, "*.txt", SearchOption.AllDirectories)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(x => new object[] { x, conversoesComSucesso.Contains(Path.GetFileName(x)) });
        }

        public ConversionResult Converter(string arquivoOrigem)
        {
            var pastaTemporaria = Path.Combine(Path.GetTempPath(), "UniNFe.Test", "NFeConvertTxt", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pastaTemporaria);

            try
            {
                var conversao = new NFe.ConvertTxt.ConversaoTXT();
                var sucesso = conversao.Converter(arquivoOrigem, pastaTemporaria);
                var arquivos = conversao.cRetorno
                    .Select(x => new ConvertedFile(x.XMLFileName, x.ChaveNFe, x.NotaFiscal, x.Serie))
                    .ToList();
                var nota = (NFe.ConvertTxt.NFe)typeof(NFe.ConvertTxt.ConversaoTXT)
                    .GetField("NFe", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(conversao);

                return new ConversionResult(sucesso, conversao.cMensagemErro, arquivos, pastaTemporaria, nota);
            }
            catch
            {
                Directory.Delete(pastaTemporaria, true);
                throw;
            }
        }
    }

    public sealed class ConversionResult : IDisposable
    {
        public ConversionResult(bool sucesso, string mensagemErro, IReadOnlyList<ConvertedFile> arquivos, string pastaTemporaria, NFe.ConvertTxt.NFe nota)
        {
            Sucesso = sucesso;
            MensagemErro = mensagemErro;
            Arquivos = arquivos;
            PastaTemporaria = pastaTemporaria;
            Nota = nota;
        }

        public bool Sucesso { get; }

        public string MensagemErro { get; }

        public IReadOnlyList<ConvertedFile> Arquivos { get; }
        public NFe.ConvertTxt.NFe Nota { get; }

        private string PastaTemporaria { get; }

        public void Dispose()
        {
            if (Directory.Exists(PastaTemporaria))
            {
                Directory.Delete(PastaTemporaria, true);
            }
        }
    }

    public sealed class ConvertedFile
    {
        public ConvertedFile(string caminho, string chave, int numero, int serie)
        {
            Caminho = caminho;
            Chave = chave;
            Numero = numero;
            Serie = serie;
        }

        public string Caminho { get; }

        public string Chave { get; }

        public int Numero { get; }

        public int Serie { get; }
    }
}
