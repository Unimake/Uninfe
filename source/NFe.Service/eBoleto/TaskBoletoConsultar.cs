using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    public class TaskBoletoConsultar : TaskAbst
    {
        public TaskBoletoConsultar(string arquivo)
        {
            Servico = Servicos.BoletoConsultar;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoConsultar).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoConsultar).RetornoXML;
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
                ApiExceptionHelper.GravarXmlRetornoEBoleto(pathXml, "BoletoConsultarResponse", "999", lastException.Message.Replace("\r\n", " | "), traceId);
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
            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoConsultar).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.BoletoConsultar).RetornoXML;

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
                var assembly = typeof(Configuracao).Assembly;
                var boletoType = assembly.GetType("Unimake.Business.DFe.Servicos.EBoleto.BoletoConsultar");
                if (boletoType == null) throw new Exception("A implementação do serviço eBoleto BoletoConsultar não foi localizada na DLL.");

                var boletoInstance = Activator.CreateInstance(boletoType, new object[] { ConteudoXML.OuterXml, configuracao });
                var executarMethod = boletoInstance.GetType().GetMethod("Executar");
                if (executarMethod == null) throw new Exception("A implementação do serviço eBoleto BoletoConsultar não expõe o método Executar().");
                executarMethod.Invoke(boletoInstance, null);

                var retornoProp = boletoInstance.GetType().GetProperty("RetornoWSString");
                if (retornoProp != null) vStrXmlRetorno = retornoProp.GetValue(boletoInstance)?.ToString();

                if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                {
                    throw new Exception("A implementação do serviço eBoleto BoletoConsultar não retornou RetornoWSString. Atualize a DLL para fornecer o XML de retorno pronto.");
                }

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                var disposeMethod = boletoInstance.GetType().GetMethod("Dispose");
                disposeMethod?.Invoke(boletoInstance, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao executar DLL eBoleto BoletoConsultar: {ex.Message}", ex);
            }
        }

        #endregion ExecuteDLL
    }
}
