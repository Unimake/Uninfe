using System;
using System.Threading;

namespace NFe.Settings
{
    internal static class PreparadorCertificadoA3
    {
        internal static ResultadoCarregamentoPinA3 Preparar(Empresa empresa, bool permitirFallbackInterativo)
        {
            if (empresa == null || !empresa.UsaCertificado || string.IsNullOrWhiteSpace(empresa.CertificadoPIN))
            {
                return SucessoSemTentativa();
            }

            if (!empresa.DeveSerializarOperacaoA3())
            {
                return SucessoSemTentativa();
            }

            var resultado = empresa.CarregarPinCertificadoA3(false);
            if (!resultado.Sucesso && resultado.PodeContinuarSemAutomacao && !permitirFallbackInterativo)
            {
                resultado = new ResultadoCarregamentoPinA3
                {
                    Sucesso = false,
                    TentativaExecutada = resultado.TentativaExecutada,
                    PodeContinuarSemAutomacao = false,
                    Mensagem = resultado.Mensagem + " A execução não é interativa e não pode solicitar o PIN ao middleware.",
                    Excecao = resultado.Excecao
                };
            }

            return resultado;
        }

        internal static void PrepararOuLancar(Empresa empresa, bool permitirFallbackInterativo)
        {
            var resultado = Preparar(empresa, permitirFallbackInterativo);
            if (!resultado.Sucesso && !resultado.PodeContinuarSemAutomacao)
            {
                throw new InvalidOperationException(resultado.Mensagem, resultado.Excecao);
            }
        }

        private static ResultadoCarregamentoPinA3 SucessoSemTentativa()
        {
            return new ResultadoCarregamentoPinA3
            {
                Sucesso = true,
                TentativaExecutada = false,
                PodeContinuarSemAutomacao = false,
                Mensagem = string.Empty
            };
        }
    }

    internal static class CoordenadorOperacaoCertificadoA3
    {
        private sealed class Liberacao : IDisposable
        {
            private int liberado;

            public void Dispose()
            {
                if (Interlocked.Exchange(ref liberado, 1) != 0)
                {
                    return;
                }

                Semaforo.Release();
            }
        }

        private sealed class SemOperacao : IDisposable
        {
            internal static readonly SemOperacao Instancia = new SemOperacao();

            public void Dispose()
            {
            }
        }

        private static readonly SemaphoreSlim Semaforo = new SemaphoreSlim(1, 1);

        internal static IDisposable Entrar(Empresa empresa)
        {
            if (empresa == null || !empresa.DeveSerializarOperacaoA3())
            {
                return SemOperacao.Instancia;
            }

            Semaforo.Wait();
            return new Liberacao();
        }
    }
}
