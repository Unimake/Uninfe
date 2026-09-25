using System;
using System.Net;
using System.Security.Authentication;
using NFe.Components;
using NFe.Service;
using Xunit;

namespace UniNFe.Test.Erros
{
    public class RetornoExcecaoTests
    {
        [Fact]
        public void PreservaCamposLegadosEContextoDaExcecaoOriginal()
        {
            Exception interna;
            try
            {
                throw new InvalidOperationException("Erro original de validação.");
            }
            catch (Exception ex)
            {
                interna = ex;
            }

            var exception = new Exception("Falha ao executar a consulta.", interna);
            var retorno = TFunctions.MontaStringErro(exception, ErroPadrao.ValidarXML, "5.1.0.154", "18/06/2026 - 11:30:20");

            Assert.Contains("\r\nErrorCode|0000000007\r\n", retorno);
            Assert.Contains("\r\nMessage|" + interna.Message + "\r\n", retorno);
            Assert.Contains("\r\nStackTrace|" + interna.StackTrace + "\r\n", retorno);
            Assert.Contains("\r\nSource|" + interna.Source + "\r\n", retorno);
            Assert.Contains("\r\nType|System.InvalidOperationException\r\n", retorno);
            Assert.Contains("\r\nTargetSite|" + interna.TargetSite + "\r\n", retorno);
            Assert.Contains("\r\nHashCode|" + interna.GetHashCode() + "\r\n", retorno);
            Assert.EndsWith("\r\nExcecaoCompleta|" + exception, retorno);
            Assert.DoesNotContain("\r\nDiagnostico|", retorno);
        }

        [Theory]
        [InlineData("O certificado remoto é inválido, de acordo com o procedimento de validação.")]
        [InlineData("The remote certificate is invalid according to the validation procedure.")]
        public void ExplicaCertificadoRemotoSemAlterarMensagemOriginal(string mensagem)
        {
            var exception = new WebException("Falha na conexão.",
                new AuthenticationException(mensagem), WebExceptionStatus.SendFailure, null);

            var retorno = TFunctions.MontaStringErro(exception, ErroPadrao.ErroNaoDetectado, "5.1.0.154", "18/06/2026 - 11:30:20");

            Assert.Contains("\r\nMessage|" + mensagem + "\r\n", retorno);
            Assert.Contains("\r\nWebExceptionStatus|SendFailure\r\n", retorno);
            Assert.Contains("\r\nDiagnostico|Falha na validação do certificado HTTPS", retorno);
            Assert.Contains("não comprova problema no certificado A1/A3", retorno);
            Assert.Contains("\r\nTargetSite|\r\n", retorno);
            Assert.EndsWith("\r\nExcecaoCompleta|" + exception, retorno);
        }

        [Fact]
        public void ReconheceTrustFailureSemDependerDoIdiomaDaMensagem()
        {
            var exception = new WebException("Falha de confiança.", null, WebExceptionStatus.TrustFailure, null);

            var retorno = TFunctions.MontaStringErro(exception, ErroPadrao.ErroNaoDetectado, "5.1.0.154", "18/06/2026 - 11:30:20");

            Assert.Contains("\r\nWebExceptionStatus|TrustFailure\r\n", retorno);
            Assert.Contains("\r\nDiagnostico|Falha na validação do certificado HTTPS", retorno);
        }

        [Fact]
        public void NaoAtribuiTodaFalhaDeAutenticacaoAoCertificadoRemoto()
        {
            var exception = new AuthenticationException("Falha na negociação de segurança.");

            var retorno = TFunctions.MontaStringErro(exception, ErroPadrao.ErroNaoDetectado, "5.1.0.154", "18/06/2026 - 11:30:20");

            Assert.Contains("\r\nDiagnostico|Falha de autenticação da conexão segura", retorno);
            Assert.DoesNotContain("Diagnostico|Falha na validação do certificado HTTPS", retorno);
        }

        [Fact]
        public void NaoClassificaTimeoutComoErroDeCertificado()
        {
            var exception = new WebException("Tempo esgotado.", WebExceptionStatus.Timeout);

            var retorno = TFunctions.MontaStringErro(exception, ErroPadrao.ErroNaoDetectado, "5.1.0.154", "18/06/2026 - 11:30:20");

            Assert.Contains("\r\nWebExceptionStatus|Timeout\r\n", retorno);
            Assert.DoesNotContain("\r\nDiagnostico|", retorno);
        }
    }
}
