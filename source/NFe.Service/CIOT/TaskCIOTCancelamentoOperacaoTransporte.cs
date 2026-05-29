using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;
using ServicosCIOT = Unimake.Business.DFe.Servicos.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTCancelamentoOperacaoTransporte : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTPedEve;

        public TaskCIOTCancelamentoOperacaoTransporte(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTCancelamentoOperacaoTransporte;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);

            try
            {
                var xml = new XmlCIOT.CancelamentoOperacaoTransporte().LerXML<XmlCIOT.CancelamentoOperacaoTransporte>(ConteudoXML);
                using (var cancelamento = new ServicosCIOT.CancelamentoOperacaoTransporte(xml, CriarConfiguracao(emp)))
                {
                    cancelamento.Executar();
                    ConteudoXML = cancelamento.ConteudoXMLAssinado;

                    SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "CancelamentoOperacaoTransporte");

                    vStrXmlRetorno = cancelamento.RetornoWSString;

                    if (cancelamento.Result != null && cancelamento.Result.Codigo == "110")
                    {
                        FinalizarCIOT(cancelamento, emp, xml);
                    }
                    else
                    {
                        oAux.MoveArqErro(arqEmProcessamento);

                        if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                        {
                            var codigoRetorno = cancelamento.Result?.Codigo ?? "SEM-CODIGO";
                            var mensagemRetorno = (cancelamento.Result?.Mensagem ?? "Retorno do serviço CIOT sem mensagem.").Trim();
                            var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                            sendMessageToWhatsApp.AlertNotification("Rejeição: " + codigoRetorno + "-" + mensagemRetorno, "UNINFE - CIOT´s estão sendo rejeitados");
                        }
                    }

                    GravarRetorno();
                }
            }
            catch (Exception ex)
            {
                GravarErro(ex);
            }
            finally
            {
                DeletarArquivo();
            }
        }

        /// <summary>
        /// Finalizar o CIOT guardando o XML de distribuição
        /// </summary>
        /// <param name="cancelamento">Objeto do serviço de envio do cancelamento de operação de transporte</param>
        /// <param name="emp">Identificador da empresa</param>
        /// <param name="xmlCIOT">Objeto XML do cancelamento de operação de transporte</param>
        private void FinalizarCIOT(ServicosCIOT.CancelamentoOperacaoTransporte cancelamento, int emp, XmlCIOT.CancelamentoOperacaoTransporte xmlCIOT)
        {
            var fileCIOT = Path.GetFileName(NomeArquivoXML);
            var dataCancelamento = cancelamento.Result.DataCancelamento.Date;

            var fullPathCIOTEmProcessamento =
                Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado,
                PastaEnviados.EmProcessamento.ToString(),
                fileCIOT);

            var pathXMLAutorizado =
                Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado,
                PastaEnviados.Autorizados.ToString(),
                Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(dataCancelamento));

            if (!Directory.Exists(pathXMLAutorizado))
            {
                Directory.CreateDirectory(pathXMLAutorizado);
            }

            var fileCIOTProc = Functions.ExtrairNomeArq(fileCIOT, Propriedade.Extensao(Propriedade.TipoEnvio.CIOTPedEve).EnvioXML) + Propriedade.ExtRetorno.ProcEventoCIOT;

            var fullPathCIOT = Path.Combine(pathXMLAutorizado, fileCIOT);
            var fullPathCIOTProc = Path.Combine(pathXMLAutorizado, fileCIOTProc);

            //Verifica se a -procEventoCIOT.xml existe na pasta de autorizados
            if (!File.Exists(fullPathCIOTProc))
            {
                if (cancelamento.CancelamentoOperacaoTransporteProcResult == null)
                {
                    Auxiliar.WriteLog("TaskCIOTCancelamentoOperacaoTransporte: O retorno autorizado do CIOT não possui XML de distribuição para gravação do arquivo " + fullPathCIOTProc + ".", false);
                }
                else
                {
                    //Gravar o XML de distribuição do CIOT na pasta de autorizados para que o cliente tenha acesso a ele, caso contrário, se o cliente tentar acessar o XML de distribuição do CIOT e ele não tiver sido gravado, vai dar erro de arquivo não encontrado.
                    cancelamento.GravarXmlDistribuicao(pathXMLAutorizado, fileCIOTProc, cancelamento.CancelamentoOperacaoTransporteProcResult.GerarXML().OuterXml);
                }
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTCancelamentoOperacaoTransporte: O arquivo " + fullPathCIOTProc + " já existe na pasta de autorizados, não será gravado novamente.", false);
            }

            if (!File.Exists(fullPathCIOT))
            {
                if (File.Exists(fullPathCIOTEmProcessamento))
                {
                    TFunctions.MoverArquivo(fullPathCIOTEmProcessamento, PastaEnviados.Autorizados, dataCancelamento);
                }
                else
                {
                    Auxiliar.WriteLog("TaskCIOTCancelamentoOperacaoTransporte: O arquivo " + fullPathCIOTEmProcessamento + " não foi encontrado para mover para a pasta de autorizados.", false);
                }
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTCancelamentoOperacaoTransporte: O arquivo " + fullPathCIOT + " já existe na pasta de autorizados, não será movido novamente.", false);

                TFunctions.MoveArqErro(fullPathCIOTEmProcessamento);
            }

            if (File.Exists(fullPathCIOTProc))
            {
                try
                {
                    TFunctions.ExecutaUniDanfe(fullPathCIOTProc, dataCancelamento, Empresas.Configuracoes[emp]);
                }
                catch (Exception ex)
                {
                    Auxiliar.WriteLog("TaskCIOTCancelamentoOperacaoTransporte: " + ex.Message, false);
                }
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTCancelamentoOperacaoTransporte: O arquivo " + fullPathCIOTProc + " não foi encontrado para gerar o DANFE do CIOT.", false);
            }
        }
    }
}
