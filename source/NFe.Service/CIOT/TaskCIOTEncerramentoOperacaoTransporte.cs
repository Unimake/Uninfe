using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;
using ServicosCIOT = Unimake.Business.DFe.Servicos.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTEncerramentoOperacaoTransporte : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTPedEve;

        public TaskCIOTEncerramentoOperacaoTransporte(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTEncerramentoOperacaoTransporte;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);

            try
            {
                var xml = new XmlCIOT.EncerramentoOperacaoTransporte().LerXML<XmlCIOT.EncerramentoOperacaoTransporte>(ConteudoXML);
                using (var encerramento = new ServicosCIOT.EncerramentoOperacaoTransporte(xml, CriarConfiguracao(emp)))
                {
                    encerramento.Executar();
                    ConteudoXML = encerramento.ConteudoXMLAssinado;

                    SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "EncerramentoOperacaoTransporte");

                    vStrXmlRetorno = encerramento.RetornoWSString;

                    if (encerramento.Result != null && encerramento.Result.Codigo == "110")
                    {
                        FinalizarCIOT(encerramento, emp, xml);
                    }
                    else
                    {
                        oAux.MoveArqErro(arqEmProcessamento);

                        if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                        {
                            var codigoRetorno = encerramento.Result?.Codigo ?? "SEM-CODIGO";
                            var mensagemRetorno = (encerramento.Result?.Mensagem ?? "Retorno do serviço CIOT sem mensagem.").Trim();
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
        /// <param name="encerramento">Objeto do serviço de envio do encerramento de operação de transporte</param>
        /// <param name="emp">Identificador da empresa</param>
        /// <param name="xmlCIOT">Objeto XML do encerramento de operação de transporte</param>
        private void FinalizarCIOT(ServicosCIOT.EncerramentoOperacaoTransporte encerramento, int emp, XmlCIOT.EncerramentoOperacaoTransporte xmlCIOT)
        {
            var fileCIOT = Path.GetFileName(NomeArquivoXML);
            var dataEncerramento = ObterDataEncerramentoParaDistribuicao(
                xmlCIOT.ProvedorCIOT,
                encerramento.Result.DataEncerramento.Date,
                DateTime.Today);

            var fullPathCIOTEmProcessamento =
                Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado,
                PastaEnviados.EmProcessamento.ToString(),
                fileCIOT);

            var pathXMLAutorizado =
                Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado,
                PastaEnviados.Autorizados.ToString(),
                Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(dataEncerramento));

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
                if (encerramento.EncerramentoOperacaoTransporteProcResult == null)
                {
                    Auxiliar.WriteLog("TaskCIOTEncerramentoOperacaoTransporte: O retorno autorizado do CIOT não possui XML de distribuição para gravação do arquivo " + fullPathCIOTProc + ".", false);
                }
                else
                {
                    //Gravar o XML de distribuição do CIOT na pasta de autorizados para que o cliente tenha acesso a ele, caso contrário, se o cliente tentar acessar o XML de distribuição do CIOT e ele não tiver sido gravado, vai dar erro de arquivo não encontrado.
                    encerramento.GravarXmlDistribuicao(pathXMLAutorizado, fileCIOTProc, encerramento.EncerramentoOperacaoTransporteProcResult.GerarXML().OuterXml);
                }
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTEncerramentoOperacaoTransporte: O arquivo " + fullPathCIOTProc + " já existe na pasta de autorizados, não será gravado novamente.", false);
            }

            if (!File.Exists(fullPathCIOT))
            {
                if (File.Exists(fullPathCIOTEmProcessamento))
                {
                    TFunctions.MoverArquivo(fullPathCIOTEmProcessamento, PastaEnviados.Autorizados, dataEncerramento);
                }
                else
                {
                    Auxiliar.WriteLog("TaskCIOTEncerramentoOperacaoTransporte: O arquivo " + fullPathCIOTEmProcessamento + " não foi encontrado para mover para a pasta de autorizados.", false);
                }
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTEncerramentoOperacaoTransporte: O arquivo " + fullPathCIOT + " já existe na pasta de autorizados, não será movido novamente.", false);

                TFunctions.MoveArqErro(fullPathCIOTEmProcessamento);
            }

            if (File.Exists(fullPathCIOTProc))
            {
                try
                {
                    UniDanfe.Executar(fullPathCIOTProc, dataEncerramento, Empresas.Configuracoes[emp]);
                }
                catch (Exception ex)
                {
                    Auxiliar.WriteLog("TaskCIOTEncerramentoOperacaoTransporte: " + ex.Message, false);
                }
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTEncerramentoOperacaoTransporte: O arquivo " + fullPathCIOTProc + " não foi encontrado para gerar o DANFE do CIOT.", false);
            }
        }

        /// <summary>
        /// Obtém a data usada exclusivamente para organizar os arquivos autorizados.
        /// </summary>
        /// <param name="provedorCIOT">Provedor informado no XML de envio.</param>
        /// <param name="dataEncerramento">Data devolvida pelo provedor.</param>
        /// <param name="dataProcessamento">Data em que o UniNFe processou o encerramento.</param>
        /// <returns>Data que deve ser usada na distribuição dos arquivos.</returns>
        internal static DateTime ObterDataEncerramentoParaDistribuicao(
            Unimake.Business.DFe.Servicos.ProvedorCIOT? provedorCIOT,
            DateTime dataEncerramento,
            DateTime dataProcessamento)
        {
            // A eFrete não devolve DataEncerramento. A data de processamento é usada
            // somente na organização física dos arquivos e não é incluída no XML retornado.
            if (provedorCIOT == Unimake.Business.DFe.Servicos.ProvedorCIOT.EFrete &&
                dataEncerramento == DateTime.MinValue)
            {
                return dataProcessamento.Date;
            }

            return dataEncerramento.Date;
        }
    }
}
