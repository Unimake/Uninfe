using EBank.Solutions.Primitives.Billet.Request;
using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.AuthServer.Security.Scope;
using Unimake.EBank.Solutions.Services.Billet;
using Unimake.Primitives.UDebug;

namespace NFe.Service
{
    public class TaskBoletoInformarPagto : TaskAbst
    {
        public static DebugScope<DebugStateObject> debugScope;

        public TaskBoletoInformarPagto(string arquivo)
        {
            Servico = Servicos.BoletoInformarPagto;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override async void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                if (string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].AppID) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].Secret))
                {
                    throw new Exception("Para utilizar o serviço do eBoleto é necessário configurar no UniNFe o AppID e Secret do eBank.");
                }

                #region Validar o XML

                var validarXML = new ValidarXML
                {
                    TipoArqXml = new TipoArquivoXML
                    {
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\eBoleto\BoletoInformarPagto_1_00.xsd"),
                        nRetornoTipoArq = 1
                    }
                };

                validarXML.ValidarArqXML(ConteudoXML, NomeArquivoXML);
                if (validarXML.Retorno != 0)
                {
                    throw new Exception(validarXML.RetornoString.Replace("\r\n", ""));
                }

                #endregion

                #region Criar objeto de envio de instrução do boleto


                var informPaymentRequest = new BaixarRequest
                {
                    ConfigurationId = TFunctions.GetXmlValue(ConteudoXML, "ConfigurationId"),
                    Testing = TFunctions.GetXmlBoolValue(ConteudoXML, "Testing"),
                    NumeroNoBanco = TFunctions.GetXmlValue(ConteudoXML, "NumeroNoBanco")
                };

                #endregion

                #region Autenticar nas APIs da Unimake

                var useHomologServer = AuthApiScopeHelper.ResolveUseHomologServer(informPaymentRequest.Testing, ConteudoXML.DocumentElement);
                debugScope = AuthApiScopeHelper.CreateDebugScopeIfNeeded(useHomologServer, "https://ebank.sandbox.unimake.software/api/v1/");

                var authenticatedScope = AuthApiScopeHelper.CreateAuthenticatedScopeEBank(emp);

                #endregion

                #region Enviar instrução para marcar o boleto como pago

                var billetService = new BilletService();
                var informPaymentResponse = await billetService.BaixarAsync(informPaymentRequest, authenticatedScope);

                authenticatedScope.Dispose();

                #endregion

                #region Gravar XML de Retorno

                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoInformarPagto).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoInformarPagto).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                if (informPaymentResponse.StatusCode == System.Net.HttpStatusCode.Accepted || informPaymentResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    GerarXmlRetorno(pathXml, "0", "");
                }
                else
                {
                    var traceId = BoletoRetornoHelper.ExtrairTraceId(informPaymentResponse);
                    GerarXmlRetorno(pathXml, "1", $"Não foi possível marcar o boleto como pago. Tente novamente mais tarde. (Status Code: {((int)informPaymentResponse.StatusCode).ToString()})" + 
                        (!string.IsNullOrWhiteSpace(informPaymentResponse.Codigo) ? " - (Erro: " + informPaymentResponse.Codigo + 
                        (!string.IsNullOrWhiteSpace(informPaymentResponse.Mensagem) ? " - " + informPaymentResponse.Mensagem : "") + ")" : ""), traceId);
                }

                #endregion
            }
            catch (Exception ex)
            {
                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoInformarPagto).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoInformarPagto).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                var lastException = ex.GetLastException();
                var traceId = BoletoRetornoHelper.ExtrairTraceId(lastException);
                GerarXmlRetorno(pathXml, "999", lastException.Message.Replace("\r\n", " | "), traceId);
            }
            finally
            {
                try
                {
                    //Deletar o arquivo de solicitação do serviço
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML de solicitação do serviço,

                    //infelizmente não posso fazer mais nada, o UniNFe vai tentar mandar
                    //o arquivo novamente para o webservice
                    //Wandrey 09/03/2010
                }
            }
        }

        private void GerarXmlRetorno(string path, string status, string motivo, string traceId = "")
        {
            if (status == "0")
            {
                motivo = "Instrução para marcar o boleto como pago enviado com sucesso";
            }

            BoletoRetornoHelper.GravarXmlRetorno(path, "BoletoInformarPagtoResponse", status, motivo, traceId);
        }
    }
}