using EBank.Solutions.Primitives.Billet.Request;
using EBank.Solutions.Primitives.Billet.Response;
using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.AuthServer.Security.Scope;
using Unimake.EBank.Solutions.Services.Billet;
using Unimake.Primitives.UDebug;

namespace NFe.Service
{
    public class TaskBoletoConsultar : TaskAbst
    {
        public static DebugScope<DebugStateObject> debugScope;

        public TaskBoletoConsultar(string arquivo)
        {
            Servico = Servicos.BoletoConsultar;
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
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\eBoleto\BoletoConsultar_1_00.xsd"),
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

                var queryInformationRequest = new QueryInformationRequest
                {
                    ConfigurationId = TFunctions.GetXmlValue(ConteudoXML, "ConfigurationId"),
                    Testing = TFunctions.GetXmlBoolValue(ConteudoXML, "Testing"),
                    DataEmissaoFinal = TFunctions.GetXmlDateTimeValue(ConteudoXML, "DataEmissaoFinal"),
                    DataEmissaoInicial = TFunctions.GetXmlDateTimeValue(ConteudoXML, "DataEmissaoInicial"),
                    PageNumber = (TFunctions.GetXmlIntValue(ConteudoXML, "PageNumber") == 0 ? 1 : TFunctions.GetXmlIntValue(ConteudoXML, "PageNumber")),
                    PageSize = (TFunctions.GetXmlIntValue(ConteudoXML, "PageSize") == 0 ? 50 : TFunctions.GetXmlIntValue(ConteudoXML, "PageSize"))
                };

                var numerosNoBancoList = ConteudoXML.GetElementsByTagName("NumerosNoBanco");
                if (numerosNoBancoList.Count > 0)
                {
                    var numerosNoBancoElement = (XmlElement)numerosNoBancoList[0];

                    foreach (var numero in numerosNoBancoElement.GetElementsByTagName("NumeroNoBanco"))
                    {
                        queryInformationRequest.NumeroNoBanco.Add(((XmlElement)numero).InnerText);
                    }
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

                #region Enviar o cancelamento do Boleto para o eBank

                var billetService = new BilletService();
                var queryResponse = await billetService.QueryAsync(queryInformationRequest, authenticatedScope);

                authenticatedScope.Dispose();

                #endregion

                #region Gravar XML de Retorno

                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoConsultar).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoConsultar).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                if (queryResponse.Count > 0)
                {
                    GerarXmlRetorno(pathXml, "0", "", queryResponse);
                }
                else
                {
                    GerarXmlRetorno(pathXml, "1", "");
                }

