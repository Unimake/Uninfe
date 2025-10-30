using NFe.Components;
using NFe.Settings;
using NFe.Threadings;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Interop;

namespace UniNFeServico
{
    [ToolboxItem(false)]
    public partial class UniNFeService : ServiceBase
    {
        public UniNFeService() => InitializeComponent();

        protected override void OnStart(string[] args)
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif
            base.OnStart(args);
            WriteLog("Serviço iniciado na pasta: " + Propriedade.PastaExecutavel);
            iniciarServicosUniNFe();
        }

        protected override void OnStop()
        {
            base.OnStop();

            WriteLog("Serviço parado");
            pararServicosUniNFe();
        }

        protected override void OnShutdown()
        {
            pararServicosUniNFe();
            WriteLog("Serviço terminado");
            base.OnShutdown();
        }

        private void iniciarServicosUniNFe()
        {
            Thread.Sleep(30000); //Dar um tempo para o windows carregar outros serviços para que não falhe na hora de buscar algum arquivo em pasta. Wandrey 28/07/2022

            Propriedade.TipoAplicativo = TipoAplicativo.Todos;
            ConfiguracaoApp.StartVersoes();

            Empresas.CarregaConfiguracao(true);

            foreach (var empresa in Empresas.Configuracoes)
            {
                if (empresa.X509Certificado == null && empresa.UsaCertificado)
                {
                    var msg = "Não pode ler o certificado da empresa: " + empresa.CNPJ + "=>" + empresa.Nome + "=>" + empresa.Servico.ToString();

                    var f = Path.Combine(empresa.PastaXmlRetorno, "uninfeServico_" + DateTime.Now.ToString("yyyy-MMM-dd_hh-mm-ss") + ".err");
                    File.WriteAllText(f, msg);

                    WriteLog(msg);
                }
            }
            // Executar as conversões de atualizações de versão quando tiver
            Auxiliar.ConversaoNovaVersao(string.Empty);

            ThreadService.Start();

            new ThreadControlEvents();
        }

        private void pararServicosUniNFe()
        {
            ThreadService.Stop();
            Empresas.ClearLockFiles(false);
        }

        protected void WriteEventEntry(
            string Message,
            EventLogEntryType EventType,
            int ID,
            short Category)
        {
            // Select the log.
            EventLog.Log = "Application";

            // Define the source.
            EventLog.Source = ServiceName;

            // Write the log entry.
            EventLog.WriteEntry(Message, EventType, ID, Category);
        }

        protected override void OnCustomCommand(int command)
        {
            // Perform the custom command.
            if (command == 130)
            {
                WriteEventEntry("Executed Custom Command", EventLogEntryType.Information, 2000, 2);
            }
            // Perform the default action.
            base.OnCustomCommand(command);
        }

        private void WriteLog(string msg)
        {
            var o = ConfiguracaoApp.GravarLogOperacoesRealizadas;
            ConfiguracaoApp.GravarLogOperacoesRealizadas = true;
            Auxiliar.WriteLog(msg, false);
            ConfiguracaoApp.GravarLogOperacoesRealizadas = o;
        }
    }
}
