using System;
using System.IO;
using Xunit;

namespace UniNFe.Test.eBoleto
{
    [Collection("eBoleto")]
    public class TaskBoletoRegistrarTests
    {
        #region Private Fields

        private readonly EBoletoTestFixture fixture;

        #endregion Private Fields

        #region Public Constructors

        public TaskBoletoRegistrarTests(EBoletoTestFixture fixture)
        {
            this.fixture = fixture;
        }

        #endregion Public Constructors

        #region Public Methods

        [Theory]
        [InlineData("pedido-BoletoRegistrar.xml")]
        public void Execute_QuandoReceberArquivo_DeveRealizarProcessoDeEnvio(string nomeArquivoEnvio)
        {
            using(var contexto = fixture.CriarContextoBoletoRegistrar(nomeArquivoEnvio, EBoletoTestFixture.GerarXmlEnvioBoletoRegistrarValido()))
            {
                fixture.ExecutarTaskBoletoRegistrar(contexto);

                var gerouArquivoRetorno = fixture.AguardarArquivo(contexto.ArquivoRetorno, TimeSpan.FromSeconds(30));

                Assert.True(gerouArquivoRetorno, "O processamento não gerou arquivo de retorno.");
                Assert.False(File.Exists(contexto.ArquivoEnvio), "O arquivo de envio não foi removido ao final do processamento.");

                var xmlRetorno = fixture.CarregarXml(contexto.ArquivoRetorno);
                var status = xmlRetorno.DocumentElement?["Status"]?.InnerText;

                Assert.Contains(status, new[] { "0", "1", "999" });
            }
        }

        #endregion Public Methods
    }
}