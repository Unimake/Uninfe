using System;
using NFe.ConvertTxt;
using NFe.ConvertTxt.Mapping;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Xunit;

namespace UniNFe.Test.NFeConvertTxt
{
    public sealed class NFeDFeMapperTests
    {
        [Fact]
        public void MapearDeveRejeitarLayoutDiferenteDe400()
        {
            var nota = CriarNotaBase();
            nota.infNFe.Versao = 3.10m;

            var exception = Assert.Throws<NotSupportedException>(() => new NFeDFeMapper().Mapear(nota));

            Assert.Contains("4.00", exception.Message);
        }

        [Fact]
        public void MapearDevePreservarIdentificacaoEmitenteEDestinatario()
        {
            var nota = CriarNotaBase();

            var resultado = new NFeDFeMapper().Mapear(nota).InfNFeField;

            Assert.Equal("4.00", resultado.Versao);
            Assert.Equal(55, (int)resultado.Ide.Mod);
            Assert.Equal("12345678", resultado.Ide.CNF);
            Assert.Equal(new DateTimeOffset(2026, 7, 10, 10, 30, 0, TimeSpan.FromHours(-3)), resultado.Ide.DhEmi);
            Assert.Equal("12345678000195", resultado.Emit.CNPJ);
            Assert.Equal("EMITENTE", resultado.Emit.XNome);
            Assert.Equal("DESTINATARIO", resultado.Dest.XNome);
            Assert.Equal("87654321000100", resultado.Dest.CNPJ);
            Assert.Equal(4106902, resultado.Dest.EnderDest.CMun);
        }

        [Fact]
        public void MapearDeveManterGruposOpcionaisAusentes()
        {
            var nota = CriarNotaBase();

            var resultado = new NFeDFeMapper().Mapear(nota).InfNFeField;

            Assert.Null(resultado.Retirada);
            Assert.Null(resultado.Entrega);
            Assert.Null(resultado.AutXML);
            Assert.Null(resultado.Ide.TpNFDebito);
            Assert.Null(resultado.Ide.TpNFCredito);
        }

        [Fact]
        public void MapearDeveAplicarNomeDeHomologacao()
        {
            var nota = CriarNotaBase();
            nota.ide.tpAmb = TipoAmbiente.Homologacao;

            var resultado = new NFeDFeMapper().Mapear(nota).InfNFeField;

            Assert.Equal("NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL", resultado.Dest.XNome);
        }

        [Fact]
        public void ModeloMapeadoDeveSerSerializadoPelaDll()
        {
            var modelo = new NFeDFeMapper().Mapear(CriarNotaBase());

            var xml = XMLUtility.Serializar(modelo);

            Assert.Equal("NFe", xml.DocumentElement.LocalName);
            Assert.Equal("http://www.portalfiscal.inf.br/nfe", xml.DocumentElement.NamespaceURI);
            Assert.Equal("4.00", xml.DocumentElement["infNFe", xml.DocumentElement.NamespaceURI].GetAttribute("versao"));
            Assert.Equal("55", xml.GetElementsByTagName("mod")[0].InnerText);
            Assert.Equal("12345678000195", xml.GetElementsByTagName("CNPJ")[0].InnerText);
        }

        [Fact]
        public void MapearDevePreservarOrdemDeEmissaoDasReferenciasDoLegado()
        {
            var nota = CriarNotaBase();
            nota.ide.NFref.Add(new NFref { refCTe = "41123456789012345678901234567890123456789012" });
            nota.ide.NFref.Add(new NFref { refNFe = "41123456789012345678901234567890123456789013" });
            nota.ide.NFref.Add(new NFref
            {
                refNF = new refNF { cUF = 41, AAMM = "2607", CNPJ = "12345678000195", mod = "01", serie = 1, nNF = 10 }
            });
            nota.ide.NFref.Add(new NFref { refCTe = "00123456789012345678901234567890123456789012" });

            var referencias = new NFeDFeMapper().Mapear(nota).InfNFeField.Ide.NFref;

            Assert.Equal(3, referencias.Count);
            Assert.NotNull(referencias[0].RefNFe);
            Assert.NotNull(referencias[1].RefNF);
            Assert.NotNull(referencias[2].RefCTe);
        }

