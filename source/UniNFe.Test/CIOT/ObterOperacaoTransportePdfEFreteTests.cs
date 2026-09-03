using NFe.Components;
using NFe.Service.CIOT;
using System;
using System.IO;
using Xunit;

namespace UniNFe.Test.CIOT
{
    public class ObterOperacaoTransportePdfEFreteTests
    {
        [Fact]
        public void TaskUsaServicoEExtensoesExclusivasDoPdfCIOT()
        {
            var arquivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "-pdfciot.xml");
            try
            {
                File.WriteAllText(arquivo, "<ObterOperacaoTransportePdf xmlns=\"http://www.antt.gov.br/ciot\"><ProvedorCIOT>EFrete</ProvedorCIOT><CodigoIdentificacaoOperacao>992000000126</CodigoIdentificacaoOperacao></ObterOperacaoTransportePdf>");
                var task = new TaskCIOTObterOperacaoTransportePdf(arquivo);
                var extensao = Propriedade.Extensao(Propriedade.TipoEnvio.CIOTPdf);

                Assert.Equal(Servicos.CIOTObterOperacaoTransportePdf, task.Servico);
                Assert.Equal("-pdfciot.xml", extensao.EnvioXML);
                Assert.Equal("-ret-pdfciot.xml", extensao.RetornoXML);
                Assert.Equal("-ret-pdfciot.err", extensao.RetornoERR);
            }
            finally
            {
                if (File.Exists(arquivo)) File.Delete(arquivo);
            }
        }

        [Fact]
        public void NovoServicoFoiAcrescentadoDepoisDoNuloSemRenumerarOsAnteriores()
        {
            Assert.True((int)Servicos.CIOTObterOperacaoTransportePdf > (int)Servicos.Nulo);
        }
    }
}
