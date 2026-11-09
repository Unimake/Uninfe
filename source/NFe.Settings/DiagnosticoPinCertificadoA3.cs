using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace NFe.Settings
{
    /// <summary>
    /// Diagnóstico síncrono restrito ao teste explícito do PIN, sem registrar credenciais.
    /// </summary>
    internal static class DiagnosticoPinCertificadoA3
    {
        [ThreadStatic]
        private static bool ativo;

        private static int sequencia;

        internal static void IniciarTeste(Action acao)
        {
            var anterior = ativo;
            ativo = true;
            try
            {
                Registrar("Ambiente: " + Environment.OSVersion.VersionString +
                    "; CLR: " + Environment.Version + "; processo: " + (IntPtr.Size * 8) +
                    " bits; DLL DFe: " + typeof(Unimake.Business.DFe.Security.ClsX509Certificate2Extension).Assembly.GetName().Version + ".");
                Executar("Teste explícito do PIN A3", acao);
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
                Registrar("FALHA #" + id + " - " + etapa + "; tempo: " + tempo.ElapsedMilliseconds + " ms.");
                RegistrarExcecao(ex);
                throw;
            }

            Registrar("FIM #" + id + " - " + etapa + "; tempo: " + tempo.ElapsedMilliseconds + " ms.");
            return resultado;
        }

        internal static void RegistrarExcecao(Exception excecao)
        {
            // Somente tipos e códigos: mensagens de provedores podem conter dados sensíveis.
            for (var nivel = 0; excecao != null && nivel < 4; nivel++, excecao = excecao.InnerException)
            {
                var erroNativo = excecao as Win32Exception;
                Registrar("Exceção (nível " + nivel + "): " + excecao.GetType().FullName +
                    "; HRESULT: 0x" + excecao.HResult.ToString("X8") +
                    (erroNativo == null ? "" : "; código nativo: 0x" + erroNativo.NativeErrorCode.ToString("X8")) + ".");
            }
        }

        internal static void Registrar(string mensagem)
        {
            if (!ativo)
                return;

            try
            {
                Auxiliar.WriteLog("[Diagnóstico PIN A3] " + mensagem, false, true);
            }
            catch (Exception ex)
            {
                // Uma falha no diagnóstico não pode alterar o resultado do teste do PIN.
                Debug.WriteLine("Falha ao gravar diagnóstico do PIN A3: " + ex.GetType().FullName);
            }
        }
    }
}
