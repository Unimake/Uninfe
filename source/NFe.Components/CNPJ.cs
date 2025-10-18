using NFe.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NFe.Components
{
    public class CNPJ
    {
        #region Atributos

        private string mValue;
        #endregion Atributos

        #region Construtores

        private CNPJ(string cnpj)
        {
            if (cnpj.Length == 0) return;

            /*
                Retirado o OnlyNumbers para atender ao novo CNPJ Alfanumérico
            */
            cnpj = cnpj.RemoveChars('/', '-', ',', '.');//Functions.OnlyNumbers(cnpj, ".,-/").ToString();
            if (CNPJ.Validate(cnpj) == false) throw new ExceptionCNPJInvalido(cnpj);
            this.mValue = cnpj;
        }
        #endregion Construtores

        #region Public Operator

        public static implicit operator CNPJ(string cnpj)
        {
            return new CNPJ(cnpj);
        }

        #endregion Public Operator

        #region Overrides

        /// <summary>
        /// gravação de dados
        /// </summary>
        /// <param name="provider">CurrentCulture</param>
        /// <returns>somente os números</returns>
        public string ToString(IFormatProvider provider)
        {
            return Functions.OnlyNumbers(mValue, ".,-").ToString();
        }

        /// <summary>
        /// Converte para string.
        /// </summary>
        /// <returns>Retorna uma string formatada para o CNPJ</returns>
        public override string ToString()
        {
            if (mValue.HasOnlyNumbers())
            {
                return CNPJ.FormatCNPJ(mValue);
            }
            else
            {
                return mValue;
            }
        }

        #endregion Overrides

        #region Métodos estáticos

        #region FormatCNPJ

        /// <summary>
        /// formata o CNPJ
        /// </summary>
        /// <param name="cnpj">valor a ser formatado</param>
        /// <returns></returns>
        public static string FormatCNPJ(string cnpj)
        {
            if (!cnpj.HasOnlyNumbers())
            {
                return cnpj;
            }
            string ret = "";
            MaskedTextProvider mascara;
            cnpj = Functions.OnlyNumbers(cnpj, "-.,/").ToString();

            //cnpj
            //##.###.###/####-##
            mascara = new MaskedTextProvider(@"00\.000\.000/0000-00");
            mascara.Set(cnpj);
            ret = mascara.ToString();
            return ret;
        }

        #endregion FormatCNPJ

        #region Validate()

        /// <summary>
        /// valida o CNPJ
        /// </summary>
        /// <param name="cnpj"></param>
        /// <returns>true se for um CNPJ válido</returns>
        public static bool Validate(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj)) return false;

            cnpj = cnpj.RemoveChars('/', '-', ',', '.'); //Functions.OnlyNumbers(cnpj, "-.,/").ToString();

            try
            {
                if (cnpj.HasOnlyNumbers())
                {
                    return ValidateOnlyNumbers(cnpj);
                }
                else    //cnpj de teste do manual: 12abc34501de-35
                {
                    return ValidateAlphanumeric(cnpj);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Realiza a validação do CNPJ na versão somente com números
        /// </summary>
        /// <param name="cnpj">CNPJ a ser validado </param>
        /// <returns></returns>
        private static bool ValidateOnlyNumbers(string cnpj)
        {
            #region Valida CNPJ tradicional

            string Cnpj_1 = cnpj.Substring(0, 12);
            string Cnpj_2 = cnpj.Substring(cnpj.Length - 2);
            string Mult = "543298765432";
            string Controle = String.Empty;
            int Soma = 0;
            int Digito = 0;

            for (int j = 1; j < 3; j++)
            {
                Soma = 0;

                for (int i = 0; i < 12; i++)
                    Soma += int.Parse(Cnpj_1.Substring(i, 1)) * int.Parse(Mult.Substring(i, 1));

                if (j == 2) Soma += (2 * Digito);
                Digito = ((Soma * 10) % 11);
                if (Digito == 10) Digito = 0;
                Controle = Controle + Digito.ToString();
                Mult = "654329876543";
            }

            if (Controle != Cnpj_2)
            {
                return false;
            }
            else
            {
                return true;
            }

            #endregion Valida CNPJ tradicional
        }

        #region Validate alfanumérico

        private static readonly Dictionary<char, int> Valores = new Dictionary<char, int>();
        private static readonly List<char> CaracteresValidos = new List<char>("abcdefghijklmnopqrstuvwxyz".ToCharArray());
        private static void ValidadorCnpjAlfanumerico()
        {
            // Atribuindo os valores de 0 até 9 
            for (int i = 0; i <= 9; i++)
            {
                Valores.Add(i.ToString()[0], i);
            }
            // Atribundo os valores de a até z
            for (int i = 65; i <= 90; i++)
            {
                Valores.Add(CaracteresValidos[i - 65], i - 48);
            }
        }

        public static int CalcularDigitoVerificador(string cnpj, bool segundoDigito = false)
        {
            if (Valores.IsNullOrEmpty())
            {
                ValidadorCnpjAlfanumerico(); // iniciar valores
            }

            List<int> pesos = (segundoDigito) ? new List<int> { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 } : new List<int> { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var soma = cnpj.Select(c => Valores[c]).Zip(pesos, (valor, peso) => valor * peso);

            int resto = soma.Sum() % 11;
            return (resto < 2) ? 0 : 11 - resto;
        }

        public static bool ValidateAlphanumeric(string cnpj)
        {
            if (cnpj.Length != 14)
            {
                return false;
            }

            string cnpjBase = cnpj.Substring(0, 12);
            int primeiroDigito = int.Parse(cnpj.Substring(12, 1));
            int segundoDigito = int.Parse(cnpj.Substring(13, 1));

            return primeiroDigito == CalcularDigitoVerificador(cnpjBase) && segundoDigito == CalcularDigitoVerificador(cnpjBase + primeiroDigito, true);
        }

        #endregion Validade alfanumérico

        #endregion Validate()

        #endregion Métodos estáticos
    }
}

namespace NFe.Exceptions
{
    /// <summary>
    /// CNPJ não é válido.
    /// </summary>
    public class ExceptionCNPJInvalido : Exception
    {
        private string mCnpj = "";

        public ExceptionCNPJInvalido(string cnpj)
        {
            mCnpj = cnpj;
        }

        public override string Message
        {
            get
            {
                return "O CNPJ informado não é válido\nCNPJ: " + mCnpj;
            }
        }
    }
}