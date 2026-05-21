using EBank.Solutions.Primitives.Billet.Request;
using EBank.Solutions.Primitives.Billet.Response;
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
    public class TaskBoletoAlterarVencto : TaskAbst
    {
        public static DebugScope<DebugStateObject> debugScope;

        public TaskBoletoAlterarVencto(string arquivo)
        {
            Servico = Servicos.BoletoAlterarVencto;
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
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\eBoleto\BoletoAlterarVencto_1_00.xsd"),
                        nRetornoTipoArq = 1
                    }
                };

                validarXML.ValidarArqXML(ConteudoXML, NomeArquivoXML);
                if (validarXML.Retorno != 0)
                {
                    throw new Exception(validarXML.RetornoString.Replace("\r\n", ""));
                }

                #endregion

                #region Criar objeto da consulta do boleto para enviar ao eBank

                var extendPaymentRequest = new ExtendPaymentRequest
                {
                    ConfigurationId = TFunctions.GetXmlValue(ConteudoXML, "ConfigurationId"),
                    Testing = TFunctions.GetXmlBoolValue(ConteudoXML, "Testing"),
                    NumeroNoBanco = TFunctions.GetXmlValue(ConteudoXML, "NumeroNoBanco"),
                    DataVencimento = TFunctions.GetXmlDateTimeValue(ConteudoXML, "DataVencimento")
                };

                #endregion

                #region Autenticar nas APIs da Unimake

                var useHomologServer = AuthApiScopeHelper.ResolveUseHomologServer(extendPaymentRequest.Testing, ConteudoXML.DocumentElement);
                debugScope = AuthApiScopeHelper.CreateDebugScopeIfNeeded(useHomologServer, "https://ebank.sandbox.unimake.software/api/v1/");

                var authenticatedScope = AuthApiScopeHelper.CreateAuthenticatedScopeEBank(emp);

                #endregion

                #region Enviar instrução para alterar o vencimento do boleto

                var billetService = new BilletService();
                var extendPaymentResponse = await billetService.ExtendPaymentAsync(extendPaymentRequest, authenticatedScope);

                authenticatedScope.Dispose();

                #endregion

                #region Gravar XML de Retorno

                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoAlterarVencto).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoAlterarVencto).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                if (extendPaymentResponse.StatusCode == System.Net.HttpStatusCode.Accepted || extendPaymentResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    GerarXmlRetorno(pathXml, "0", "");
                }
                else
                {
                    var traceId = ApiExceptionHelper.ExtrairTraceId(extendPaymentResponse);
                    GerarXmlRetorno(pathXml, "1", $"Não foi possível alterar o vencimento do boleto. Tente novamente mais tarde. (Status Code: {((int)extendPaymentResponse.StatusCode).ToString()})" +
                    (!string.IsNullOrWhiteSpace(extendPaymentResponse.Codigo) ? " - (Erro: " + extendPaymentResponse.Codigo +
                    (!string.IsNullOrWhiteSpace(extendPaymentResponse.Mensagem) ? " - " + extendPaymentResponse.Mensagem : "") + ")" : ""), traceId);
                }

                #endregion
            }
            catch (Exception ex)
            {
                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoAlterarVencto).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoAlterarVencto).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                var lastException = ex.GetLastException();
                var traceId = ApiExceptionHelper.ExtrairTraceId(lastException);
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
                motivo = "Vencimento do boleto alterado";
            }

            ApiExceptionHelper.GravarXmlRetornoEBoleto(path, "BoletoAlterarVenctoResponse", status, motivo, traceId);
        }
    }
}