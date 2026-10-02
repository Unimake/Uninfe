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
                Program.WriteLog("Iniciando processamento do serviço na pasta: " + Propriedade.PastaExecutavel);
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
                var acessivel = Directory.Exists(Propriedade.PastaExecutavel);
                Program.WriteLog($"[Inicialização do serviço] Verificação da pasta local '{Propriedade.PastaExecutavel}': {acessivel}. " +
                    "Esta verificação não testa o acesso às pastas de rede das empresas.");
                return acessivel;
            }
            catch (Exception ex)
            {
                Program.WriteLog($"[Inicialização do serviço] Falha na verificação da pasta local: {ex}");
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
            var etapa = "Carregar versões";
            try
            {
                Propriedade.TipoAplicativo = TipoAplicativo.Todos;
                Program.WriteLog("[Inicialização do serviço] " + etapa + ".");
                ConfiguracaoApp.StartVersoes();
                etapa = "Carregar configurações das empresas";
                Program.WriteLog("[Inicialização do serviço] " + etapa + ".");
                Empresas.CarregaConfiguracao(true);
                Program.WriteLog($"[Inicialização do serviço] Empresas carregadas: {Empresas.Configuracoes.Count}; " +
                    $"há erro de diretório: {Empresas.ExisteErroDiretorio}.");

                foreach (var empresa in Empresas.Configuracoes)
                {
                    etapa = "Verificar configuração da empresa " + empresa.CNPJ;
                    Program.WriteLog($"[Inicialização do serviço] Empresa: {empresa.CNPJ}; serviço: {empresa.Servico}; " +
                        $"usa certificado: {empresa.UsaCertificado}; certificado instalado no Windows: {empresa.CertificadoInstalado}; " +
                        $"certificado carregado: {empresa.X509Certificado != null}.");
                    RegistrarPasta(empresa.CNPJ, "Envio", empresa.PastaXmlEnvio);
                    RegistrarPasta(empresa.CNPJ, "Retorno", empresa.PastaXmlRetorno);
                    RegistrarPasta(empresa.CNPJ, "Erro", empresa.PastaXmlErro);

                    if (empresa.X509Certificado == null && empresa.UsaCertificado)
                    {
                        var msg = $"Não pode ler o certificado da empresa: {empresa.CNPJ} => {empresa.Nome} => {empresa.Servico}";
                        var f = Path.Combine(empresa.PastaXmlRetorno, $"uninfeServico_{DateTime.Now:yyyy-MMM-dd_hh-mm-ss}.err");
                        etapa = "Gravar aviso de certificado no arquivo " + f;
                        Program.WriteLog($"[Inicialização do serviço] {msg}. Tentando gravar aviso em '{f}'.");
                        File.WriteAllText(f, msg);
                        Program.WriteLog(msg);
                    }
                }

                etapa = "Executar conversões de atualização";
                Program.WriteLog("[Inicialização do serviço] " + etapa + ".");
                Auxiliar.ConversaoNovaVersao(string.Empty);
                etapa = "Iniciar threads de processamento";
                Program.WriteLog("[Inicialização do serviço] " + etapa + ".");
                ThreadService.Start();
                etapa = "Iniciar controle de eventos";
                Program.WriteLog("[Inicialização do serviço] " + etapa + ".");
                new ThreadControlEvents();
                Program.WriteLog("[Inicialização do serviço] Inicialização do processamento concluída.");
            }
            catch (Exception ex)
            {
                Program.WriteLog($"[Inicialização do serviço] Inicialização interrompida. Etapa: {etapa}; " +
                    $"HRESULT: 0x{ex.HResult:X8}. Exceção original: {ex}");
                throw;
            }
        }

        private static void RegistrarPasta(string cnpj, string finalidade, string pasta)
        {
            Program.WriteLog($"[Inicialização do serviço] Empresa: {cnpj}; pasta de {finalidade}: '{pasta}'; " +
                $"Directory.Exists: {Directory.Exists(pasta)}. O resultado não comprova permissão de gravação.");
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
