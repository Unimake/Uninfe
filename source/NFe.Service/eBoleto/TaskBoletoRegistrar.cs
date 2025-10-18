using EBank.Solutions.Primitives.Billet;
using EBank.Solutions.Primitives.Billet.Models;
using EBank.Solutions.Primitives.Billet.Request;
using EBank.Solutions.Primitives.Enumerations;
using EBank.Solutions.Primitives.Enumerations.Billet;
using EBank.Solutions.Primitives.PDF.Models;
using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Unimake.AuthServer.Security.Scope;
using Unimake.EBank.Solutions.Services.Billet;
using Unimake.Primitives.UDebug;

namespace NFe.Service
{
    public class TaskBoletoRegistrar : TaskAbst
    {
        public static DebugScope<DebugStateObject> debugScope;

        public TaskBoletoRegistrar(string arquivo)
        {
            Servico = Servicos.BoletoRegistrar;
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
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\eBoleto\BoletoRegistrar_1_00.xsd"),
                        nRetornoTipoArq = 1
                    }
                };

                validarXML.ValidarArqXML(ConteudoXML, NomeArquivoXML);
                if (validarXML.Retorno != 0)
                {
                    throw new Exception(validarXML.RetornoString.Replace("\r\n", ""));
                }

                #endregion

                #region Criar objeto do boleto para enviar ao eBank

                var registerRequest = new RegisterRequest
                {
                    ConfigurationId = TFunctions.GetXmlValue(ConteudoXML, "ConfigurationId"),
                    AgenciaColetora = TFunctions.GetXmlValue(ConteudoXML, "AgenciaColetora"),
                    Carteira = TFunctions.GetXmlValue(ConteudoXML, "Carteira"),
                    CodigoBarraNumerico = TFunctions.GetXmlValue(ConteudoXML, "CodigoBarraNumerico"),
                    DiasParaBaixaOuDevolucao = TFunctions.GetXmlIntValue(ConteudoXML, "DiasParaBaixaOuDevolucao"),
                    DigitoVerificadorNumeroNoBanco = TFunctions.GetXmlValue(ConteudoXML, "DigitoVerificadorNumeroNoBanco"),
                    Emissao = TFunctions.GetXmlDateTimeValue(ConteudoXML, "Emissao"),
                    Especie = (EspecieTitulo)TFunctions.GetXmlIntValue(ConteudoXML, "Especie"),
                    IdentificacaoDistribuicao = (IdentificacaoDistribuicao)TFunctions.GetXmlIntValue(ConteudoXML, "IdentificacaoDistribuicao"),
                    IdentificacaoEmissao = (IdentificacaoEmissao)TFunctions.GetXmlIntValue(ConteudoXML, "IdentificacaoEmissao"),
                    LinhaDigitavel = TFunctions.GetXmlValue(ConteudoXML, "LinhaDigitavel"),
                    NumeroDiasLimiteRecebimento = TFunctions.GetXmlIntValue(ConteudoXML, "NumeroDiasLimiteRecebimento"),
                    NumeroNaEmpresa = TFunctions.GetXmlValue(ConteudoXML, "NumeroNaEmpresa"),
                    NumeroParcela = TFunctions.GetXmlIntValue(ConteudoXML, "NumeroParcela"),
                    NumeroVariacaoCarteira = TFunctions.GetXmlValue(ConteudoXML, "NumeroVariacaoCarteira"),
                    PermiteRecebimentoParcial = TFunctions.GetXmlCharValue(ConteudoXML, "PermiteRecebimentoParcial"),
                    Testing = TFunctions.GetXmlBoolValue(ConteudoXML, "Testing"),
                    TipoBaixaDevolucao = (TipoBaixaDevolucao)TFunctions.GetXmlIntValue(ConteudoXML, "TipoBaixaDevolucao"),
                    ValorAbatimento = TFunctions.GetXmlDecimalValue(ConteudoXML, "ValorAbatimento"),
                    ValorIOF = TFunctions.GetXmlDecimalValue(ConteudoXML, "ValorIOF"),
                    ValorNominal = TFunctions.GetXmlDecimalValue(ConteudoXML, "ValorNominal"),
                    Vencimento = TFunctions.GetXmlDateTimeValue(ConteudoXML, "Vencimento")
                };

                #region Avalista

                var numeroNoBanco = TFunctions.GetXmlValue(ConteudoXML, "NumeroNoBanco");
                if (!string.IsNullOrWhiteSpace(numeroNoBanco))
                {
                    registerRequest.NumeroNoBanco = numeroNoBanco;
                }

                var avalista = ConteudoXML.GetElementsByTagName("Avalista");

