using NFe.Service;
using Xunit;

namespace UniNFe.Test.Autorizacao
{
    public class AutorizacaoNFeTests
    {
        [Theory]
        [InlineData("104")]
        [InlineData("100")]
        [InlineData("120")]
        [InlineData("150")]
        public void RetornoSincronoAutorizadoDeveSerFinalizado(string cStat)
        {
            Assert.True(TaskNFeRecepcao.RetornoSincronoDeveSerFinalizado(cStat));
        }

        [Theory]
        [InlineData("103")]
        [InlineData("108")]
        [InlineData("109")]
        [InlineData("539")]
        [InlineData(null)]
        [InlineData("")]
        public void RetornoSincronoNaoAutorizadoNaoDeveSerFinalizado(string cStat)
        {
            Assert.False(TaskNFeRecepcao.RetornoSincronoDeveSerFinalizado(cStat));
        }
    }
}
