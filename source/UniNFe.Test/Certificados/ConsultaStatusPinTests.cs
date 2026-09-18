using NFe.Settings;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace UniNFe.Test.Certificados
{
    [Collection("Certificados Serial")]
    public class ConsultaStatusPinTests : IDisposable
    {
        private sealed class ProvedorFake : IProvedorCertificadoA3
        {
            internal bool EhA3 = true;
            internal int ChamadasIsA3;
            internal int ChamadasSetPin;

            public bool HasCachedPrivateKeyContext(X509Certificate2 certificado) => true;

            public bool IsA3(X509Certificate2 certificado)
            {
                ChamadasIsA3++;
                return EhA3;
            }

            public void SetPinPrivateKey(X509Certificate2 certificado, string pin)
            {
                ChamadasSetPin++;
            }
        }

        public ConsultaStatusPinTests()
        {
            GerenciadorPinCertificadoA3.ReiniciarParaTestes();
        }

        [Fact]
        public void ConsultaDiretaPreparaPinAntesDoServicoEUmaUnicaVez()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            Preparar(empresa);
            Preparar(empresa);

            Assert.True(empresa.CertificadoPINCarregado);
            Assert.Equal(1, provedor.ChamadasSetPin);
        }

        [Fact]
        public void ConsultaDiretaSemPinNaoSondaNemConfiguraCertificado()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa(string.Empty);

            Preparar(empresa);

            Assert.Equal(0, provedor.ChamadasIsA3);
            Assert.Equal(0, provedor.ChamadasSetPin);
        }

        [Fact]
        public void ConsultaDiretaComA1NaoConfiguraPinResidual()
        {
            var provedor = new ProvedorFake { EhA3 = false };
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            Preparar(empresa);

            Assert.Equal(1, provedor.ChamadasIsA3);
            Assert.Equal(0, provedor.ChamadasSetPin);
            Assert.False(empresa.CertificadoPINCarregado);
        }

        public void Dispose()
        {
            GerenciadorPinCertificadoA3.ReiniciarParaTestes();
        }

        private void Preparar(Empresa empresa)
        {
            PreparadorCertificadoA3.PrepararOuLancar(empresa, true);
        }

        private static Empresa CriarEmpresa(string pin)
        {
            return new Empresa
            {
                UsaCertificado = true,
                CertificadoInstalado = true,
                CertificadoPIN = pin,
                CertificadoDigitalThumbPrint = "00112233",
                X509Certificado = CriarCertificadoComChavePrivada()
            };
        }

        private static X509Certificate2 CriarCertificadoComChavePrivada()
        {
            using (var rsa = RSA.Create(2048))
            {
                var requisicao = new CertificateRequest("CN=UniNFe Consulta Status A3", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return requisicao.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));
            }
        }
    }
}
