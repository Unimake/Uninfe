using NFe.Components;
using NFe.Service;
using NFe.Service.NFeABI;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using UniNFe.Test.Abstractions;

namespace UniNFe.Test.NFeABI
{
    public abstract class NFeABITestFixture : TaskTestFixtureBase
    {
        internal const string Chave = "41260912345678000123770010000000011012345670";
        internal const string Namespace = "http://www.portalfiscal.inf.br/nfeabi";

        internal static string PedidoStatus(int ambiente = 2)
        {
            return "<consStatServNFeABI xmlns=\"" + Namespace + "\" versao=\"1.00\"><tpAmb>" + ambiente +
                "</tpAmb><cUF>41</cUF><xServ>STATUS</xServ></consStatServNFeABI>";
        }

        internal static string PedidoConsulta(string chave = Chave, int ambiente = 2)
        {
            return "<consSitNFeABI xmlns=\"" + Namespace + "\" versao=\"1.00\"><tpAmb>" + ambiente +
                "</tpAmb><xServ>CONSULTAR</xServ><chNFeABI>" + chave + "</chNFeABI></consSitNFeABI>";
        }

        internal static XmlDocument Documento(bool responsavel = true)
        {
            var xml = new XmlDocument();
            xml.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NFeABI", "Resources", "NFeABI.xml"));
            // O exemplo mínimo não possui o responsável técnico exigido pelo schema atual.
            if(responsavel)
            {
                var grupo = xml.CreateElement("infRespTec", Namespace);
                grupo.InnerXml = "<CNPJ xmlns=\"" + Namespace + "\">12345678000123</CNPJ>" +
                    "<xContato xmlns=\"" + Namespace + "\">CONTATO TESTE</xContato>" +
                    "<email xmlns=\"" + Namespace + "\">teste@example.com</email>" +
                    "<fone xmlns=\"" + Namespace + "\">41999999999</fone>";
                xml.GetElementsByTagName("infNFeABI")[0].AppendChild(grupo);
            }
            return xml;
        }

        internal static string Protocolo(string chave = Chave, string digest = "AA==", int status = 100)
        {
            return "<protNFeABI xmlns=\"" + Namespace + "\" versao=\"1.00\"><infProt><tpAmb>2</tpAmb>" +
                "<verAplic>TESTE</verAplic><chNFeABI>" + chave + "</chNFeABI><dhRecbto>2026-09-14T12:01:00-03:00</dhRecbto>" +
                "<nProt>141260000000001</nProt><digVal>" + digest + "</digVal><cStat>" + status +
                "</cStat><xMotivo>Retorno teste</xMotivo></infProt></protNFeABI>";
        }

        internal static string Retorno(string raiz, int status, string protocolo = "")
        {
            return "<" + raiz + " xmlns=\"" + Namespace + "\" versao=\"1.00\"><tpAmb>2</tpAmb>" +
                "<verAplic>TESTE</verAplic><cStat>" + status + "</cStat><xMotivo>Retorno teste</xMotivo>" +
                "<cUF>41</cUF><dhRecbto>2026-09-14T12:01:00-03:00</dhRecbto>" + protocolo + "</" + raiz + ">";
        }

        internal void Executar(TaskAbst task)
        {
            ExecutarEmThread("0", task.Execute);
        }

        internal void Processar(Contexto contexto)
        {
            ExecutarEmThread("0", () => new Processar().ProcessaArquivo(0, contexto.ArquivoEnvio));
        }

        internal sealed class Contexto : TaskTestContextBase, IDisposable
        {
            private readonly bool proxyAnterior = ConfiguracaoApp.Proxy;
            private readonly bool proxyAutoAnterior = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
            private readonly bool conexaoAnterior = ConfiguracaoApp.ChecarConexaoInternet;
            private readonly string servidorAnterior = ConfiguracaoApp.ProxyServidor;
            private readonly int portaAnterior = ConfiguracaoApp.ProxyPorta;
            private readonly string usuarioAnterior = ConfiguracaoApp.ProxyUsuario;
            private readonly string senhaAnterior = ConfiguracaoApp.ProxySenha;
            private X509Certificate2 certificado;

            internal Contexto(string nome, string xml) : base(nome, xml)
            {
                Propriedade.PastaExecutavel = PastaTemporaria;
                Empresa = new Empresa
                {
                    CNPJ = "12345678000123",
                    UnidadeFederativaCodigo = 41,
                    PastaXmlEnvio = PastaTemporaria,
                    PastaXmlEnviado = Path.Combine(PastaTemporaria, "Enviados"),
                    PastaXmlRetorno = PastaRetorno,
                    PastaXmlErro = Path.Combine(PastaTemporaria, "Erro"),
                    PastaXmlEmLote = Path.Combine(PastaTemporaria, "Lote"),
                    PastaValidar = Path.Combine(PastaTemporaria, "Validar"),
                    PastaBackup = string.Empty,
                    AmbienteCodigo = 2,
                    Servico = TipoAplicativo.NFeABI,
                    UsaCertificado = false,
                    AtivarPreparacaoTLSAntesEnvioXML = true
                };
                Directory.CreateDirectory(Empresa.PastaXmlErro);
                Directory.CreateDirectory(Empresa.PastaXmlEmLote);
                Directory.CreateDirectory(Empresa.PastaValidar);
                Directory.CreateDirectory(Empresa.PastaEmpresa);
                Empresa.CriarSubPastaEnviado();
                DefinirConfiguracoesEmpresas(new List<Empresa> { Empresa });
                ConfiguracaoApp.ChecarConexaoInternet = false;
                ConfiguracaoApp.Proxy = false;
                ConfiguracaoApp.DetectarConfiguracaoProxyAuto = false;
            }

            internal Empresa Empresa { get; }
            internal string EmProcessamento => Path.Combine(Empresa.PastaXmlEnviado, "EmProcessamento");

            internal X509Certificate2 UsarCertificado()
            {
                using(var rsa = RSA.Create(2048))
                {
                    certificado = new CertificateRequest("CN=UniNFe Teste", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                        .CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
                }
                Empresa.X509Certificado = certificado;
                return certificado;
            }

            internal string GuardarDocumento(string nome = "pedido-nfeabi.xml", bool fluxo = false)
            {
                var arquivo = Path.Combine(EmProcessamento, nome);
                Documento().Save(arquivo);
                if(fluxo) new FluxoNfe(0).InserirNfeFluxo("NFeABI" + Chave, "77", arquivo);
                return arquivo;
            }

            internal string Destino(string nome, string pasta = "Autorizados")
            {
                return Path.Combine(Empresa.PastaXmlEnviado, pasta,
                    Empresa.DiretorioSalvarComo.ToString(new DateTime(2026, 9, 14)), nome);
            }

            public new void Dispose()
            {
                TaskConsultaStatusNFeABI.ExecutarConsulta = null;
                TaskConsultaSituacaoNFeABI.ExecutarConsulta = null;
                TaskNFeABIRecepcaoSinc.ExecutarAutorizacao = null;
                ConfiguracaoApp.Proxy = proxyAnterior;
                ConfiguracaoApp.DetectarConfiguracaoProxyAuto = proxyAutoAnterior;
                ConfiguracaoApp.ChecarConexaoInternet = conexaoAnterior;
                ConfiguracaoApp.ProxyServidor = servidorAnterior;
                ConfiguracaoApp.ProxyPorta = portaAnterior;
                ConfiguracaoApp.ProxyUsuario = usuarioAnterior;
                ConfiguracaoApp.ProxySenha = senhaAnterior;
                certificado?.Dispose();
                base.Dispose();
            }
        }
    }
}
