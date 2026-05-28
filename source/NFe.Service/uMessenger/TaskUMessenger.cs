using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service
{
    public class TaskUMessenger : TaskAbst
    {

        private readonly XmlWriterSettings XmlSetting = new XmlWriterSettings();
        private XmlWriter XmlGravar = null;


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
                // Usar padrão dos outros serviços: executar via DLL local (ExecuteDLL)
                ExecuteDLL(emp);

            }
            catch (Exception ex)
            {
                var lastException = ex.GetLastException();
                var traceId = ApiExceptionHelper.ExtrairTraceId(lastException);
                GerarXmlRetorno(pathXml, "999", lastException.Message.Replace("\r\n", " - "), "", true, true, "", traceId);
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

        #region ExecuteDLL

        /// <summary>
        /// Executa o serviço uMessenger utilizando a DLL local (padrão dos outros Task*).
        /// </summary>
        /// <param name="emp">Empresa que está solicitando o envio</param>
        private void ExecuteDLL(int emp)
        {
            var conteudoXML = ConteudoXML;

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.UMessenger).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.UMessenger).RetornoXML;

            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo,
                AppId = Empresas.Configuracoes[emp].AppID_UMessenger,
                Secret = Empresas.Configuracoes[emp].Secret_UMessenger
            };

            try
            {
                var assembly = typeof(Configuracao).Assembly;

                object publishInstance = null;
                var publishType = assembly.GetType("Unimake.Business.DFe.Servicos.UMessenger.PublishUMessenger");

                publishInstance = Activator.CreateInstance(publishType, new object[] { conteudoXML.OuterXml, configuracao });

                var executarMethod = publishInstance.GetType().GetMethod("Executar");
                if (executarMethod == null) throw new Exception("A implementação do serviço uMessenger não expõe o método Executar().");
                executarMethod.Invoke(publishInstance, null);

                var retornoProp = publishInstance.GetType().GetProperty("RetornoWSString");
                if (retornoProp != null) vStrXmlRetorno = retornoProp.GetValue(publishInstance)?.ToString();

                XmlRetorno(finalArqEnvio, finalArqRetorno);

                if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                {
                    throw new Exception("A implementação do serviço uMessenger não retornou RetornoWSString. Atualize a DLL para fornecer o XML de retorno pronto.");
                }

                var disposeMethod = publishInstance.GetType().GetMethod("Dispose");
                disposeMethod?.Invoke(publishInstance, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao executar DLL uMessenger: {ex.Message}", ex);
            }
        }

        #endregion

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

        private void GerarXmlRetorno(string path, string status, string motivo, string returnMessageID = "", bool criarXML = true, bool encerrarXML = true, string messageID = "", string traceId = "")
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

                ApiExceptionHelper.GravarXmlRetornoUMessenger(XmlGravar, status, motivo, returnMessageID, messageID, traceId);

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