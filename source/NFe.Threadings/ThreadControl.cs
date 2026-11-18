using NFe.Service;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace NFe.Threadings
{


    #region ThreadItem

    /// <summary>
    /// classe de item da thread
    /// </summary>
    public class ThreadItem : IDisposable
    {
        #region delegates

        public delegate void ThreadStartHandler(ThreadItem item);

        public delegate void ThreadEndedHandler(ThreadItem item);

        public delegate void ThreadReleasedHandler(ThreadItem item);

        #endregion delegates

        #region Eventos

        /// <summary>
        /// acontece quando a thread começou a leitura do arquivo
        /// </summary>
        public static event ThreadStartHandler OnStarted;

        /// <summary>
        /// acontece quando a thread finalizou a leitura do arquivo
        /// </summary>
        public static event ThreadEndedHandler OnEnded;

        /// <summary>
        /// acontece quando a thread removeu o arquivo do buffer
        /// </summary>
        public static event ThreadReleasedHandler OnReleased;

        #endregion Eventos

        public ThreadItem(System.IO.FileInfo fi, int empresa)
        {
            FileInfo = fi;
            Empresa = empresa;
        }

        public System.IO.FileInfo FileInfo { get; private set; }
        public int Empresa { get; private set; }

        /*<#8084>
         * Com a morte da classe ThreadControl, este método passou a ser responsável
         * pela execução dos eventos que antes eram feitos pela ThreadControl
         *
         */

        /// <summary>
        /// Método responsável por executar os eventos de forma síncrona em uma thread separada
        /// </summary>
        public void Run()
        {
            var empresa = Empresa >= 0 && Empresa < Empresas.Configuracoes.Count
                ? Empresas.Configuracoes[Empresa]
                : null;
            using (CoordenadorOperacaoCertificadoA3.Entrar(empresa))
            {
                Processar(this);
            }
        }

        private void Processar(ThreadItem item)
        {
            if (String.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                if (item.FileInfo.DirectoryName.EndsWith("geral\\temp", StringComparison.OrdinalIgnoreCase))
                {
                    item.Empresa = -1;
                }

                Thread.CurrentThread.Name = item.Empresa.ToString();
            }

            try
            {
                //avisa que vai iniciar
                if (OnStarted != null) OnStarted(item);

                //avisa que vai finalizar
                if (OnEnded != null) OnEnded(item);
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog("Ocorreu um erro na execução da thread que está sendo executada.\r\nThreadControl.cs (1)\r\n" + ex.Message, true);
            }
            finally
            {
                try
                {
                    //remove o item
                    //avisa que removeu o item
                    if (OnReleased != null) OnReleased(item);
                }
                catch (Exception ex)
                {
                    Auxiliar.WriteLog("Ocorreu um erro ao tentar remover o item da Thread que está sendo executada.\r\nThreadControl.cs (2)\r\n" + ex.Message, true);
                }
            }
        }

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                //need to do something?
            }

#if DEBUG
            Debug.WriteLine(String.Format("ThreadItem Dipose(disposing: {0});", disposing));
#endif
        }

        ~ThreadItem()
        {
#if DEBUG
            Debug.WriteLine("ThreadItem ~Destructor");
#endif

            Dispose(false);
        }

        #endregion IDisposable members

        //</#8084>
    }

    #endregion ThreadItem

    /*<#8084>
     * A classe ThreadControl deixou de existir por não ser mais utilizada dentro do aplicação,
     * foi substituída pelo método ThreadItem.Run()_
     *</#8084>
     */

    internal static class ControleThreads
    {
        private static readonly object Sincronizacao = new object();
        private static readonly HashSet<Thread> Ativas = new HashSet<Thread>();

        internal static Thread Iniciar(Action executar)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    executar();
                }
                finally
                {
                    lock (Sincronizacao)
                    {
                        Ativas.Remove(Thread.CurrentThread);
                        Monitor.PulseAll(Sincronizacao);
                    }
                }
            }) { IsBackground = true };

            lock (Sincronizacao)
            {
                Ativas.Add(thread);
            }
            try
            {
                thread.Start();
                return thread;
            }
            catch
            {
                lock (Sincronizacao)
                {
                    Ativas.Remove(thread);
                    Monitor.PulseAll(Sincronizacao);
                }
                throw;
            }
        }

        internal static void Aguardar(Action informarEspera = null)
        {
            while (true)
            {
                lock (Sincronizacao)
                {
                    if (Ativas.Count == 0)
                    {
                        return;
                    }
                    if (Ativas.Contains(Thread.CurrentThread))
                    {
                        throw new InvalidOperationException("A conclusão dos processamentos deve ser aguardada fora das threads de serviço.");
                    }
                    Monitor.Wait(Sincronizacao, 5000);
                }
                informarEspera?.Invoke();
            }
        }
    }

    #region ThreadService

    /// <summary>
    /// Classe responsável por executar as thread´s base de verificação dos serviços a serem executados
    /// </summary>
    public static class ThreadService
    {
        public static NotifyIcon NotifyIconUniNFe;
        public static List<Thread> Threads = new List<Thread>();
        private static readonly object CicloDeVida = new object();
        private static readonly object Retomada = new object();
        private static bool preparandoAtualizacao;

        public static void Stop()
        {
            lock (CicloDeVida)
            {
                Parar();
            }
        }

        public static void PararParaAtualizacao(Action informarEspera = null)
        {
            lock (CicloDeVida)
            {
                preparandoAtualizacao = true;
                Parar();
            }

            // A espera ocorre fora do lock: um arquivo de configuração em execução
            // pode chamar Stop/Start antes de liberar a própria thread.
            ControleThreads.Aguardar(informarEspera);
        }

        public static void RetomarAposAtualizacao(bool iniciarMonitor = true)
        {
            RetomarAposAtualizacao(iniciarMonitor ? (Action)Iniciar : null);
        }

        private static void RetomarAposAtualizacao(Action iniciarMonitor)
        {
            // Serializa também a limpeza de uma retomada que falhou, sem manter
            // CicloDeVida bloqueado enquanto os arquivos concluem o processamento.
            lock (Retomada)
            {
                lock (CicloDeVida)
                {
                    if (!preparandoAtualizacao)
                    {
                        return;
                    }
                }
                ControleThreads.Aguardar();
                try
                {
                    lock (CicloDeVida)
                    {
                        if (preparandoAtualizacao)
                        {
                            try
                            {
                                iniciarMonitor?.Invoke();
                                preparandoAtualizacao = false;
                            }
                            catch
                            {
                                Parar();
                                throw;
                            }
                        }
                    }
                }
                catch
                {
                    // Uma inicialização parcial pode já ter recebido arquivos.
                    // Aguarda esses arquivos antes de permitir uma nova tentativa.
                    ControleThreads.Aguardar();
                    lock (CicloDeVida)
                    {
                        MonitoraPasta.fsw.Clear();
                        Threads.Clear();
                        preparandoAtualizacao = false;
                    }
                    throw;
                }
            }
        }

        private static void Parar()
        {
            for (int i = 0; i < MonitoraPasta.fsw.Count; i++)
            {
                try
                {
                    FileSystemWatcher fsw = MonitoraPasta.fsw[i];

                    fsw.StopWatch = true;
                    fsw.Dispose();
                    fsw = null;
                }
                catch (Exception ex)
                {
                    Auxiliar.WriteLog("Ocorreu um erro ao tentar parar o FSW: " + MonitoraPasta.fsw[i].Directory + ".\r\nThreadService.cs\r\n" + ex.Message, false);
                }
            }

            ControleEncerramento.Sinalizar();
            for (int i = 0; i < Threads.Count; i++)
            {
                Thread t = Threads[i];
                if (t.IsAlive && !t.Join(10000))
                {
                    Auxiliar.WriteLog("A thread " + (t.Name ?? i.ToString()) + " não encerrou dentro do tempo limite e não será abortada durante possível operação criptográfica.", false);
                }
            }
            Threads.Clear();
        }

        public static void Start()
        {
            lock (CicloDeVida)
            {
                if (!preparandoAtualizacao && !MonitoraPasta.fsw.Exists(watcher => !watcher.StopWatch))
                {
                    Iniciar();
                }
            }
        }

        private static void Iniciar()
        {
            ControleEncerramento.Reiniciar();
            Empresas.CarregaConfiguracao();

            #region Ticket #110

            Empresas.CreateLockFile(true);

            #endregion Ticket #110

            //Executar o monitoramento de pastas das empresas cadastradas
            MonitoraPasta e = new MonitoraPasta();

            Threads.Clear();

            //Executa a thread que faz a limpeza dos arquivos temporários
            Thread t = ControleThreads.Iniciar(new Processar().LimpezaTemporario);
            Threads.Add(t);

            //Executa a thread que faz a verificação das notas em processamento
            Thread t2 = ControleThreads.Iniciar(new Processar().EmProcessamento);
            Threads.Add(t2);

            //Executar a thread que faz a consulta do recibo das notas fiscais enviadas
            Processar srv = new Processar();
            Thread t3 = ControleThreads.Iniciar(() => srv.GerarXMLPedRec(new TaskNFeGerarXMLPedRec()));
            Threads.Add(t3);
        }
    }

    #endregion ThreadService
}
