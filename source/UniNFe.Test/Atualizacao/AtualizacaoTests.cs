using NFe.Components;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static UniNFe.Test.Atualizacao.AtualizacaoTestFixture;

namespace UniNFe.Test.Atualizacao
{
    [Collection("Atualizacao Serial")]
    public class AtualizacaoTests
    {
        [Theory]
        [InlineData("9.0.0.0", true)]
        [InlineData("5.0.0.0", false)]
        [InlineData("4.0.0.0", false)]
        public void ConsultaApenasInformaDisponibilidade(string versao, bool esperado)
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta) { Versao = versao })
            {
                contexto.LiberarConsulta();
                Assert.Equal(esperado, update.VerificaVersao());
                Assert.Equal(new[] { "consultar" }, update.Eventos);
                Assert.False(update.InstaladorIniciado);
            }
        }

        [Fact]
        public void ConsultaRespeitaIntervaloSemAcessarRede()
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta))
            {
                update.AoConsultar = () => throw new Exception("A consulta não deveria ocorrer.");
                Assert.False(update.VerificaVersao());
                Assert.False(update.VerificaVersao());
                Assert.Empty(update.Eventos);
            }
        }

        [Fact]
        public void FalhaNaConsultaNaoBaixaNemInstala()
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta))
            {
                contexto.LiberarConsulta();
                update.AoConsultar = () => throw new IOException("Sem rede.");
                Assert.Throws<IOException>(() => update.VerificaVersao());
                Assert.Empty(update.Eventos);
                Assert.False(update.InstaladorIniciado);
            }
        }

        [Fact]
        public void DownloadPodeOcorrerComServicoAtivo()
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta) { ServicoAtivo = true })
            {
                update.Baixar();
                Assert.Equal(new[] { "baixar" }, update.Eventos);
                Assert.Throws<InvalidOperationException>(() => update.InstalarBaixado());
                Assert.False(update.InstaladorIniciado);
                update.ServicoAtivo = false;
                update.InstalarBaixado();
                Assert.True(update.InstaladorIniciado);
            }
        }

        [Fact]
        public void EntradaLegadaContinuaBaixandoAntesDeInstalar()
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta))
            {
                update.Instalar();
                Assert.Equal(new[] { "baixar", "instalar" }, update.Eventos);
                Assert.True(update.InstaladorIniciado);
            }
        }

        [Fact]
        public void EntradaLegadaRespeitaServicoAtivo()
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta) { ServicoAtivo = true })
            {
                update.Instalar();
                Assert.Empty(update.Eventos);
            }
        }

        [Fact]
        public void FalhaEmNovoDownloadImpedeUsoDeInstaladorAnterior()
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta))
            {
                Assert.Throws<InvalidOperationException>(() => update.InstalarBaixado());
                update.Baixar();
                update.AoBaixar = () => throw new IOException("Download interrompido.");
                Assert.Throws<IOException>(() => update.Baixar());
                Assert.Throws<InvalidOperationException>(() => update.InstalarBaixado());
                Assert.DoesNotContain("instalar", update.Eventos);
            }
        }

        [Fact]
        public void AvisosEDownloadMantemMonitorAtivoEImpedemExecucaoDuplicada() => ExecutarNaInterface(async () =>
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta) { ServicoAtivo = true })
            using (var tela = new Tela(update))
            using (var liberar = new ManualResetEventSlim())
            {
                var iniciou = new TaskCompletionSource<bool>();
                update.AoBaixar = () =>
                {
                    iniciou.SetResult(true);
                    if (!liberar.Wait(5000)) throw new TimeoutException();
                };
                var tarefa = tela.Executar();
                try
                {
                    await iniciou.Task;
                    Assert.Equal(0, tela.Paradas);
                    Assert.True(update.ServicoAtivo);
                    Assert.False(tela.PodeFechar());
                    await tela.Executar();
                    Assert.Equal(new[] { "baixar" }, update.Eventos);
                }
                finally
                {
                    liberar.Set();
                    await tarefa;
                }

                Assert.Null(tela.Falha);
                Assert.Equal(new[] { "baixar", "parar", "instalar", "encerrar" }, update.Eventos);
                Assert.True(tela.Encerrou);
                Assert.Equal(0, tela.Retomadas);
            }
        });

        [Fact]
        public void CancelamentoNaoParaMonitor() => ExecutarNaInterface(async () =>
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta))
            using (var tela = new Tela(update) { Confirmacao = false })
            {
                await tela.Executar();
                Assert.Empty(update.Eventos);
                Assert.Equal(0, tela.Paradas);
                Assert.True(tela.PodeTentarNovamente);
            }
        });

        [Fact]
        public void FalhaNoDownloadMantemMonitorEPermiteNovaTentativa() => ExecutarNaInterface(async () =>
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta))
            using (var tela = new Tela(update))
            {
                update.AoBaixar = () => throw new IOException("Download interrompido.");
                await tela.Executar();
                Assert.IsType<IOException>(tela.Falha);
                Assert.Equal(0, tela.Paradas);
                Assert.Equal(0, tela.Retomadas);
                Assert.False(tela.Encerrou);
                Assert.True(tela.PodeTentarNovamente);
                Assert.True(tela.PodeFechar());

                using (var nova = new Atualizador(contexto.Pasta))
                {
                    tela.Atualizacao = nova;
                    await tela.Executar();
                    Assert.True(tela.Encerrou);
                    Assert.Equal(new[] { "baixar", "parar", "instalar", "encerrar" }, nova.Eventos);
                }
            }
        });

        [Fact]
        public void FechamentoDuranteDownloadNaoIniciaInstalacao() => ExecutarNaInterface(async () =>
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta))
            using (var tela = new Tela(update))
            using (var liberar = new ManualResetEventSlim())
            {
                var iniciou = new TaskCompletionSource<bool>();
                update.AoBaixar = () =>
                {
                    iniciou.SetResult(true);
                    if (!liberar.Wait(5000)) throw new TimeoutException();
                };
                var tarefa = tela.Executar();
                try
                {
                    await iniciou.Task;
                    tela.Dispose();
                }
                finally
                {
                    liberar.Set();
                    await tarefa;
                }

                Assert.Equal(new[] { "baixar" }, update.Eventos);
                Assert.Equal(0, tela.Paradas);
                Assert.False(tela.Encerrou);
            }
        });

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FalhaAntesDoInstaladorRetomaMonitor(bool falharAoParar) => ExecutarNaInterface(async () =>
        {
            using (var contexto = new Contexto())
            using (var update = new Atualizador(contexto.Pasta))
            using (var tela = new Tela(update))
            {
                Action falha = () => throw new InvalidOperationException("Falha simulada.");
                if (falharAoParar) tela.AoParar = falha;
                else update.AoInstalar = falha;
                await tela.Executar();
                Assert.IsType<InvalidOperationException>(tela.Falha);
                Assert.Equal(1, tela.Paradas);
                Assert.Equal(1, tela.Retomadas);
                Assert.False(tela.Encerrou);
                Assert.False(update.InstaladorIniciado);
                Assert.True(tela.PodeTentarNovamente);
            }
        });

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(10, -1, 0)]
        [InlineData(1, 2, 50)]
        [InlineData(11, 10, 100)]
        public void ProgressoPermaneceValido(long lidos, long total, int esperado) =>
            Assert.Equal(esperado, new UpdateProgessEventArgs { BytesRead = lidos, TotalBytes = total }.ProgressPercentage);
    }
}
