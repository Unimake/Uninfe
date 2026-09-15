using System;
using System.Linq;
using System.Xml.Serialization;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFeABI;
using Xunit;
using NFeABIAutorizacaoSinc = Unimake.Business.DFe.Servicos.NFeABI.AutorizacaoSinc;
using NFeABIStatusServico = Unimake.Business.DFe.Servicos.NFeABI.StatusServico;
using XmlNFeABI = Unimake.Business.DFe.Xml.NFeABI.NFeABI;

namespace UniNFe.Test.NFeABI
{
    public class DllHandoffTests
    {
        private const string NamespaceNFeABI = "http://www.portalfiscal.inf.br/nfeabi";

        [Theory]
        [InlineData(typeof(XmlNFeABI), "NFeABI")]
        [InlineData(typeof(ConsStatServNFeABI), "consStatServNFeABI")]
        [InlineData(typeof(RetConsStatServNFeABI), "retConsStatServNFeABI")]
        [InlineData(typeof(RetNFeABI), "retNFeABI")]
        public void TiposPublicosDevemExporRaizNFeABI(Type tipo, string elemento)
        {
            var atributo = tipo
                .GetCustomAttributes(typeof(XmlRootAttribute), false)
                .Cast<XmlRootAttribute>()
                .Single();

            Assert.True(tipo.IsPublic);
            Assert.Equal(elemento, atributo.ElementName);
            Assert.Equal(NamespaceNFeABI, atributo.Namespace);
        }

        [Fact]
        public void ModeloNFeABIDeveEstarDisponivelNoConsumidor()
        {
            Assert.Equal(77, (int)ModeloDFe.NFeABI);
        }

        [Fact]
        public void ServicosPublicadosDevemExporConstrutoresTipados()
        {
            var construtorStatus = typeof(NFeABIStatusServico)
                .GetConstructor(new[] { typeof(ConsStatServNFeABI), typeof(Configuracao) });
            var construtorAutorizacao = typeof(NFeABIAutorizacaoSinc)
                .GetConstructor(new[] { typeof(XmlNFeABI), typeof(Configuracao) });

            Assert.NotNull(construtorStatus);
            Assert.NotNull(construtorAutorizacao);
            Assert.True(typeof(NFeABIStatusServico).IsPublic);
            Assert.True(typeof(NFeABIAutorizacaoSinc).IsPublic);
        }
    }
}
