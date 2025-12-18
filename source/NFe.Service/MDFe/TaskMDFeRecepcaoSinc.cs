using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.MDFe;

namespace NFe.Service
{
    public class TaskMDFeRecepcaoSinc : TaskAbst
    {
        public TaskMDFeRecepcaoSinc(string arquivo)
        {
            Servico = Servicos.MDFeEnviarSinc;

            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);

            try
            {
                var xmlMDFe = new MDFe();
                xmlMDFe = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<MDFe>(ConteudoXML);

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.MDFe,
                    TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                };

                if (ConfiguracaoApp.Proxy)
                {
                    configuracao.HasProxy = true;
                    configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                    configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                    configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                }

                var autorizacaoSinc = new Unimake.Business.DFe.Servicos.MDFe.AutorizacaoSinc(xmlMDFe, configuracao);
                autorizacaoSinc.Executar();

                ConteudoXML = autorizacaoSinc.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento);

                vStrXmlRetorno = autorizacaoSinc.RetornoWSString;

                if (autorizacaoSinc.Result.CStat == 100)
                {
                    FinalizarMDFeSincrono(vStrXmlRetorno, emp, xmlMDFe.InfMDFe.Chave);
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacaoSinc.Result.CStat.ToString("000") + "-" + autorizacaoSinc.Result.XMotivo.Trim(), "UNINFE - MDFe´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);

                if (File.Exists(NomeArquivoXML))
                {
                    File.Delete(NomeArquivoXML);
                }

                autorizacaoSinc.Dispose();
            }
            catch (Exception ex)
            {
                try
                {
                    var arqXML = NomeArquivoXML;

                    if (File.Exists(arqEmProcessamento))
                    {
                        arqXML = arqEmProcessamento;

                        if (File.Exists(NomeArquivoXML))
                        {
                            TFunctions.MoveArqErro(NomeArquivoXML);
                        }
                    }

                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(arqXML, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML, Propriedade.ExtRetorno.ProRec_ERR, ex);
                }
                catch { }
            }
        }

        /// <summary>
        /// Salvar o arquivo do MDFe assinado na pasta EmProcessamento
        /// </summary>
        /// <param name="emp">Codigo da empresa</param>
        private void SalvarArquivoEmProcessamento(int emp, string arqEmProcessamento)
        {
            Empresas.Configuracoes[emp].CriarSubPastaEnviado();

            var sw = File.CreateText(arqEmProcessamento);
            sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + ConteudoXML.GetElementsByTagName("MDFe")[0].OuterXml);
            sw.Close();

            //if (File.Exists(arqEmProcessamento))
            //{
            //    File.Delete(NomeArquivoXML);
            //}
        }

        #endregion Execute

        #region FinalizarNFeSincrono()

        /// <summary>
        /// Finalizar a NFe no processo Síncrono
        /// </summary>
        /// <param name="xmlRetorno">Conteúdo do XML retornado da SEFAZ</param>
        /// <param name="emp">Código da empresa para buscar as configurações</param>
        private void FinalizarMDFeSincrono(string xmlRetorno, int emp, string chMDFe)
        {
            //#region Modelo de retorno para ser utilizando em testes, não desmarque ou se esqueça de marcar como comentário depois de testado. Não apague.
            //xmlRetorno = "<retMDFe xmlns=\"http://www.portalfiscal.inf.br/mdfe\" versao=\"3.00\"><tpAmb>1</tpAmb><cUF>41</cUF><verAplic>RS20210422122725</verAplic><cStat>104</cStat><xMotivo>Arquivo Processado</xMotivo><protMDFe versao=\"3.00\" xmlns=\"http://www.portalfiscal.inf.br/mdfe\"><infProt Id=\"MDFe941200015321345\"><tpAmb>1</tpAmb><verAplic>RS20200915181452</verAplic><chMDFe>41201280568835000181580010000010401406004659</chMDFe><dhRecbto>2020-12-04T08:36:36-03:00</dhRecbto><nProt>941200015321345</nProt><digVal>op++bKeqQeIEdBOQ5osoQvWnStQ=</digVal><cStat>100</cStat><xMotivo>Autorizado o uso do MDF-e</xMotivo></infProt></protMDFe></retMDFe>";
            //#endregion

            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(xmlRetorno));

            var protMDFe = xml.GetElementsByTagName("protMDFe");

            var fluxoNFe = new FluxoNfe();

            FinalizarMDFe(protMDFe, fluxoNFe, emp, ConteudoXML);
        }

        #endregion FinalizarNFeSincrono()

        private void FinalizarMDFe(XmlNodeList protNFeList, FluxoNfe fluxoNFe, int emp, XmlDocument conteudoXMLLote)
        {
            var oLerXml = new LerXML();

            foreach (XmlNode protNFeNode in protNFeList)
            {
                var protNFeElemento = (XmlElement)protNFeNode;

                var strProtNfe = protNFeElemento.OuterXml;

                var infProtList = protNFeElemento.GetElementsByTagName("infProt");

                foreach (XmlNode infProtNode in infProtList)
                {
                    var tirarFluxo = true;
                    var infProtElemento = (XmlElement)infProtNode;

                    var strChaveNFe = string.Empty;
                    var strStat = string.Empty;
                    var xMotivo = string.Empty;

                    if (infProtElemento.GetElementsByTagName(TpcnResources.chMDFe.ToString())[0] != null)
                    {
                        strChaveNFe = "MDFe" + infProtElemento.GetElementsByTagName(TpcnResources.chMDFe.ToString())[0].InnerText;
                    }

                    if (infProtElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0] != null)
                    {
                        strStat = infProtElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;
                    }

                    if (infProtElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString())[0] != null)
                    {
                        xMotivo = infProtElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString())[0].InnerText;
                    }

                    //Definir o nome do arquivo da NFe e seu caminho
                    var strNomeArqNfe = "";
                    strNomeArqNfe = new FileInfo(NomeArquivoXML).Name;

                    // se por algum motivo o XML não existir no "Fluxo", então o arquivo tem que existir
                    // na pasta "EmProcessamento" assinada.
                    if (string.IsNullOrEmpty(strNomeArqNfe))
                    {
                        if (string.IsNullOrEmpty(strChaveNFe))
                        {
                            oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                            throw new Exception("LerRetornoLoteMDFe(): Não pode obter o nome do arquivo");
                        }

                        strNomeArqNfe = strChaveNFe.Substring(4) + Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML;
                    }
                    var strArquivoNFe = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqNfe;

                    //Atualizar a Tag de status da NFe no fluxo para que se ocorrer alguma falha na exclusão eu tenha esta campo para ter uma referencia em futuras consultas
                    fluxoNFe.AtualizarTag(strChaveNFe, FluxoNfe.ElementoEditavel.cStat, strStat);

                    switch (strStat)
                    {
                        case "100": //MDFe Autorizado
                            if (File.Exists(strArquivoNFe))
                            {
                                //Juntar o protocolo com a NFE já copiando para a pasta de autorizadas
                                var strArquivoNFeProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                    PastaEnviados.EmProcessamento.ToString() + "\\" +
                                    Functions.ExtrairNomeArq(strNomeArqNfe, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML) + Propriedade.ExtRetorno.ProcMDFe;

                                //Ler o XML para pegar a data de emissão para criar a pasta dos XML´s autorizados
                                if (conteudoXMLLote == null)
                                {
                                    conteudoXMLLote = new XmlDocument();
                                    conteudoXMLLote.Load(strArquivoNFe);
                                }
                                oLerXml.Mdfe(conteudoXMLLote);

                                //Verificar se a -nfe.xml existe na pasta de autorizados
                                var NFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML);

                                //Verificar se o -procNfe.xml existe na past de autorizados
                                var procNFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML, Propriedade.ExtRetorno.ProcMDFe);

                                //Se o XML de distribuição não estiver na pasta de autorizados
                                if (!procNFeJaNaAutorizada)
                                {
                                    if (!File.Exists(strArquivoNFeProc))
                                    {
                                        oGerarXML.XmlDistMDFe(strArquivoNFe, strProtNfe, Propriedade.ExtRetorno.ProcMDFe, oLerXml.oDadosNfe.versao);
                                    }
                                }

                                if (!(procNFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML, Propriedade.ExtRetorno.ProcMDFe)))
                                {
                                    //Mover a nfePRoc da pasta de NFE em processamento para a NFe Autorizada
                                    //Para envitar falhar, tenho que mover primeiro o XML de distribuição (-procnfe.xml) para
                                    //depois mover o da nfe (-nfe.xml), pois se ocorrer algum erro, tenho como reconstruir o senário,
                                    //assim sendo não inverta as posições. Wandrey 08/10/2009
                                    TFunctions.MoverArquivo(strArquivoNFeProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                    //Atualizar a situação para que eu só mova o arquivo com final -NFe.xml para a pasta autorizado se
                                    //a procnfe já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                    //Isso vai dar uma maior segurança para não deixar sem gerar o -procnfe.xml. Wandrey 13/12/2012
                                    procNFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML, Propriedade.ExtRetorno.ProcMDFe);
                                }

                                if (!NFeJaNaAutorizada && procNFeJaNaAutorizada)
                                {
                                    //Mover a NFE da pasta de NFE em processamento para NFe Autorizada
                                    //Para envitar falhar, tenho que mover primeiro o XML de distribuição (-procnfe.xml) para
                                    //depois mover o da nfe (-nfe.xml), pois se ocorrer algum erro, tenho como reconstruir o senário.
                                    //assim sendo não inverta as posições. Wandrey 08/10/2009
                                    if (!Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao)
                                    {
                                        TFunctions.MoverArquivo(strArquivoNFe, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);
                                    }
                                    else
                                    {
                                        TFunctions.MoverArquivo(strArquivoNFe, PastaEnviados.Originais, oLerXml.oDadosNfe.dEmi);
                                    }
                                }

                                //Disparar a geração/impressao do UniDanfe. 03/02/2010 - Wandrey
                                if (procNFeJaNaAutorizada)
                                {
                                    try
                                    {
                                        var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                            PastaEnviados.Autorizados.ToString() + "\\" +
                                            Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                            Path.GetFileName(strArquivoNFeProc);

                                        TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                    }
                                    catch (Exception ex)
                                    {
                                        Auxiliar.WriteLog("TaskMDFeRetRecepcao: " + ex.Message, false);
                                    }
                                }
                                //Vou verificar se estão os dois arquivos na pasta Autorizados, se tiver eu tiro do fluxo caso contrário não. Wandrey 13/02/2012
                                NFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML);
                                procNFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML, Propriedade.ExtRetorno.ProcMDFe);
                                if (!procNFeJaNaAutorizada || !NFeJaNaAutorizada)
                                {
                                    tirarFluxo = false;
                                }
                            }
                            break;

                        default: //NFe foi rejeitada
                                 //O Status da NFe tem que ser maior que 1 ou deu algum erro na hora de ler o XML de retorno da consulta do recibo, sendo assim, vou mantar a nota no fluxo para consultar novamente.
                            if (Convert.ToInt32(strStat) >= 1)
                            {
                                //Mover o XML da NFE a pasta de XML´s com erro
                                oAux.MoveArqErro(strArquivoNFe);

                                if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                                {
                                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(strStat).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - MDFe´s estão sendo rejeitados");
                                }
                            }
                            else
                            {
                                tirarFluxo = false;
                            }

                            break;
                    }

                    //Deletar a NFE do arquivo de controle de fluxo
                    if (tirarFluxo)
                    {
                        fluxoNFe.ExcluirNfeFluxo(strChaveNFe);
                    }

                    break;
                }
            }
        }
    }
}