                if (avalista.Count > 0)
                {
                    var elementoAvalista = (XmlElement)avalista[0];

                    registerRequest.Avalista = new Avalista
                    {
                        Nome = TFunctions.GetXmlValue(elementoAvalista, "Nome"),
                        TipoInscricao = (TipoDeInscricao)TFunctions.GetXmlIntValue(elementoAvalista, "TipoInscricao"),
                        Inscricao = TFunctions.GetXmlValue(elementoAvalista, "Inscricao")
                    };

                    var enderecoAvalista = ((XmlElement)avalista[0]).GetElementsByTagName("Endereco");

                    if (enderecoAvalista.Count > 0)
                    {
                        var elementoEnderecoAvalista = (XmlElement)((XmlElement)avalista[0]).GetElementsByTagName("Endereco")[0];

                        registerRequest.Avalista.Endereco = new Endereco
                        {
                            Logradouro = TFunctions.GetXmlValue(elementoEnderecoAvalista, "Logradouro"),
                            Numero = TFunctions.GetXmlValue(elementoEnderecoAvalista, "Numero"),
                            Complemento = TFunctions.GetXmlValue(elementoEnderecoAvalista, "Complemento"),
                            Bairro = TFunctions.GetXmlValue(elementoEnderecoAvalista, "Bairro"),
                            Cidade = TFunctions.GetXmlValue(elementoEnderecoAvalista, "Cidade"),
                            UF = TFunctions.GetXmlValue(elementoEnderecoAvalista, "UF"),
                            CEP = TFunctions.GetXmlValue(elementoEnderecoAvalista, "CEP")
                        };
                    }
                }

                #endregion

                #region Desconto

                var desconto = ConteudoXML.GetElementsByTagName("Desconto");

                if (desconto.Count > 0)
                {
                    var elementoDesconto = (XmlElement)desconto[0];

                    registerRequest.Desconto = new Desconto
                    {
                        Data = TFunctions.GetXmlDateTimeValue(elementoDesconto, "Data"),
                        Tipo = (TipoDesconto)TFunctions.GetXmlIntValue(elementoDesconto, "Tipo"),
                        Valor = TFunctions.GetXmlDecimalValue(elementoDesconto, "Valor")
                    };
                }

                #endregion

                #region Juros

                var juros = ConteudoXML.GetElementsByTagName("Juros");

                if (juros.Count > 0)
                {
                    var elementoJuros = (XmlElement)juros[0];

                    registerRequest.Juros = new Juros
                    {
                        Data = TFunctions.GetXmlDateTimeValue(elementoJuros, "Data"),
                        Tipo = (TipoJuros)TFunctions.GetXmlIntValue(elementoJuros, "Tipo"),
                        Valor = TFunctions.GetXmlDecimalValue(elementoJuros, "Valor")
                    };
                }

                #endregion

                #region Mensagens

                var mensagens = ConteudoXML.GetElementsByTagName("Mensagens");

                if (mensagens.Count > 0)
                {
                    var nodes = mensagens[0].SelectNodes("Mensagem");

                    registerRequest.Mensagens = nodes.Cast<XmlNode>().Select(n => n.InnerText).ToArray();
                }

                #endregion

                #region Mensagens Recibo

                var mensagensRecibo = ConteudoXML.GetElementsByTagName("MensagensRecibo");

                if (mensagensRecibo.Count > 0)
                {
                    var nodes = mensagensRecibo[0].SelectNodes("Mensagem");

                    registerRequest.MensagensRecibo = nodes.Cast<XmlNode>().Select(n => n.InnerText).ToArray();
                }

                #endregion

                #region Multa

                var multa = ConteudoXML.GetElementsByTagName("Multa");

                if (multa.Count > 0)
                {
                    var elementoMulta = (XmlElement)multa[0];

                    registerRequest.Multa = new Multa
                    {
                        Data = TFunctions.GetXmlDateTimeValue(elementoMulta, "Data"),
                        Tipo = (TipoMulta)TFunctions.GetXmlIntValue(elementoMulta, "Tipo"),
                        Valor = TFunctions.GetXmlDecimalValue(elementoMulta, "Valor")
                    };
                }

                #endregion

                #region Pagador

                var pagador = ConteudoXML.GetElementsByTagName("Pagador");

