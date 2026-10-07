using NFe.Components;
using NFe.Exceptions;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFe;
using Unimake.Exceptions;

namespace NFe.Service
{
    public class TaskNFeRecepcao : TaskAbst
    {
        #region Private Fields

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do recibo do lote
        /// </summary>
        private DadosRecClass dadosRec;

        private readonly HashSet<string> arquivosNumeroLotePublicados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        #endregion Private Fields

        #region Private Methods

        private IEnumerable<KeyValuePair<string, string>> ObterArquivosNumeroLote()
        {
            if(ArquivosTemporariosNumeroLote != null && ArquivosTemporariosNumeroLote.Count > 0)
            {
                return ArquivosTemporariosNumeroLote;
            }

            return new[] { new KeyValuePair<string, string>(NomeArqTempXMLLote, NomeArqTempTXTLote) };
        }

        private void PublicarNumerosLote(int emp, bool continuarEmCasoDeErro)
        {
            foreach(var arquivos in ObterArquivosNumeroLote())
            {
                PublicarNumeroLote(emp, arquivos.Key, continuarEmCasoDeErro);
                if(Empresas.Configuracoes[emp].GravarRetornoTXTNFe && !string.IsNullOrWhiteSpace(arquivos.Key))
                {
                    PublicarNumeroLote(emp, arquivos.Value, continuarEmCasoDeErro);
                }
            }
        }

