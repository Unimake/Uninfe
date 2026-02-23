using System;
using System.Security.Cryptography;
using System.Text;

namespace NFe.Components
{
    public static class Criptografia
    {
        private static string _chave = "unimake_uninfe";

        public static string criptografaSenha(string senhaCripto)
        {
            try
            {
                if (String.IsNullOrEmpty(senhaCripto))
                    return "";
                else
                    return criptografaSenha(senhaCripto, _chave);
            }
            catch (Exception ex)
            {
                return "String errada. " + ex.Message;
            }

        }

        public static string descriptografaSenha(string senhaDescripto)
        {
            try
            {
                if (String.IsNullOrEmpty(senhaDescripto))
                    return "";
                else
                    if (IsCriptografadaSenha(senhaDescripto))
                        return descriptografaSenha(senhaDescripto, _chave);
                    else
                        return senhaDescripto;
            }
            catch (Exception ex)
            {
                return "Wrong Input. " + ex.Message;
            }
        }

        public static string criptografaSenha(string senhaCripto, string chave)
        {
            try
            {
                TripleDESCryptoServiceProvider objcriptografaSenha = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objcriptoMd5 = new MD5CryptoServiceProvider();

                byte[] byteHash, byteBuff;
                string strTempKey = chave;

                byteHash = objcriptoMd5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strTempKey));
                objcriptoMd5 = null;
                objcriptografaSenha.Key = byteHash;
                objcriptografaSenha.Mode = CipherMode.ECB;

                byteBuff = System.Text.Encoding.UTF8.GetBytes(senhaCripto);
                return Convert.ToBase64String(objcriptografaSenha.CreateEncryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            }
            catch (Exception ex)
            {
                return "Digite os valores Corretamente." + ex.Message;
            }
        }

        public static string descriptografaSenha(string strCriptografada, string chave)
        {
            try
            {
                TripleDESCryptoServiceProvider objdescriptografaSenha = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objcriptoMd5 = new MD5CryptoServiceProvider();

                byte[] byteHash, byteBuff;
                string strTempKey = chave;

                byteHash = objcriptoMd5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strTempKey));
                objcriptoMd5 = null;
                objdescriptografaSenha.Key = byteHash;
                objdescriptografaSenha.Mode = CipherMode.ECB;

                byteBuff = Convert.FromBase64String(strCriptografada);
                string strDecrypted = System.Text.Encoding.UTF8.GetString(objdescriptografaSenha.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                objdescriptografaSenha = null;

                return strDecrypted;
            }
            catch (Exception ex)
            {
                return "Digite os valores Corretamente." + ex.Message;
            }
        }

        /// <summary>
        /// Metodo que verifica se a string encontra-se criptografada, pode ser utilizada
        /// antes de se tentar descriptografar uma senha evitando exceções na aplicação.
        /// </summary>
        /// <param name="senhaCripto">string com a senha</param>
        /// <returns>booleano que se a senha esta criptografda</returns>
        /// <author>Renan Borges</author>
        public static bool IsCriptografadaSenha(string senhaCripto)
        {
            try
            {
                if (String.IsNullOrEmpty(senhaCripto))
                    return false;
                else
                    return IsCriptografadaSenha(senhaCripto, _chave);
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Metodo que verifica se a string encontra-se criptografada, pode ser utilizada
        /// antes de se tentar descriptografar uma senha evitando exceções na aplicação.
        /// </summary>
        /// <param name="senhaCripto">string com a senha</param>
        /// <returns>booleano que se a senha esta criptografda</returns>
        /// <author>Renan Borges</author>
        public static bool IsCriptografadaSenha(string strCriptografada, string chave)
        {
            try
            {
                TripleDESCryptoServiceProvider objdescriptografaSenha = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objcriptoMd5 = new MD5CryptoServiceProvider();

                byte[] byteHash, byteBuff;
                string strTempKey = chave;

                byteHash = objcriptoMd5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(strTempKey));
                objcriptoMd5 = null;
                objdescriptografaSenha.Key = byteHash;
                objdescriptografaSenha.Mode = CipherMode.ECB;

                byteBuff = Convert.FromBase64String(strCriptografada);
                string strDecrypted = System.Text.Encoding.UTF8.GetString(objdescriptografaSenha.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                objdescriptografaSenha = null;

                return true;
            }
            catch
            {
                return false;
            }
        }


        public static string GetSHA1HashData(string data)
        {
            return GetSHA1HashData(data, false);
        }

        public static string GetSHA1HashData(string data, bool toUpper)
        {
            HashAlgorithm algorithm = new SHA1CryptoServiceProvider();
            byte[] buffer = algorithm.ComputeHash(System.Text.Encoding.ASCII.GetBytes(data));
            System.Text.StringBuilder builder = new System.Text.StringBuilder(buffer.Length);
            foreach (byte num in buffer)
            {
                if (toUpper)
                    builder.Append(num.ToString("X2"));
                else
                    builder.Append(num.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}