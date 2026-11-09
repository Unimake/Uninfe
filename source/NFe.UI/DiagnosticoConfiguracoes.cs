using NFe.Settings;
using System;
using System.Diagnostics;
using System.Threading;

namespace NFe.UI
{
    /// <summary>
    /// Registra etapas síncronas somente durante a abertura da tela de configurações.
    /// Não registra valores dos controles, documentos, caminhos ou dados do certificado.
    /// </summary>
    internal static class DiagnosticoConfiguracoes
    {
        [ThreadStatic]
        private static bool ativo;

        private static int sequencia;

        internal static void Abrir(Action acao)
        {
            var anterior = ativo;
            ativo = true;
            try
            {
                Registrar("Ambiente: " + Environment.OSVersion.VersionString +
                    "; CLR: " + Environment.Version + "; processo: " + (IntPtr.Size * 8) +
                    " bits; empresas cadastradas: " + Empresas.Configuracoes.Count + ".");
                Executar("Abertura da tela de configurações", acao);
            }
            finally
            {
                ativo = anterior;
            }
        }

        internal static void Executar(string etapa, Action acao)
        {
            Executar<object>(etapa, () =>
            {
                acao();
                return null;
            });
        }

        internal static T Executar<T>(string etapa, Func<T> acao)
        {
            if (!ativo)
                return acao();

            var id = Interlocked.Increment(ref sequencia);
            Registrar("INÍCIO #" + id + " - " + etapa);
            var tempo = Stopwatch.StartNew();
            T resultado;
            try
            {
                resultado = acao();
            }
            catch (Exception ex)
            {
                Registrar("FALHA #" + id + " - " + etapa + "; tempo: " + tempo.ElapsedMilliseconds +
                    " ms; tipo: " + ex.GetType().FullName + ".");
                throw;
            }

            Registrar("FIM #" + id + " - " + etapa + "; tempo: " + tempo.ElapsedMilliseconds + " ms.");
            return resultado;
        }

        internal static void Registrar(string mensagem)
        {
            if (ativo)
            {
                // Forçar a gravação: o diagnóstico precisa funcionar sem acessar as configurações.
                Auxiliar.WriteLog("[Diagnóstico configurações] " + mensagem, false, true);
            }
        }
    }
}
