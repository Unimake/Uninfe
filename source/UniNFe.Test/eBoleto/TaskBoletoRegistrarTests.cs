using System;
using System.IO;
using System.Text;
using NFe.Components;
using NFe.Service;
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

        [Fact]
        public void ExtrairPDFRetorno_QuandoRetornoPossuirPdfContentBase64_DeveGravarPdfNaPastaRetorno()
        {
            using(var contexto = fixture.CriarContextoBoletoRegistrar("0000109401001-BoletoRegistrar.xml", EBoletoTestFixture.GerarXmlEnvioBoletoRegistrarValido()))
            {
                var conteudoPdf = Encoding.ASCII.GetBytes("%PDF-1.4\n%EOF");
                var task = new TaskBoletoRegistrar(contexto.ArquivoEnvio)
                {
                    vStrXmlRetorno = "<BoletoRegistrarResponse>" +
                                      "<Status>0</Status>" +
                                      "<PdfContentSuccess>true</PdfContentSuccess>" +
                                      "<PdfContentBase64>" + Convert.ToBase64String(conteudoPdf) + "</PdfContentBase64>" +
                                      "</BoletoRegistrarResponse>"
                };
                var extensao = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar);

                task.ExtrairPDFRetorno(0, extensao.EnvioXML, extensao.RetornoXML);

                Assert.True(File.Exists(contexto.ArquivoRetornoPDF), "O PDF do e-Boleto não foi gravado na pasta de retorno.");
                Assert.Equal(conteudoPdf, File.ReadAllBytes(contexto.ArquivoRetornoPDF));
            }
        }

        [Fact]
        public void ExtrairPDFRetorno_QuandoPdfContentBase64Invalido_DevePreservarXmlRetornoSucesso()
        {
            using(var contexto = fixture.CriarContextoBoletoRegistrar("0000109401001-BoletoRegistrar.xml", EBoletoTestFixture.GerarXmlEnvioBoletoRegistrarValido()))
            {
                var xmlRetornoSucesso = "<BoletoRegistrarResponse>" +
                                        "<Status>0</Status>" +
                                        "<Motivo>Boleto registrado</Motivo>" +
                                        "<LinhaDigitavel>03399006495780000000200109401018615360001135546</LinhaDigitavel>" +
                                        "<PdfContentSuccess>true</PdfContentSuccess>" +
                                        "<PdfContentBase64>base64-invalido</PdfContentBase64>" +
                                        "</BoletoRegistrarResponse>";
                File.WriteAllText(contexto.ArquivoRetorno, xmlRetornoSucesso, Encoding.UTF8);

                var task = new TaskBoletoRegistrar(contexto.ArquivoEnvio)
                {
                    vStrXmlRetorno = xmlRetornoSucesso
                };
                var extensao = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar);

                task.ExtrairPDFRetorno(0, extensao.EnvioXML, extensao.RetornoXML);

                Assert.False(File.Exists(contexto.ArquivoRetornoPDF), "PDF não deveria ser gerado quando o conteúdo Base64 é inválido.");
                Assert.Equal(xmlRetornoSucesso, File.ReadAllText(contexto.ArquivoRetorno, Encoding.UTF8));
            }
        }

        #endregion Public Methods
    }
}
