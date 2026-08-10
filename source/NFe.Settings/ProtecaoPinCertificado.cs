using NFe.Components;
using System;
using System.Security.Cryptography;
using System.Text;

namespace NFe.Settings
{
    internal static class ProtecaoPinCertificado
    {
        internal const string Prefixo = "dpapi:v1:";
        private static readonly byte[] Entropia = Encoding.UTF8.GetBytes("UniNFe:CertificadoPIN:DPAPI:v1");

        internal static string Proteger(string pin)
        {
            if (string.IsNullOrEmpty(pin))
            {
                return string.Empty;
            }

            byte[] texto = null;
            byte[] protegido = null;
            try
            {
                texto = Encoding.UTF8.GetBytes(pin);
                protegido = ProtectedData.Protect(texto, Entropia, DataProtectionScope.LocalMachine);
                return Prefixo + Convert.ToBase64String(protegido);
            }
            finally
            {
                Limpar(texto);
                Limpar(protegido);
            }
        }

        internal static bool TryDesproteger(string valorPersistido, out string pin, out string mensagem)
        {
            pin = string.Empty;
            mensagem = string.Empty;

            if (string.IsNullOrEmpty(valorPersistido))
            {
                return true;
            }

            if (valorPersistido.StartsWith(Prefixo, StringComparison.Ordinal))
            {
                return TryDesprotegerDpapi(valorPersistido.Substring(Prefixo.Length), out pin, out mensagem);
            }

            if (EhTextoDeErroLegado(valorPersistido))
            {
                mensagem = "O PIN armazenado está inválido e deve ser informado novamente.";
                return false;
            }

            if (Criptografia.IsCriptografadaSenha(valorPersistido))
            {
                var valorLegado = Criptografia.descriptografaSenha(valorPersistido);
                if (EhTextoDeErroLegado(valorLegado))
                {
                    mensagem = "Não foi possível ler o PIN legado. Informe o PIN novamente.";
                    return false;
                }

                pin = valorLegado;
                return true;
            }

            // Compatibilidade temporária com configurações antigas que receberam texto puro.
            pin = valorPersistido;
            return true;
        }

        private static bool TryDesprotegerDpapi(string base64, out string pin, out string mensagem)
        {
            pin = string.Empty;
            mensagem = string.Empty;
            byte[] protegido = null;
            byte[] texto = null;
            try
            {
                protegido = Convert.FromBase64String(base64);
                texto = ProtectedData.Unprotect(protegido, Entropia, DataProtectionScope.LocalMachine);
                pin = Encoding.UTF8.GetString(texto);
                return true;
            }
            catch (Exception ex) when (ex is FormatException || ex is CryptographicException)
            {
                mensagem = "Não foi possível abrir o PIN protegido nesta máquina. Informe o PIN novamente.";
                return false;
            }
            finally
            {
                Limpar(protegido);
                Limpar(texto);
            }
        }

        private static bool EhTextoDeErroLegado(string valor)
        {
            return valor != null &&
                (valor.StartsWith("Wrong Input.", StringComparison.OrdinalIgnoreCase) ||
                 valor.StartsWith("Digite os valores Corretamente.", StringComparison.OrdinalIgnoreCase) ||
                 valor.StartsWith("String errada.", StringComparison.OrdinalIgnoreCase));
        }

        private static void Limpar(byte[] buffer)
        {
            if (buffer != null)
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }
}
