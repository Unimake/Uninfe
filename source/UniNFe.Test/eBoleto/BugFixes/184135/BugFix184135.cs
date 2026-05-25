using System;
using System.IO;
using Xunit;

namespace UniNFe.Test.eBoleto.BugFixes
{
    [Collection("eBoleto")]
    [Trait("Category", "BugFix")]
    public class BugFix184135
    {
        #region Private Fields

        private readonly EBoletoTestFixture testFixture;

        #endregion Private Fields

        #region Public Constructors

        public BugFix184135(EBoletoTestFixture fixture) => testFixture = fixture;

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        public void Fix_Pagador_Is_Null()
        {
            var xmlFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eBoleto\\BugFixes\\184135", "pagador-null-0000105601001-BoletoRegistrar.xml"));

            using(var contexto = testFixture.CriarContextoBoletoRegistrar(xmlFile.Name, EBoletoTestFixture.RecuperarXmlDeArquivo(xmlFile.FullName)))
            {
                testFixture.ExecutarTaskBoletoRegistrar(contexto);

                var gerouArquivoRetorno = testFixture.AguardarArquivo(contexto.ArquivoRetorno, TimeSpan.FromSeconds(30));

                Assert.True(gerouArquivoRetorno, $"O processamento não gerou arquivo de retorno para o cenário do bugfix {nameof(Fix_Pagador_Is_Null)}@{nameof(BugFix184135)}.");

                var allText = File.ReadAllText(contexto.ArquivoRetorno);

                Assert.Contains("The Pagador field is required.", allText);
            }
        }

        [Fact]
        public void Fix_The_Nome_Field_is_Required()
        {
            var xmlFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eBoleto\\BugFixes\\184135", "full-0000105601001-BoletoRegistrar.xml"));

            using(var contexto = testFixture.CriarContextoBoletoRegistrar(xmlFile.Name, EBoletoTestFixture.RecuperarXmlDeArquivo(xmlFile.FullName)))
            {
                testFixture.ExecutarTaskBoletoRegistrar(contexto);

                var gerouArquivoRetorno = testFixture.AguardarArquivo(contexto.ArquivoRetorno, TimeSpan.FromSeconds(30));

                Assert.True(gerouArquivoRetorno, $"O processamento não gerou arquivo de retorno para o cenário do bugfix {nameof(Fix_The_Nome_Field_is_Required)}@{nameof(BugFix184135)}.");

                var allText = File.ReadAllText(contexto.ArquivoRetorno);

                Assert.DoesNotContain("The Nome field is required.", allText);
                Assert.DoesNotContain("The Endereco field is required.", allText);
                Assert.DoesNotContain("The Inscricao field is required.", allText);
                Assert.Contains("A configuração 'CONFIGURATION_ID' não é valida para este contexto. Verifique se a configuração existe e se está configurada corretamente.", allText);
            }
        }

        #endregion Public Methods
    }
}