using EBank.Solutions.Primitives.Billet.Models;
using EBank.Solutions.Primitives.Enumerations;
using EBank.Solutions.Primitives.Enumerations.PIX;
using EBank.Solutions.Primitives.PIX.Models;
using EBank.Solutions.Primitives.PIX.Models.Cobranca;
using EBank.Solutions.Primitives.PIX.QrCode;
using EBank.Solutions.Primitives.PIX.Request.Cobranca;
using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.AuthServer.Authentication;
using Unimake.AuthServer.Security.Scope;
using Unimake.EBank.Solutions.Services.PIX;
using Unimake.Primitives.UDebug;

namespace NFe.Service
{
    public class TaskPIXCobrancaCreateRequest : TaskAbst
    {
        public static DebugScope<DebugStateObject> debugScope;

        public TaskPIXCobrancaCreateRequest(string arquivo)
        {
            Servico = Servicos.PIXCobrancaCreateRequest;
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
                    throw new Exception("Para utilizar o serviço de PIX é necessário configurar no UniNFe o AppID e Secret do eBank.");
                }

                #region Validar o XML

                var validarXML = new ValidarXML
                {
                    TipoArqXml = new TipoArquivoXML
                    {
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\PIX\PIXCobrancaCreateRequest_1_00.xsd"),
                        nRetornoTipoArq = 1
                    }
                };

                validarXML.ValidarArqXML(ConteudoXML, NomeArquivoXML);
                if (validarXML.Retorno != 0)
                {
                    throw new Exception(validarXML.RetornoString.Replace("\r\n", ""));
                }

                #endregion

                #region Validar TAGS do XML

                if (ConteudoXML.GetElementsByTagName("SolicitacaoPagador").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <SolicitacaoPagador> no XML.");
                }
                if (ConteudoXML.GetElementsByTagName("TipoCobranca").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <TipoCobranca> no XML.");
                }
                if (ConteudoXML.GetElementsByTagName("Valor").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <Valor> no XML.");
                }
                if (ConteudoXML.GetElementsByTagName("Chave").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <Chave> no XML.");
                }
                if (ConteudoXML.GetElementsByTagName("TxId").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <TxId> no XML.");
                }
                if (ConteudoXML.GetElementsByTagName("GerarQRCode").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <GerarQRCode> no XML.");
                }

                var beneficiario = ConteudoXML.GetElementsByTagName("Beneficiario");

                if (beneficiario.Count <= 0)
                {
                    throw new Exception("E obrigatorio informar o grupo de tag <Beneficiario> no XML.");
                }

                var conta = ((XmlElement)beneficiario[0]).GetElementsByTagName("Conta");

                if (conta.Count <= 0)
                {
                    throw new Exception("E obrigatorio informar o grupo de tag <Conta>, que fica dentro do grupo de tag <Beneficiario>, no XML.");
                }
                if (((XmlElement)conta[0]).GetElementsByTagName("Agencia").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <Agencia>, que fica dentro do grupo de tag <Conta>, no XML.");
                }
                if (((XmlElement)conta[0]).GetElementsByTagName("Numero").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <Numero>, que fica dentro do grupo de tag <Conta>, no XML.");
                }
                if (((XmlElement)conta[0]).GetElementsByTagName("Banco").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <Banco>, que fica dentro do grupo de tag <Conta>, no XML.");
                }

                if (((XmlElement)beneficiario[0]).GetElementsByTagName("Inscricao").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <Inscricao>, que fica dentro do grupo de tag <Beneficiario>, no XML.");
                }
                if (((XmlElement)beneficiario[0]).GetElementsByTagName("Nome").Count <= 0)
                {
                    throw new Exception("E obrigatorio informar a tag <Nome>, que fica dentro do grupo de tag <Beneficiario>, no XML.");
                }

                #endregion

                #region Criar cobrança PIX

                var pixCobrancaCreateRequest = new PIXCobrancaCreateRequest
                {
                    SolicitacaoPagador = ConteudoXML.GetElementsByTagName("SolicitacaoPagador")[0].InnerText,
                    TipoCobranca = (TipoCobranca)Convert.ToInt32(ConteudoXML.GetElementsByTagName("TipoCobranca")[0].InnerText),
                    Valor = Convert.ToDecimal(ConteudoXML.GetElementsByTagName("Valor")[0].InnerText, CultureInfo.InvariantCulture),
                    Chave = ConteudoXML.GetElementsByTagName("Chave")[0].InnerText,
                    TxId = ConteudoXML.GetElementsByTagName("TxId")[0].InnerText.Trim(),
                    GerarQRCode = Convert.ToBoolean(ConteudoXML.GetElementsByTagName("GerarQRCode")[0].InnerText),
                    Testing = Convert.ToBoolean(ConteudoXML.GetElementsByTagName("Testing")[0].InnerText),
                    Beneficiario = new Beneficiario
                    {
                        Conta = new ContaCorrente
                        {
                            Agencia = ((XmlElement)conta[0]).GetElementsByTagName("Agencia")[0].InnerText,
                            Numero = ((XmlElement)conta[0]).GetElementsByTagName("Numero")[0].InnerText,
                            Banco = (Banco)Convert.ToInt32(((XmlElement)conta[0]).GetElementsByTagName("Banco")[0].InnerText)
                        },
                        Inscricao = ((XmlElement)beneficiario[0]).GetElementsByTagName("Inscricao")[0].InnerText,
                        Nome = ((XmlElement)beneficiario[0]).GetElementsByTagName("Nome")[0].InnerText
                    }
                };

