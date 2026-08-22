using NFe.Components;
using NFe.Settings;
using Xunit;

namespace UniNFe.Test.Certificados
{
    [Collection("Certificados Serial")]
    public class ProtecaoPinCertificadoTests
    {
        [Fact]
        public void DpapiFazRoundTripSemPersistirPin()
        {
            const string pinOriginal = "PIN-Teste-123";
            var protegido = ProtecaoPinCertificado.Proteger(pinOriginal);

            string pin;
            string mensagem;
            var sucesso = ProtecaoPinCertificado.TryDesproteger(protegido, out pin, out mensagem);

            Assert.True(sucesso, mensagem);
            Assert.StartsWith(ProtecaoPinCertificado.Prefixo, protegido);
            Assert.DoesNotContain(pinOriginal, protegido);
            Assert.Equal(pinOriginal, pin);
        }

        [Fact]
        public void ConfiguracaoLegada3DesContinuaLegivel()
        {
            const string pinOriginal = "987654";
            var legado = Criptografia.criptografaSenha(pinOriginal);

            string pin;
            string mensagem;
            var sucesso = ProtecaoPinCertificado.TryDesproteger(legado, out pin, out mensagem);

            Assert.True(sucesso, mensagem);
            Assert.Equal(pinOriginal, pin);
        }

        [Fact]
        public void TextoPuroLegadoEhAceitoParaMigracao()
        {
            string pin;
            string mensagem;
            var sucesso = ProtecaoPinCertificado.TryDesproteger("PIN-legado", out pin, out mensagem);

            Assert.True(sucesso, mensagem);
            Assert.Equal("PIN-legado", pin);
        }

        [Theory]
        [InlineData("dpapi:v1:corrompido")]
        [InlineData("Wrong Input. falha simulada")]
        [InlineData("Digite os valores Corretamente. falha simulada")]
        public void FalhaDeDescriptografiaNuncaViraPin(string persistido)
        {
            string pin;
            string mensagem;
            var sucesso = ProtecaoPinCertificado.TryDesproteger(persistido, out pin, out mensagem);

            Assert.False(sucesso);
            Assert.Equal(string.Empty, pin);
            Assert.False(string.IsNullOrWhiteSpace(mensagem));
        }
    }
}
