using NFe.Components;
using NFe.Service;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UniNFe.Test.Abstractions;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFe;
using Xunit;

namespace UniNFe.Test.Autorizacao
{
    [CollectionDefinition("NFe Recepcao Serial", DisableParallelization = true)]
    public class NFeRecepcaoCollection : ICollectionFixture<NFeRecepcaoTestFixture>
    {
    }

    public class NFeRecepcaoTestContext : TaskTestContextBase, IDisposable
    {
        private readonly bool gravarLogOriginal;

        public Empresa Empresa { get; }
        public List<ArquivoXMLDFe> Notas { get; } = new List<ArquivoXMLDFe>();
        public string PastaEmProcessamento => Path.Combine(Empresa.PastaXmlEnviado, PastaEnviados.EmProcessamento.ToString());
        public string ArquivoErroLote => Path.Combine(PastaRetorno, "000000000000201" +
            (Notas.Count == 1 ? Propriedade.ExtRetorno.ProRec_ERR : Propriedade.ExtRetorno.Rec_ERR));

        public NFeRecepcaoTestContext(int quantidade = 1, bool gravarTXT = false, int modelo = 55,
            bool ipiInvalido = false, bool emLote = false, bool emProcessamento = false)
            : base("pedido-env-lot.xml", "<enviNFe />")
        {
            Propriedade.PastaExecutavel = PastaTemporaria;
            Directory.CreateDirectory(Propriedade.PastaLog);
            gravarLogOriginal = ConfiguracaoApp.GravarLogOperacoesRealizadas;
            ConfiguracaoApp.GravarLogOperacoesRealizadas = true;
            Empresa = new Empresa
            {
                CNPJ = "99999999000191",
                Servico = TipoAplicativo.Nfe,
                AmbienteCodigo = 2,
                PastaXmlEnvio = Path.Combine(PastaTemporaria, "envio"),
                PastaXmlEmLote = Path.Combine(PastaTemporaria, "emlote"),
                PastaXmlEnviado = Path.Combine(PastaTemporaria, "enviados"),
                PastaXmlErro = Path.Combine(PastaTemporaria, "erro"),
                PastaXmlRetorno = PastaRetorno,
                GravarRetornoTXTNFe = gravarTXT,
                VersaoQRCodeNFCe = 3,
                IdentificadorCSC = string.Empty,
                TokenCSC = string.Empty
            };
            DefinirConfiguracoesEmpresas(new List<Empresa> { Empresa });
            foreach(var pasta in new[] { Empresa.PastaEmpresa, Empresa.PastaXmlErro,
                Path.Combine(Empresa.PastaXmlEnvio, "temp"), Path.Combine(Empresa.PastaXmlEmLote, "temp"), PastaEmProcessamento })
            {
                Directory.CreateDirectory(pasta);
            }

            File.WriteAllText(Path.Combine(Empresa.PastaEmpresa, "UniNfeLote.xml"),
                "<DadosLoteNfe><UltimoLoteEnviado>200</UltimoLoteEnviado></DadosLoteNfe>");
            for(var i = 1; i <= quantidade; i++)
            {
                var xml = new XmlDocument();
                xml.LoadXml(GerarNota(i, modelo, ipiInvalido));
                var pasta = emProcessamento ? PastaEmProcessamento :
                    Path.Combine(emLote ? Empresa.PastaXmlEmLote : Empresa.PastaXmlEnvio, "temp");
                var arquivo = Path.Combine(pasta, "nota" + i + Propriedade.ExtEnvio.NFe);
                xml.Save(arquivo);
                Notas.Add(new ArquivoXMLDFe { NomeArquivoXML = arquivo, ConteudoXML = xml });
            }
        }

