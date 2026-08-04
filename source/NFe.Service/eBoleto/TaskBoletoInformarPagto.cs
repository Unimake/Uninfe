using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using BoletoCancelarService = Unimake.Business.DFe.Servicos.EBoleto.BoletoCancelar;

namespace NFe.Service
{
    public class TaskBoletoInformarPagto : TaskAbst
    {
        public TaskBoletoInformarPagto(string arquivo)
        {
            Servico = Servicos.BoletoInformarPagto;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoInformarPagto).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoInformarPagto).RetornoXML;
            var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

            try
            {
                if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].AppID) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].Secret))
                {
                    throw new Exception("Para utilizar o serviço do eBoleto é necessário configurar no UniNFe o AppID e Secret do eBank.");
                }

                ExecuteDLL(emp);
            }
            catch (Exception ex)
            {
                var lastException = ex.GetLastException();
                var traceId = ApiExceptionHelper.ExtrairTraceId(lastException);
                ApiExceptionHelper.GravarXmlRetornoEBoleto(pathXml, "BoletoInformarPagtoResponse", "999", lastException.Message.Replace("\r\n", " | "), traceId);
            }
            finally
            {
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                }
            }
        }

        #region ExecuteDLL

        private void ExecuteDLL(int emp)
        {
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoInformarPagto).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoInformarPagto).RetornoXML;

            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo,
                AppId = Empresas.Configuracoes[emp].AppID,
                Secret = Empresas.Configuracoes[emp].Secret
            };

            try
            {
                var xmlCancelar = ConverterRaiz(ConteudoXML.OuterXml, "BoletoCancelar");

                using (var boleto = new BoletoCancelarService(xmlCancelar, configuracao))
                {
                    boleto.Executar();
                    vStrXmlRetorno = ConverterRaiz(boleto.RetornoWSString, "BoletoInformarPagtoResponse");

                    if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                    {
                        throw new Exception("A implementação do serviço eBoleto BoletoCancelar não retornou RetornoWSString. Atualize a DLL para fornecer o XML de retorno pronto.");
                    }

                    vStrXmlRetorno = AdicionarUniNFeVersaoAoRetorno(vStrXmlRetorno);

                    XmlRetorno(finalArqEnvio, finalArqRetorno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao executar DLL eBoleto BoletoCancelar para compatibilidade com BoletoInformarPagto: {ex.Message}", ex);
            }
        }

        private static string ConverterRaiz(string conteudoXml, string nomeRaiz)
        {
            if (string.IsNullOrWhiteSpace(conteudoXml))
            {
                return conteudoXml;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(conteudoXml);

            if (xmlDoc.DocumentElement != null && xmlDoc.DocumentElement.Name != nomeRaiz)
            {
                var raizAtual = xmlDoc.DocumentElement;
                var novaRaiz = xmlDoc.CreateElement(raizAtual.Prefix, nomeRaiz, raizAtual.NamespaceURI);

                foreach (XmlAttribute atributo in raizAtual.Attributes)
                {
                    novaRaiz.Attributes.Append((XmlAttribute)xmlDoc.ImportNode(atributo, true));
                }

                while (raizAtual.HasChildNodes)
                {
                    novaRaiz.AppendChild(raizAtual.FirstChild);
                }

                xmlDoc.ReplaceChild(novaRaiz, raizAtual);
            }

            return xmlDoc.OuterXml;
        }

        private string AdicionarUniNFeVersaoAoRetorno(string xmlRetorno)
        {
            if (string.IsNullOrWhiteSpace(xmlRetorno))
            {
                return xmlRetorno;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlRetorno);

            var root = xmlDoc.DocumentElement;
            if (root == null || root["UniNFeVersao"] != null)
            {
                return xmlRetorno;
            }

            var versaoNode = xmlDoc.CreateElement("UniNFeVersao");
            versaoNode.InnerText = Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-");
            root.AppendChild(versaoNode);

            return xmlDoc.OuterXml;
        }
        #endregion ExecuteDLL
    }
}
