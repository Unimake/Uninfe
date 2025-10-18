using EBank.Solutions.Primitives.Billet.Request;
using EBank.Solutions.Primitives.Enumerations.Billet;
using EBank.Solutions.Primitives.Enumerations.Instructions;
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
    public class TaskBoletoEnviarInstrucao : TaskAbst
    {
        public static DebugScope<DebugStateObject> debugScope;

        public TaskBoletoEnviarInstrucao(string arquivo)
        {
            Servico = Servicos.BoletoEnviarInstrucao;
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
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\eBoleto\BoletoEnviarInstrucao_1_00.xsd"),
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

                var putInstructionsRequest = new PutInstructionsRequest
                {
                    ConfigurationId = TFunctions.GetXmlValue(ConteudoXML, "ConfigurationId"),
                    Testing = TFunctions.GetXmlBoolValue(ConteudoXML, "Testing"),
                    Data = TFunctions.GetXmlDateTimeValue(ConteudoXML, "Data"),
                    Instrucao = (Instrucao)TFunctions.GetXmlIntValue(ConteudoXML, "Instrucao"),
                    NossoNumero = TFunctions.GetXmlValue(ConteudoXML, "NossoNumero"),
                    NumeroNoBanco = TFunctions.GetXmlValue(ConteudoXML, "NumeroNoBanco"),
                    QuantidadeDiasDesconto = TFunctions.GetXmlIntValue(ConteudoXML, "QuantidadeDiasDesconto"),
                    SeuNumero = TFunctions.GetXmlValue(ConteudoXML, "SeuNumero"),
                    Valor = TFunctions.GetXmlDecimalValue(ConteudoXML, "Valor")
                };

                if (TFunctions.GetXmlIntValue(ConteudoXML, "InstrucaoAdicional", true) != null)
                {
                    putInstructionsRequest.InstrucaoAdicional = (InstrucaoAdicional)TFunctions.GetXmlIntValue(ConteudoXML, "InstrucaoAdicional");
                }

                if (TFunctions.GetXmlIntValue(ConteudoXML, "TipoDesconto", true) != null)
                {
                    putInstructionsRequest.TipoDesconto = (TipoDesconto)TFunctions.GetXmlIntValue(ConteudoXML, "TipoDesconto");
                }

                #endregion

                #region Autenticar nas APIs da Unimake

                var useHomologServer = false;

                if (ConteudoXML.GetElementsByTagName("UseHomologServer").Count > 0)
                {
                    useHomologServer = Convert.ToBoolean(ConteudoXML.GetElementsByTagName("UseHomologServer")[0].InnerText);
                }

                debugScope = null;
                if (useHomologServer)
                {
                    debugScope = new DebugScope<DebugStateObject>(new DebugStateObject
                    {
                        AuthServerUrl = "https://auth.sandbox.unimake.software/api/auth/",
                        AnotherServerUrl = "https://ebank.sandbox.unimake.software/api/v1/"
                    });
                }

                var authenticatedScope = new AuthenticatedScope(new Unimake.Primitives.Security.Credentials.AuthenticationToken
                {
                    AppId = Empresas.Configuracoes[emp].AppID,
                    Secret = Empresas.Configuracoes[emp].Secret
                });

                #endregion

                #region Enviar a instrução

                var billetService = new BilletService();
                var putInstructionsResponse = await billetService.PutInstructionsAsync(putInstructionsRequest, authenticatedScope);

                authenticatedScope.Dispose();

                #endregion

                #region Gravar XML de Retorno

                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoEnviarInstrucao).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoEnviarInstrucao).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                if (putInstructionsResponse.StatusCode == System.Net.HttpStatusCode.Accepted || putInstructionsResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    GerarXmlRetorno(pathXml, "0", "");
                }
                else
                {
                    GerarXmlRetorno(pathXml, "1", $"Não foi possível enviar a instrução do boleto. Tente novamente mais tarde. (Status Code: {((int)putInstructionsResponse.StatusCode).ToString()})" +
                    (!string.IsNullOrWhiteSpace(putInstructionsResponse.Codigo) ? " - (Erro: " + putInstructionsResponse.Codigo +
                        (!string.IsNullOrWhiteSpace(putInstructionsResponse.Mensagem) ? " - " + putInstructionsResponse.Mensagem : "") + ")" : ""));
                }

                #endregion
            }
            catch (Exception ex)
            {
                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoEnviarInstrucao).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoEnviarInstrucao).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                GerarXmlRetorno(pathXml, "999", ex.GetLastException().Message.Replace("\r\n", " | "));
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

        private void GerarXmlRetorno(string path, string status, string motivo)
        {
            var oSettings = new XmlWriterSettings();
            var c = new UTF8Encoding(false);

            oSettings.Encoding = c;
            oSettings.Indent = true;
            oSettings.IndentChars = " ";
            oSettings.NewLineOnAttributes = false;
            oSettings.OmitXmlDeclaration = false;
            XmlWriter oXmlGravar = null;

            try
            {
                switch (status)
                {
                    case "0":
                        motivo = "Instruções do boleto enviado com sucesso";
                        break;
                }

                oXmlGravar = XmlWriter.Create(path, oSettings);
                oXmlGravar.WriteStartDocument();
                oXmlGravar.WriteStartElement("BoletoEnviarInstrucaoResponse");
                oXmlGravar.WriteElementString("Status", status);
                oXmlGravar.WriteElementString("Motivo", motivo);
                oXmlGravar.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
                oXmlGravar.WriteEndElement(); //BoletoEnviarInstrucaoResponse
                oXmlGravar.WriteEndDocument();
                oXmlGravar.Flush();
                oXmlGravar.Close();
            }
            finally
            {
                if (oXmlGravar != null)
                {
                    if (oXmlGravar.WriteState != WriteState.Closed)
                    {
                        oXmlGravar.Close();
                    }
                }
            }
        }
    }
}