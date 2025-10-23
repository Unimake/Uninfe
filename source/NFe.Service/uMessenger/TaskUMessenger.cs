using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Unimake.AuthServer.Security.Scope;
using Unimake.MessageBroker.Primitives.Enumerations;
using Unimake.MessageBroker.Primitives.Model.Messages;
using Unimake.MessageBroker.Primitives.Model.Notifications;
using Unimake.MessageBroker.Services;
using Unimake.Primitives.UDebug;

namespace NFe.Service
{
    public class TaskUMessenger : TaskAbst
    {
        public static DebugScope<DebugStateObject> debugScope;

        private readonly XmlWriterSettings XmlSetting = new XmlWriterSettings();
        private XmlWriter XmlGravar = null;

        public enum ServiceUMessenger
        {
            PIXNotification,
            BilletNotification,
            SendTextMessage,
            NaoIdentificado
        }

        public TaskUMessenger(string arquivo)
        {
            Servico = Servicos.UMessenger;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override async void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            var file = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.UMessenger).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.UMessenger).RetornoXML;
            var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

            ConfigXMLRetorno();

            try
            {
                #region Validar o XML

                var tagUMessenger = ConteudoXML.GetElementsByTagName("uMessenger");
                var tagServico = ((XmlElement)tagUMessenger[0]).GetElementsByTagName("PIXNotification");

                var serviceMessage = ServiceUMessenger.PIXNotification;

                if (tagServico.Count <= 0)
                {
                    tagServico = ((XmlElement)tagUMessenger[0]).GetElementsByTagName("BilletNotification");

                    if (tagServico.Count > 0)
                    {
                        serviceMessage = ServiceUMessenger.BilletNotification;
                    }
                    else
                    {
                        tagServico = ((XmlElement)tagUMessenger[0]).GetElementsByTagName("SendTextMessage");
                        if (tagServico.Count > 0)
                        {
                            serviceMessage = ServiceUMessenger.SendTextMessage;
                        }
                        else
                        {
                            serviceMessage = ServiceUMessenger.NaoIdentificado;
                        }
                    }
                }

                var validarXML = new ValidarXML();

                switch (serviceMessage)
                {
                    case ServiceUMessenger.PIXNotification:
                        validarXML.TipoArqXml = new TipoArquivoXML
                        {
                            cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\uMessenger\uMessengerPIX_1_00.xsd"),
                            nRetornoTipoArq = 1
                        };
                        break;

                    case ServiceUMessenger.BilletNotification:
                        validarXML.TipoArqXml = new TipoArquivoXML
                        {
                            cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\uMessenger\uMessengerBoleto_1_00.xsd"),
                            nRetornoTipoArq = 1
                        };
                        break;

                    case ServiceUMessenger.SendTextMessage:
                        validarXML.TipoArqXml = new TipoArquivoXML
                        {
                            cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\uMessenger\uMessengerText_1_00.xsd"),
                            nRetornoTipoArq = 1
                        };
                        break;

                    case ServiceUMessenger.NaoIdentificado:
                        throw new Exception("Não foi possível identificar qual o tipo de serviço de envio de mensagens via WhatsApp deve ser utilizado.");
                }

                validarXML.ValidarArqXML(ConteudoXML, NomeArquivoXML);
                if (validarXML.Retorno != 0)
                {
                    throw new Exception(validarXML.RetornoString.Replace("\r\n", " - "));
                }

                #endregion

                var serviceNodeList = ConteudoXML.GetElementsByTagName(tagUMessenger[0].FirstChild.Name);
                var contagem = 0;

                foreach (var serviceNode in serviceNodeList)
                {
                    contagem++;

                    var serviceElement = (XmlElement)serviceNode;

                    #region Criar mensagem para WhatsApp e enviar

                    var messageID = string.Empty;
                    var returnMessageID = string.Empty;
                    var tags = serviceElement;

                    var useHomologServer = false;

                    if (serviceElement.GetElementsByTagName("UseHomologServer").Count > 0)
                    {
                        useHomologServer = Convert.ToBoolean(serviceElement.GetElementsByTagName("UseHomologServer")[0].InnerText);
                    }

                    debugScope = null;
                    if (useHomologServer)
                    {
                        debugScope = new DebugScope<DebugStateObject>(new DebugStateObject
                        {
                            AuthServerUrl = "https://auth.sandbox.unimake.software/api/auth/",
                            AnotherServerUrl = "https://umessenger.sandbox.unimake.software/api/v1/"
                        });
                    }



                    var authenticatedScope = new AuthenticatedScope(new Unimake.Primitives.Security.Credentials.AuthenticationToken
                    {
                        AppId = (Empresas.Configuracoes[emp].MesmosDadosEb_Mb ? Empresas.Configuracoes[emp].AppID : Empresas.Configuracoes[emp].AppID_UMessenger),
                        Secret = (Empresas.Configuracoes[emp].MesmosDadosEb_Mb ? Empresas.Configuracoes[emp].Secret : Empresas.Configuracoes[emp].Secret_UMessenger)
                    });



                    var service = new MessageService(MessagingService.WhatsApp);

                    switch (serviceMessage)
                    {
                        case ServiceUMessenger.PIXNotification:
                            var pixNotification = new PIXNotification
                            {
                                CopyAndPaste = tags.GetElementsByTagName("CopyAndPaste")[0].InnerText,
                                CompanyName = tags.GetElementsByTagName("CompanyName")[0].InnerText,
                                ContactPhone = tags.GetElementsByTagName("ContactPhone")[0].InnerText,
                                CustomerName = tags.GetElementsByTagName("CustomerName")[0].InnerText,
                                Description = tags.GetElementsByTagName("Description")[0].InnerText,
                                IssuedDate = tags.GetElementsByTagName("IssuedDate")[0].InnerText,
                                QueryString = tags.GetElementsByTagName("QueryString")[0].InnerText,
                                To = tags.GetElementsByTagName("To")[0].InnerText,
                                Value = tags.GetElementsByTagName("Value")[0].InnerText,
                                Testing = Convert.ToBoolean(tags.GetElementsByTagName("Testing")[0].InnerText),
                            };

                            var responseNotifyPIX = await service.NotifyPIXCollectionAsync(pixNotification, authenticatedScope);

                            returnMessageID = responseNotifyPIX.MessageId;

                            break;

                        case ServiceUMessenger.BilletNotification:
                            var notifyBilletAsync = new BilletNotification
                            {
                                BarCode = tags.GetElementsByTagName("BarCode")[0].InnerText,
                                BilletNumber = tags.GetElementsByTagName("BilletNumber")[0].InnerText,
                                CompanyName = tags.GetElementsByTagName("CompanyName")[0].InnerText,
                                ContactPhone = tags.GetElementsByTagName("ContactPhone")[0].InnerText,
                                CustomerName = tags.GetElementsByTagName("CustomerName")[0].InnerText,
                                Description = tags.GetElementsByTagName("Description")[0].InnerText,
                                DueDate = tags.GetElementsByTagName("DueDate")[0].InnerText,
                                QueryString = tags.GetElementsByTagName("QueryString")[0].InnerText,
                                To = tags.GetElementsByTagName("To")[0].InnerText,
                                Value = tags.GetElementsByTagName("Value")[0].InnerText,
                                Testing = Convert.ToBoolean(tags.GetElementsByTagName("Testing")[0].InnerText),
                            };

                            var responseNotifyBillet = await service.NotifyBilletAsync(notifyBilletAsync, authenticatedScope);

                            returnMessageID = responseNotifyBillet.MessageId;

                            break;

                        case ServiceUMessenger.SendTextMessage:
                            if (!string.IsNullOrWhiteSpace(tags.GetAttribute("Id")))
                            {
                                messageID = tags.GetAttribute("Id");
                            }

                            var textMessage = new TextMessage
                            {
                                To = new Unimake.MessageBroker.Primitives.Model.Recipient
                                {                                    
                                    Destination = tags.GetElementsByTagName("To")[0].InnerText,
                                },
                                InstanceName = tags.GetElementsByTagName("InstanceName")[0].InnerText,
                                Text = tags.GetElementsByTagName("Text")[0].InnerText.Replace("\\r", "\r").Replace("\\n", "\n"),
                                Testing = Convert.ToBoolean(tags.GetElementsByTagName("Testing")[0].InnerText)
                            };

                            if (tags.GetElementsByTagName("Files").Count > 0)
                            {
                                textMessage.Files = new System.Collections.Generic.List<Unimake.MessageBroker.Primitives.Model.Media.UploadFile>();

                                var filesElements = (XmlElement)tags.GetElementsByTagName("Files")[0];

                                foreach (XmlElement fileElement in filesElements.GetElementsByTagName("File"))
                                {
                                    if (fileElement.GetElementsByTagName("FullPath").Count > 0)
                                    {
                                        var fullPath = fileElement.GetElementsByTagName("FullPath")[0].InnerText;

                                        if (!File.Exists(fullPath))
                                        {
                                            throw new Exception($"O arquivo '{fullPath}' não foi encontrado.");
                                        }

                                        var description = string.Empty;
                                        if (fileElement.GetElementsByTagName("Description").Count > 0)
                                        {
                                            description = fileElement.GetElementsByTagName("Description")[0].InnerText;
                                        }

                                        var mediaType = MediaType.None;
                                        if (fileElement.GetElementsByTagName("MediaType").Count > 0)
                                        {
                                            mediaType = (MediaType)Enum.Parse(enumType: typeof(MediaType),
                                                fileElement.GetElementsByTagName("MediaType")[0].InnerText);
                                        }
                                        else
                                        {
                                            throw new Exception($"O tipo de mídia do '{fullPath}' arquivo não foi informado.");
                                        }

                                        textMessage.Files.Add(new Unimake.MessageBroker.Primitives.Model.Media.UploadFile
                                        {
                                            Base64Content = Convert.ToBase64String(File.ReadAllBytes(fullPath)),
                                            FileName = Path.GetFileName(fullPath),
                                            Caption = description,
                                            MediaType = mediaType
                                        });
                                    }
                                }
                            }

                            if (textMessage.Files != null)
                            {
                                if (textMessage.Files.Count > 0)
                                {
                                    service.TimeoutInSeconds = 180; //Aumentar o tempo de timeout para 2 minutos, para envio de mensagens com arquivos maiores.
                                }
                            }

                            var responseSendTextMessage = await service.SendTextMessageAsync(textMessage, authenticatedScope);
                        
         
                            returnMessageID = responseSendTextMessage.MessageId;

                            break;  
                    }

                    authenticatedScope.Dispose();

                    #endregion

                    #region Gravar o XML de retorno

                    
                    GerarXmlRetorno(pathXml, "1", "", returnMessageID, contagem == 1, contagem == serviceNodeList.Count, messageID);

                    #endregion

                    if (contagem != serviceNodeList.Count)
                    {
                        Thread.Sleep(3000); //Aguardar 3 segundos para enviar a próxima menagem.
                    }
                }
            }
            catch (Exception ex)
            {
                GerarXmlRetorno(pathXml, "999", ex.GetLastException().Message.Replace("\r\n", " - "), "");
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
        /// Definir configurações do XML de Retorno.
        /// </summary>
        private void ConfigXMLRetorno()
        {
            var c = new UTF8Encoding(false);

            XmlSetting.Encoding = c;
            XmlSetting.Indent = true;
            XmlSetting.IndentChars = " ";
            XmlSetting.NewLineOnAttributes = false;
            XmlSetting.OmitXmlDeclaration = false;
        }

        private void GerarXmlRetorno(string path, string status, string motivo, string returnMessageID = "", bool criarXML = true, bool encerrarXML = true, string messageID = "")
        {
            try
            {
                switch (status)
                {
                    case "1":
                        motivo = "Mensagem enviada com sucesso.";
                        break;
                }

                if (criarXML)
                {
                    XmlGravar = XmlWriter.Create(path, XmlSetting);
                    XmlGravar.WriteStartDocument();
                    XmlGravar.WriteStartElement("uMessengerResponse");
                }

                XmlGravar.WriteStartElement("Mensagem");

                if (!string.IsNullOrWhiteSpace(messageID))
                {
                    XmlGravar.WriteAttributeString("Id", messageID);
                }

                XmlGravar.WriteElementString("Status", status);
                XmlGravar.WriteElementString("Motivo", motivo);

                if (!string.IsNullOrWhiteSpace(returnMessageID) && status == "1")
                {
                    XmlGravar.WriteElementString("messageID", returnMessageID);
                }

                XmlGravar.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));

                XmlGravar.WriteEndElement(); //Mensagem

                if (encerrarXML)
                {
                    XmlGravar.WriteEndElement(); //uMessengerResponse
                    XmlGravar.WriteEndDocument();
                    XmlGravar.Flush();
                    XmlGravar.Close();
                }
            }
            finally
            {
                if (encerrarXML)
                {
                    if (XmlGravar != null)
                    {
                        if (XmlGravar.WriteState != WriteState.Closed)
                        {
                            XmlGravar.Close();
                        }
                    }
                }
            }
        }
    }
}