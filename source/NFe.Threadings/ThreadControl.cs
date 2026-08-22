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
        private static readonly SemaphoreSlim SemaforoA3 = new SemaphoreSlim(1, 1);

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
            var serializarA3 = Empresa >= 0 && Empresa < Empresas.Configuracoes.Count &&
                Empresas.Configuracoes[Empresa].DeveSerializarOperacaoA3();
            if (serializarA3)
            {
                SemaforoA3.Wait();
                try
                {
                    Processar(this);
                }
                finally
                {
                    SemaforoA3.Release();
                }

                return;
            }

            Processar(this);
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

    #region ThreadService

    /// <summary>
    /// Classe responsável por executar as thread´s base de verificação dos serviços a serem executados
    /// </summary>
    public static class ThreadService
    {
        public static NotifyIcon NotifyIconUniNFe;
        public static List<Thread> Threads = new List<Thread>();

        public static void Stop()
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
            ControleEncerramento.Reiniciar();
            Empresas.CarregaConfiguracao();

            #region Ticket #110

            Empresas.CreateLockFile(true);

            #endregion Ticket #110

            //Executar o monitoramento de pastas das empresas cadastradas
            MonitoraPasta e = new MonitoraPasta();

            Threads.Clear();

            //Executa a thread que faz a limpeza dos arquivos temporários
            Thread t = new Thread(new Processar().LimpezaTemporario);
            t.IsBackground = true;
            t.Start();
            Threads.Add(t);

            //Executa a thread que faz a verificação das notas em processamento
            Thread t2 = new Thread(new Processar().EmProcessamento);
            t2.IsBackground = true;
            t2.Start();
            Threads.Add(t2);

            //Executar a thread que faz a consulta do recibo das notas fiscais enviadas
            Processar srv = new Processar();
            Thread t3 = new Thread(srv.GerarXMLPedRec);
            t3.IsBackground = true;
            t3.Start(new TaskNFeGerarXMLPedRec());
            Threads.Add(t3);
        }
    }

    #endregion ThreadService
}
