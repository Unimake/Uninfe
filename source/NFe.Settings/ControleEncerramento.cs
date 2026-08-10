using System.Threading;

namespace NFe.Settings
{
    public static class ControleEncerramento
    {
        private static readonly ManualResetEventSlim Sinal = new ManualResetEventSlim(false);

        public static bool Solicitado => Sinal.IsSet;

        public static void Sinalizar() => Sinal.Set();

        public static void Reiniciar() => Sinal.Reset();

        public static bool Aguardar(int milissegundos) => Sinal.Wait(milissegundos);
    }
}
