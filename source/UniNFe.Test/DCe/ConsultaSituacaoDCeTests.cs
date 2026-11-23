using NFe.Components;
using NFe.Service;
using NFe.Service.DCe;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using UniNFe.Test.Abstractions;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Security;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Unimake.Business.DFe.Xml.DCe;
using Xunit;
using XmlDCe = Unimake.Business.DFe.Xml.DCe.DCe;

namespace UniNFe.Test.DCe
{
    [CollectionDefinition("DCe Recuperacao Serial", DisableParallelization = true)]
    public class DCeRecuperacaoCollection
    {
    }

    [Collection("DCe Recuperacao Serial")]
    public class ConsultaSituacaoDCeTests : TaskTestFixtureBase
    {
        private const string NsDCe = "http://www.portalfiscal.inf.br/dce";
        private const string NsAssinatura = "http://www.w3.org/2000/09/xmldsig#";
        private const string CnpjSintetico = "00000000000199";
        private const string NumeroProtocolo = "1410000000000001";
        private const string NomeOriginal = "sintetico-dce.xml";
        private static readonly DateTimeOffset DataEmissao = new DateTimeOffset(2026, 11, 14, 12, 0, 0, TimeSpan.FromHours(-3));

        [Theory]
        [InlineData(100, false)]
        [InlineData(100, true)]
        [InlineData(150, false)]
        [InlineData(150, true)]
        public void RecuperacaoPreservaProtocoloRecebidoEOriginalAssinado(int status, bool protocoloAssinado)
        {
            using (var rsa = new RSACng(2048))
            {
                var requisicao = new CertificateRequest("CN=DCe TESTE OFFLINE", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using (var certificado = requisicao.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1)))
                {
                    var dce = CriarDCe();
                    var original = dce.GerarXML();
                    AssinaturaDigital.Assinar(original, "DCe", "infDCe", certificado);
                    var bytesOriginais = Encoding.UTF8.GetBytes(original.OuterXml);
                    var chave = dce.InfDCe.Chave;
                    var consulta = new ConsSitDCe { Versao = "1.00", TpAmb = TipoAmbiente.Homologacao, ChDCe = chave };
                    var retorno = CriarRetorno(chave, original, status, protocoloAssinado);
                    if (protocoloAssinado)
                        AssinaturaDigital.Assinar(retorno, "protDCe", "infProt", certificado);
                    var protocoloRecebido = retorno.DocumentElement["protDCe", NsDCe];

                    using (var contexto = new Contexto(consulta))
                    {
                        File.WriteAllBytes(contexto.ArquivoEmProcessamento, bytesOriginais);
                        ExecutarEmThread("0", () =>
                        {
                            var fluxo = new FluxoNfe();
                            fluxo.InserirNfeFluxo("DCe" + chave, "99", contexto.ArquivoEmProcessamento);
                            var tarefa = new TaskConsultaSituacaoDCe(contexto.ArquivoEnvio) { vStrXmlRetorno = retorno.OuterXml };
                            // Usa a mesma finalização de Execute após a consulta, sem executar o transporte fiscal.
                            var finalizar = typeof(TaskConsultaSituacaoDCe).GetMethod("LerRetornoSitDCe",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            Assert.NotNull(finalizar);
                            finalizar.Invoke(tarefa, new object[]
                            {
                                consulta, XMLUtility.Deserializar<RetConsSitDCe>(retorno), retorno, 0
                            });
                            Assert.False(fluxo.NfeExiste("DCe" + chave));
                        });

                        Assert.False(File.Exists(contexto.ArquivoEmProcessamento));
                        Assert.Equal(bytesOriginais, File.ReadAllBytes(contexto.ArquivoOriginal));
                        var distribuido = CarregarXml(contexto.ArquivoDistribuicao);
                        Assert.Equal("dceProc", distribuido.DocumentElement.LocalName);
                        Assert.Equal(NsDCe, distribuido.DocumentElement.NamespaceURI);
                        Assert.Equal(2, distribuido.DocumentElement.ChildNodes.Count);
                        var documentoDistribuido = Assert.IsType<XmlElement>(distribuido.DocumentElement.FirstChild);
                        var protocoloDistribuido = Assert.IsType<XmlElement>(distribuido.DocumentElement.LastChild);
                        Assert.Equal(original.DocumentElement.OuterXml, documentoDistribuido.OuterXml);
                        Assert.Equal(protocoloRecebido.OuterXml, protocoloDistribuido.OuterXml);
                        Assert.Equal(status.ToString(), protocoloDistribuido["infProt", NsDCe]["cStat", NsDCe].InnerText);
                        ValidarAssinatura(documentoDistribuido, "DCe" + chave, certificado);
                        if (protocoloAssinado)
                            ValidarAssinatura(protocoloDistribuido, "ID" + NumeroProtocolo, certificado);
                        var schema = new ValidarSchema();
                        schema.Validar(distribuido, "DCe.procDCe_v1.00.xsd");
                        Assert.True(schema.Success, schema.ErrorMessage);
                    }
                }
            }
        }

        private static void ValidarAssinatura(XmlElement elemento, string id, X509Certificate2 certificado)
        {
            var xml = new XmlDocument { PreserveWhitespace = true };
            xml.LoadXml(elemento.OuterXml);
            var assinatura = xml.DocumentElement["Signature", NsAssinatura];
            Assert.NotNull(assinatura);
            var verificador = new SignedXml(xml);
            verificador.LoadXml(assinatura);
            Assert.Single(verificador.SignedInfo.References);
            var referencia = Assert.IsType<Reference>(verificador.SignedInfo.References[0]);
            Assert.Equal("#" + id, referencia.Uri);
            Assert.True(verificador.CheckSignature(certificado, true));
        }

