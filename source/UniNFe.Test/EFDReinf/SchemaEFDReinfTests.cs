using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Unimake.Business.DFe;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Utility;
using Unimake.Business.DFe.Xml.EFDReinf;
using Unimake.Business.DFe.Xml.Validar;
using Xunit;

namespace UniNFe.Test.EFDReinf
{
    public class SchemaEFDReinfTests
    {
        [Theory]
        [InlineData(false, true)]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        public void DeveAplicarPatternDoIDDoPacoteNoEventoENoLote(bool emLote, bool idValido)
        {
            var id = idValido ? "ID112ABC34501DEAB2026101012000000001" : "ID112ABC34501DEABA026101012000000001";
            var evento = new Reinf2098
            {
                EvtReabreEvPer = new EvtReabreEvPer
                {
                    ID = id,
                    IdeEvento = new IdeEvento2098
                    {
                        PerApur = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero),
                        TpAmb = TipoAmbiente.Homologacao,
                        ProcEmi = ProcessoEmissaoReinf.AplicativoContribuinte,
                        VerProc = "TESTE"
                    },
                    IdeContri = new IdeContri { TpInsc = TiposInscricao.CNPJ, NrInsc = "12ABC345" }
                }
            };
            XmlDocument xml;
            if (emLote)
            {
                xml = new ReinfEnvioLoteEventos
                {
                    EnvioLoteEventos = new EnvioLoteEventosReinf
                    {
                        IdeContribuinte = new IdeContribuinte { TpInsc = TiposInscricao.CNPJ, NrInsc = "12ABC345" },
                        Eventos = new EventosReinf
                        {
                            Evento = new List<EventoReinf> { new EventoReinf { ID = id, Reinf2098 = evento } }
                        }
                    }
                }.GerarXML();
            }
            else
            {
                xml = evento.GerarXML();
            }
            using (var rsa = RSA.Create())
            {
                rsa.KeySize = 2048;
                var pedido = new CertificateRequest("CN=EFD REINF TESTE", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using (var certificado = pedido.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1)))
                {
                    var resultado = new ValidarEstruturaXML().ValidarServico(xml, new Configuracao
                    {
                        TipoDFe = TipoDFe.EFDReinf,
                        CodigoUF = (int)UFBrasil.AN,
                        TipoAmbiente = TipoAmbiente.Homologacao,
                        CertificadoDigital = certificado
                    });
                    if (!idValido)
                    {
                        Assert.False(resultado.Validado);
                        Assert.Contains("id", resultado.MensagemRetorno);
                        return;
                    }
                    Assert.True(resultado.Validado, resultado.MensagemRetorno);
                    Assert.Equal(id, ((XmlElement)xml.SelectSingleNode("//*[@id]")).GetAttribute("id"));
                    Assert.Equal("#" + id, ((XmlElement)xml.SelectSingleNode("//*[local-name()='Reference']")).GetAttribute("URI"));
                    if (emLote)
                    {
                        var lido = XMLUtility.Deserializar<ReinfEnvioLoteEventos>(xml).EnvioLoteEventos.Eventos.Evento[0];
                        Assert.Equal(id, lido.ID);
                        Assert.Equal(id, lido.Reinf2098.EvtReabreEvPer.ID);
                    }
                    else
                    {
                        Assert.Equal(id, XMLUtility.Deserializar<Reinf2098>(xml).EvtReabreEvPer.ID);
                    }
                }
            }
        }
    }
}