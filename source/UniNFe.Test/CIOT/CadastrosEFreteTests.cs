using NFe.Components;
using NFe.Service.CIOT;
using System;
using System.IO;
using Xunit;

namespace UniNFe.Test.CIOT
{
    public class CadastrosEFreteTests
    {
        [Theory]
        [InlineData("GravarMotorista", Servicos.CIOTGravarMotorista, typeof(TaskCIOTGravarMotorista))]
        [InlineData("GravarProprietario", Servicos.CIOTGravarProprietario, typeof(TaskCIOTGravarProprietario))]
        [InlineData("GravarVeiculo", Servicos.CIOTGravarVeiculo, typeof(TaskCIOTGravarVeiculo))]
        public void TasksCadastrosUsamServicoEExtensaoEspecificos(string raiz, Servicos servicoEsperado, Type tipoTask)
        {
            var arquivo = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "-cadciot.xml");
            try
            {
                File.WriteAllText(arquivo, "<" + raiz + " xmlns=\"http://www.antt.gov.br/ciot\"><ProvedorCIOT>EFrete</ProvedorCIOT></" + raiz + ">");
                var task = (TaskCIOTBase)Activator.CreateInstance(tipoTask, arquivo);
                Assert.Equal(servicoEsperado, task.Servico);
                Assert.Equal("-cadciot.xml", Propriedade.Extensao(Propriedade.TipoEnvio.CIOTCadastro).EnvioXML);
                Assert.Equal("-ret-cadciot.xml", Propriedade.Extensao(Propriedade.TipoEnvio.CIOTCadastro).RetornoXML);
                Assert.Equal("-ret-cadciot.err", Propriedade.Extensao(Propriedade.TipoEnvio.CIOTCadastro).RetornoERR);
            }
            finally
            {
                if (File.Exists(arquivo)) File.Delete(arquivo);
            }
        }

        [Fact]
        public void NovosServicosForamAcrescentadosDepoisDoNuloSemRenumerarOsAnteriores()
        {
            Assert.True((int)Servicos.CIOTGravarMotorista > (int)Servicos.Nulo);
            Assert.True((int)Servicos.CIOTGravarProprietario > (int)Servicos.Nulo);
            Assert.True((int)Servicos.CIOTGravarVeiculo > (int)Servicos.Nulo);
        }
    }
}
