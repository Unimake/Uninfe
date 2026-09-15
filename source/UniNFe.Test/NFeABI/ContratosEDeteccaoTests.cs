using NFe.Components;
using NFe.Service;
using NFe.Settings;
using NFe.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace UniNFe.Test.NFeABI
{
    [Collection("NFeABI Serial")]
    public class ContratosEDeteccaoTests : IDisposable
    {
        private readonly List<Empresa> configuracoesAnteriores;
        private readonly MethodInfo definirTipoServico;
        private readonly MethodInfo validarExtensao;
        private readonly string pastaTemporaria;

        public ContratosEDeteccaoTests()
        {
            configuracoesAnteriores = Empresas.Configuracoes;
            pastaTemporaria = Path.Combine(Path.GetTempPath(), "UniNFe.Test.NFeABI", Guid.NewGuid().ToString("N"));

            var pastaEnvio = Path.Combine(pastaTemporaria, "Envio");
            var pastaRetorno = Path.Combine(pastaTemporaria, "Retorno");
            var pastaErro = Path.Combine(pastaTemporaria, "Erro");
            var pastaLote = Path.Combine(pastaTemporaria, "Lote");
            var pastaValidar = Path.Combine(pastaTemporaria, "Validar");

            Directory.CreateDirectory(pastaEnvio);
            Directory.CreateDirectory(pastaRetorno);
            Directory.CreateDirectory(pastaErro);
            Directory.CreateDirectory(pastaLote);
            Directory.CreateDirectory(pastaValidar);

            Empresas.Configuracoes = new List<Empresa>
            {
                new Empresa
                {
                    PastaXmlEnvio = pastaEnvio,
                    PastaXmlRetorno = pastaRetorno,
                    PastaXmlErro = pastaErro,
                    PastaXmlEmLote = pastaLote,
                    PastaValidar = pastaValidar,
                    Servico = TipoAplicativo.NFeABI
                }
            };

            definirTipoServico = typeof(Processar).GetMethod("DefinirTipoServico", BindingFlags.Instance | BindingFlags.NonPublic);
            validarExtensao = typeof(Processar).GetMethod("ValidarExtensao", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [Fact]
        public void ValoresPublicosSaoAcrescentadosSemRenumerarOsExistentes()
        {
            Assert.Equal(16, (int)TipoAplicativo.BPe);
            Assert.Equal(17, (int)TipoAplicativo.NFeABI);
            Assert.Equal(109, (int)Propriedade.TipoEnvio.CIOTPdf);
            Assert.Equal(110, (int)Propriedade.TipoEnvio.NFeABI);
            Assert.Equal(151, (int)Servicos.CIOTObterOperacaoTransportePdf);
            Assert.Equal(152, (int)Servicos.NFeABIStatusServico);
            Assert.Equal(153, (int)Servicos.NFeABIAutorizacaoSinc);
            Assert.Equal(732, (int)TpcnResources.vCredPresumido);
            Assert.Equal(733, (int)TpcnResources.chNFeABI);
        }

        [Fact]
        public void ContratosDeArquivoENamespacePermanecemExatos()
        {
            var extensao = Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI);

            Assert.Equal("-nfeabi.xml", extensao.EnvioXML);
            Assert.Equal("-ret-nfeabi.xml", extensao.RetornoXML);
            Assert.Equal("-ret-nfeabi.err", extensao.RetornoERR);
            Assert.Equal("-ret-nfeabi.err", Propriedade.ExtRetorno.NFeABI_ERR);
            Assert.Equal("-procNFeABI.xml", Propriedade.ExtRetorno.ProcNFeABI);
            Assert.Equal("http://www.portalfiscal.inf.br/nfeabi", NFeStrConstants.NAME_SPACE_NFEABI);
            Assert.Equal("chNFeABI", TpcnResources.chNFeABI.ToString());
            Assert.Equal("NF-e ABI", EnumHelper.GetDescription(TipoAplicativo.NFeABI));
            Assert.Contains("NF-e ABI", EnumHelper.GetDescription(TipoAplicativo.Todos));
        }

        [Theory]
        [InlineData("NFeABI", "-nfeabi.xml", Servicos.NFeABIAutorizacaoSinc)]
        [InlineData("consStatServNFeABI", "-ped-sta.xml", Servicos.NFeABIStatusServico)]
        public void RaizesOficiaisSaoDetectadas(string raiz, string sufixo, Servicos esperado)
        {
            Assert.Equal(esperado, Detectar(raiz, sufixo));
        }

        [Fact]
        public void ModoTodosReconheceNFeABI()
        {
            Empresas.Configuracoes[0].Servico = TipoAplicativo.Todos;

            Assert.Equal(Servicos.NFeABIAutorizacaoSinc, Detectar("NFeABI", "-nfeabi.xml"));
            Assert.Equal(Servicos.NFeABIStatusServico, Detectar("consStatServNFeABI", "-ped-sta.xml"));
        }

        [Theory]
        [InlineData("RaizDesconhecida")]
        [InlineData("nfeabi")]
        [InlineData("ConsStatServNFeABI")]
        public void RaizDesconhecidaOuComCaseIncorretoNaoERoteada(string raiz)
        {
            Assert.Equal(Servicos.Nulo, Detectar(raiz, "-nfeabi.xml"));
        }

        [Fact]
        public void SufixoNaoPublicadoERejeitado()
        {
            Assert.NotNull(validarExtensao);
            var ex = Assert.Throws<TargetInvocationException>(() =>
                validarExtensao.Invoke(new Processar(), new object[] { Path.Combine(pastaTemporaria, "documento-abi.xml") }));

            Assert.Contains("Não pode identificar o tipo de arquivo", ex.InnerException.Message);
        }

        [Theory]
        [InlineData("NFeABI", "-nfgas.xml")]
        [InlineData("consStatServNFeABI", "-nfeabi.xml")]
        public void RaizNFeABINaoAceitaSufixoDeOutroContrato(string raiz, string sufixo)
        {
            Assert.Equal(Servicos.Nulo, Detectar(raiz, sufixo));
        }

        [Fact]
        public void SeletorDeConfiguracaoIncluiNFeABIEMantemConsultaForaDaEtapa()
        {
            var configuracao = uninfeDummy.DatasouceTipoAplicativo(false);
            var consulta = uninfeDummy.DatasouceTipoAplicativo(true);

            Assert.True(ContemAplicativo(configuracao, TipoAplicativo.Todos));
            Assert.True(ContemAplicativo(configuracao, TipoAplicativo.NFeABI));
            Assert.False(ContemAplicativo(consulta, TipoAplicativo.NFeABI));
        }

        [Theory]
        [InlineData(Servicos.NFeABIAutorizacaoSinc, "-nfeabi.xml", "-ret-nfeabi.err")]
        [InlineData(Servicos.NFeABIStatusServico, "-ped-sta.xml", "-sta.err")]
        public void ErroERPUsaContratoAprovado(Servicos servico, string entrada, string erro)
        {
            var metodo = typeof(Processar).GetMethod("TryGetExtensoesErroNFeABI", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(metodo);

            var argumentos = new object[] { servico, null, null };
            Assert.True((bool)metodo.Invoke(null, argumentos));
            Assert.Equal(entrada, argumentos[1]);
            Assert.Equal(erro, argumentos[2]);
        }

        public void Dispose()
        {
            Empresas.Configuracoes = configuracoesAnteriores;

            if (Directory.Exists(pastaTemporaria))
            {
                Directory.Delete(pastaTemporaria, true);
            }
        }

        private Servicos Detectar(string raiz, string sufixo)
        {
            Assert.NotNull(definirTipoServico);

            var pastaEntrada = Path.Combine(pastaTemporaria, "Entrada");
            Directory.CreateDirectory(pastaEntrada);

            var arquivo = Path.Combine(pastaEntrada, Guid.NewGuid().ToString("N") + sufixo);
            File.WriteAllText(arquivo, "<" + raiz + " xmlns=\"" + NFeStrConstants.NAME_SPACE_NFEABI + "\"><dummy /></" + raiz + ">");

            return (Servicos)definirTipoServico.Invoke(new Processar(), new object[] { 0, arquivo });
        }

        private static bool ContemAplicativo(IList itens, TipoAplicativo aplicativo)
        {
            foreach (KeyValuePair<int, string> item in itens)
            {
                if (item.Key == (int)aplicativo)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
