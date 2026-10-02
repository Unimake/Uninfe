using NFe.Components;
using NFe.Service;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UniNFe.Test.Abstractions;
using Xunit;

namespace UniNFe.Test.Certificados
{
    [Collection("Certificados Serial")]
    public class CertificadoVencidoTests
    {
        private sealed class Contexto : TaskTestContextBase
        {
            internal Empresa Empresa { get; }
            internal string PastaErro { get; }

            internal Contexto() : base(new string('1', 44) + "-nfe.xml",
                "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe versao=\"4.00\" /></NFe>")
            {
                PastaErro = Path.Combine(PastaTemporaria, "erro");
                Directory.CreateDirectory(PastaErro);
                Empresa = new Empresa
                {
                    Servico = TipoAplicativo.Nfe,
                    AmbienteCodigo = 1,
                    UsaCertificado = true,
                    CertificadoInstalado = false,
                    CertificadoArquivo = Path.Combine(PastaTemporaria, "teste.pfx"),
                    CertificadoSenha = "senha-teste",
                    PastaXmlRetorno = PastaRetorno,
                    PastaXmlErro = PastaErro,
                    PastaXmlEmLote = PastaTemporaria,
                    PastaXmlEnvio = Path.Combine(PastaTemporaria, "envio"),
                    PastaValidar = Path.Combine(PastaTemporaria, "validar")
                };
                DefinirConfiguracoesEmpresas(new List<Empresa> { Empresa });
            }

            internal void GravarCertificado(int inicioDias, int fimDias, bool chavePrivada, X509KeyUsageFlags uso)
            {
                using (var rsa = RSA.Create(2048))
                {
                    var requisicao = new CertificateRequest("CN=UniNFe Teste", rsa,
                        HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    requisicao.CertificateExtensions.Add(new X509KeyUsageExtension(uso, true));
                    using (var certificado = requisicao.CreateSelfSigned(
                        DateTimeOffset.Now.AddDays(inicioDias), DateTimeOffset.Now.AddDays(fimDias)))
                    using (var publico = new X509Certificate2(certificado.Export(X509ContentType.Cert)))
                    {
                        var exportado = chavePrivada ? certificado : publico;
                        File.WriteAllBytes(Empresa.CertificadoArquivo,
                            exportado.Export(X509ContentType.Pfx, Empresa.CertificadoSenha));
                    }
                }
            }
        }

        private sealed class Fixture : TaskTestFixtureBase
        {
            internal void Processar(Contexto contexto) =>
                ExecutarEmThread("0", () => new Processar().ProcessaArquivo(0, contexto.ArquivoEnvio));
        }

        [Fact]
        public void PfxVencidoCarregaMasProcessamentoRetornaErroComNomeDaNFe()
        {
            using (var contexto = new Contexto())
            {
                contexto.GravarCertificado(-2, -1, true, X509KeyUsageFlags.DigitalSignature);
                using (var certificado = contexto.Empresa.BuscaConfiguracaoCertificado())
                {
                    Assert.NotNull(certificado);
                    Assert.True(certificado.NotAfter < DateTime.Now);
                    contexto.Empresa.X509Certificado = certificado;

                    new Fixture().Processar(contexto);

                    var nomeRetorno = new string('1', 44) + "-nfe.err";
                    var retorno = Path.Combine(contexto.PastaRetorno, nomeRetorno);
                    Assert.True(File.Exists(retorno));
                    Assert.Contains("ErrorCode|" + ((int)ErroPadrao.CertificadoVencido).ToString("0000000000"),
                        File.ReadAllText(retorno));
                    Assert.Single(Directory.GetFiles(contexto.PastaRetorno));
                    Assert.False(File.Exists(contexto.ArquivoEnvio));
                    Assert.True(File.Exists(Path.Combine(contexto.PastaErro, Path.GetFileName(contexto.ArquivoEnvio))));
                }
            }
        }

        [Theory]
        [InlineData(1, 2, true, X509KeyUsageFlags.DigitalSignature)]
        [InlineData(-2, -1, false, X509KeyUsageFlags.DigitalSignature)]
        [InlineData(-2, -1, true, X509KeyUsageFlags.KeyEncipherment)]
        public void CarregamentoContinuaRejeitandoCertificadoFuturoOuSemPermissaoDeAssinatura(
            int inicioDias, int fimDias, bool chavePrivada, X509KeyUsageFlags uso)
        {
            using (var contexto = new Contexto())
            {
                contexto.GravarCertificado(inicioDias, fimDias, chavePrivada, uso);
                Assert.Throws<Exception>(() => contexto.Empresa.BuscaConfiguracaoCertificado());
            }
        }
    }
}
