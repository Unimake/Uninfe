using NFe.Components;
using NFe.Settings;
using NFe.Threadings;
using System;
using System.IO;
using System.Timers;

namespace UniNFe.Service
{
    internal class UniNFeService : IDisposable
    {
        private readonly Timer _timer;
        private bool _disposed;
        private bool _servicosIniciados;
        private int _tentativas;
        private const int MaxTentativas = 18; // 3 minutos em intervalos de 10 segundos
        private const int IntervaloVerificacaoMs = 10000; // 10 segundos

        public UniNFeService()
        {
            Program.WriteLog("Aguardando dependências do sistema para iniciar o processamento dos arquivos de envio.");
            _timer = new Timer(IntervaloVerificacaoMs);
            _timer.Elapsed += _timer_Elapsed;
#if DEBUG
            Program.WriteLog($"[DEBUG] Diretório atual: {Directory.GetCurrentDirectory()}");
            Program.WriteLog($"[DEBUG] Assembly: {System.Reflection.Assembly.GetExecutingAssembly()}");
            _timer_Elapsed(null, null);
#endif
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_servicosIniciados || _disposed)
                return;

            _tentativas++;
            if (DependenciasProntas())
            {
                _timer.Enabled = false;
                _servicosIniciados = true;
                Program.WriteLog("Serviço iniciado na pasta: " + Propriedade.PastaExecutavel);
                IniciarServicosUniNFe();
            }
            else if (_tentativas >= MaxTentativas)
            {
                _timer.Enabled = false;
                Program.WriteLog("Dependências não prontas após tempo máximo de espera. Iniciando serviço mesmo assim.");
                _servicosIniciados = true;
                IniciarServicosUniNFe();
            }
            else
            {
                Program.WriteLog($"Aguardando dependências... Tentativa {_tentativas}/{MaxTentativas}");
            }
        }

        private bool DependenciasProntas()
        {
            // Exemplo: Verifica se a pasta de envio existe e está acessível
            try
            {
                // Adicione aqui outras verificações necessárias (ex: rede, certificado, etc)
                return Directory.Exists(Propriedade.PastaExecutavel);
            }
            catch
            {
                return false;
            }
        }

        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(UniNFeService));
            _timer.Start();
        }

        public void Stop()
        {
            Program.WriteLog("Serviço parado - " + Propriedade.PastaExecutavel);
            PararServicosUniNFe();
            _timer.Stop();
        }

        public void OnShutdown()
        {
            Program.WriteLog("Serviço encerrado.");
            PararServicosUniNFe();
            _timer.Stop();
        }

        private void IniciarServicosUniNFe()
        {
            Propriedade.TipoAplicativo = TipoAplicativo.Todos;
            ConfiguracaoApp.StartVersoes();
            Empresas.CarregaConfiguracao(true);

            foreach (var empresa in Empresas.Configuracoes)
            {
                if (empresa.X509Certificado == null && empresa.UsaCertificado)
                {
                    var msg = $"Não pode ler o certificado da empresa: {empresa.CNPJ} => {empresa.Nome} => {empresa.Servico}";
                    var f = Path.Combine(empresa.PastaXmlRetorno, $"uninfeServico_{DateTime.Now:yyyy-MMM-dd_hh-mm-ss}.err");
                    File.WriteAllText(f, msg);
                    Program.WriteLog(msg);
                }
            }

            Auxiliar.ConversaoNovaVersao(string.Empty);
            ThreadService.Start();
            new ThreadControlEvents();
        }

        private void PararServicosUniNFe()
        {
            ThreadService.Stop();
            Empresas.ClearLockFiles(false);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer?.Dispose();
                _disposed = true;
            }
        }
    }
}