                if (pagador.Count > 0)
                {
                    var elementoPagador = (XmlElement)pagador[0];

                    registerRequest.Pagador = new Pagador
                    {
                        Nome = TFunctions.GetXmlValue(elementoPagador, "Nome"),
                        TipoInscricao = (TipoDeInscricao)TFunctions.GetXmlIntValue(elementoPagador, "TipoInscricao"),
                        Inscricao = TFunctions.GetXmlValue(elementoPagador, "Inscricao"),
                        Codigo = TFunctions.GetXmlValue(elementoPagador, "Codigo"),
                        Email = TFunctions.GetXmlValue(elementoPagador, "Email"),
                        Telefone = TFunctions.GetXmlValue(elementoPagador, "Telefone")
                    };

                    var enderecoPagador = elementoPagador.GetElementsByTagName("Endereco");

                    if (enderecoPagador.Count > 0)
                    {
                        var elementoEndereco = (XmlElement)enderecoPagador[0];

                        registerRequest.Pagador.Endereco = new Endereco
                        {
                            Logradouro = TFunctions.GetXmlValue(elementoEndereco, "Logradouro"),
                            Numero = TFunctions.GetXmlValue(elementoEndereco, "Numero"),
                            Complemento = TFunctions.GetXmlValue(elementoEndereco, "Complemento"),
                            Bairro = TFunctions.GetXmlValue(elementoEndereco, "Bairro"),
                            Cidade = TFunctions.GetXmlValue(elementoEndereco, "Cidade"),
                            UF = TFunctions.GetXmlValue(elementoEndereco, "UF"),
                            CEP = TFunctions.GetXmlValue(elementoEndereco, "CEP")
                        };
                    }
                }

                #endregion

                #region PDF Config

                var pdfConfig = ConteudoXML.GetElementsByTagName("PDFConfig");

                if (pdfConfig.Count > 0)
                {
                    var elementoPdfConfig = (XmlElement)pdfConfig[0];

                    registerRequest.PDFConfig = new PDFConfig
                    {
                        Password = TFunctions.GetXmlValue(elementoPdfConfig, "Password"),
                        PermitAnnotations = TFunctions.GetXmlBoolValue(elementoPdfConfig, "PermitAnnotations"),
                        PermitAssembleDocument = TFunctions.GetXmlBoolValue(elementoPdfConfig, "PermitAssembleDocument"),
                        PermitExtractContent = TFunctions.GetXmlBoolValue(elementoPdfConfig, "PermitExtractContent"),
                        PermitFormsFill = TFunctions.GetXmlBoolValue(elementoPdfConfig, "PermitFormsFill"),
                        PermitFullQualityPrint = TFunctions.GetXmlBoolValue(elementoPdfConfig, "PermitFullQualityPrint"),
                        PermitModifyDocument = TFunctions.GetXmlBoolValue(elementoPdfConfig, "PermitModifyDocument"),
                        PermitPrint = TFunctions.GetXmlBoolValue(elementoPdfConfig, "PermitPrint"),
                        SignPDF = TFunctions.GetXmlBoolValue(elementoPdfConfig, "SignPDF"),
                        TryGeneratePDF = TFunctions.GetXmlBoolValue(elementoPdfConfig, "TryGeneratePDF")
                    };
                }

                #endregion

                #region PIX Config

                var nodePixConfig = ConteudoXML.GetElementsByTagName("PixConfig");

                if (nodePixConfig.Count > 0)
                {
                    var elementPixConfig = (XmlElement)nodePixConfig[0];

                    registerRequest.PIXConfig = new PIXBilletConfig
                    {
                        Chave = TFunctions.GetXmlValue(elementPixConfig, "Chave"),
                        RegistrarPIX = TFunctions.GetXmlBoolValue(elementPixConfig, "RegistrarPIX")
                    };

                    var nodeQrCodeConfig = elementPixConfig.GetElementsByTagName("QrCodeConfig");

                    if (nodeQrCodeConfig.Count > 0)
                    {
                        var elementQrCodeConfig = (XmlElement)nodeQrCodeConfig[0];
                        registerRequest.PIXConfig.QRCodeConfig.Height = TFunctions.GetXmlIntValue(elementQrCodeConfig, "Height");
                        registerRequest.PIXConfig.QRCodeConfig.ImageFormat = (EBank.Solutions.Primitives.Enumerations.PIX.QrCodeImageFormat)TFunctions.GetXmlIntValue(elementQrCodeConfig, "ImageFormat");
                        registerRequest.PIXConfig.QRCodeConfig.Quality = TFunctions.GetXmlIntValue(elementQrCodeConfig, "Quality");
                        registerRequest.PIXConfig.QRCodeConfig.Width = TFunctions.GetXmlIntValue(elementQrCodeConfig, "Width");
                    }
                }

                #endregion

                #region Protesto

                var protesto = ConteudoXML.GetElementsByTagName("Protesto");

