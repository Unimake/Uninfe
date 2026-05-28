using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;
using ServicosCIOT = Unimake.Business.DFe.Servicos.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTDeclaracaoOperacaoTransporte : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOT;

        public TaskCIOTDeclaracaoOperacaoTransporte(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTDeclaracaoOperacaoTransporte;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);

            try
            {
                var xml = new XmlCIOT.DeclaracaoOperacaoTransporte().LerXML<XmlCIOT.DeclaracaoOperacaoTransporte>(ConteudoXML);
                var declaracao = new ServicosCIOT.DeclaracaoOperacaoTransporte(xml, CriarConfiguracao(emp));
                declaracao.Executar();
                ConteudoXML = declaracao.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "DeclaracaoOperacaoTransporte");

                vStrXmlRetorno = declaracao.RetornoWSString;

                if (declaracao.Result.Codigo == "110")
                {
                    FinalizarCIOT(declaracao, emp, xml);
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + declaracao.Result.Codigo + "-" + declaracao.Result.Mensagem.Trim(), "UNINFE - CIOT´s estão sendo rejeitados");
                    }
                }

                GravarRetorno();

                declaracao.Dispose();
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
        /// <param name="declaracao">Objeto do serviço de envio da declaração de operação de transporte</param>
        /// <param name="emp">Identificador da empresa</param>
        /// <param name="xmlCIOT">Objeto XML da declaração de operação de transporte</param>
        private void FinalizarCIOT(ServicosCIOT.DeclaracaoOperacaoTransporte declaracao, int emp, XmlCIOT.DeclaracaoOperacaoTransporte xmlCIOT)
        {
            var fileCIOT = Path.GetFileName(NomeArquivoXML);

            var fullPathCIOTEmProcessamento =
                Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado,
                PastaEnviados.EmProcessamento.ToString(),
                fileCIOT);

            var pathXMLAutorizado =
                Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado,
                PastaEnviados.Autorizados.ToString(),
                Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(xmlCIOT.DataDeclaracao.Date));

            if (!Directory.Exists(pathXMLAutorizado))
            {
                Directory.CreateDirectory(pathXMLAutorizado);
            }

            var fileCIOTProc = Functions.ExtrairNomeArq(fileCIOT, Propriedade.Extensao(Propriedade.TipoEnvio.CIOT).EnvioXML) + Propriedade.ExtRetorno.ProcCIOT;

            var fullPathCIOT = Path.Combine(pathXMLAutorizado, fileCIOT);
            var fullPathCIOTProc = Path.Combine(pathXMLAutorizado, fileCIOTProc);

            //Verifica se a -procCIOT.xml existe na pasta de autorizados
            if (!File.Exists(fullPathCIOTProc))
            {
                //Gravar o XML de distribuição do CIOT na pasta de autorizados para que o cliente tenha acesso a ele, caso contrário, se o cliente tentar acessar o XML de distribuição do CIOT e ele não tiver sido gravado, vai dar erro de arquivo não encontrado.
                declaracao.GravarXmlDistribuicao(pathXMLAutorizado, fileCIOTProc, declaracao.DeclaracaoOperacaoTransporteProcResult.GerarXML().OuterXml);
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTDeclaracaoOperacaoTransporte: O arquivo " + fullPathCIOTProc + " já existe na pasta de autorizados, não será gravado novamente.", false);
            }

            if (!File.Exists(fullPathCIOT))
            {
                if (File.Exists(fullPathCIOTEmProcessamento))
                {
                    TFunctions.MoverArquivo(fullPathCIOTEmProcessamento, PastaEnviados.Autorizados, xmlCIOT.DataDeclaracao.Date);
                }
                else
                {
                    Auxiliar.WriteLog("TaskCIOTDeclaracaoOperacaoTransporte: O arquivo " + fullPathCIOTEmProcessamento + " não foi encontrado para mover para a pasta de autorizados.", false);
                }
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTDeclaracaoOperacaoTransporte: O arquivo " + fullPathCIOT + " já existe na pasta de autorizados, não será movido novamente.", false);

                TFunctions.MoveArqErro(fullPathCIOTEmProcessamento);
            }

            if (File.Exists(fullPathCIOTProc))
            {
                try
                {
                    TFunctions.ExecutaUniDanfe(fullPathCIOTProc, xmlCIOT.DataDeclaracao.Date, Empresas.Configuracoes[emp]);
                }
                catch (Exception ex)
                {
                    Auxiliar.WriteLog("TaskCIOTDeclaracaoOperacaoTransporte: " + ex.Message, false);
                }
            }
            else
            {
                Auxiliar.WriteLog("TaskCIOTDeclaracaoOperacaoTransporte: O arquivo " + fullPathCIOTProc + " não foi encontrado para gerar o DANFE do CIOT.", false);
            }
        }
    }
}
