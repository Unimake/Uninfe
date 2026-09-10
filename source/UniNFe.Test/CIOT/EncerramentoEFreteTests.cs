using NFe.Service.CIOT;
using System;
using Unimake.Business.DFe.Servicos;
using Xunit;

namespace UniNFe.Test.CIOT
{
    public class EncerramentoEFreteTests
    {
        [Fact]
        public void UsaDataDoProcessamentoQuandoEFreteNaoRetornaDataEncerramento()
        {
            var dataProcessamento = new DateTime(2026, 9, 10, 15, 30, 45);

            var resultado = TaskCIOTEncerramentoOperacaoTransporte.ObterDataEncerramentoParaDistribuicao(
                ProvedorCIOT.EFrete,
                DateTime.MinValue,
                dataProcessamento);

            Assert.Equal(new DateTime(2026, 9, 10), resultado);
        }

        [Fact]
        public void PreservaDataRetornadaQuandoEFretePassaAInformaLa()
        {
            var dataEncerramento = new DateTime(2026, 8, 28, 18, 20, 30);

            var resultado = TaskCIOTEncerramentoOperacaoTransporte.ObterDataEncerramentoParaDistribuicao(
                ProvedorCIOT.EFrete,
                dataEncerramento,
                new DateTime(2026, 9, 10));

            Assert.Equal(new DateTime(2026, 8, 28), resultado);
        }

        [Theory]
        [InlineData(ProvedorCIOT.ANTT)]
        [InlineData(null)]
        public void NaoAplicaFallbackAoFluxoANTT(ProvedorCIOT? provedorCIOT)
        {
            var resultado = TaskCIOTEncerramentoOperacaoTransporte.ObterDataEncerramentoParaDistribuicao(
                provedorCIOT,
                DateTime.MinValue,
                new DateTime(2026, 9, 10));

            Assert.Equal(DateTime.MinValue, resultado);
        }
    }
}