                var qrCodeConfig = ConteudoXML.GetElementsByTagName("QRCodeConfig");
                if (qrCodeConfig.Count > 0)
                {
                    pixCobrancaCreateRequest.QRCodeConfig = new QRCodeConfig
                    {
                        Width = ((XmlElement)qrCodeConfig[0]).GetElementsByTagName("Width").Count > 0 ? Convert.ToInt32(((XmlElement)qrCodeConfig[0]).GetElementsByTagName("Width")[0].InnerText) : 512,
                        Height = ((XmlElement)qrCodeConfig[0]).GetElementsByTagName("Height").Count > 0 ? Convert.ToInt32(((XmlElement)qrCodeConfig[0]).GetElementsByTagName("Height")[0].InnerText) : 512,
                        Quality = ((XmlElement)qrCodeConfig[0]).GetElementsByTagName("Quality").Count > 0 ? Convert.ToInt32(((XmlElement)qrCodeConfig[0]).GetElementsByTagName("Quality")[0].InnerText) : 100,
                        ImageFormat = (QrCodeImageFormat)(((XmlElement)qrCodeConfig[0]).GetElementsByTagName("ImageFormat").Count > 0 ? Convert.ToInt32(((XmlElement)qrCodeConfig[0]).GetElementsByTagName("ImageFormat")[0].InnerText) : 0)
                    };
                }

                var calendario = ConteudoXML.GetElementsByTagName("Calendario");

                if (pixCobrancaCreateRequest.TipoCobranca == TipoCobranca.CobV && calendario.Count <= 0)
                {
                    throw new Exception("Se o conteudo da tag <TipoCobranca> for igual a 1 e obrigatorio informar o grupo de tag <Calendario>.");
                }

                if (calendario.Count > 0)
                {
                    pixCobrancaCreateRequest.Calendario = new Calendario();

                    if (((XmlElement)calendario[0]).GetElementsByTagName("Criacao").Count > 0)
                    {
                        pixCobrancaCreateRequest.Calendario.Criacao = DateTime.ParseExact(((XmlElement)calendario[0]).GetElementsByTagName("Criacao")[0].InnerText, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        throw new Exception("E obrigatorio informar a tag <Criacao>, que fica dentro do grupo de tag <Calendario>, no XML.");
                    }

                    if (((XmlElement)calendario[0]).GetElementsByTagName("Expiracao").Count > 0)
                    {
                        pixCobrancaCreateRequest.Calendario.Expiracao = Convert.ToInt32(((XmlElement)calendario[0]).GetElementsByTagName("Expiracao")[0].InnerText);
                    }
                    else
                    {
                        if (((XmlElement)calendario[0]).GetElementsByTagName("ValidadeAposVencimento").Count > 0)
                        {
                            pixCobrancaCreateRequest.Calendario.ValidadeAposVencimento = Convert.ToInt32(((XmlElement)calendario[0]).GetElementsByTagName("ValidadeAposVencimento")[0].InnerText);
                        }
                        if (((XmlElement)calendario[0]).GetElementsByTagName("DataDeVencimento").Count > 0)
                        {
                            pixCobrancaCreateRequest.Calendario.DataDeVencimento = DateTime.ParseExact(((XmlElement)calendario[0]).GetElementsByTagName("DataDeVencimento")[0].InnerText, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
                        }
                    }
                }

                var devedor = ConteudoXML.GetElementsByTagName("Devedor");

                if (pixCobrancaCreateRequest.TipoCobranca == TipoCobranca.CobV && devedor.Count <= 0)
                {
                    throw new Exception("Se o conteudo da tag <TipoCobranca> for igual a 1 e obrigatorio informar o grupo de tag <Devedor>.");
                }

                if (devedor.Count > 0)
                {
                    pixCobrancaCreateRequest.Devedor = new Devedor();

                    if (((XmlElement)devedor[0]).GetElementsByTagName("Nome").Count > 0)
                    {
                        pixCobrancaCreateRequest.Devedor.Nome = ((XmlElement)devedor[0]).GetElementsByTagName("Nome")[0].InnerText;
                    }

                    if (((XmlElement)devedor[0]).GetElementsByTagName("Inscricao").Count > 0)
                    {
                        pixCobrancaCreateRequest.Devedor.Inscricao = ((XmlElement)devedor[0]).GetElementsByTagName("Inscricao")[0].InnerText;
                    }
                    else
                    {
                        throw new Exception("E obrigatorio informar a tag <Inscricao>, que fica dentro do grupo de tag <Devedor>, no XML.");
                    }

                    if (((XmlElement)devedor[0]).GetElementsByTagName("CEP").Count > 0)
                    {
                        pixCobrancaCreateRequest.Devedor.CEP = ((XmlElement)devedor[0]).GetElementsByTagName("CEP")[0].InnerText;
                    }
                    if (((XmlElement)devedor[0]).GetElementsByTagName("Logradouro").Count > 0)
                    {
                        pixCobrancaCreateRequest.Devedor.Logradouro = ((XmlElement)devedor[0]).GetElementsByTagName("Logradouro")[0].InnerText;
                    }
                    if (((XmlElement)devedor[0]).GetElementsByTagName("Cidade").Count > 0)
                    {
                        pixCobrancaCreateRequest.Devedor.Cidade = ((XmlElement)devedor[0]).GetElementsByTagName("Cidade")[0].InnerText;
                    }
                    if (((XmlElement)devedor[0]).GetElementsByTagName("UF").Count > 0)
                    {
                        pixCobrancaCreateRequest.Devedor.UF = ((XmlElement)devedor[0]).GetElementsByTagName("UF")[0].InnerText;
                    }
                }

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

                var pixService = new PIXService();

                var pixResponse = await pixService.CreateCobAsync(pixCobrancaCreateRequest, authenticatedScope);

                authenticatedScope.Dispose();

                #endregion

                #region Gravar XML de Retorno

                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                //Verifica se o PIX foi ativo, se sim, podemos demonstrar o QRCode para o pagador
                if (pixResponse.Status == StatusCobranca.Ativa)
                {
                    //Se configurado para gerar o QRCode
                    var ext = "." + pixCobrancaCreateRequest.QRCodeConfig.ImageFormat.ToString().ToLower();
                    file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).RetornoXML;
                    var pathQRCode = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file.Replace(".xml", ext));
                    if (pixCobrancaCreateRequest.GerarQRCode)
                    {
                        WriteBase64ToFile(pixResponse.QRCodeImage, pathQRCode);
                    }


                    GerarXmlRetorno(
                       pathXml,
                       ((int)pixResponse.Status).ToString(),
                       pixResponse.PixCopiaECola,
                       pathQRCode,
                       "");
                }
                else
                {
                    GerarXmlRetorno(
                       pathXml,
                       ((int)pixResponse.Status).ToString(),
                       "",
                       "",
                       "");
                }

