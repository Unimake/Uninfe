using NFe.Exceptions;
using System;
using System.ComponentModel;
using Unimake.Validators;

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
            try
            {
                return CNPJValidator.Validate(ref cnpj, false);
            }
            catch
            {
                return false;
            }
        }

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
        private string CNPJ;

        public ExceptionCNPJInvalido(string cnpj)
        {
            CNPJ = cnpj;
        }
    }
}