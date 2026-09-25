using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using NFe.Components;
using NFe.Service;
using NFe.Settings;
using UniNFe.Test.Abstractions;
using Xunit;

namespace UniNFe.Test.Erros
{
    [CollectionDefinition("Logs Serial", DisableParallelization = true)]
    public class LogsCollection { }

    [Collection("Logs Serial")]
    public class LogDiagnosticoTests : TaskTestFixtureBase
    {
        [Fact]
        public void RegistraContextoDoErroSemPilhaDoLoggerEMantemRetornoERP()
        {
            using (var contexto = new LogContext())
            {
                ExecutarEmThread("0", () => TFunctions.GravarArqErroServico(contexto.ArquivoEnvio,
                    "-env-loterps.xml", "-ret-loterps.err",
                    new AuthenticationException("O certificado remoto é inválido.")));

                var log = contexto.LerLog();
                Assert.Contains("ArquivoEntrada|" + contexto.ArquivoEnvio, log);
                Assert.Contains("ArquivoRetornoPrevisto|" + contexto.ArquivoRetorno, log);
                Assert.Contains("AmbienteCodigo|2", log);
                Assert.Contains("ExcecaoCompleta|System.Security.Authentication.AuthenticationException", log);
                Assert.Contains("Retorno de erro gravado para o ERP", log);
                Assert.DoesNotContain("PILHA DO PONTO DE REGISTRO", log);
                Assert.DoesNotContain("STACK TRACE:", log);
                Assert.False(File.Exists(contexto.ArquivoEnvio));
                Assert.True(File.Exists(Path.Combine(contexto.PastaErro, Path.GetFileName(contexto.ArquivoEnvio))));
                Assert.Contains("Message|O certificado remoto é inválido.", File.ReadAllText(contexto.ArquivoRetorno));
            }
        }

        [Fact]
        public void PreservaCausaOriginalNoLogSeMovimentacaoFalhar()
        {
            using (var contexto = new LogContext())
            {
                Directory.Delete(contexto.PastaErro);
                Assert.Throws<Exception>(() => ExecutarEmThread("0", () =>
                    TFunctions.GravarArqErroServico(contexto.ArquivoEnvio, "-env-loterps.xml",
                        "-ret-loterps.err", new Exception("Falha original na comunicação."))));

                var log = contexto.LerLog();
                Assert.Contains("Falha original na comunicação.", log);
                Assert.Contains("Falha ao movimentar o arquivo ou gravar o retorno", log);
                Assert.DoesNotContain("Retorno de erro gravado para o ERP", log);
                Assert.True(File.Exists(contexto.ArquivoEnvio));
            }
        }

        [Fact]
        public void GravaUmaUnicaPilhaAposDisputaPeloArquivoDeLog()
        {
            using (var contexto = new LogContext())
            {
                Functions.WriteLog("Preparação", false, true, "teste");
                var caminho = Directory.GetFiles(Propriedade.PastaLog, "*.log")[0];
                Exception falha = null;
                using (var iniciou = new ManualResetEventSlim())
                {
                    Thread thread;
                    using (var bloqueio = new FileStream(caminho, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        thread = new Thread(() =>
                        {
                            iniciou.Set();
                            try { Functions.WriteLog("Registro após bloqueio", true, true, "teste"); }
                            catch (Exception ex) { falha = ex; }
                        }) { Name = "teste-log" };
                        thread.Start();
                        Assert.True(iniciou.Wait(TimeSpan.FromSeconds(5)));
                        Thread.Sleep(200);
                    }

                    Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
                }

                Assert.Null(falha);
                var log = File.ReadAllText(caminho);
                Assert.Equal(1, log.Split(new[] { "PILHA DO PONTO DE REGISTRO" }, StringSplitOptions.None).Length - 1);
                Assert.Contains(" / teste-log]", log);
                Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} -", log);
            }
        }

        private sealed class LogContext : TaskTestContextBase, IDisposable
        {
            private readonly bool gravarLogOriginal;

            public LogContext() : base("teste-env-loterps.xml", "<teste />")
            {
                gravarLogOriginal = ConfiguracaoApp.GravarLogOperacoesRealizadas;
                ConfiguracaoApp.GravarLogOperacoesRealizadas = true;
                Propriedade.PastaExecutavel = PastaTemporaria;
                Directory.CreateDirectory(Propriedade.PastaLog);
                PastaErro = Path.Combine(PastaTemporaria, "erro");
                Directory.CreateDirectory(PastaErro);
                DefinirConfiguracoesEmpresas(new List<Empresa>
                {
                    new Empresa { CNPJ = "99999999000191", AmbienteCodigo = 2,
                        PastaXmlRetorno = PastaRetorno, PastaXmlErro = PastaErro }
                });
            }

            public string PastaErro { get; }
            public string ArquivoRetorno => Path.Combine(PastaRetorno, "teste-ret-loterps.err");
            public string LerLog() => File.ReadAllText(Directory.GetFiles(Propriedade.PastaLog, "*.log")[0]);

            public new void Dispose()
            {
                ConfiguracaoApp.GravarLogOperacoesRealizadas = gravarLogOriginal;
                base.Dispose();
            }
        }
    }
}
