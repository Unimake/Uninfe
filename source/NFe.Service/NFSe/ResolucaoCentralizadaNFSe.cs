using System.Xml;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.NFSe
{
    internal sealed class ResultadoResolucaoNFSe
    {
        internal ResultadoResolucaoNFSe(string versao, Servico servico)
        {
            Versao = versao;
            Servico = servico;
        }

        internal Servico Servico { get; }

        internal string Versao { get; }
    }

    internal static class ResolucaoCentralizadaNFSe
    {
        internal static string DefinirVersao(XmlDocument conteudoXML, PadraoNFSe padraoNFSe, int codigoMunicipio)
        {
            if (padraoNFSe == PadraoNFSe.ISSNET)
            {
                return "1.01";
            }

            return Unimake.Business.DFe.ValidarEstruturaXML.DefinirVersaoNFSe(conteudoXML, padraoNFSe, codigoMunicipio);
        }

        internal static ResultadoResolucaoNFSe Resolver(XmlDocument conteudoXML, PadraoNFSe padraoNFSe, int codigoMunicipio)
        {
            return Resolver(conteudoXML, padraoNFSe, codigoMunicipio, TipoAmbiente.Producao);
        }

        internal static ResultadoResolucaoNFSe Resolver(XmlDocument conteudoXML, PadraoNFSe padraoNFSe, int codigoMunicipio, TipoAmbiente tipoAmbiente)
        {
            var versao = DefinirVersao(conteudoXML, padraoNFSe, codigoMunicipio);
            var servico = Unimake.Business.DFe.ValidarEstruturaXML.DefinirTipoServicoNFSe(
                conteudoXML,
                padraoNFSe,
                versao,
                codigoMunicipio,
                tipoAmbiente);

            return new ResultadoResolucaoNFSe(versao, servico);
        }
    }
}