        public string Chave(int indice) => ((XmlElement)Notas[indice].ConteudoXML.GetElementsByTagName("infNFe")[0]).GetAttribute("Id");
        public string NomeNota(int indice) => Path.GetFileName(Notas[indice].NomeArquivoXML);
        public string Referencia(int indice, string extensao = ".xml", bool temporaria = false) =>
            Path.Combine(temporaria ? Path.Combine(Empresa.PastaXmlEnvio, "temp") : PastaRetorno,
                Functions.ExtrairNomeArq(NomeNota(indice), Propriedade.ExtEnvio.NFe) + "-num-lot" + extensao);

        public string LerLogs() => string.Join("\n", Directory.GetFiles(Propriedade.PastaLog, "*", SearchOption.AllDirectories)
            .Select(File.ReadAllText));

        public new void Dispose()
        {
            ConfiguracaoApp.GravarLogOperacoesRealizadas = gravarLogOriginal;
            base.Dispose();
        }

        private static string GerarNota(int numero, int modelo, bool ipiInvalido)
        {
            var codigoNumerico = (10000000 + numero).ToString("00000000");
            var chaveSemDV = "41261099999999000191" + modelo + "001" + numero.ToString("000000000") + "1" + codigoNumerico;
            var dv = Unimake.Business.DFe.Utility.XMLUtility.CalcularDVChave(chaveSemDV);
            return
            "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe versao=\"4.00\" Id=\"NFe" +
            chaveSemDV + dv + "\">" +
            "<ide><cUF>41</cUF><cNF>" + codigoNumerico + "</cNF><natOp>VENDA TESTE</natOp>" +
            "<mod>" + modelo + "</mod><serie>1</serie><nNF>" + numero + "</nNF>" +
            "<dhEmi>2026-10-07T10:00:00-03:00</dhEmi><tpNF>1</tpNF><idDest>1</idDest><cMunFG>4106902</cMunFG>" +
            "<tpImp>1</tpImp><tpEmis>1</tpEmis><cDV>" + dv + "</cDV><tpAmb>2</tpAmb><finNFe>1</finNFe>" +
            "<indFinal>1</indFinal><indPres>1</indPres><procEmi>0</procEmi><verProc>TESTE</verProc></ide>" +
            "<emit><CNPJ>99999999000191</CNPJ><xNome>EMPRESA TESTE</xNome>" +
            "<enderEmit><xLgr>RUA EXEMPLO</xLgr><nro>1</nro><xBairro>CENTRO</xBairro><cMun>4106902</cMun>" +
            "<xMun>CURITIBA</xMun><UF>PR</UF><CEP>80000000</CEP></enderEmit><IE>9999999999</IE><CRT>3</CRT></emit>" +
            "<det nItem=\"1\"><prod><cProd>1</cProd><cEAN>SEM GTIN</cEAN><xProd>PRODUTO TESTE</xProd><NCM>96039000</NCM>" +
            "<CFOP>5102</CFOP><uCom>UN</uCom><qCom>1.0000</qCom><vUnCom>10.00</vUnCom><vProd>10.00</vProd>" +
            "<cEANTrib>SEM GTIN</cEANTrib><uTrib>UN</uTrib><qTrib>1.0000</qTrib><vUnTrib>10.00</vUnTrib><indTot>1</indTot></prod>" +
            "<imposto><ICMS><ICMS00><orig>0</orig><CST>00</CST><modBC>3</modBC><vBC>10.00</vBC><pICMS>18.00</pICMS>" +
            "<vICMS>1.80</vICMS></ICMS00></ICMS>" +
            (ipiInvalido ? "<IPI><cEnq>999</cEnq><IPINT><CST>00</CST></IPINT></IPI>" : string.Empty) +
            "<PIS><PISNT><CST>08</CST></PISNT></PIS><COFINS><COFINSNT><CST>08</CST></COFINSNT></COFINS></imposto></det>" +
            "<total><ICMSTot><vBC>10.00</vBC><vICMS>1.80</vICMS><vICMSDeson>0.00</vICMSDeson><vFCP>0.00</vFCP>" +
            "<vBCST>0.00</vBCST><vST>0.00</vST><vFCPST>0.00</vFCPST><vFCPSTRet>0.00</vFCPSTRet><vProd>10.00</vProd>" +
            "<vFrete>0.00</vFrete><vSeg>0.00</vSeg><vDesc>0.00</vDesc><vII>0.00</vII><vIPI>0.00</vIPI><vIPIDevol>0.00</vIPIDevol>" +
            "<vPIS>0.00</vPIS><vCOFINS>0.00</vCOFINS><vOutro>0.00</vOutro><vNF>10.00</vNF></ICMSTot></total>" +
            "<transp><modFrete>9</modFrete></transp><pag><detPag><tPag>01</tPag><vPag>10.00</vPag></detPag></pag></infNFe></NFe>";
        }
    }

