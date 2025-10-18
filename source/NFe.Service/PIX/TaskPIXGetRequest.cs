using EBank.Solutions.Primitives.Billet.Models;
using EBank.Solutions.Primitives.Enumerations;
using EBank.Solutions.Primitives.PIX.Models.Consulta;
using EBank.Solutions.Primitives.PIX.Request.Consulta;
using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Unimake.AuthServer.Authentication;
using Unimake.AuthServer.Security.Scope;
using Unimake.EBank.Solutions.Services.PIX;
using Unimake.Primitives.UDebug;

namespace NFe.Service
{
    public class TaskPIXGetRequest : TaskAbst
    {
        public static DebugScope<DebugStateObject> debugScope;

        public TaskPIXGetRequest(string arquivo)
        {
            Servico = Servicos.PIXGetRequest;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override async void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            var extEnvio = Propriedade.Extensao((Servico == Servicos.PIXGetRequest ? Propriedade.TipoEnvio.PIXGetRequest : Propriedade.TipoEnvio.PIXConsultaRequest)).EnvioXML;
            var extRetorno = Propriedade.Extensao((Servico == Servicos.PIXGetRequest ? Propriedade.TipoEnvio.PIXGetRequest : Propriedade.TipoEnvio.PIXConsultaRequest)).RetornoXML;

            try
            {
                #region Validar o XML

                var validarXML = new ValidarXML
                {
                    TipoArqXml = new TipoArquivoXML
                    {
                        cArquivoSchema = Path.Combine(Propriedade.PastaExecutavel, @"NFe\schemas\PIX\" + (Servico == Servicos.PIXGetRequest ? "PIXGetRequest_1_00.xsd" : "PIXConsultaRequest_1_00.xsd")),
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

                var beneficiario = ConteudoXML.GetElementsByTagName("Beneficiario");

                if (beneficiario.Count <= 0)
                {
                    throw new Exception("É obrigatório informar o grupo de tag <Beneficiario> no XML.");
                }

                var conta = ((XmlElement)beneficiario[0]).GetElementsByTagName("Conta");

                if (conta.Count <= 0)
                {
                    throw new Exception("É obrigatório informar o grupo de tag <Conta>, que fica dentro do grupo de tag <Beneficiario>, no XML.");
                }
                if (((XmlElement)conta[0]).GetElementsByTagName("Agencia").Count <= 0)
                {
                    throw new Exception("É obrigatório informar a tag <Agencia>, que fica dentro do grupo de tag <Conta>, no XML.");
                }
                if (((XmlElement)conta[0]).GetElementsByTagName("Numero").Count <= 0)
                {
                    throw new Exception("É obrigatório informar a tag <Numero>, que fica dentro do grupo de tag <Conta>, no XML.");
                }
                if (((XmlElement)conta[0]).GetElementsByTagName("Banco").Count <= 0)
                {
                    throw new Exception("É obrigatório informar a tag <Banco>, que fica dentro do grupo de tag <Conta>, no XML.");
                }

                if (((XmlElement)beneficiario[0]).GetElementsByTagName("Inscricao").Count <= 0)
                {
                    throw new Exception("É obrigatório informar a tag <Inscricao>, que fica dentro do grupo de tag <Beneficiario>, no XML.");
                }
                if (((XmlElement)beneficiario[0]).GetElementsByTagName("Nome").Count <= 0)
                {
                    throw new Exception("É obrigatório informar a tag <Nome>, que fica dentro do grupo de tag <Beneficiario>, no XML.");
                }

                #endregion

                #region Consultar

                var pixGetRequest = new PIXGetRequest
                {
                    StartDate = DateTime.ParseExact(ConteudoXML.GetElementsByTagName("StartDate")[0].InnerText, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EndDate = DateTime.ParseExact(ConteudoXML.GetElementsByTagName("EndDate")[0].InnerText, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TxId = (ConteudoXML.GetElementsByTagName("TxId").Count > 0 ? ConteudoXML.GetElementsByTagName("TxId")[0].InnerText.Trim() : null),
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

                //Verificar se o intervalo entre a data inicial e final não supera 5 dias, se superar tem que rejeitar pq no eBank só aceita no máximo 5 dias, assim evitamos o consumo da API.
                if (((TimeSpan)(pixGetRequest.EndDate - pixGetRequest.StartDate)).Days >= 5)
                {
                    throw new Exception("O período de consulta para o PIX, entre as datas \"StartDate\" e \"EndDate\", não pode exceder 5 dias.");
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

                var respostas = new List<PIXItem>();

                var pagina = 1;
                while (true)
                {
                    pixGetRequest.PageNumber = pagina;

                    var pixService = new PIXService();
                    var response = await pixService.GetAsync(pixGetRequest, authenticatedScope);

                    respostas.AddRange(response.Items);

                    if (pagina >= response.PageInfo.TotalPages)
                    {
                        break;
                    }

                    ++pagina;
                }

                authenticatedScope.Dispose();

                #endregion

                #region Gravar XML de Retorno

                var file = Functions.ExtrairNomeArq(NomeArquivoXML, extEnvio) + extRetorno;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                GerarXmlRetorno(pathXml, respostas);

                #endregion
            }
            catch (Unimake.EBank.Solutions.Exceptions.ResponseException ex)
            {
                var file = Functions.ExtrairNomeArq(NomeArquivoXML, extEnvio) + extRetorno;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    GerarXmlRetorno(pathXml, null, "Nenhum Movimento PIX foi localizado.", "2");
                }
                else
                {
                    GerarXmlRetorno(pathXml, null, ex.GetLastException().Message.Replace("\r\n", ""));
                }
            }
            catch (Exception ex)
            {
                var file = Functions.ExtrairNomeArq(NomeArquivoXML, extEnvio) + extRetorno;
                var pathXml = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, file);

                GerarXmlRetorno(pathXml, null, ex.GetLastException().Message.Replace("\r\n", ""));
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

        private void GerarXmlRetorno(string path, List<PIXItem> respostas, string exceptionMessage = "", string status = "")
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

                oXmlGravar = XmlWriter.Create(path, oSettings);
                oXmlGravar.WriteStartDocument();
                oXmlGravar.WriteStartElement((Servico == Servicos.PIXGetRequest ? "PIXGetResponse" : "PIXConsultaResponse"));

                if (respostas.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        oXmlGravar.WriteElementString("Status", status);
                    }
                    else
                    {
                        oXmlGravar.WriteElementString("Status", "999");
                    }
                    oXmlGravar.WriteElementString("Motivo", exceptionMessage);
                }
                else
                {
                    oXmlGravar.WriteElementString("Status", (respostas.Count > 0 ? "1" : "2"));
                    oXmlGravar.WriteElementString("Motivo", (respostas.Count == 1 ? "Movimento PIX foi localizado." : (respostas.Count == 0 ? "Nenhum Movimento PIX foi localizado." : "Movimentos PIX localizados.")));

                    if (Servico == Servicos.PIXGetRequest)
                    {
                        if (respostas.Count == 1)
                        {
                            oXmlGravar.WriteElementString("TxId", respostas[0].TxId);
                            oXmlGravar.WriteElementString("Valor", respostas[0].Valor.ToString("N2", cultura));
                            oXmlGravar.WriteElementString("Horario", respostas[0].Horario.ToString("yyyy-MM-dd'T'HH:mm:ss"));

                            oXmlGravar.WriteStartElement("Pagador");
                            oXmlGravar.WriteElementString("Nome", respostas[0].Pagador.Nome);
                            oXmlGravar.WriteElementString("Inscricao", respostas[0].Pagador.Inscricao);
                            oXmlGravar.WriteEndElement(); //Pagador
                        }
                    }
                    else
                    {
                        if (respostas.Count > 0)
                        {
                            oXmlGravar.WriteStartElement("Items");

                            for (var item = 0; item < respostas.Count; item++)
                            {
                                oXmlGravar.WriteStartElement("Item");
                                oXmlGravar.WriteAttributeString("Id", (item + 1).ToString());

                                oXmlGravar.WriteElementString("TxId", respostas[item].TxId);
                                oXmlGravar.WriteElementString("Valor", respostas[item].Valor.ToString("N2", cultura));
                                oXmlGravar.WriteElementString("Horario", respostas[item].Horario.ToString("yyyy-MM-dd'T'HH:mm:ss"));

                                oXmlGravar.WriteStartElement("Pagador");
                                oXmlGravar.WriteElementString("Nome", respostas[item].Pagador.Nome);
                                oXmlGravar.WriteElementString("Inscricao", respostas[item].Pagador.Inscricao);
                                oXmlGravar.WriteEndElement(); //Pagador

                                oXmlGravar.WriteEndElement(); //Item
                            }

                            oXmlGravar.WriteEndElement(); //Items
                        }
                    }
                }

                oXmlGravar.WriteElementString("UniNFeVersao", Propriedade.Versao + " | " + Propriedade.DataHoraUltimaModificacaoAplicacao.Replace("/", "-"));
                oXmlGravar.WriteEndElement(); //PIXGetResponse ou PIXConsultaResponse
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