                #endregion
            }
            catch (Exception ex)
            {
                var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PIXCobrancaCreateRequest).RetornoXML;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                GerarXmlRetorno(
                   pathXml,
                   "999",
                   "",
                   "",
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

        /// <summary>
        /// Escreve uma string base64 em um arquivo.
        /// <para>A string já deve ser um arquivo válido. Este método apenas escreve o arquivo</para>
        /// </summary>
        /// <param name="content">Conteúdo que será escrito no arquivo</param>
        /// <param name="path">Pasta e nome do arquivo onde deve ser gravado</param>
        /// <exception cref="ArgumentNullException">Se o <paramref name="content"/> for nulo</exception>
        /// <exception cref="ArgumentException">Se o <paramref name="path"/> for nulo, vazio ou espaços</exception>
        private void WriteBase64ToFile(string content, string path)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            WriteBytesToFile(Convert.FromBase64String(content), path);
        }

        /// <summary>
        /// Escreve os bytes um arquivo.
        /// <para>Os bytes já devem ser um arquivo válido. Este método apenas escreve o arquivo</para>
        /// </summary>
        /// <param name="byteArray">Bytes que serão escritos no arquivo</param>
        /// <param name="path">Pasta e nome do arquivo onde deve ser gravado</param>
        /// <exception cref="ArgumentNullException">Se o <paramref name="content"/> for nulo</exception>
        /// <exception cref="ArgumentException">Se o <paramref name="path"/> for nulo, vazio ou espaços</exception>
        private void WriteBytesToFile(byte[] byteArray, string path)
        {
            if (byteArray is null)
            {
                throw new ArgumentNullException(nameof(byteArray));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
            }

            var fi = new FileInfo(path);

            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            if (fi.Exists)
            {
                fi.Delete();
            }

            File.WriteAllBytes(fi.FullName, byteArray);
        }

        private void GerarXmlRetorno(string path, string status, string pixCopiaECola, string pathQRCode, string motivo)
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
                        motivo = "PIX Ativo (Cobranca gerada)";
                        break;
                    case "1":
                        motivo = "PIX Concluído (PIX ja foi pago)";
                        break;
                    case "2":
                        motivo = "Cobranca do PIX removida pelo usuario recebedor";
                        break;
                    case "3":
                        motivo = "Cobranca do PIX removida pelo PSP do recebedor";
                        break;
                }

                oXmlGravar = XmlWriter.Create(path, oSettings);
                oXmlGravar.WriteStartDocument();
                oXmlGravar.WriteStartElement("PIXCobrancaCreateResponse");
                oXmlGravar.WriteElementString("Status", status);
                oXmlGravar.WriteElementString("Motivo", motivo);
                oXmlGravar.WriteElementString("PixCopiaECola", pixCopiaECola);
                oXmlGravar.WriteElementString("ImageQRCode", pathQRCode);
                oXmlGravar.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
                oXmlGravar.WriteEndElement(); //PIXCobrancaCreateResponse
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