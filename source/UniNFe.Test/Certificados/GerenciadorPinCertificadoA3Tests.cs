using NFe.Settings;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UniNFe.Test.Certificados
{
    [Collection("Certificados Serial")]
    public class GerenciadorPinCertificadoA3Tests : IDisposable
    {
        private sealed class ProvedorFake : IProvedorCertificadoA3
        {
            internal int Chamadas;
            internal bool Falhar;
            internal int Espera;

            public bool IsA3(X509Certificate2 certificado) => true;

            public void SetPinPrivateKey(X509Certificate2 certificado, string pin)
            {
                Interlocked.Increment(ref Chamadas);
                if (Espera > 0) Thread.Sleep(Espera);
                if (Falhar) throw new InvalidOperationException("Falha simulada do provedor.");
            }
        }

        public GerenciadorPinCertificadoA3Tests()
        {
            GerenciadorPinCertificadoA3.ReiniciarParaTestes();
        }

        [Fact]
        public void SucessoChamaProvedorUmaUnicaVez()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            var primeiro = empresa.CarregarPinCertificadoA3(false);
            var segundo = empresa.CarregarPinCertificadoA3(false);

            Assert.True(primeiro.Sucesso);
            Assert.True(segundo.Sucesso);
            Assert.True(empresa.CertificadoPINCarregado);
            Assert.Equal(1, provedor.Chamadas);
        }

        [Fact]
        public async Task ChamadasConcorrentesExecutamUmaTentativa()
        {
            var provedor = new ProvedorFake { Espera = 50 };
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            var tarefas = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(() => empresa.CarregarPinCertificadoA3(false)))
                .ToArray();
            var resultados = await Task.WhenAll(tarefas);

            Assert.All(resultados, resultado => Assert.True(resultado.Sucesso));
            Assert.Equal(1, provedor.Chamadas);
        }

        [Fact]
        public void FalhaNaoEhRepetidaAutomaticamente()
        {
            var provedor = new ProvedorFake { Falhar = true };
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            var primeiro = empresa.CarregarPinCertificadoA3(false);
            var segundo = empresa.CarregarPinCertificadoA3(false);

            Assert.False(primeiro.Sucesso);
            Assert.False(segundo.Sucesso);
            Assert.False(empresa.CertificadoPINCarregado);
            Assert.Equal(1, provedor.Chamadas);
            Assert.DoesNotContain("1234", primeiro.Mensagem);
        }

        [Fact]
        public void TentativaManualPodeOcorrerDepoisDaCorrecao()
        {
            var provedor = new ProvedorFake { Falhar = true };
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");
            Assert.False(empresa.CarregarPinCertificadoA3(false).Sucesso);

            provedor.Falhar = false;
            empresa.CertificadoPIN = "5678";
            var resultado = empresa.CarregarPinCertificadoA3(true);

            Assert.True(resultado.Sucesso);
            Assert.True(empresa.CertificadoPINCarregado);
            Assert.Equal(2, provedor.Chamadas);
        }

        [Fact]
        public void MudancaDeCertificadoInvalidaSucessoAnterior()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");
            Assert.True(empresa.CarregarPinCertificadoA3(false).Sucesso);

            empresa.X509Certificado = new X509Certificate2();

            Assert.False(empresa.CertificadoPINCarregado);
            Assert.True(empresa.CarregarPinCertificadoA3(false).Sucesso);
            Assert.Equal(2, provedor.Chamadas);
        }

        [Fact]
        public void PinVazioNaoChamaProvedor()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa(string.Empty);

            Assert.False(empresa.CarregarPinCertificadoA3(false).Sucesso);
            Assert.Equal(0, provedor.Chamadas);
        }

        public void Dispose()
        {
            GerenciadorPinCertificadoA3.ReiniciarParaTestes();
        }

        private static Empresa CriarEmpresa(string pin)
        {
            return new Empresa
            {
                UsaCertificado = true,
                CertificadoInstalado = true,
                CertificadoPIN = pin,
                CertificadoDigitalThumbPrint = "00112233",
                X509Certificado = new X509Certificate2()
            };
        }
    }
}
