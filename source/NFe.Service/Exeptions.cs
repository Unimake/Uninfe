﻿using NFe.Components;
using System;

namespace NFe.Exceptions
{
    /// <summary>
    /// Classe para tratamento de exceções da classe Invocar Objeto
    /// </summary>
    public class ExceptionSemInternet : Exception
    {
        public ErroPadrao ErrorCode { get; private set; }

        /// <summary>
        /// Construtor que já define uma mensagem pré-definida de exceção
        /// </summary>
        /// <param name="CodigoErro">Código da mensagem de erro (Classe MsgErro)</param>
        /// <by>Wandrey Mundin Ferreira</by>
        /// <date>24/11/2009</date>
        public ExceptionSemInternet(ErroPadrao Erro)
            : base(MsgErro.ErroPreDefinido(Erro))
        {
            this.ErrorCode = Erro;
        }

        /// <summary>
        /// Construtor que já define uma mensagem pré-definida de exceção com possibilidade de complemento da mensagem
        /// </summary>
        /// <param name="CodigoErro">Código da mensagem de erro (Classe MsgErro)</param>
        /// <param name="ComplementoMensagem">Complemento da mensagem de exceção</param>
        public ExceptionSemInternet(ErroPadrao Erro, string ComplementoMensagem)
            : base(MsgErro.ErroPreDefinido(Erro, ComplementoMensagem))
        {
            this.ErrorCode = Erro;
        }
    }

    /// <summary>
    /// Classe para tratamento de exceções da classe Invocar Objeto, mas exatamente no ponto em que vai enviar o XML para o SEFAZ
    /// </summary>
    public class ExceptionEnvioXML : Exception
    {
        public ErroPadrao ErrorCode { get; private set; }

        /// <summary>
        /// Construtor que já define uma mensagem pré-definida de exceção
        /// </summary>
        /// <param name="CodigoErro">Código da mensagem de erro (Classe MsgErro)</param>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 16/03/2010
        /// </remarks>
        public ExceptionEnvioXML(ErroPadrao Erro)
            : base(MsgErro.ErroPreDefinido(Erro))
        {
            this.ErrorCode = Erro;
        }

        /// <summary>
        /// Construtor que ´já define uma mensagem pré-definida de exceção com possibilidade de complemento da mensagem
        /// </summary>
        /// <param name="CodigoErro">Código da mensagem de erro (Classe MsgErro)</param>
        /// <param name="ComplementoMensagem">Complemento da mensagem de exceção</param>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 16/03/2010
        /// </remarks>
        public ExceptionEnvioXML(ErroPadrao Erro, string ComplementoMensagem)
            : base(MsgErro.ErroPreDefinido(Erro, ComplementoMensagem))
        {
            this.ErrorCode = Erro;
        }
    }
}
