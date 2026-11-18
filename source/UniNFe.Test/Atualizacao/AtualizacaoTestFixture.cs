using NFe.Components;
using NFe.Settings;
using NFe.UI.Formularios;
using UniNFe.Test.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace UniNFe.Test.Atualizacao
{
    [CollectionDefinition("Atualizacao Serial", DisableParallelization = true)]
    public class AtualizacaoCollection
    {
    }

    public class AtualizacaoTestFixture
    {
        internal sealed class Contexto : TaskTestContextBase
        {
            private readonly bool encerramentoAnterior = Propriedade.EncerrarApp;
            internal string Pasta => PastaTemporaria;

            internal Contexto() : base("controle.txt", "TESTE")
            {
                Propriedade.PastaExecutavel = Pasta;
                Directory.CreateDirectory(Propriedade.PastaLog);
                Propriedade.EncerrarApp = false;
            }

            internal void LiberarConsulta() => File.WriteAllText(Path.Combine(Pasta, "UltimaAtualizacao.xml"),
                "<UltimaAtualizacao><data>2000-01-01T00:00:00</data></UltimaAtualizacao>");

            public new void Dispose()
            {
                Propriedade.EncerrarApp = encerramentoAnterior;
                base.Dispose();
            }
        }

        internal sealed class Atualizador : UniNFeUpdate
        {
            private readonly string pasta;
            internal readonly List<string> Eventos = new List<string>();
            internal string Versao = "9.0.0.0";
            internal bool ServicoAtivo;
            internal Action AoConsultar;
            internal Action AoBaixar;
            internal Action AoInstalar;

            internal Atualizador(string pasta) : base(null, pasta) => this.pasta = pasta;

            protected override bool ServicoEmExecucao => ServicoAtivo;
            protected override string VersaoAtual => "5.0.0.0";

            protected override List<string> BuscaURL()
            {
                AoConsultar?.Invoke();
                return new List<string> { "https://example.com" };
            }

            protected override void DownloadArquivo(string url, string destino, Action<UpdateProgessEventArgs> progresso = null)
            {
                Eventos.Add("consultar");
                File.WriteAllText(destino, "<atualizacao><versao>" + Versao + "</versao></atualizacao>");
            }

            protected override void Download(Action<UpdateProgessEventArgs> progresso = null)
            {
                Eventos.Add("baixar");
                AoBaixar?.Invoke();
#if _BETA
                var nome = "iuninfe5_beta.exe";
#else
                var nome = "iuninfe5.exe";
#endif
                File.WriteAllText(Path.Combine(pasta, nome), "INSTALADOR SIMULADO");
                progresso?.Invoke(new UpdateProgessEventArgs { BytesRead = 1, TotalBytes = 1 });
            }

            protected override void ExecutarInstalador(string arquivo, string parametros)
            {
                Eventos.Add("instalar");
                AoInstalar?.Invoke();
                Assert.True(File.Exists(arquivo));
                Assert.Contains("/SILENT", parametros);
                Assert.Contains(pasta, parametros);
            }
        }

        internal sealed class Tela : FormUpdate
        {
            internal Atualizador Atualizacao;
            internal bool Confirmacao = true;
            internal int Paradas;
            internal int Retomadas;
            internal bool Encerrou;
            internal Exception Falha;
            internal Action AoParar;

            internal Tela(Atualizador update) => Atualizacao = update;

            internal Task Executar() => Atualizar();

            internal bool PodeTentarNovamente => Controls["btnAtualizar"].Enabled;

            internal bool PodeFechar()
            {
                var args = new FormClosingEventArgs(CloseReason.UserClosing, false);
                OnFormClosing(args);
                return !args.Cancel;
            }

            protected override bool ConfirmarAtualizacao() => Confirmacao;
            protected override UniNFeUpdate CriarAtualizacao() => Atualizacao;

            protected override void PararServicos()
            {
                Paradas++;
                Atualizacao.Eventos.Add("parar");
                AoParar?.Invoke();
                Atualizacao.ServicoAtivo = false;
            }

            protected override void RetomarServicos()
            {
                Retomadas++;
                Atualizacao.Eventos.Add("retomar");
            }

            protected override void EncerrarAplicacao()
            {
                Encerrou = true;
                Atualizacao.Eventos.Add("encerrar");
            }

            protected override void ExibirFalha(Exception ex) => Falha = ex;
        }

        internal static void ExecutarNaInterface(Func<Task> teste)
        {
            Exception falha = null;
            var thread = new Thread(() =>
            {
                using (var contexto = new ApplicationContext())
                {
                    EventHandler inicio = null;
                    inicio = async (sender, args) =>
                    {
                        Application.Idle -= inicio;
                        try
                        {
                            using (var controle = new Control())
                            {
                                var handle = controle.Handle;
                                await teste();
                            }
                        }
                        catch (Exception ex)
                        {
                            falha = ex;
                        }
                        finally
                        {
                            contexto.ExitThread();
                        }
                    };
                    Application.Idle += inicio;
                    Application.Run(contexto);
                }
            }) { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.True(thread.Join(15000), "O teste da interface não terminou.");
            if (falha != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(falha).Throw();
            }
        }
    }
}
