using NFe.Threadings;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static UniNFe.Test.Atualizacao.AtualizacaoTestFixture;

namespace UniNFe.Test.Atualizacao
{
    [Collection("Atualizacao Serial")]
    public class ParadaAtualizacaoTests
    {
        [Fact]
        public void InstaladorAguardaArquivoRealEGravacaoDoRetorno() => ExecutarNaInterface(async () =>
        {
            using (var contexto = new ContextoMonitor())
            using (var update = new Atualizador(contexto.Pasta))
            using (var tela = new Tela(update))
            {
                contexto.Iniciar();
                Assert.True(contexto.Iniciou.Wait(3000), "O watcher não iniciou o arquivo de teste.");
                tela.AoParar = () => ThreadService.PararParaAtualizacao();
                update.AoInstalar = () =>
                {
                    Assert.True(contexto.Concluiu.IsSet);
                    Assert.True(File.Exists(Path.Combine(contexto.PastaRetorno, "retorno.xml")));
                };
                var atualizacao = tela.Executar();
                try
                {
                    Assert.True(await Task.Run(() => SpinWait.SpinUntil(() => contexto.Monitor.StopWatch, 3000)));
                    Assert.False(atualizacao.IsCompleted);
                    Assert.False(update.InstaladorIniciado);
                    Assert.False(tela.Encerrou);
                    // A interface continua disponível enquanto a parada aguarda o arquivo.
                    Assert.False(tela.PodeFechar());
                    File.WriteAllText(Path.Combine(contexto.PastaEnvio, "proximo.xml"), "<teste />");
                }
                finally
                {
                    contexto.Liberar.Set();
                    await atualizacao;
                }

                Assert.Null(tela.Falha);
                Assert.True(tela.Encerrou);
                Assert.True(update.InstaladorIniciado);
                Assert.True(File.Exists(Path.Combine(contexto.PastaEnvio, "proximo.xml")));
                Assert.False(File.Exists(Path.Combine(contexto.PastaEnvio, "Temp", "proximo.xml")));
            }
        });

        [Fact]
        public async Task ParadaAguardaWorkerDeWatcherJaDescartado()
        {
            using (var contexto = new ContextoMonitor())
            {
                contexto.Iniciar();
                Assert.True(contexto.Iniciou.Wait(3000));
                ThreadService.Stop();
                MonitoraPasta.fsw.Clear();
                var parada = Task.Run(() => ThreadService.PararParaAtualizacao());
                try
                {
                    Assert.NotSame(parada, await Task.WhenAny(parada, Task.Delay(100)));
                }
                finally
                {
                    contexto.Liberar.Set();
                    await parada;
                }
                Assert.True(contexto.Concluiu.IsSet);
            }
        }

        [Fact]
        public async Task ReconfiguracaoEmAndamentoNaoReiniciaMonitorDurantePreparacao()
        {
            using (var contexto = new ContextoMonitor())
            {
                contexto.AoConcluir = () =>
                {
                    ThreadService.Stop();
                    ThreadService.Start();
                };
                contexto.Iniciar();
                Assert.True(contexto.Iniciou.Wait(3000));
                var parada = Task.Run(() => ThreadService.PararParaAtualizacao());
                try
                {
                    Assert.True(SpinWait.SpinUntil(() => contexto.Monitor.StopWatch, 3000));
                }
                finally
                {
                    contexto.Liberar.Set();
                    Assert.Same(parada, await Task.WhenAny(parada, Task.Delay(3000)));
                    await parada;
                }
                Assert.True(contexto.Concluiu.IsSet);
                Assert.Single(MonitoraPasta.fsw);
                Assert.Same(contexto.Monitor, MonitoraPasta.fsw[0]);
                Assert.Empty(ThreadService.Threads);
            }
        }

        [Fact]
        public void FalhaDoInstaladorRetomaMonitorUmaUnicaVez() => ExecutarNaInterface(async () =>
        {
            using (var contexto = new ContextoMonitor())
            using (var update = new Atualizador(contexto.Pasta))
            using (var tela = new Tela(update))
            {
                contexto.Iniciar();
                Assert.True(contexto.Iniciou.Wait(3000));
                tela.AoParar = () => ThreadService.PararParaAtualizacao();
                tela.AoRetomar = () => ThreadService.RetomarAposAtualizacao();
                update.AoInstalar = () => throw new InvalidOperationException("Falha simulada ao abrir o instalador.");
                var atualizacao = tela.Executar();
                try
                {
                    Assert.True(await Task.Run(() => SpinWait.SpinUntil(() => contexto.Monitor.StopWatch, 3000)));
                    Assert.False(atualizacao.IsCompleted);
                }
                finally
                {
                    contexto.Liberar.Set();
                    await atualizacao;
                }

                Assert.IsType<InvalidOperationException>(tela.Falha);
                Assert.Equal(1, tela.Retomadas);
                Assert.False(tela.Encerrou);
                var retomado = Assert.Single(MonitoraPasta.fsw);
                Assert.NotSame(contexto.Monitor, retomado);
                Assert.False(retomado.StopWatch);
                ThreadService.RetomarAposAtualizacao();
                Assert.Same(retomado, Assert.Single(MonitoraPasta.fsw));
            }
        });

        [Fact]
        public void CancelarPreparacaoSemMonitorPermiteInicializacaoPosterior()
        {
            using (var contexto = new ContextoMonitor())
            {
                File.Delete(Path.Combine(contexto.PastaEnvio, "arquivo-teste.xml"));
                ThreadService.PararParaAtualizacao();
                ThreadService.RetomarAposAtualizacao(false);
                Assert.Empty(ThreadService.Threads);
                ThreadService.Start();
                Assert.False(Assert.Single(MonitoraPasta.fsw).StopWatch);
            }
        }

        [Fact]
        public void WatcherCanceladoAntesDeIniciarNaoMoveArquivos()
        {
            using (var contexto = new ContextoMonitor())
            {
                contexto.Monitor.StopWatch = true;
                contexto.Iniciar();
                ThreadService.PararParaAtualizacao();
                Assert.False(contexto.Iniciou.IsSet);
                Assert.True(File.Exists(Path.Combine(contexto.PastaEnvio, "arquivo-teste.xml")));
            }
        }
    }
}
