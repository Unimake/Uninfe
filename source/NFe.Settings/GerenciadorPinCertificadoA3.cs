using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Unimake.Business.DFe.Security;

namespace NFe.Settings
{
    internal enum EstadoPinCertificadoA3
    {
        NaoTentado,
        Carregado,
        Falhou,
        Invalidado
    }

    internal interface IProvedorCertificadoA3
    {
        bool HasCachedPrivateKeyContext(X509Certificate2 certificado);
        bool IsA3(X509Certificate2 certificado);
        void SetPinPrivateKey(X509Certificate2 certificado, string pin);
    }

    internal sealed class ProvedorCertificadoA3 : IProvedorCertificadoA3
    {
        public bool HasCachedPrivateKeyContext(X509Certificate2 certificado)
        {
            if (certificado == null)
            {
                return false;
            }

            try
            {
                return HasCertificateProperty(certificado, 1) ||
                    HasCertificateProperty(certificado, 5) ||
                    HasCertificateProperty(certificado, 78);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsA3(X509Certificate2 certificado) => certificado != null && certificado.IsA3();

        public void SetPinPrivateKey(X509Certificate2 certificado, string pin) => certificado.SetPinPrivateKey(pin);

        private static bool HasCertificateProperty(X509Certificate2 certificado, int propertyId)
        {
            var size = 0u;
            return CertGetCertificateContextProperty(certificado.Handle, propertyId, IntPtr.Zero, ref size);
        }

        [DllImport("crypt32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CertGetCertificateContextProperty(
            IntPtr certificateContext,
            int propertyId,
            IntPtr data,
            ref uint dataSize);
    }

    public sealed class ResultadoCarregamentoPinA3
    {
        public bool Sucesso { get; internal set; }
        public bool TentativaExecutada { get; internal set; }
        public bool PodeContinuarSemAutomacao { get; internal set; }
        public string Mensagem { get; internal set; }
        public Exception Excecao { get; internal set; }
    }

    internal static class GerenciadorPinCertificadoA3
    {
        private sealed class Controle
        {
            internal readonly object Sincronizacao = new object();
            internal EstadoPinCertificadoA3 Estado = EstadoPinCertificadoA3.NaoTentado;
        }

        private static readonly ConcurrentDictionary<Empresa, Controle> Controles = new ConcurrentDictionary<Empresa, Controle>();
        private static readonly ConcurrentDictionary<string, byte> CertificadosA3Conhecidos = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private static readonly object SincronizacaoIdentificacao = new object();
        internal static IProvedorCertificadoA3 Provedor { get; set; } = new ProvedorCertificadoA3();

        internal static ResultadoCarregamentoPinA3 Carregar(Empresa empresa, bool tentativaExplicita)
        {
            if (empresa == null)
            {
                return Falha("A empresa não foi informada para carregar o PIN do certificado.");
            }

            if (!empresa.UsaCertificado || string.IsNullOrWhiteSpace(empresa.CertificadoPIN))
            {
                return Falha("Não há PIN de certificado configurado para esta empresa.");
            }

            DiagnosticoPinCertificadoA3.Registrar("Certificado em memória: " + (empresa.X509Certificado != null) +
                "; certificado instalado: " + empresa.CertificadoInstalado + ".");
            if (!DiagnosticoPinCertificadoA3.Executar("Confirmação de certificado A3", () => DeveSerializar(empresa)))
            {
                DiagnosticoPinCertificadoA3.Registrar("A3 não confirmado; aplicação do PIN não executada.");
                return Falha("O certificado atual não foi confirmado como A3. O PIN armazenado não será aplicado.");
            }

            var controle = Controles.GetOrAdd(empresa, _ => new Controle());
            DiagnosticoPinCertificadoA3.Registrar("Aguardando exclusividade para carregar o PIN.");
            lock (controle.Sincronizacao)
            {
                DiagnosticoPinCertificadoA3.Registrar("Exclusividade para carregar o PIN obtida.");
                if (!tentativaExplicita && controle.Estado == EstadoPinCertificadoA3.Carregado)
                {
                    if (Provedor.HasCachedPrivateKeyContext(empresa.X509Certificado))
                    {
                        empresa.CertificadoPINCarregado = true;
                        return Sucesso(false);
                    }

                    controle.Estado = EstadoPinCertificadoA3.Invalidado;
                    empresa.CertificadoPINCarregado = false;
                }

                if (!tentativaExplicita && controle.Estado == EstadoPinCertificadoA3.Falhou)
                {
                    empresa.CertificadoPINCarregado = false;
                    return FalhaAutomacao("O carregamento automático do PIN já falhou nesta configuração e não será repetido.", false, null);
                }

                try
                {
                    if (empresa.X509Certificado == null)
                    {
                        empresa.X509Certificado = DiagnosticoPinCertificadoA3.Executar("Busca do certificado configurado",
                            empresa.BuscaConfiguracaoCertificado);
                    }

                    if (empresa.X509Certificado == null)
                    {
                        return Falha("O certificado configurado não foi localizado.");
                    }

                    if (!DiagnosticoPinCertificadoA3.Executar("Verificação de presença da chave privada",
                        () => empresa.X509Certificado.HasPrivateKey))
                    {
                        return Falha("O certificado selecionado não possui uma chave privada disponível.");
                    }
                }
                catch (Exception ex)
                {
                    empresa.CertificadoPINCarregado = false;
                    return new ResultadoCarregamentoPinA3
                    {
                        Sucesso = false,
                        TentativaExecutada = false,
                        PodeContinuarSemAutomacao = false,
                        Mensagem = "Não foi possível preparar o certificado configurado.",
                        Excecao = ex
                    };
                }

                try
                {
                    DiagnosticoPinCertificadoA3.Executar("Aplicação do PIN e preparação da chave privada (SetPinPrivateKey)",
                        () => Provedor.SetPinPrivateKey(empresa.X509Certificado, empresa.CertificadoPIN));
                    controle.Estado = EstadoPinCertificadoA3.Carregado;
                    empresa.CertificadoPINCarregado = true;
                    MarcarA3Conhecido(empresa);
                    return Sucesso(true);
                }
                catch (Exception ex)
                {
                    controle.Estado = EstadoPinCertificadoA3.Falhou;
                    empresa.CertificadoPINCarregado = false;
                    return FalhaAutomacao(ClassificarFalha(ex), true, ex);
                }
            }
        }

        internal static void Invalidar(Empresa empresa)
        {
            if (empresa == null)
            {
                return;
            }

            Controle controle;
            if (Controles.TryGetValue(empresa, out controle))
            {
                lock (controle.Sincronizacao)
                {
                    controle.Estado = EstadoPinCertificadoA3.Invalidado;
                }
            }

            empresa.CertificadoPINCarregado = false;
        }

        internal static void Liberar(Empresa empresa)
        {
            if (empresa == null)
            {
                return;
            }

            Controle controle;
            if (Controles.TryRemove(empresa, out controle))
            {
                lock (controle.Sincronizacao)
                {
                    controle.Estado = EstadoPinCertificadoA3.Invalidado;
                }
            }

            empresa.CertificadoPINCarregado = false;
        }

        internal static bool DeveSerializar(Empresa empresa)
        {
            if (empresa == null || !empresa.UsaCertificado)
            {
                return false;
            }

            DiagnosticoPinCertificadoA3.Registrar("Aguardando exclusividade para identificar o certificado.");
            lock (SincronizacaoIdentificacao)
            {
                DiagnosticoPinCertificadoA3.Registrar("Exclusividade para identificar o certificado obtida.");
                if (EhA3Conhecido(empresa))
                {
                    DiagnosticoPinCertificadoA3.Registrar("A3 confirmado pelo reconhecimento anterior; consulta à DLL dispensada.");
                    return true;
                }

                try
                {
                    var ehA3 = DiagnosticoPinCertificadoA3.Executar("Identificação do tipo pela DLL (IsA3)",
                        () => Provedor.IsA3(empresa.X509Certificado));
                    DiagnosticoPinCertificadoA3.Registrar("Resultado de IsA3: " + ehA3 +
                        (ehA3 ? "." : ". A API atual não informa o motivo nem códigos de falhas internas quando retorna false."));
                    if (ehA3)
                    {
                        MarcarA3Conhecido(empresa);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    DiagnosticoPinCertificadoA3.RegistrarExcecao(ex);
                    // Uma consulta inconclusiva não deve apagar reconhecimento positivo anterior.
                }

                return EhA3Conhecido(empresa);
            }
        }

        private static void MarcarA3Conhecido(Empresa empresa)
        {
            var identidade = IdentidadeCertificado(empresa);
            if (!string.IsNullOrEmpty(identidade))
            {
                CertificadosA3Conhecidos[identidade] = 0;
            }
        }

        private static bool EhA3Conhecido(Empresa empresa)
        {
            var identidade = IdentidadeCertificado(empresa);
            return !string.IsNullOrEmpty(identidade) && CertificadosA3Conhecidos.ContainsKey(identidade);
        }

        private static string IdentidadeCertificado(Empresa empresa)
        {
            if (!string.IsNullOrWhiteSpace(empresa.CertificadoDigitalThumbPrint))
            {
                return empresa.CertificadoDigitalThumbPrint.Replace(" ", string.Empty);
            }

            try
            {
                return empresa.X509Certificado?.Thumbprint?.Replace(" ", string.Empty);
            }
            catch
            {
                return null;
            }
        }

        private static ResultadoCarregamentoPinA3 Sucesso(bool tentou) => new ResultadoCarregamentoPinA3
        {
            Sucesso = true,
            TentativaExecutada = tentou,
            PodeContinuarSemAutomacao = false,
            Mensagem = string.Empty
        };

        private static ResultadoCarregamentoPinA3 Falha(string mensagem) => new ResultadoCarregamentoPinA3
        {
            Sucesso = false,
            TentativaExecutada = false,
            PodeContinuarSemAutomacao = false,
            Mensagem = mensagem
        };

        private static ResultadoCarregamentoPinA3 FalhaAutomacao(string mensagem, bool tentou, Exception excecao) => new ResultadoCarregamentoPinA3
        {
            Sucesso = false,
            TentativaExecutada = tentou,
            PodeContinuarSemAutomacao = true,
            Mensagem = mensagem,
            Excecao = excecao
        };

        private static string ClassificarFalha(Exception ex)
        {
            if (ex is InvalidOperationException)
            {
                return ex.Message;
            }

            const string mensagem = "O provedor criptográfico recusou ou não conseguiu configurar o PIN. Confira o token, o certificado e o middleware. Uma tentativa pode ter sido consumida.";

            if (ex is Win32Exception erroNativo)
            {
                return $"{mensagem} Detalhes do provedor: {erroNativo.Message} (código 0x{erroNativo.NativeErrorCode:X8}).";
            }

            if (ex is CryptographicException erroCriptografico)
            {
                return $"{mensagem} Detalhes criptográficos: {erroCriptografico.Message} (código 0x{erroCriptografico.HResult:X8}).";
            }

            return mensagem;
        }

        internal static void ReiniciarParaTestes()
        {
            Controles.Clear();
            CertificadosA3Conhecidos.Clear();
            Provedor = new ProvedorCertificadoA3();
        }

        internal static int QuantidadeControlesParaTestes => Controles.Count;
    }
}
