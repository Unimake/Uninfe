using NFe.Components;
using NFe.Settings;
using NFe.Threadings;
using System;
using System.IO;
using System.Timers;

namespace UniNFe.Service
{
    internal class UniNFeService
    {
        private readonly Timer _timer = null;
        public UniNFeService()
        {
            WriteLog("Aguarde até 3 minutos: o serviço ainda está inicializando e só depois poderá processar os arquivos da pasta de envio.");
            _timer = new Timer(TimeSpan.FromMinutes(3).TotalMilliseconds);
            _timer.Elapsed += _timer_Elapsed;
#if DEBUG
            Console.WriteLine(Directory.GetCurrentDirectory());
            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly());
            _timer_Elapsed(null, null);
#endif
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;

            WriteLog("Serviço iniciado na pasta: " + Propriedade.PastaExecutavel);
            IniciarServicosUniNFe();
        }

        public void Start() => _timer.Start();

        public void Stop()
        {
            WriteLog("Serviço parado - " + Propriedade.PastaExecutavel);
            PararServicosUniNFe();
            _timer.Stop();
        }

        public void OnShutdown()
        {
            WriteLog("Serviço terminado");
            PararServicosUniNFe();
            _timer.Stop();
        }

        private void IniciarServicosUniNFe()
        {
            Propriedade.TipoAplicativo = TipoAplicativo.Todos;
            ConfiguracaoApp.StartVersoes();

            Empresas.CarregaConfiguracao(true);

            Propriedade.VerificaArquivos(out var error, out var msg);
            if (error)
            {
                WriteLog(msg);
            }
            else
            {
                foreach (var empresa in Empresas.Configuracoes)
                {
                    if (empresa.X509Certificado == null && empresa.UsaCertificado)
                    {
                        msg = "Não pode ler o certificado da empresa: " + empresa.CNPJ + "=>" + empresa.Nome + "=>" + empresa.Servico.ToString();

                        var f = Path.Combine(empresa.PastaXmlRetorno, "uninfeServico_" + DateTime.Now.ToString("yyyy-MMM-dd_hh-mm-ss") + ".err");
                        File.WriteAllText(f, msg);

                        WriteLog(msg);
                    }
                }
            }

            if (!error)
            {
                // Executar as conversões de atualizações de versão quando tiver
                Auxiliar.ConversaoNovaVersao(string.Empty);

                ThreadService.Start();

                new ThreadControlEvents();
            }
            else
            {
                WriteLog("Serviço do UniNFe não está sendo executado.");
            }
        }

        private void PararServicosUniNFe()
        {
            ThreadService.Stop();
            Empresas.ClearLockFiles(false);
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
