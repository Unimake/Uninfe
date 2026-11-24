using NFe.Components;
using NFe.Service;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using UniNFe.Test.Abstractions;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Security;
using Xunit;

namespace UniNFe.Test.NFeInutilizacao
{
    [CollectionDefinition("Inutilizacao Serial", DisableParallelization = true)]
    public class InutilizacaoCollection
    {
    }

    [Collection("Inutilizacao Serial")]
    public class InutilizacaoDistribuicaoTests : TaskTestFixtureBase
    {
        private const string NsNFe = "http://www.portalfiscal.inf.br/nfe";
        private const string CnpjSintetico = "99999999000191";
        private const string CamposFaixa = "<serie>59</serie><nNFIni>50</nNFIni><nNFFin>52</nNFFin>";

        [Theory]
        [InlineData(55)]
        [InlineData(65)]
        public void Retorno102PublicaProcInutNFeValidoEComPedidoAssinado(int modelo)
        {
            var id = $"ID3526{CnpjSintetico}{modelo}059000000050000000052";
            var pedido = new XmlDocument();
            pedido.LoadXml($"<inutNFe xmlns=\"{NsNFe}\" versao=\"4.00\"><infInut Id=\"{id}\">" +
                $"<tpAmb>2</tpAmb><xServ>INUTILIZAR</xServ><cUF>35</cUF><ano>26</ano><CNPJ>{CnpjSintetico}</CNPJ><mod>{modelo}</mod>" +
                CamposFaixa + "<xJust>Pedido sintetico para teste de inutilizacao</xJust></infInut></inutNFe>");
            var retorno = new XmlDocument();
            retorno.LoadXml($"<retInutNFe xmlns=\"{NsNFe}\" versao=\"4.00\"><infInut>" +
                $"<tpAmb>2</tpAmb><verAplic>TESTE</verAplic><cStat>102</cStat><xMotivo>Retorno sintetico</xMotivo>" +
                $"<cUF>35</cUF><ano>26</ano><CNPJ>{CnpjSintetico}</CNPJ><mod>{modelo}</mod>" +
                CamposFaixa + "<dhRecbto>2026-11-24T12:00:00-03:00</dhRecbto><nProt>135260000000001</nProt></infInut></retInutNFe>");

            using (var rsa = new RSACng(2048))
            {
                var requisicao = new CertificateRequest("CN=INUTILIZACAO TESTE OFFLINE", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using (var certificado = requisicao.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1)))
                {
                    AssinaturaDigital.Assinar(pedido, "inutNFe", "infInut", certificado);
                    using (var contexto = new Contexto(pedido.OuterXml))
                    {
                        ExecutarEmThread("0", () =>
                        {
                            var tarefa = new TaskNFeInutilizacao(contexto.ArquivoEnvio) { vStrXmlRetorno = retorno.OuterXml };
                            var finalizar = typeof(TaskNFeInutilizacao).GetMethod("LerRetornoInut",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            Assert.NotNull(finalizar);
                            finalizar.Invoke(tarefa, null);
                        });

                        var arquivo = Assert.Single(Directory.GetFiles(contexto.Autorizados,
                            "faixa-procInutNFe.xml", SearchOption.AllDirectories));
                        var distribuicao = CarregarXml(arquivo);
                        var raiz = distribuicao.DocumentElement;
                        Assert.Equal("ProcInutNFe", raiz.LocalName);
                        Assert.Equal(NsNFe, raiz.NamespaceURI);
                        Assert.Equal(2, raiz.ChildNodes.Count);
                        Assert.Equal(pedido.DocumentElement.OuterXml, raiz.FirstChild.OuterXml);
                        Assert.Equal(retorno.DocumentElement.OuterXml, raiz.LastChild.OuterXml);
                        var schema = new ValidarSchema();
                        schema.Validar(distribuicao, "NFe.procInutNFe_v4.00.xsd");
                        Assert.True(schema.Success, schema.ErrorMessage);
                        var assinatura = new SignedXml(distribuicao);
                        assinatura.LoadXml((XmlElement)distribuicao.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)[0]);
                        Assert.True(assinatura.CheckSignature(certificado, true));

                        var original = Assert.Single(Directory.GetFiles(contexto.Autorizados,
                            "faixa-ped-inu.xml", SearchOption.AllDirectories));
                        Assert.Equal(pedido.OuterXml, File.ReadAllText(original));
                    }
                }
            }
        }

        private sealed class Contexto : TaskTestContextBase
        {
            public Contexto(string pedido) : base("faixa-ped-inu.xml", pedido)
            {
                Propriedade.PastaExecutavel = PastaTemporaria;
                var empresa = new Empresa
                {
                    CNPJ = CnpjSintetico, UnidadeFederativaCodigo = 35, AmbienteCodigo = 2,
                    PastaXmlEnvio = PastaTemporaria, PastaXmlRetorno = PastaRetorno,
                    PastaXmlEnviado = Path.Combine(PastaTemporaria, "enviados"),
                    PastaXmlErro = Path.Combine(PastaTemporaria, "erro")
                };
                DefinirConfiguracoesEmpresas(new List<Empresa> { empresa });
                Directory.CreateDirectory(empresa.PastaEmpresa);
                Directory.CreateDirectory(empresa.PastaXmlErro);
                empresa.CriarSubPastaEnviado();
                Autorizados = Path.Combine(empresa.PastaXmlEnviado, "Autorizados");
            }

            public string Autorizados { get; }
        }
    }
}
