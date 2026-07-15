using NFe.Components;
using NFe.Service;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using UniNFe.Test.Abstractions;
using Xunit;

namespace UniNFe.Test.eBoleto
{
    public class BoletoRegistrarTestContext : TaskTestContextBase
    {
        #region Public Properties

        public string ArquivoRetorno { get; }
        public string ArquivoRetornoPDF { get; }

        #endregion Public Properties

        #region Public Constructors

        public BoletoRegistrarTestContext(string nomeArquivoEnvio, string xmlEnvio)
            : base(nomeArquivoEnvio, xmlEnvio)
        {
            DefinirConfiguracoesEmpresas(new List<Empresa>
            {
                new Empresa
                {
                    AppID = Environment.GetEnvironmentVariable("UNIMAKE_APPKEY"),
                    Secret = Environment.GetEnvironmentVariable("UNIMAKE_SECRET"),
                    MesmosDadosEb_Mb = true,
                    PastaXmlRetorno = PastaRetorno
                }
            });

            var extensao = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar);
            ArquivoRetorno = Path.Combine(PastaRetorno, Functions.ExtrairNomeArq(ArquivoEnvio, extensao.EnvioXML) + extensao.RetornoXML);
            ArquivoRetornoPDF = ArquivoRetorno.Replace(".xml", ".pdf");
        }

        #endregion Public Constructors
    }

    [CollectionDefinition("eBoleto", DisableParallelization = true)]
    public class EBoletoSerialCollection : ICollectionFixture<EBoletoTestFixture>
    {
    }

    public class EBoletoTestFixture : TaskTestFixtureBase
    {
        #region Internal Methods

        internal static string RecuperarXmlDeArquivo(string path) => File.ReadAllText(path);

        #endregion Internal Methods

        #region Public Methods

        public static string GerarXmlEnvioBoletoRegistrarValido() =>
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<BoletoRegistrar versao=\"1.00\">" +
            "<ConfigurationId>CFG-001</ConfigurationId>" +
            "<Aceite>true</Aceite>" +
            "<Emissao>2026-01-01</Emissao>" +
            "<Especie>1</Especie>" +
            "<NumeroParcela>1</NumeroParcela>" +
            "<Pagador>" +
            "<Nome>Cliente Teste</Nome>" +
            "<Inscricao>12345678901</Inscricao>" +
            "<TipoInscricao>1</TipoInscricao>" +
            "<Endereco>" +
            "<Logradouro>Rua Teste</Logradouro>" +
            "<Numero>100</Numero>" +
            "<Complemento>Sala 1</Complemento>" +
            "<Bairro>Centro</Bairro>" +
            "<Cidade>Sao Paulo</Cidade>" +
            "<UF>SP</UF>" +
            "<CEP>01000-000</CEP>" +
            "</Endereco>" +
            "</Pagador>" +
            "<PDFConfig>" +
            "<TryGeneratePDF>false</TryGeneratePDF>" +
            "</PDFConfig>" +
            "<ValorNominal>100.00</ValorNominal>" +
            "<Vencimento>2026-01-15</Vencimento>" +
            "<Testing>true</Testing>" +
            "</BoletoRegistrar>";

        public BoletoRegistrarTestContext CriarContextoBoletoRegistrar(string nomeArquivoEnvio, string xmlEnvio)
        {
            return new BoletoRegistrarTestContext(nomeArquivoEnvio, xmlEnvio);
        }

        public void ExecutarTaskBoletoRegistrar(BoletoRegistrarTestContext contexto)
        {
            ExecutarEmThread("0", () =>
            {
                var task = new TaskBoletoRegistrar(contexto.ArquivoEnvio);
                task.Execute();
            });
        }

        #endregion Public Methods
    }
}
