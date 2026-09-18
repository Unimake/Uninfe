using NFe.Settings;
using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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
            internal Exception Excecao;
            internal bool Falhar;
            internal bool EhA3 = true;
            internal bool ContextoDisponivel = true;
            internal int Espera;
            internal int ChamadasIsA3;

            public bool HasCachedPrivateKeyContext(X509Certificate2 certificado) => ContextoDisponivel;

            public bool IsA3(X509Certificate2 certificado)
            {
                Interlocked.Increment(ref ChamadasIsA3);
                if (Espera > 0) Thread.Sleep(Espera);
                return EhA3;
            }

            public void SetPinPrivateKey(X509Certificate2 certificado, string pin)
            {
                Interlocked.Increment(ref Chamadas);
                if (Espera > 0) Thread.Sleep(Espera);
                if (Excecao != null) throw Excecao;
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
        public async Task IdentificacaoConcorrenteDoMesmoCertificadoExecutaUmaSonda()
        {
            var provedor = new ProvedorFake { Espera = 50 };
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            var tarefas = Enumerable.Range(0, 10)
                .Select(_ => Task.Run(() => empresa.DeveSerializarOperacaoA3()))
                .ToArray();
            var resultados = await Task.WhenAll(tarefas);

            Assert.All(resultados, Assert.True);
            Assert.Equal(1, provedor.ChamadasIsA3);
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
            Assert.True(primeiro.PodeContinuarSemAutomacao);
            Assert.True(segundo.PodeContinuarSemAutomacao);
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
        public void FalhaNativaInformaOperacaoECodigoSemExporPin()
        {
            var provedor = new ProvedorFake
            {
                Excecao = new Win32Exception(5, "CryptSetProvParam(KeyExchangePin) falhou.")
            };
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            var resultado = empresa.CarregarPinCertificadoA3(true);

            Assert.False(resultado.Sucesso);
            Assert.Contains("CryptSetProvParam(KeyExchangePin)", resultado.Mensagem);
            Assert.Contains("0x00000005", resultado.Mensagem);
            Assert.DoesNotContain("1234", resultado.Mensagem);
        }

        [Fact]
        public void MudancaDeCertificadoInvalidaSucessoAnterior()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");
            Assert.True(empresa.CarregarPinCertificadoA3(false).Sucesso);

            empresa.X509Certificado = CriarCertificadoComChavePrivada();

            Assert.False(empresa.CertificadoPINCarregado);
            Assert.True(empresa.CarregarPinCertificadoA3(false).Sucesso);
            Assert.Equal(2, provedor.Chamadas);
        }

        [Fact]
        public void ContextoNativoPerdidoReaplicaPin()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");
            Assert.True(empresa.CarregarPinCertificadoA3(false).Sucesso);

            provedor.ContextoDisponivel = false;
            var resultado = empresa.CarregarPinCertificadoA3(false);

            Assert.True(resultado.Sucesso);
            Assert.Equal(2, provedor.Chamadas);
        }

        [Fact]
        public void FalhaAutomaticaEmExecucaoNaoInterativaEhImpeditiva()
        {
            var provedor = new ProvedorFake { Falhar = true };
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            var resultado = PreparadorCertificadoA3.Preparar(empresa, false);

            Assert.False(resultado.Sucesso);
            Assert.False(resultado.PodeContinuarSemAutomacao);
            Assert.Contains("não é interativa", resultado.Mensagem);
        }

        [Fact]
        public void LiberarEmpresaRemoveControleRetido()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");
            Assert.True(empresa.CarregarPinCertificadoA3(false).Sucesso);
            Assert.Equal(1, GerenciadorPinCertificadoA3.QuantidadeControlesParaTestes);

            GerenciadorPinCertificadoA3.Liberar(empresa);

            Assert.Equal(0, GerenciadorPinCertificadoA3.QuantidadeControlesParaTestes);
            Assert.False(empresa.CertificadoPINCarregado);
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

        [Fact]
        public void PinResidualEmCertificadoA1EhIgnorado()
        {
            var provedor = new ProvedorFake { EhA3 = false };
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");

            var deveSerializar = empresa.DeveSerializarOperacaoA3();
            var resultado = empresa.CarregarPinCertificadoA3(false);

            Assert.False(deveSerializar);
            Assert.False(resultado.Sucesso);
            Assert.False(resultado.PodeContinuarSemAutomacao);
            Assert.Equal(0, provedor.Chamadas);
        }

        [Fact]
        public void CertificadoSemChavePrivadaContinuaSendoFalhaImpeditiva()
        {
            var provedor = new ProvedorFake();
            GerenciadorPinCertificadoA3.Provedor = provedor;
            var empresa = CriarEmpresa("1234");
            empresa.X509Certificado = new X509Certificate2();

            var resultado = empresa.CarregarPinCertificadoA3(false);

            Assert.False(resultado.Sucesso);
            Assert.False(resultado.PodeContinuarSemAutomacao);
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
                X509Certificado = CriarCertificadoComChavePrivada()
            };
        }

        private static X509Certificate2 CriarCertificadoComChavePrivada()
        {
            using (var rsa = RSA.Create(2048))
            {
                var requisicao = new CertificateRequest("CN=UniNFe Teste A3", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return requisicao.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(1));
            }
        }
    }
}
