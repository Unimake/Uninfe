using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using BoletoCancelarService = Unimake.Business.DFe.Servicos.EBoleto.BoletoCancelar;

namespace NFe.Service
{
    public class TaskBoletoCancelar : TaskAbst
    {
        public TaskBoletoCancelar(string arquivo)
        {
            Servico = Servicos.BoletoCancelar;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoCancelar).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoCancelar).RetornoXML;
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
                ApiExceptionHelper.GravarXmlRetornoEBoleto(pathXml, "BoletoCancelarResponse", "999", lastException.Message.Replace("\r\n", " | "), traceId);
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
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoCancelar).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoCancelar).RetornoXML;

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
                using (var boleto = new BoletoCancelarService(ConteudoXML.OuterXml, configuracao))
                {
                    boleto.Executar();
                    vStrXmlRetorno = boleto.RetornoWSString;

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
                throw new Exception($"Erro ao executar DLL eBoleto BoletoCancelar: {ex.Message}", ex);
            }
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