                if (protesto.Count > 0)
                {
                    var elementoProtesto = (XmlElement)protesto[0];

                    registerRequest.Protesto = new Protesto
                    {
                        Tipo = (TipoProtesto)TFunctions.GetXmlIntValue(elementoProtesto, "Tipo"),
                        Valor = TFunctions.GetXmlIntValue(elementoProtesto, "Valor")
                    };
                }

                #endregion

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

                #region Enviar o Boleto para o eBank

                var billetService = new BilletService();
                var responseBilletService = await billetService.RegisterAsync(registerRequest, authenticatedScope);

                authenticatedScope.Dispose();

                #endregion

                #region Gravar XML de Retorno e o PDF do Boleto

                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).RetornoXML;
                var pathPDF = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file.Replace(".xml", ".pdf"));
                if (responseBilletService.PDFContent.Success)
                {
                    responseBilletService.PDFContent.SaveToFile(pathPDF);
                }

                GerarXmlRetorno(pathXml,
                    "0",
                    "",
                    responseBilletService,
                    (responseBilletService.PDFContent.Success ? pathPDF : "")
                    );

                #endregion
            }
            catch (Exception ex)
            {
                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.BoletoRegistrar).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                GerarXmlRetorno(
                   pathXml,
                   "999",
                   ex.GetLastException().Message.Replace("\r\n", ""));
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

        private void GerarXmlRetorno(string path, string status, string motivo, EBank.Solutions.Primitives.Billet.Response.RegisterResponse registerResponse = null, string pdfPath = "")
        {
            var cultura = CultureInfo.CreateSpecificCulture("en-US");
            cultura.NumberFormat.NumberDecimalSeparator = ".";

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
                        motivo = "Boleto registrado";
                        break;
                }

                oXmlGravar = XmlWriter.Create(path, oSettings);
                oXmlGravar.WriteStartDocument();
                oXmlGravar.WriteStartElement("BoletoRegistrarResponse");
                oXmlGravar.WriteElementString("Status", status);
                oXmlGravar.WriteElementString("Motivo", motivo);
                if (status == "0")
                {
                    oXmlGravar.WriteElementString("CodigoBarraNumerico", registerResponse.CodigoBarraNumerico);
                    oXmlGravar.WriteElementString("NumeroNoBanco", registerResponse.NumeroNoBanco);
                    oXmlGravar.WriteElementString("LinhaDigitavel", registerResponse.LinhaDigitavel);
                    oXmlGravar.WriteElementString("PdfContentSuccess", registerResponse.PDFContent.Success.ToString());
                    oXmlGravar.WriteElementString("PdfContentMessage", registerResponse.PDFContent.Message);
                    oXmlGravar.WriteElementString("PdfContentBase64", registerResponse.PDFContent.Content);
                    oXmlGravar.WriteElementString("PdfPath", pdfPath);

                    if (registerResponse.PIXPagamentoDetalhe != null)
                    {
                        oXmlGravar.WriteStartElement("PixPagamentoDetalhe");
                        oXmlGravar.WriteElementString("DataPagamento", registerResponse.PIXPagamentoDetalhe.DataPagamento.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                        oXmlGravar.WriteElementString("TxId", registerResponse.PIXPagamentoDetalhe.TxId);
                        oXmlGravar.WriteElementString("ValorAbatimento", registerResponse.PIXPagamentoDetalhe.ValorAbatimento?.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorDesconto", registerResponse.PIXPagamentoDetalhe.ValorDesconto?.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorJuros", registerResponse.PIXPagamentoDetalhe.ValorJuros?.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorLiquidado", registerResponse.PIXPagamentoDetalhe.ValorLiquidado.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorMulta", registerResponse.PIXPagamentoDetalhe.ValorMulta?.ToString("N2", cultura));
                        oXmlGravar.WriteElementString("ValorOriginal", registerResponse.PIXPagamentoDetalhe.ValorOriginal.ToString("N2", cultura));

                        oXmlGravar.WriteEndElement(); //PixPagamentoDetalhe
                    }

                    if (!registerResponse.QrCodeContent.IsNullOrEmpty())
                    {
                        oXmlGravar.WriteStartElement("QRCodeContent");
                        oXmlGravar.WriteElementString("Image", registerResponse.QrCodeContent.Image);
                        oXmlGravar.WriteElementString("Success", registerResponse.QrCodeContent.Success.ToString());
                        oXmlGravar.WriteElementString("Text", registerResponse.QrCodeContent.Text);
                        oXmlGravar.WriteEndElement(); //QRCodeContent
                    }
                }

                oXmlGravar.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
                oXmlGravar.WriteEndElement(); //BoletoRegistrarResponse
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