        private void PublicarNumeroLote(int emp, string arquivoTemporario, bool continuarEmCasoDeErro)
        {
            if(string.IsNullOrWhiteSpace(arquivoTemporario) || arquivosNumeroLotePublicados.Contains(arquivoTemporario))
            {
                return;
            }

            try
            {
                var arquivoRetorno = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, Path.GetFileName(arquivoTemporario));
                File.Copy(arquivoTemporario, arquivoRetorno, true);
                // O ERP pode consumir o retorno antes de uma falha na publicação do próximo arquivo.
                arquivosNumeroLotePublicados.Add(arquivoTemporario);
                Auxiliar.WriteLog("TaskNFeRecepcao: Número do lote publicado para o ERP. Arquivo=" + arquivoRetorno, false);
                Functions.DeletarArquivo(arquivoTemporario);
            }
            catch(Exception ex)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao: Falha ao publicar o número do lote para o ERP. Arquivo=" + arquivoTemporario + ", erro=" + ex.GetAllMessages(), true);
                if(!continuarEmCasoDeErro)
                {
                    throw;
                }
            }
        }

        private void LimparNumerosLotePublicados(int emp)
        {
            foreach(var arquivos in ObterArquivosNumeroLote())
            {
                LimparNumeroLote(arquivos.Key, arquivosNumeroLotePublicados.Contains(arquivos.Key ?? string.Empty));
                LimparNumeroLote(arquivos.Value, !Empresas.Configuracoes[emp].GravarRetornoTXTNFe ||
                    arquivosNumeroLotePublicados.Contains(arquivos.Value ?? string.Empty));
            }
        }

        private static void LimparNumeroLote(string arquivo, bool excluir)
        {
            if(!excluir || string.IsNullOrWhiteSpace(arquivo))
            {
                return;
            }

            try
            {
                Functions.DeletarArquivo(arquivo);
            }
            catch(Exception ex)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao: Falha ao excluir referência temporária de lote já publicada. Arquivo=" + arquivo + ", erro=" + ex.GetAllMessages(), true);
            }
        }

        private string ObterNomeArquivoNFe(int emp, string chaveNFe)
        {
            var nome = new FluxoNfe().LerTag(chaveNFe, FluxoNfe.ElementoFixo.ArqNFe);
            if(!string.IsNullOrWhiteSpace(nome))
            {
                return nome;
            }

            foreach(var pasta in ObterPastasNFe(emp))
            {
                if(!Directory.Exists(pasta))
                {
                    continue;
                }

                foreach(var arquivo in Directory.GetFiles(pasta, "*" + Propriedade.ExtEnvio.NFe))
                {
                    try
                    {
                        var xml = new XmlDocument();
                        xml.Load(arquivo);
                        foreach(XmlElement infNFe in xml.GetElementsByTagName("infNFe"))
                        {
                            if(infNFe.GetAttribute("Id") == chaveNFe)
                            {
                                return Path.GetFileName(arquivo);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Auxiliar.WriteLog("TaskNFeRecepcao: Falha ao identificar XML da nota. Arquivo=" + arquivo + ", erro=" + ex.GetAllMessages(), true);
                    }
                }
            }

            throw new Exception("Não foi possível localizar o arquivo XML da nota com chave " + chaveNFe + ".");
        }

        private static IEnumerable<string> ObterPastasNFe(int emp)
        {
            var empresa = Empresas.Configuracoes[emp];
            if(!string.IsNullOrWhiteSpace(empresa.PastaXmlEnvio))
            {
                yield return Path.Combine(empresa.PastaXmlEnvio, "temp");
            }

            if(!string.IsNullOrWhiteSpace(empresa.PastaXmlEmLote))
            {
                yield return Path.Combine(empresa.PastaXmlEmLote, "temp");
            }

            if(!string.IsNullOrWhiteSpace(empresa.PastaXmlEnviado))
            {
                yield return Path.Combine(empresa.PastaXmlEnviado, PastaEnviados.EmProcessamento.ToString());
            }
        }

        private void EncerrarFalhaLocal(int emp)
        {
            foreach(XmlElement infNFe in ConteudoXML.GetElementsByTagName("infNFe"))
            {
                EncerrarFalhaLocal(emp, infNFe);
            }
        }

        private void EncerrarFalhaLocal(int emp, XmlElement infNFe)
        {
            var empresa = Empresas.Configuracoes[emp];
            var chave = infNFe.GetAttribute("Id");
            try
            {
                var nome = ObterNomeArquivoNFe(emp, chave);
                var moveu = false;
                foreach(var pasta in ObterPastasNFe(emp))
                {
                    var arquivo = Path.Combine(pasta, nome);
                    if(File.Exists(arquivo))
                    {
                        TFunctions.MoveArqErro(arquivo);
                        if(File.Exists(arquivo) || !File.Exists(Path.Combine(empresa.PastaXmlErro, nome)))
                        {
                            throw new IOException("Não foi possível confirmar o XML da nota na pasta de erro.");
                        }

                        moveu = true;
                    }
                }

                if(!moveu)
                {
                    throw new IOException("O arquivo XML da nota não foi localizado para movimentação à pasta de erro.");
                }

                new FluxoNfe().ExcluirNfeFluxo(chave);
                Auxiliar.WriteLog("TaskNFeRecepcao: Falha local encerrada. XML movido para Erro e nota retirada do fluxo. Chave=" + chave, false);
            }
            catch(Exception ex)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao: Falha ao encerrar nota com erro local. XML e fluxo pendentes de recuperação. Chave=" + chave + ", erro=" + ex.GetAllMessages(), true);
            }
        }

        /// <summary>
        /// Copia o arquivo temporário para o retorno, substituindo eventual retorno anterior, e exclui a origem.
        /// </summary>
        /// <param name="arquivoTemporario">Arquivo temporário.</param>
        /// <param name="arquivoRetorno">Arquivo que será disponibilizado para o ERP.</param>
        private static void PublicarArquivoNumeroLote(string arquivoTemporario, string arquivoRetorno)
        {
            File.Copy(arquivoTemporario, arquivoRetorno, true);
            Functions.DeletarArquivo(arquivoTemporario);
        }

        /// <summary>
        /// Ajusta grupos monofásicos legados para a modalidade exigida no ano da emissão em homologação.
        /// Produção permanece no leiaute legado até que sua ativação seja deliberadamente autorizada.
        /// </summary>
        /// <param name="lote">Lote de NFe/NFCe que será autorizado.</param>
        internal static void AjustarLeiauteMonofasiaHomologacao(EnviNFe lote)
        {
            if(lote?.NFe == null)
            {
                return;
            }

            foreach(var nfe in lote.NFe)
            {
                var infNFe = nfe?.InfNFeField;
                if(infNFe?.Ide == null || infNFe.Ide.TpAmb != TipoAmbiente.Homologacao || infNFe.Det == null)
                {
                    continue;
                }

                var versaoLeiaute = ObterVersaoLeiauteMonofasia(infNFe.Ide.DhEmi.Year);
                foreach(var detalhe in infNFe.Det)
                {
                    var monofasia = detalhe?.Imposto?.IBSCBS?.GIBSCBSMono;
                    if(monofasia != null && monofasia.VersaoLeiaute == VersaoLeiauteMonofasia.Legado)
                    {
                        monofasia.VersaoLeiaute = versaoLeiaute;
                    }
                }
            }
        }

        private static VersaoLeiauteMonofasia ObterVersaoLeiauteMonofasia(int anoEmissao)
        {
            if(anoEmissao < 2026)
            {
                return VersaoLeiauteMonofasia.Legado;
            }

            if(anoEmissao == 2026)
            {
                return VersaoLeiauteMonofasia.Atual2026;
            }

            return anoEmissao <= 2028
                ? VersaoLeiauteMonofasia.Atual2027A2028
                : VersaoLeiauteMonofasia.Atual2029EmDiante;
        }

        /// <summary>
        /// Finalizar a NFe no processo Síncrono
        /// </summary>
        /// <param name="xmlRetorno">Conteúdo do XML retornado da SEFAZ</param>
        /// <param name="emp">Código da empresa para buscar as configurações</param>
        private void FinalizarNFeSincrono(string xmlRetorno, int emp, string chNFe)
        {
            try
            {
                var xml = new XmlDocument();
                xml.Load(Functions.StringXmlToStream(xmlRetorno));

                var protNFe = xml.GetElementsByTagName("protNFe");

                if(protNFe == null || protNFe.Count == 0)
                {
                    Auxiliar.WriteLog("TaskNFeRecepcao.FinalizarNFeSincrono: XML de retorno sem tag protNFe. Chave=" + chNFe, true);
                    throw new Exception("Não foi possível localizar a tag protNFe no retorno do envio síncrono.");
                }

                var fluxoNFe = new FluxoNfe();

                var retRecepcao = new TaskNFeRetRecepcao
                {
                    chNFe = chNFe
                };

                retRecepcao.FinalizarNFe(protNFe, fluxoNFe, emp, ConteudoXML);
            }
            catch(Exception ex)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao.FinalizarNFeSincrono: Erro ao finalizar NFe síncrona. " + ex.Message, true);
                throw;
            }
        }

        /// <summary>
        /// Preservar a NFe em EmProcessamento quando ocorrer falha tecnica sem retorno fiscal conclusivo.
        /// </summary>
        /// <param name="emp">Empresa</param>
        /// <param name="dadosNFe">Dados da NFe</param>
        /// <param name="ex">Excecao ocorrida</param>
        private void PreservarArquivoParaRecuperacao(int emp, DadosNFeClass dadosNFe, Exception ex)
        {
            try
            {
                SalvarArquivoEmProcessamento(emp, "TaskNFeRecepcao: Falha tecnica sem retorno fiscal conclusivo. XML mantido em EmProcessamento para recuperacao via consulta situacao. Erro=" + ex.GetAllMessages());
            }
            catch(Exception salvarEx)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao: Falha tecnica sem retorno fiscal conclusivo, mas nao foi possivel confirmar/salvar XML em EmProcessamento. Chave=" + dadosNFe.chavenfe + ", erroOriginal=" + ex.GetAllMessages() + ", erroPreservacao=" + salvarEx.GetAllMessages(), true);
            }
        }

        /// <summary>
        /// Faz leitura do protocolo quando configurado para processo Síncrono
        /// </summary>
        /// <param name="strXml">String contendo o XML</param>
        private void Protocolo(string strXml)
        {
            dadosRec.cStat =
                dadosRec.nRec = string.Empty;
            dadosRec.tMed = 0;

            try
            {
                var xml = new XmlDocument();
                xml.Load(Functions.StringXmlToStream(strXml));

                var nomeTagRetorno = xml.DocumentElement != null ? xml.DocumentElement.Name : "retEnviNFe";
                var retEnviNFeList = xml.GetElementsByTagName(nomeTagRetorno);

                foreach(XmlNode retEnviNFeNode in retEnviNFeList)
                {
                    var retEnviNFeElemento = (XmlElement)retEnviNFeNode;

                    if(retEnviNFeElemento.GetElementsByTagName(TpcnResources.cStat.ToString()).Count > 0)
                    {
                        dadosRec.cStat = retEnviNFeElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;
                    }

                    if(retEnviNFeElemento.GetElementsByTagName(TpcnResources.nRec.ToString()).Count > 0)
                    {
                        dadosRec.nRec = retEnviNFeElemento.GetElementsByTagName(TpcnResources.nRec.ToString())[0].InnerText;
                    }
                }

                if(string.IsNullOrWhiteSpace(dadosRec.cStat))
                {
                    Auxiliar.WriteLog("TaskNFeRecepcao.Protocolo: Não foi possível identificar a tag cStat no retorno do envio síncrono.", true);
                }
            }
            catch(Exception ex)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao.Protocolo: Erro ao ler retorno do protocolo. " + ex.Message, true);
                throw;
            }
        }

        /// <summary>
        /// Faz a leitura do XML do Recibo do lote enviado e disponibiliza os valores
        /// de algumas tag´s
        /// </summary>
        /// <param name="strXml">String contendo o XML</param>
        private void Recibo(string strXml, int emp)
        {
            dadosRec.cStat =
                dadosRec.nRec = string.Empty;
            dadosRec.tMed = 0;

            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(strXml));

            var retEnviNFeList = xml.GetElementsByTagName("retEnviNFe");

            foreach(XmlNode retEnviNFeNode in retEnviNFeList)
            {
                var retEnviNFeElemento = (XmlElement)retEnviNFeNode;

                dadosRec.cStat = retEnviNFeElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;

                var infRecList = xml.GetElementsByTagName("infRec");

                foreach(XmlNode infRecNode in infRecList)
                {
                    var infRecElemento = (XmlElement)infRecNode;

                    dadosRec.nRec = infRecElemento.GetElementsByTagName(TpcnResources.nRec.ToString())[0].InnerText;
                    dadosRec.tMed = Convert.ToInt32(infRecElemento.GetElementsByTagName(TpcnResources.tMed.ToString())[0].InnerText);

                    if(dadosRec.tMed > 15)
                    {
                        dadosRec.tMed = 15;
                    }

                    if(dadosRec.tMed <= 0)
                    {
                        dadosRec.tMed = Empresas.Configuracoes[emp].TempoConsulta;
                    }
                }
            }
        }

        /// <summary>
        /// Salvar o arquivo do NFe assinado na pasta EmProcessamento
        /// </summary>
        /// <param name="emp">Codigo da empresa</param>
        private void SalvarArquivoEmProcessamento(int emp) => SalvarArquivoEmProcessamento(emp, "TaskNFeRecepcao: XML assinado salvo em EmProcessamento.");

        /// <summary>
        /// Salvar o arquivo do NFe assinado na pasta EmProcessamento com log de diagnostico.
        /// </summary>
        /// <param name="emp">Codigo da empresa</param>
        /// <param name="mensagemLog">Mensagem de log para acompanhamento</param>
        private void SalvarArquivoEmProcessamento(int emp, string mensagemLog)
        {
            var msgLog = "";
            try
            {
                Empresas.Configuracoes[emp].CriarSubPastaEnviado();

                var nodeListNFe = ConteudoXML.GetElementsByTagName("NFe");

                foreach(var nodeNFe in nodeListNFe)
                {
                    var xmlElementNFe = (XmlElement)nodeNFe;
                    var chaveNFe = ((XmlElement)xmlElementNFe.GetElementsByTagName("infNFe")[0]).GetAttribute("Id");

                    var nomeArqNFe = ObterNomeArquivoNFe(emp, chaveNFe);

                    var arqEmProcessamento = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnviado, PastaEnviados.EmProcessamento.ToString(), nomeArqNFe);

                    msgLog += "\r\n arqEmProcessamento = " + arqEmProcessamento;
                    msgLog += "\r\n nomeArqNFe = " + nomeArqNFe;
                    msgLog += "\r\n chaveNFe = " + chaveNFe;
                    msgLog += "\r\n NomeArqXML = " + NomeArquivoXML;

                    var sw = File.CreateText(arqEmProcessamento);
                    sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + xmlElementNFe.OuterXml);
                    sw.Close();

                    if(File.Exists(arqEmProcessamento))
                    {
                        Auxiliar.WriteLog(mensagemLog + " Chave=" + chaveNFe + ", arquivo=" + arqEmProcessamento, false);
                        File.Delete(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnvio, "temp", nomeArqNFe));
                        File.Delete(Path.Combine(Empresas.Configuracoes[emp].PastaXmlEmLote, "temp", nomeArqNFe));
                    }
                }
            }
            catch(Exception ex)
            {
                Auxiliar.WriteLog(ex.Message + "\r\n" + msgLog, true);
                throw (ex);
            }
        }

        /// <summary>
        /// Gravar arquivo de erro na pasta retorno para o ERP e mover o arquivo do XML da nota para pasta com erro.
        /// </summary>
        /// <param name="emp">Codigo da empresa</param>
        /// <param name="exception">Exceção gerada</param>
        private void SalvarArquivoErroValidacao(int emp, ValidarXMLException exception)
        {
            PublicarNumerosLote(emp, true);
            foreach(XmlElement infNFe in ConteudoXML.GetElementsByTagName("infNFe"))
            {
                try
                {
                    var nome = ObterNomeArquivoNFe(emp, infNFe.GetAttribute("Id"));
                    var arquivo = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnvio, "temp", nome);
                    // Encerrar antes do .ERR permite ao ERP reenviar a nota corrigida assim que consumir o erro.
                    EncerrarFalhaLocal(emp, infNFe);
                    // A movimentação ocorre separadamente para não impedir o retorno se a pasta Erro falhar.
                    TFunctions.GravarArqErroServico(arquivo, Propriedade.ExtEnvio.NFe, Propriedade.ExtRetorno.Nfe_ERR, exception, ErroPadrao.ValidarXML, false);
                }
                catch(Exception ex)
                {
                    Auxiliar.WriteLog("TaskNFeRecepcao: Falha ao gravar erro de validação para o ERP. ErroOriginal=" + exception.GetAllMessages() + ", erroRetorno=" + ex.GetAllMessages(), true);
                }
            }
        }

        /// <summary>
        /// Tratar exceção
        /// </summary>
        /// <param name="ex">Objeto com a exception</param>
        /// <param name="dadosNFe">Dados da NFe/NFCe</param>
        private void TrataException(Exception ex, DadosNFeClass dadosNFe, int emp, bool envioIniciado)
        {
            PublicarNumerosLote(emp, true);
            if(envioIniciado)
            {
                PreservarArquivoParaRecuperacao(emp, dadosNFe, ex);
            }
            else
            {
                EncerrarFalhaLocal(emp);
            }

            try
            {
                if(dadosNFe.indSinc)
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.ExtEnvio.EnvLot, Propriedade.ExtRetorno.ProRec_ERR, ex, ErroPadrao.ErroNaoDetectado, false);
                }
                else
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML, Propriedade.ExtRetorno.Rec_ERR, ex, ErroPadrao.ErroNaoDetectado, false);
                }
            }
            catch(Exception retornoEx)
            {
                Auxiliar.WriteLog("TaskNFeRecepcao: Falha ao gravar erro para o ERP. ErroOriginal=" + ex.GetAllMessages() + ", erroRetorno=" + retornoEx.GetAllMessages(), true);
            }
        }

        #endregion Private Methods

        #region Internal Properties

        internal List<KeyValuePair<string, string>> ArquivosTemporariosNumeroLote { get; set; }

        internal Func<EnviNFe, Configuracao, Unimake.Business.DFe.Servicos.NFe.Autorizacao> CriarAutorizacao { get; set; } =
            (xml, configuracao) => configuracao.TipoDFe == TipoDFe.NFCe
                ? new Unimake.Business.DFe.Servicos.NFCe.Autorizacao(xml, configuracao)
                : new Unimake.Business.DFe.Servicos.NFe.Autorizacao(xml, configuracao);

        #endregion Internal Properties

        #region Internal Methods

        /// <summary>
        /// Publica na pasta de retorno os arquivos com o número do lote gerado pelo UniNFe.
        /// </summary>
        /// <param name="pastaRetorno">Pasta de retorno da empresa.</param>
        /// <param name="arquivoTemporarioXML">Arquivo XML temporário com o número do lote.</param>
        /// <param name="arquivoTemporarioTXT">Arquivo TXT temporário com o número do lote.</param>
        /// <param name="publicarTXT">Indica se o retorno TXT deve ser publicado.</param>
        /// <returns>Verdadeiro quando os arquivos pertencem ao fluxo de lote gerado pelo UniNFe.</returns>
        internal static bool PublicarArquivosNumeroLote(string pastaRetorno, string arquivoTemporarioXML, string arquivoTemporarioTXT, bool publicarTXT)
        {
            if(string.IsNullOrWhiteSpace(arquivoTemporarioXML))
            {
                return false;
            }

            var prefixoArqLote = "-num-lot";
            var arquivoRetornoXML = Path.Combine(pastaRetorno, Functions.ExtrairNomeArq(arquivoTemporarioXML, prefixoArqLote + ".xml") + "-num-lot.xml");

            PublicarArquivoNumeroLote(arquivoTemporarioXML, arquivoRetornoXML);

            if(publicarTXT)
            {
                var arquivoRetornoTXT = Path.Combine(pastaRetorno, Functions.ExtrairNomeArq(arquivoTemporarioTXT, prefixoArqLote + ".txt") + "-num-lot.txt");

                PublicarArquivoNumeroLote(arquivoTemporarioTXT, arquivoRetornoTXT);
            }

            return true;
        }

        internal static bool RetornoSincronoDeveSerFinalizado(string cStat) =>
                    cStat == "104" ||
                    cStat == "100" ||
                    cStat == "120" ||
                    cStat == "150";

        #endregion Internal Methods

        #region Public Properties

        /// <summary>
        /// Nome do arquivo temporário com o número do lote que será disponibilizado ao ERP no sucesso ou no erro (TXT).
        /// </summary>
        public string NomeArqTempTXTLote { get; set; }

        /// <summary>
        /// Nome do arquivo temporário com o número do lote que será disponibilizado ao ERP no sucesso ou no erro (XML).
        /// </summary>
        public string NomeArqTempXMLLote { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public TaskNFeRecepcao(string arquivo)
        {
            Servico = Servicos.NFeEnviarLote;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public TaskNFeRecepcao(XmlDocument conteudoXML)
        {
            Servico = Servicos.NFeEnviarLote;

            ConteudoXML = conteudoXML;
            ConteudoXML.PreserveWhitespace = false;
            NomeArquivoXML = Empresas.Configuracoes[Empresas.FindEmpresaByThread()].PastaXmlEnvio + "\\temp\\" +
                conteudoXML.GetElementsByTagName(TpcnResources.idLote.ToString())[0].InnerText + Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML;
        }

        #endregion Public Constructors

        #region Public Methods

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            var oFluxoNfe = new FluxoNfe();
            var ler = new LerXML();
            Configuracao configuracao = null;
            var envioIniciado = false;

            try
            {
                dadosRec = new DadosRecClass();

                //Ler o XML de Lote para pegar o número do lote que está sendo enviado
                ler.Nfe(ConteudoXML);

                var idLote = ler.oDadosNfe.idLote;

                var xmlNFe = new EnviNFe();
                xmlNFe = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<EnviNFe>(ConteudoXML);

                AjustarLeiauteMonofasiaHomologacao(xmlNFe);

                //remover assinatura gerada pelo ERP para que o UNINFE assine novamente.
                for(var i = 0; i < xmlNFe.NFe.Count; i++)
                {
                    xmlNFe.NFe[i].Signature = null;
                }

                configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = (ler.oDadosNfe.mod == "65" ? TipoDFe.NFCe : TipoDFe.NFe),
                    TipoEmissao = (Unimake.Business.DFe.Servicos.TipoEmissao)(Convert.ToInt32(ler.oDadosNfe.tpEmis)),
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    ColetarTelemetriaDisponibilidade = true
                };

                ConfiguracaoApp.AplicarConfiguracaoProxy(configuracao);

                EnviNFe EnviNFe = null;

                var cStat = 0;
                var xMotivo = string.Empty;

                if(ler.oDadosNfe.mod == "65")
                {
                    // Se na configuração foi informado o número 2, vai configurar para o QrCode versão 2
                    // VersaoQRCodeNFCe da DLL, por padrão, é 3
                    if(Empresas.Configuracoes[emp].VersaoQRCodeNFCe == 2)
                    {
                        configuracao.VersaoQRCodeNFCe = 2;
                    }

                    if(ConteudoXML.GetElementsByTagName("qrCode").Count == 0 && Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3)
                    {
                        if(string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].IdentificadorCSC.Trim()) || string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC))
                        {
                            throw new Exception("Para autorizar NFC-e é obrigatório informar nas configurações do UniNFe os campos CSC e IDToken do CSC.");
                        }
                    }
                    if(Empresas.Configuracoes[emp].VersaoQRCodeNFCe < 3)
                    {
                        configuracao.CSC = Empresas.Configuracoes[emp].IdentificadorCSC;
                        configuracao.CSCIDToken = Convert.ToInt32((string.IsNullOrWhiteSpace(Empresas.Configuracoes[emp].TokenCSC) ? "0" : Empresas.Configuracoes[emp].TokenCSC));
                    }

                    var autorizacao = CriarAutorizacao(xmlNFe, configuracao);
                    ConteudoXML = autorizacao.ConteudoXMLAssinado;
                    SalvarArquivoEmProcessamento(emp, "TaskNFeRecepcao: XML assinado salvo em EmProcessamento antes do envio NFCe para preservar recuperacao em caso de falha tecnica.");

                    envioIniciado = true;
                    autorizacao.Executar();

                    ConteudoXML = autorizacao.ConteudoXMLAssinado;

                    vStrXmlRetorno = autorizacao.RetornoWSString;

                    EnviNFe = autorizacao.EnviNFe;

                    cStat = autorizacao.Result.CStat;
                    xMotivo = autorizacao.Result.XMotivo;

                    autorizacao.Dispose();
                }
                else
                {
                    var autorizacao = CriarAutorizacao(xmlNFe, configuracao);
                    ConteudoXML = autorizacao.ConteudoXMLAssinado;
                    SalvarArquivoEmProcessamento(emp, "TaskNFeRecepcao: XML assinado salvo em EmProcessamento antes do envio NFe para preservar recuperacao em caso de falha tecnica.");

                    envioIniciado = true;
                    autorizacao.Executar();

                    ConteudoXML = autorizacao.ConteudoXMLAssinado;

                    vStrXmlRetorno = autorizacao.RetornoWSString;

                    EnviNFe = autorizacao.EnviNFe;

                    cStat = autorizacao.Result.CStat;
                    xMotivo = autorizacao.Result.XMotivo;

                    autorizacao.Dispose();
                }

                #region Publicar números do lote após a execução da autorização

                PublicarNumerosLote(emp, false);

                #endregion Publicar números do lote após a execução da autorização

                if(string.IsNullOrWhiteSpace(vStrXmlRetorno))
                {
                    throw new Exception("A SEFAZ, Receita ou prefeitura está com instabilidade, pois o XML retornado pelo Web-Service não pode ser reconhecido. Conteúdo retornado: " + (vStrXmlRetorno ?? "null"));
                }

                if(ler.oDadosNfe.indSinc || EnviNFe.NFe.Count <= 1)
                {
                    Protocolo(vStrXmlRetorno);

                    Auxiliar.WriteLog("TaskNFeRecepcao: Resultado leitura protocolo. cStat=" + dadosRec.cStat + ", nRec=" + dadosRec.nRec, false);
                }
                else
                {
                    Recibo(vStrXmlRetorno, emp);

                    oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML, Propriedade.ExtRetorno.Rec, vStrXmlRetorno);
                }

                #region Parte que trata o retorno do lote, ou seja, o número do recibo ou protocolo

                if(RetornoSincronoDeveSerFinalizado(dadosRec.cStat))
                {
                    FinalizarNFeSincrono(vStrXmlRetorno, emp, ler.oDadosNfe.chavenfe);

                    oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                }
                else if(dadosRec.cStat == "103") //Lote recebido com sucesso - Processo da NFe Assíncrono
                {
                    if(dadosRec.tMed > 0)
                    {
                        Thread.Sleep(dadosRec.tMed * 1000);
                    }

                    try
                    {
                        var xmlPedRec = oGerarXML.XmlPedRecNFe(dadosRec.nRec, ler.oDadosNfe.versao, ler.oDadosNfe.mod, emp);

                        var nfeRetRecepcao = new TaskNFeRetRecepcao(xmlPedRec)
                        {
                            chNFe = ler.oDadosNfe.chavenfe,
                            EnviNFe = EnviNFe
                        };

                        nfeRetRecepcao.Execute();
                    }
                    catch(ExceptionEnvioXML)
                    {
                        throw;
                    }
                    catch(ExceptionSemInternet)
                    {
                        throw;
                    }
                    catch(Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        //Atualizar o número do recibo no XML de controle do fluxo de notas enviadas
                        oFluxoNfe.AtualizarTag(ler.oDadosNfe.chavenfe, FluxoNfe.ElementoEditavel.tMed, dadosRec.tMed.ToString());
                        oFluxoNfe.AtualizarTagRec(idLote, dadosRec.nRec);
                    }
                }
                else if(Convert.ToInt32(dadosRec.cStat) > 200 ||
                    Convert.ToInt32(dadosRec.cStat) == 108 || //Verifica se o servidor de processamento está paralisado momentaneamente. Wandrey 13/04/2012
                    Convert.ToInt32(dadosRec.cStat) == 109 || //Verifica se o servidor de processamento está paralisado sem previsão. Wandrey 13/04/2012
                    Convert.ToInt32(dadosRec.cStat) == 114)   //SVC Desativado para o estado em questão.
                {
                    if(ler.oDadosNfe.indSinc)
                    {
                        // OPS!!! Processo sincrono rejeição da SEFAZ, temos que gravar o XML para o ERP, pois no processo síncrono isso não pode ser feito dentro do método Invocar
                        oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                    }

                    //Se o status do retorno do lote for maior que 200 ou for igual a 108 ou 109,
                    //vamos ter que excluir a nota do fluxo, porque ela foi rejeitada pelo SEFAZ
                    //Primeiro vamos mover o xml da nota da pasta EmProcessamento para pasta de XML´s com erro e depois a tira do fluxo
                    //Wandrey 30/04/2009
                    oAux.MoveArqErro(Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + oFluxoNfe.LerTag(ler.oDadosNfe.chavenfe, FluxoNfe.ElementoFixo.ArqNFe));
                    oFluxoNfe.ExcluirNfeFluxo(ler.oDadosNfe.chavenfe);

                    if(Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + cStat.ToString("000") + "-" + xMotivo.Trim(), "UNINFE - Notas estão sendo rejeitadas");
                    }
                }

                #endregion Parte que trata o retorno do lote, ou seja, o número do recibo ou protocolo

                //Deleta o arquivo de lote
                Functions.DeletarArquivo(NomeArquivoXML);
            }
            catch(ExceptionEnvioXML ex)
            {
                TrataException(ex, ler.oDadosNfe, emp, envioIniciado);
            }
            catch(ExceptionSemInternet ex)
            {
                TrataException(ex, ler.oDadosNfe, emp, envioIniciado);
            }
            catch(ValidatorDFeException ex)
            {
                TrataException(ex, ler.oDadosNfe, emp, false);
            }
            catch(ValidarXMLException ex)
            {
                SalvarArquivoErroValidacao(emp, ex);
            }
            catch(Exception ex)
            {
                TrataException(ex, ler.oDadosNfe, emp, envioIniciado);
            }
            finally
            {
                LimparNumerosLotePublicados(emp);

                DiagnosticoDisponibilidadeDFeHelper.Gravar(emp, configuracao, NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.EnvLot).EnvioXML);
            }
        }

        #endregion Public Methods
    }
}
