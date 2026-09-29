using System.Xml;
using NFe.Service.NFSe;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;
using Xunit;

namespace UniNFe.Test.NFSe
{
    public class NFSeResolucaoCentralizadaTests
    {
        [Theory]
        [InlineData(PadraoNFSe.DSF, "<ns1:ConsultaSeqRps xmlns:ns1=\"urn:dsf\"><Cabecalho Versao=\"1.00\" /></ns1:ConsultaSeqRps>", 0, Servico.NFSeConsultarSequenciaLoteNotaRPS)]
        [InlineData(PadraoNFSe.PAULISTANA, "<p1:PedidoInformacoesLote xmlns:p1=\"http://www.prefeitura.sp.gov.br/nfe\"><Cabecalho Versao=\"1\" /></p1:PedidoInformacoesLote>", 3550308, Servico.NFSeConsultaInformacoesLote)]
        [InlineData(PadraoNFSe.GIF, "<pedRegEvento versao=\"1.01\"><infPedReg /></pedRegEvento>", 0, Servico.NFSeCancelarNfse)]
        [InlineData(PadraoNFSe.GIF, "<pedCancelaNFSe><CNPJ>1</CNPJ></pedCancelaNFSe>", 0, Servico.NFSeCancelarNotaFiscal)]
        [InlineData(PadraoNFSe.DSF, "<ns1:ReqConsultaNotas xmlns:ns1=\"urn:dsf\"><Cabecalho Versao=\"1.00\" /></ns1:ReqConsultaNotas>", 0, Servico.NFSeConsultarNotaValida)]
        [InlineData(PadraoNFSe.SMARAPD, "<EnviarLoteRpsSincronoEnvio xmlns=\"http://www.abrasf.org.br/nfse.xsd\"><LoteRps versao=\"2.04\" /></EnviarLoteRpsSincronoEnvio>", 0, Servico.NFSeRecepcionarLoteRpsSincrono)]
        [InlineData(PadraoNFSe.SMARAPD, "<EnviarLoteRpsSincronoEnvio xmlns=\"http://www.abrasf.org.br/nfse.xsd\"><LoteRps versao=\"2.04\" /></EnviarLoteRpsSincronoEnvio>", 2111300, Servico.NFSeRecepcionarLoteRps)]
        [InlineData(PadraoNFSe.FIORILLI, "<ConsultarNfseEnvio versao=\"1.01\"><Prestador /></ConsultarNfseEnvio>", 0, Servico.NFSeConsultarNfse)]
        [InlineData(PadraoNFSe.SIGCORP, "<ConsultarRpsServicoPrestadoEnvio><numero_rps>1</numero_rps></ConsultarRpsServicoPrestadoEnvio>", 4113700, Servico.NFSeConsultarRpsServicoPrestado)]
        public void DeveResolverServicoUsadoPelasTasksNFSe(PadraoNFSe padraoNFSe, string conteudoXML, int codigoMunicipio, Servico servicoEsperado)
        {
            var xml = CriarXml(conteudoXML);
            var resolucao = ResolucaoCentralizadaNFSe.Resolver(xml, padraoNFSe, codigoMunicipio);

            Assert.False(string.IsNullOrWhiteSpace(resolucao.Versao));
            Assert.Equal(servicoEsperado, resolucao.Servico);
        }

        [Theory]
        [InlineData(TipoAmbiente.Producao, Servico.NFSeEnvioLoteRps)]
        [InlineData(TipoAmbiente.Homologacao, Servico.NFSeTesteEnvioLoteRps)]
        public void DeveResolverEnvioPaulistanaConformeAmbiente(TipoAmbiente tipoAmbiente, Servico servicoEsperado)
        {
            const string conteudoXML = "<PedidoEnvioLoteRPS xmlns=\"http://www.prefeitura.sp.gov.br/nfe\"><Cabecalho Versao=\"1\" /><RPS /></PedidoEnvioLoteRPS>";
            var xml = CriarXml(conteudoXML);
            var resolucao = ResolucaoCentralizadaNFSe.Resolver(xml, PadraoNFSe.PAULISTANA, 3550308, tipoAmbiente);

            Assert.Equal("1.00", resolucao.Versao);
            Assert.Equal(servicoEsperado, resolucao.Servico);
        }

        [Theory]
        [InlineData("<ConsultarDpsDisponivelEnvio xmlns=\"http://www.sped.fazenda.gov.br/nfse\"><IM>1</IM></ConsultarDpsDisponivelEnvio>", "1.01")]
        [InlineData("<ConsultarRpsDisponivelEnvio xmlns=\"http://www.abrasf.org.br/nfse.xsd\"><Pedido /></ConsultarRpsDisponivelEnvio>", "2.04")]
        public void DeveUsarVersaoConfiguradaNaConsultaRpsDisponivelISSNET(string conteudoXML, string versaoEsperada)
        {
            var xml = CriarXml(conteudoXML);

            var versao = ResolucaoCentralizadaNFSe.DefinirVersao(xml, PadraoNFSe.ISSNET, 0);

            Assert.Equal(versaoEsperada, versao);
        }

        [Fact]
        public void DeveUsarVersaoConfiguradaParaSmarapd3530607()
        {
            var xml = CriarXml("<ConsultarNfseRpsEnvio xmlns=\"http://www.abrasf.org.br/nfse.xsd\"><Prestador /><IdentificacaoRps /></ConsultarNfseRpsEnvio>");

            var versao = ResolucaoCentralizadaNFSe.DefinirVersao(xml, PadraoNFSe.SMARAPD, 3530607);

            Assert.Equal("2.03", versao);
        }

        private static XmlDocument CriarXml(string conteudoXML)
        {
            var xml = new XmlDocument();
            xml.LoadXml(conteudoXML);

            return xml;
        }
    }
}