                #endregion
            }
            catch (Exception ex)
            {
                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoConsultar).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoConsultar).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                var lastException = ex.GetLastException();
                var traceId = BoletoRetornoHelper.ExtrairTraceId(lastException);
                GerarXmlRetorno(pathXml, "999", lastException.Message.Replace("\r\n", ""), null, traceId);
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

        private void GerarXmlRetorno(string path, string status, string motivo, List<QueryInformationResponse> queryInformationResponse = null, string traceId = "")
        {
            var cultura = CultureInfo.CreateSpecificCulture("en-US");
            cultura.NumberFormat.NumberDecimalSeparator = ".";

            if (status == "0")
            {
                motivo = "Boletos encontrados";
            }
            else if (status == "1")
            {
                motivo = "Nenhum boleto encontrado";
            }

            BoletoRetornoHelper.GravarXmlRetorno(path, "BoletoConsultarResponse", status, motivo, traceId, xmlWriter =>
            {
                if (queryInformationResponse != null)
                {
                    foreach (var boleto in queryInformationResponse)
                    {
                        xmlWriter.WriteStartElement("BoletoResponse");
                        xmlWriter.WriteElementString("CodigoBarras", boleto.CodigoBarras);
                        xmlWriter.WriteElementString("DataEmissao", boleto.DataEmissao.FormatDate("dd-MM-yyyy"));
                        xmlWriter.WriteElementString("DataLiquidacao", boleto.DataLiquidacao.FormatDate("dd-MM-yyyy"));
                        xmlWriter.WriteElementString("DataVencimento", boleto.DataVencimento.FormatDate("dd-MM-yyyy"));
                        xmlWriter.WriteElementString("NumeroNaEmpresa", boleto.NumeroNaEmpresa);
                        xmlWriter.WriteElementString("NumeroNoBanco", boleto.NumeroNoBanco);

                        if (boleto.Pagador != null)
                        {
                            xmlWriter.WriteStartElement("Pagador");
                            xmlWriter.WriteElementString("Codigo", boleto.Pagador.Codigo);
                            xmlWriter.WriteElementString("Nome", boleto.Pagador.Nome);
                            xmlWriter.WriteElementString("Inscricao", boleto.Pagador.Inscricao);
                            xmlWriter.WriteElementString("Telefone", boleto.Pagador.Telefone);
                            xmlWriter.WriteElementString("Email", boleto.Pagador.Email);
                            xmlWriter.WriteElementString("TipoInscricao", Convert.ToInt32(boleto.Pagador.TipoInscricao).ToString());

                            if (boleto.Pagador.Endereco != null)
                            {
                                xmlWriter.WriteStartElement("Endereco");
                                xmlWriter.WriteElementString("Logradouro", boleto.Pagador.Endereco.Logradouro);
                                xmlWriter.WriteElementString("Numero", boleto.Pagador.Endereco.Numero);
                                xmlWriter.WriteElementString("Complemento", boleto.Pagador.Endereco.Complemento);
                                xmlWriter.WriteElementString("Bairro", boleto.Pagador.Endereco.Bairro);
                                xmlWriter.WriteElementString("Cidade", boleto.Pagador.Endereco.Cidade);
                                xmlWriter.WriteElementString("UF", boleto.Pagador.Endereco.UF);
                                xmlWriter.WriteElementString("CEP", boleto.Pagador.Endereco.CEP);
                                xmlWriter.WriteEndElement();
                            }

                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("PdfContent");
                        xmlWriter.WriteElementString("Content", boleto.PdfContent.Content);
                        xmlWriter.WriteElementString("Success", boleto.PdfContent.Success.ToString());
                        xmlWriter.WriteElementString("Message", boleto.PdfContent.Message);
                        xmlWriter.WriteEndElement();

                        if (boleto.PIXPagamentoDetalhe != null)
                        {
                            xmlWriter.WriteStartElement("PIXPagamentoDetalhe");
                            xmlWriter.WriteElementString("DataPagamento", boleto.PIXPagamentoDetalhe.DataPagamento.FormatDate("dd-MM-yyyy"));
                            xmlWriter.WriteElementString("ValorDesconto", boleto.PIXPagamentoDetalhe.ValorDesconto?.ToString("N2", cultura));
                            xmlWriter.WriteElementString("ValorJuros", boleto.PIXPagamentoDetalhe.ValorJuros?.ToString("N2", cultura));
                            xmlWriter.WriteElementString("ValorLiquidado", boleto.PIXPagamentoDetalhe.ValorLiquidado.ToString("N2", cultura));
                            xmlWriter.WriteElementString("ValorMulta", boleto.PIXPagamentoDetalhe.ValorMulta?.ToString("N2", cultura));
                            xmlWriter.WriteElementString("ValorOriginal", boleto.PIXPagamentoDetalhe.ValorOriginal.ToString("N2", cultura));
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteStartElement("QrCodeContent");
                        xmlWriter.WriteElementString("Text", boleto.QrCodeContent.Text);
                        xmlWriter.WriteElementString("Image", boleto.QrCodeContent.Image);
                        xmlWriter.WriteElementString("Success", boleto.QrCodeContent.Success.ToString());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteElementString("Situacao", Convert.ToInt32(boleto.Situacao).ToString());
                        xmlWriter.WriteElementString("TipoLiquidacao", Convert.ToInt32(boleto.TipoLiquidacao).ToString());
                        xmlWriter.WriteElementString("Valor", boleto.Valor.ToString("N2", cultura));
                        xmlWriter.WriteElementString("ValorAbatimento", boleto.ValorAbatimento.ToString("N2", cultura));
                        xmlWriter.WriteElementString("ValorDesconto", boleto.ValorDesconto.ToString("N2", cultura));
                        xmlWriter.WriteElementString("ValorJuros", boleto.ValorJuros.ToString("N2", cultura));
                        xmlWriter.WriteElementString("ValorLiquidado", boleto.ValorLiquidado.ToString("N2", cultura));
                        xmlWriter.WriteElementString("ValorMulta", boleto.ValorMulta.ToString("N2", cultura));

                        xmlWriter.WriteEndElement();
                    }
                }
            });
        }
    }
}