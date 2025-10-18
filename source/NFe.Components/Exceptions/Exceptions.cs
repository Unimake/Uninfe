using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.Components.Exceptions
{
    /// <summary>
    /// Se ocorrer alguma falha na execução do UniNFe vai gerar esta exceção
    /// </summary>
    public class ProblemaExecucaoUniNFe : Exception
    {
        private string Mensagem = "";

        public ProblemaExecucaoUniNFe(string mensagem)
        {
            Mensagem = mensagem;
        }

        public override string Message
        {
            get
            {
                return Mensagem;
            }
        }
    }

    /// <summary>
    /// Se já tiver algum UniNFe executando vai gerar esta exceção
    /// </summary>
    public class AppJaExecutando : Exception
    {
        private string Mensagem = "";

        public AppJaExecutando(string mensagem)
        {
            Mensagem = mensagem;
        }

        public override string Message
        {
            get
            {
                return Mensagem;
            }
        }
    }

    /// <summary>
    /// Classe para tratamento de exceções da classe CertificadoDigital
    /// </summary>
    public class CertificadoDigitalException : Exception
    {
        public ErroPadrao ErrorCode { get; private set; }

        /// <summary>
        /// Construtor que já define uma mensagem pré-definida de exceção
        /// </summary>
        /// <param name="codigoErro">Código da mensagem de erro (Classe MsgErro)</param>
        /// <by>Wandrey Mundin Ferreira</by>
        /// <date>24/11/2009</date>
        public CertificadoDigitalException(ErroPadrao codigoErro)
            : base(MsgErro.ErroPreDefinido(codigoErro))
        {
            this.ErrorCode = codigoErro;
        }

        /// <summary>
        /// Construtor que já define uma mensagem pré-definida de exceção com possibilidade de complemento da mensagem
        /// </summary>
        /// <param name="codigoErro">Código da mensagem de erro (Classe MsgErro)</param>
        /// <param name="complementoMensagem">Complemento da mensagem de exceção</param>
        public CertificadoDigitalException(ErroPadrao codigoErro, string complementoMensagem)
            : base(MsgErro.ErroPreDefinido(codigoErro, complementoMensagem))
        {
            this.ErrorCode = codigoErro;
        }
    }
}
