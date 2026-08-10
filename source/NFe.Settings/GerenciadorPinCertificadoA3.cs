using System;
using System.Collections.Concurrent;
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
        bool IsA3(X509Certificate2 certificado);
        void SetPinPrivateKey(X509Certificate2 certificado, string pin);
    }

    internal sealed class ProvedorCertificadoA3 : IProvedorCertificadoA3
    {
        public bool IsA3(X509Certificate2 certificado) => certificado != null && certificado.IsA3();

        public void SetPinPrivateKey(X509Certificate2 certificado, string pin) => certificado.SetPinPrivateKey(pin);
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

            if (!DeveSerializar(empresa))
            {
                return Falha("O certificado atual não foi confirmado como A3. O PIN armazenado não será aplicado.");
            }

            var controle = Controles.GetOrAdd(empresa, _ => new Controle());
            lock (controle.Sincronizacao)
            {
                if (!tentativaExplicita && controle.Estado == EstadoPinCertificadoA3.Carregado)
                {
                    empresa.CertificadoPINCarregado = true;
                    return Sucesso(false);
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
                        empresa.X509Certificado = empresa.BuscaConfiguracaoCertificado();
                    }

                    if (empresa.X509Certificado == null)
                    {
                        return Falha("O certificado configurado não foi localizado.");
                    }

                    if (!empresa.X509Certificado.HasPrivateKey)
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
                    Provedor.SetPinPrivateKey(empresa.X509Certificado, empresa.CertificadoPIN);
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

        internal static bool DeveSerializar(Empresa empresa)
        {
            if (empresa == null || !empresa.UsaCertificado)
            {
                return false;
            }

            if (EhA3Conhecido(empresa))
            {
                return true;
            }

            try
            {
                if (Provedor.IsA3(empresa.X509Certificado))
                {
                    MarcarA3Conhecido(empresa);
                    return true;
                }
            }
            catch
            {
                // Uma consulta inconclusiva não deve apagar reconhecimento positivo anterior.
            }

            return EhA3Conhecido(empresa);
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

            return "O provedor criptográfico recusou ou não conseguiu configurar o PIN. Confira o token, o certificado e o middleware. Uma tentativa pode ter sido consumida.";
        }

        internal static void ReiniciarParaTestes()
        {
            Controles.Clear();
            CertificadosA3Conhecidos.Clear();
            Provedor = new ProvedorCertificadoA3();
        }
    }
}