    public class NFeRecepcaoTestFixture : TaskTestFixtureBase
    {
        public void Executar(NFeRecepcaoTestContext contexto, Action<TaskNFeRecepcao, XmlDocument> configurar,
            bool lotePronto = false, bool propriedadesSingulares = false)
        {
            EmThread(() =>
            {
                PrepararFluxo(contexto);
                var gerador = new GerarXML(0);
                var xml = gerador.LoteNfe(Servicos.NFeMontarLoteUma, contexto.Notas, "4.00", "55");
                if(lotePronto)
                {
                    foreach(var arquivos in gerador.ArquivosTemporariosNumeroLote)
                    {
                        Functions.DeletarArquivo(arquivos.Key);
                        Functions.DeletarArquivo(arquivos.Value);
                    }
                }

                var task = new TaskNFeRecepcao(xml);
                if(!lotePronto)
                {
                    task.NomeArqTempXMLLote = gerador.NomeArqTempXMLLote;
                    task.NomeArqTempTXTLote = gerador.NomeArqTempTXTLote;
                    if(!propriedadesSingulares)
                    {
                        task.ArquivosTemporariosNumeroLote = new List<KeyValuePair<string, string>>(gerador.ArquivosTemporariosNumeroLote);
                    }
                }

                configurar(task, xml);
                var criarAutorizacao = task.CriarAutorizacao;
                task.CriarAutorizacao = (lote, configuracao) =>
                {
                    try
                    {
                        return criarAutorizacao(lote, configuracao);
                    }
                    finally
                    {
                        // Impede sondas externas de diagnóstico, inclusive nas compilações Debug e Beta.
                        configuracao.TipoDFe = (TipoDFe)(-1);
                    }
                };
                task.Execute();
            });
        }

        public void EmThread(Action action) => ExecutarEmThread("0", action);

        public void PrepararFluxo(NFeRecepcaoTestContext contexto)
        {
            var fluxo = new FluxoNfe();
            for(var i = 0; i < contexto.Notas.Count; i++)
            {
                fluxo.InserirNfeFluxo(contexto.Chave(i), "55", contexto.Notas[i].NomeArquivoXML);
            }
        }

        public void VerificarFluxo(NFeRecepcaoTestContext contexto, bool existe)
        {
            EmThread(() =>
            {
                var fluxo = new FluxoNfe();
                for(var i = 0; i < contexto.Notas.Count; i++)
                {
                    Assert.Equal(existe, fluxo.NfeExiste(contexto.Chave(i)));
                }
            });
        }
    }

    internal sealed class AutorizacaoNFeFake : Unimake.Business.DFe.Servicos.NFe.Autorizacao
    {
        private readonly XmlDocument assinado;
        private readonly Action aoExecutar;

        public override XmlDocument ConteudoXMLAssinado => assinado;

        internal AutorizacaoNFeFake(EnviNFe lote, XmlDocument assinado, Action aoExecutar, string retorno = null)
        {
            EnviNFe = lote;
            this.assinado = assinado;
            this.aoExecutar = aoExecutar;
            RetornoWSString = retorno;
            if(retorno != null)
            {
                RetornoWSXML = new XmlDocument();
                RetornoWSXML.LoadXml(retorno);
            }
        }

        public override void Executar() => aoExecutar();
    }
}