        [Fact]
        public void MapearDeveCriarCompraGovernamentalSomenteQuandoEnteForDefinido()
        {
            var nota = CriarNotaBase();
            Assert.Null(new NFeDFeMapper().Mapear(nota).InfNFeField.Ide.GCompraGov);

            nota.ide.gCompraGov.tpEnteGov = TpcnTipoEnteGovernamental.tEnteGovernamentalEstado;
            nota.ide.gCompraGov.pRedutor = 1.2345;
            nota.ide.gCompraGov.tpOperGov = TpcnTipoOperacaoEnteGovernamental.tOperacaoEnteGovernamentalFornecimento;

            var grupo = new NFeDFeMapper().Mapear(nota).InfNFeField.Ide.GCompraGov;

            Assert.NotNull(grupo);
            Assert.Equal(2, (int)grupo.TpEnteGov);
            Assert.Equal(1.2345, grupo.PRedutor);
            Assert.Equal(1, (int)grupo.TpOperGov);
        }

        [Fact]
        public void MapearDeveCriarPagamentoAntecipadoSomenteQuandoExistiremReferencias()
        {
            var nota = CriarNotaBase();
            Assert.Null(new NFeDFeMapper().Mapear(nota).InfNFeField.Ide.GPagAntecipado);

            nota.ide.gPagAntecipado.refNFe.Add("41123456789012345678901234567890123456789012");

            var grupo = new NFeDFeMapper().Mapear(nota).InfNFeField.Ide.GPagAntecipado;

            Assert.Single(grupo.RefDFe);
            Assert.Equal("41123456789012345678901234567890123456789012", grupo.RefDFe[0]);
        }

        internal static NFe.ConvertTxt.NFe CriarNotaBase()
        {
            var nota = new NFe.ConvertTxt.NFe();
            nota.infNFe.Versao = 4m;
            nota.ide.cUF = 41;
            nota.ide.cNF = 12345678;
            nota.ide.natOp = "VENDA";
            nota.ide.mod = TpcnMod.modNFe;
            nota.ide.serie = 1;
            nota.ide.nNF = 123;
            nota.ide.dhEmi = "2026-07-10T10:30:00-03:00";
            nota.ide.tpNF = TpcnTipoNFe.tnSaida;
            nota.ide.idDest = TpcnDestinoOperacao.doInterna;
            nota.ide.cMunFG = 4106902;
            nota.ide.tpImp = TpcnTipoImpressao.tiRetrato;
            nota.ide.tpEmis = TipoEmissao.Normal;
            nota.ide.tpAmb = TipoAmbiente.Producao;
            nota.ide.finNFe = TpcnFinalidadeNFe.fnNormal;
            nota.ide.indFinal = TpcnConsumidorFinal.cfConsumidorFinal;
            nota.ide.indPres = TpcnPresencaComprador.pcPresencial;
            nota.ide.procEmi = TpcnProcessoEmissao.peAplicativoContribuinte;
            nota.ide.verProc = "UniNFe.Test";

            nota.emit.CNPJ = "12345678000195";
            nota.emit.xNome = "EMITENTE";
            nota.emit.IE = "1234567890";
            nota.emit.enderEmit.xLgr = "RUA EMITENTE";
            nota.emit.enderEmit.nro = "10";
            nota.emit.enderEmit.xBairro = "CENTRO";
            nota.emit.enderEmit.cMun = 4106902;
            nota.emit.enderEmit.xMun = "CURITIBA";
            nota.emit.enderEmit.UF = "PR";
            nota.emit.enderEmit.CEP = 80000000;
            nota.emit.enderEmit.cPais = 1058;
            nota.emit.enderEmit.xPais = "BRASIL";

            nota.dest.CNPJ = "87654321000100";
            nota.dest.xNome = "DESTINATARIO";
            nota.dest.indIEDest = TpcnindIEDest.inContribuinte;
            nota.dest.IE = "9876543210";
            nota.dest.enderDest.xLgr = "RUA DESTINATARIO";
            nota.dest.enderDest.nro = "20";
            nota.dest.enderDest.xBairro = "CENTRO";
            nota.dest.enderDest.cMun = 4106902;
            nota.dest.enderDest.xMun = "CURITIBA";
            nota.dest.enderDest.UF = "PR";
            nota.dest.enderDest.CEP = 80000001;
            nota.dest.enderDest.cPais = 1058;
            nota.dest.enderDest.xPais = "BRASIL";
            return nota;
        }
    }
}
