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

                GerarXmlRetorno(pathXml, "999", ex.GetLastException().Message.Replace("\r\n", ""));
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

        private void GerarXmlRetorno(string path, string status, string motivo, List<QueryInformationResponse> queryInformationResponse = null)
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
                var cultura = CultureInfo.CreateSpecificCulture("en-US");
                cultura.NumberFormat.NumberDecimalSeparator = ".";

                switch (status)
                {
                    case "0":
                        motivo = "Boletos encontrados";
                        break;

                    case "1":
                        motivo = "Nenhum boleto encontrado";
                        break;
                }

                oXmlGravar = XmlWriter.Create(path, oSettings);
                oXmlGravar.WriteStartDocument();
                oXmlGravar.WriteStartElement("BoletoConsultarResponse");
                oXmlGravar.WriteElementString("Status", status);
                oXmlGravar.WriteElementString("Motivo", motivo);
                if (queryInformationResponse != null)
                {
                    foreach (var boleto in queryInformationResponse)
                    {
                        oXmlGravar.WriteStartElement("BoletoResponse");
                        oXmlGravar.WriteElementString("CodigoBarras", boleto.CodigoBarras);
                        oXmlGravar.WriteElementString("DataEmissao", boleto.DataEmissao.FormatDate("dd-MM-yyyy"));
                        oXmlGravar.WriteElementString("DataLiquidacao", boleto.DataLiquidacao.FormatDate("dd-MM-yyyy"));
                        oXmlGravar.WriteElementString("DataVencimento", boleto.DataVencimento.FormatDate("dd-MM-yyyy"));
                        oXmlGravar.WriteElementString("NumeroNaEmpresa", boleto.NumeroNaEmpresa);
                        oXmlGravar.WriteElementString("NumeroNoBanco", boleto.NumeroNoBanco);

                        if (boleto.Pagador != null)
                        {
                            oXmlGravar.WriteStartElement("Pagador");
                            oXmlGravar.WriteElementString("Codigo", boleto.Pagador.Codigo);
                            oXmlGravar.WriteElementString("Nome", boleto.Pagador.Nome);
                            oXmlGravar.WriteElementString("Inscricao", boleto.Pagador.Inscricao);
                            oXmlGravar.WriteElementString("Telefone", boleto.Pagador.Telefone);
                            oXmlGravar.WriteElementString("Email", boleto.Pagador.Email);
                            oXmlGravar.WriteElementString("TipoInscricao", Convert.ToInt32(boleto.Pagador.TipoInscricao).ToString());

                            if (boleto.Pagador.Endereco != null)
                            {
                                oXmlGravar.WriteStartElement("Endereco");
                                oXmlGravar.WriteElementString("Logradouro", boleto.Pagador.Endereco.Logradouro);
                                oXmlGravar.WriteElementString("Numero", boleto.Pagador.Endereco.Numero);
                                oXmlGravar.WriteElementString("Complemento", boleto.Pagador.Endereco.Complemento);
                                oXmlGravar.WriteElementString("Bairro", boleto.Pagador.Endereco.Bairro);
                                oXmlGravar.WriteElementString("Cidade", boleto.Pagador.Endereco.Cidade);
                                oXmlGravar.WriteElementString("UF", boleto.Pagador.Endereco.UF);
                                oXmlGravar.WriteElementString("CEP", boleto.Pagador.Endereco.CEP);
                                oXmlGravar.WriteEndElement(); //Endereco
                            }

                            oXmlGravar.WriteEndElement(); //Pagador
                        }

                        oXmlGravar.WriteStartElement("PdfContent");
                        oXmlGravar.WriteElementString("Content", boleto.PdfContent.Content);
                        oXmlGravar.WriteElementString("Success", boleto.PdfContent.Success.ToString());
                        oXmlGravar.WriteElementString("Message", boleto.PdfContent.Message);
                        oXmlGravar.WriteEndElement(); //PdfContent

                        if (boleto.PIXPagamentoDetalhe != null)
                        {
                            oXmlGravar.WriteStartElement("PIXPagamentoDetalhe");
                            oXmlGravar.WriteElementString("DataPagamento", boleto.PIXPagamentoDetalhe.DataPagamento.FormatDate("dd-MM-yyyy"));
                            oXmlGravar.WriteElementString("ValorDesconto", boleto.PIXPagamentoDetalhe.ValorDesconto?.ToString("N2", cultura));
                            oXmlGravar.WriteElementString("ValorJuros", boleto.PIXPagamentoDetalhe.ValorJuros?.ToString("N2", cultura));
                            oXmlGravar.WriteElementString("ValorLiquidado", boleto.PIXPagamentoDetalhe.ValorLiquidado.ToString("N2", cultura));
                            oXmlGravar.WriteElementString("ValorMulta", boleto.PIXPagamentoDetalhe.ValorMulta?.ToString("N2", cultura));
                            oXmlGravar.WriteElementString("ValorOriginal", boleto.PIXPagamentoDetalhe.ValorOriginal.ToString("N2", cultura));
                            oXmlGravar.WriteEndElement(); //PIXPagamentoDetalhe
                        }

                        oXmlGravar.WriteStartElement("QrCodeContent");
                        oXmlGravar.WriteElementString("Text", boleto.QrCodeContent.Text);
                        oXmlGravar.WriteElementString("Image", boleto.QrCodeContent.Image);
                        oXmlGravar.WriteElementString("Success", boleto.QrCodeContent.Success.ToString());
                        oXmlGravar.WriteEndElement(); //QrCodeContent

                        oXmlGravar.WriteElementString("Situacao", Convert.ToInt32(boleto.Situacao).ToString());
                        oXmlGravar.WriteElementString("TipoLiquidacao", Convert.ToInt32(boleto.TipoLiquidacao).ToString());
                        oXmlGravar.WriteElementString("Valor", boleto.Valor.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorAbatimento", boleto.ValorAbatimento.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorDesconto", boleto.ValorDesconto.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorJuros", boleto.ValorJuros.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorLiquidado", boleto.ValorLiquidado.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorMulta", boleto.ValorMulta.ToString("N2", cultura));

                        oXmlGravar.WriteEndElement(); //BoletoResponse
                    }
                }
                oXmlGravar.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
                oXmlGravar.WriteEndElement(); //BoletoConsultarResponse
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