        private static XmlDocument CriarRetorno(string chave, XmlDocument original, int status, bool protocoloAssinado)
        {
            var digest = original.DocumentElement["Signature", NsAssinatura]["SignedInfo", NsAssinatura]
                ["Reference", NsAssinatura]["DigestValue", NsAssinatura].InnerText;
            return new RetConsSitDCe
            {
                Versao = "1.00", TpAmb = TipoAmbiente.Homologacao, CUF = UFBrasil.PR, ChDCe = chave,
                VerAplic = "OFFLINE", CStat = status, XMotivo = "CONSULTA SINTETICA", DhRecbto = DataEmissao,
                ProtDCe = new ProtDCe
                {
                    Versao = "1.00",
                    InfProt = new InfProt
                    {
                        Id = protocoloAssinado ? "ID" + NumeroProtocolo : null, TpAmb = TipoAmbiente.Homologacao,
                        VerAplic = "OFFLINE", ChDCe = chave, DhRecbto = DataEmissao, NProt = NumeroProtocolo,
                        CStat = status, XMotivo = "PROTOCOLO SINTETICO", DigVal = digest
                    }
                }
            }.GerarXML();
        }

        private static XmlDCe CriarDCe()
        {
            var dce = new XmlDCe
            {
                InfDCe = new InfDCe
                {
                    Versao = "1.00",
                    Ide = new Ide
                    {
                        CUF = UFBrasil.PR, CDC = "189670", Mod = ModeloDFe.DCe, Serie = 59, NDC = 50,
                        DhEmi = DataEmissao, TpEmis = TipoEmissao.Normal, TpEmit = TipoEmitenteDCe.EmissorProprio,
                        NSiteAutoriz = "0", TpAmb = TipoAmbiente.Homologacao, VerProc = "OFFLINE"
                    },
                    Emit = new Emit
                    {
                        CNPJ = CnpjSintetico, XNome = "EMITENTE SINTETICO",
                        EnderEmit = new EnderEmit
                        {
                            XLgr = "RUA SINTETICA", Nro = "1", XBairro = "TESTE", CMun = "4106902",
                            XMun = "CURITIBA", UF = UFBrasil.PR, CEP = "80010000", CPais = "1058", XPais = "BRASIL"
                        }
                    },
                    Dest = new Dest
                    {
                        CPF = "12345678909", XNome = "DESTINATARIO SINTETICO",
                        EnderDest = new EnderDest
                        {
                            XLgr = "RUA SINTETICA", Nro = "2", XBairro = "TESTE", CMun = "4106902",
                            XMun = "CURITIBA", UF = UFBrasil.PR, CEP = "80010000", CPais = "1058", XPais = "BRASIL",
                            Email = "destinatario@example.com"
                        }
                    },
                    Det = new List<Det>
                    {
                        new Det { NItem = 1, Prod = new Prod { XProd = "PRODUTO SINTETICO", NCM = "99", QCom = 1, VUnCom = 10, VProd = 10 } }
                    },
                    Total = new Total { VDC = 10 },
                    Transp = new Transp { ModTrans = ModalidadeTransporteDCe.Correios, CNPJTransp = CnpjSintetico },
                    InfDec = new InfDec { XObs1 = "DECLARACAO SINTETICA UM", XObs2 = "DECLARACAO SINTETICA DOIS" }
                }
            };
            dce.InfDCeSupl = new InfDCeSupl
            {
                QrCodDCe = "https://example.invalid/dce/consulta?chDCe=" + dce.InfDCe.Chave + "&tpAmb=2",
                UrlChave = "https://example.invalid/dce/consulta"
            };
            return dce;
        }

        private sealed class Contexto : TaskTestContextBase
        {
            public Contexto(ConsSitDCe consulta) : base("sintetico-ped-sit.xml", consulta.GerarXML().OuterXml)
            {
                Propriedade.PastaExecutavel = PastaTemporaria;
                var empresa = new Empresa
                {
                    CNPJ = CnpjSintetico, UnidadeFederativaCodigo = 41, AmbienteCodigo = 2,
                    PastaXmlEnvio = PastaTemporaria, PastaXmlRetorno = PastaRetorno,
                    PastaXmlEnviado = Path.Combine(PastaTemporaria, "enviados"),
                    PastaXmlErro = Path.Combine(PastaTemporaria, "erro"),
                    SalvarSomenteXMLDistribuicao = true, CompararDigestValueDFeRetornadoSEFAZ = true
                };
                DefinirConfiguracoesEmpresas(new List<Empresa> { empresa });
                Directory.CreateDirectory(empresa.PastaEmpresa);
                Directory.CreateDirectory(empresa.PastaXmlErro);
                empresa.CriarSubPastaEnviado();
                ArquivoEmProcessamento = Path.Combine(empresa.PastaXmlEnviado, "EmProcessamento", NomeOriginal);
                ArquivoOriginal = Path.Combine(empresa.PastaXmlEnviado, "Originais", "202611", NomeOriginal);
                ArquivoDistribuicao = Path.Combine(empresa.PastaXmlEnviado, "Autorizados", "202611", "sintetico-procDCe.xml");
            }

            public string ArquivoEmProcessamento { get; }
            public string ArquivoOriginal { get; }
            public string ArquivoDistribuicao { get; }
        }
    }
}
