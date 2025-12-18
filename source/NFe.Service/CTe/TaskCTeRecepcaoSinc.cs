using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.CTe;

namespace NFe.Service
{
    public class TaskCTeRecepcaoSinc : TaskAbst
    {
        public TaskCTeRecepcaoSinc(string arquivo)
        {
            Servico = Servicos.CTeEnviarSinc;

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
                var xmlCTe = new CTe();
                xmlCTe = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<CTe>(ConteudoXML);

                if (xmlCTe.InfCTe.InfRespTec == null)
                {
                    if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecCNPJ) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecEmail) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecTelefone) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecXContato))
                    {
                        xmlCTe.InfCTe.InfRespTec = new Unimake.Business.DFe.Xml.CTe.InfRespTec
                        {
                            CNPJ = Empresas.Configuracoes[emp].RespTecCNPJ,
                            Email = Empresas.Configuracoes[emp].RespTecEmail,
                            XContato = Empresas.Configuracoes[emp].RespTecXContato,
                            Fone = Empresas.Configuracoes[emp].RespTecTelefone
                        };
                    }
                }

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.CTe,
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

                var autorizacaoSinc = new Unimake.Business.DFe.Servicos.CTe.AutorizacaoSinc(xmlCTe, configuracao);
                autorizacaoSinc.Executar();

                ConteudoXML = autorizacaoSinc.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "CTe");

                vStrXmlRetorno = autorizacaoSinc.RetornoWSString;

                if (autorizacaoSinc.Result.CStat == 104 || autorizacaoSinc.Result.CStat == 100)
                {
                    FinalizarCTeSincrono(vStrXmlRetorno, emp);
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacaoSinc.Result.CStat.ToString("000") + "-" + autorizacaoSinc.Result.XMotivo, "UNINFE - CTe´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);

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

                    TFunctions.GravarArqErroServico(arqXML, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.ExtRetorno.ProRec_ERR, ex);
                }
                catch { }
            }
        }

        /// <summary>
        /// Salvar o arquivo do CTe assinado na pasta EmProcessamento
        /// </summary>
        /// <param name="arqEmProcessamento">Onde será salvo o XML assinado</param>
        /// <param name="nomeTag">Nome da tag que abre o XML</param>
        protected void SalvarArquivoEmProcessamento(int emp, string arqEmProcessamento, string nomeTag)
        {
            Empresas.Configuracoes[emp].CriarSubPastaEnviado();

            var sw = File.CreateText(arqEmProcessamento);
            sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>" + ConteudoXML.GetElementsByTagName(nomeTag)[0].OuterXml);
            sw.Close();

            //if (File.Exists(arqEmProcessamento))
            //{
            //    File.Delete(NomeArquivoXML);
            //}
        }

        #endregion Execute

        #region FinalizarCTeSincrono()

        /// <summary>
        /// Finalizar a NFe no processo Síncrono
        /// </summary>
        /// <param name="xmlRetorno">Conteúdo do XML retornado da SEFAZ</param>
        /// <param name="emp">Código da empresa para buscar as configurações</param>
        protected void FinalizarCTeSincrono(string xmlRetorno, int emp)
        {
            //#region Modelo de retorno para ser utilizando em testes, não desmarque ou se esqueça de marcar como comentário depois de testado. Não apague.
            //xmlRetorno = "<protCTe versao=\"3.00\" xmlns=\"http://www.portalfiscal.inf.br/cte\"><infProt><tpAmb>1</tpAmb><verAplic>PR-v3_1_25</verAplic><chCTe>41201280568835000181570010000004841004185096</chCTe><dhRecbto>2020-12-04T08:00:44-03:00</dhRecbto><nProt>141200136850558</nProt><digVal>VFHL16ctT5MqBqQld/8D9CwBbIA=</digVal><cStat>100</cStat><xMotivo>Autorizado o uso do CT-e</xMotivo></infProt></protCTe>";
            //#endregion

            var xml = new XmlDocument();
            xml.Load(Functions.StringXmlToStream(xmlRetorno));

            var protCTe = xml.GetElementsByTagName("protCTe");
            var fluxoNFe = new FluxoNfe();

            FinalizarCTe(protCTe, fluxoNFe, emp, ConteudoXML);
        }

        #endregion FinalizarCTeSincrono()


        #region FinalizarCTe

        private void FinalizarCTe(XmlNodeList protCTeList, FluxoNfe fluxoNFe, int emp, XmlDocument conteudoXMLLote)
        {
            var oLerXml = new LerXML();

            foreach (XmlNode protNFeNode in protCTeList)
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

                    if (infProtElemento.GetElementsByTagName(TpcnResources.chCTe.ToString())[0] != null)
                    {
                        strChaveNFe = "CTe" + infProtElemento.GetElementsByTagName(TpcnResources.chCTe.ToString())[0].InnerText;
                    }

                    if (infProtElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0] != null)
                    {
                        strStat = infProtElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;
                    }

                    if (infProtElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString()).Count > 0)
                    {
                        xMotivo = infProtElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString())[0].InnerText;
                    }

                    //Definir o nome do arquivo da NFe e seu caminho
                    var strNomeArqNfe = new FileInfo(NomeArquivoXML).Name;

                    // se por algum motivo o XML não existir no "Fluxo", então o arquivo tem que existir
                    // na pasta "EmProcessamento" assinada.
                    if (string.IsNullOrEmpty(strNomeArqNfe))
                    {
                        if (string.IsNullOrEmpty(strChaveNFe))
                        {
                            oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);
                            throw new Exception("LerRetornoLoteCTe(): Não pode obter o nome do arquivo");
                        }

                        strNomeArqNfe = strChaveNFe.Substring(3) + Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML;
                    }
                    var strArquivoNFe = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + strNomeArqNfe;

                    //Atualizar a Tag de status da NFe no fluxo para que se ocorrer alguma falha na exclusão eu tenha esta campo para ter uma referencia em futuras consultas
                    fluxoNFe.AtualizarTag(strChaveNFe, FluxoNfe.ElementoEditavel.cStat, strStat);

                    switch (strStat)
                    {
                        case "100": //NFe Autorizada
                            if (File.Exists(strArquivoNFe))
                            {
                                //Juntar o protocolo com a CTe já copiando para a pasta de autorizadas
                                var strArquivoNFeProc = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                    PastaEnviados.EmProcessamento.ToString() + "\\" +
                                    Functions.ExtrairNomeArq(strNomeArqNfe, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML) + Propriedade.ExtRetorno.ProcCTe;

                                //Ler o XML para pegar a data de emissão para criar a pasta dos XML´s autorizados
                                if (conteudoXMLLote == null)
                                {
                                    conteudoXMLLote = new XmlDocument();
                                    conteudoXMLLote.Load(strArquivoNFe);
                                }
                                oLerXml.Cte(conteudoXMLLote);

                                //Verificar se a -nfe.xml existe na pasta de autorizados
                                var NFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML);


                                //Verificar se o -procNfe.xml existe na past de autorizados
                                var procNFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.ExtRetorno.ProcCTe);

                                //Se o XML de distribuição não estiver na pasta de autorizados
                                if (!procNFeJaNaAutorizada)
                                {
                                    if (!File.Exists(strArquivoNFeProc))
                                    {
                                        oGerarXML.XmlDistCTe(strArquivoNFe, strProtNfe, oLerXml.oDadosNfe.versao);
                                    }
                                }

                                if (!(procNFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.ExtRetorno.ProcCTe)))
                                {
                                    //Mover a nfePRoc da pasta de NFE em processamento para a NFe Autorizada
                                    //Para envitar falhar, tenho que mover primeiro o XML de distribuição (-procnfe.xml) para
                                    //depois mover o da nfe (-nfe.xml), pois se ocorrer algum erro, tenho como reconstruir o senário,
                                    //assim sendo não inverta as posições. Wandrey 08/10/2009
                                    TFunctions.MoverArquivo(strArquivoNFeProc, PastaEnviados.Autorizados, oLerXml.oDadosNfe.dEmi);

                                    //Atualizar a situação para que eu só mova o arquivo com final -NFe.xml para a pasta autorizado se
                                    //a procnfe já estiver lá, ou vai ficar na pasta emProcessamento para tentar gerar novamente.
                                    //Isso vai dar uma maior segurança para não deixar sem gerar o -procnfe.xml. Wandrey 13/12/2012
                                    procNFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.ExtRetorno.ProcCTe);
                                }

                                if (!NFeJaNaAutorizada && procNFeJaNaAutorizada)
                                {

                                    //Mover a CTe da pasta de CTe em processamento para CTe Autorizada
                                    //Para envitar falhar, tenho que mover primeiro o XML de distribuição (-procCTe.xml) para
                                    //depois mover o da nfe (-cte.xml), pois se ocorrer algum erro, tenho como reconstruir o cenário.
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
                                        Auxiliar.WriteLog("TaskCTeRetRecepcao: " + ex.Message, false);
                                    }
                                }

                                //Vou verificar se estão os dois arquivos na pasta Autorizados, se tiver eu tiro do fluxo caso contrário não. Wandrey 13/02/2012
                                NFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML);
                                procNFeJaNaAutorizada = oAux.EstaAutorizada(strArquivoNFe, oLerXml.oDadosNfe.dEmi, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.ExtRetorno.ProcCTe);
                                if (!procNFeJaNaAutorizada || !NFeJaNaAutorizada)
                                {
                                    tirarFluxo = false;
                                }
                            }
                            break;

                        //case "302": //CTe Denegada - Irregularidade fiscal do remetente
                        //case "303": //CTe Denegada - Irregularidade fiscal do destinatário
                        //case "304": //CTe Denegada - Irregularidade fiscal do expedidor
                        //case "305": //CTe Denegada - Irregularidade fiscal do recebedor
                        //case "306": //CTe Denegada - Irregularidade fiscal do tomador
                        //case "301": //CTe Denegada - Irregularidade fiscal do emitente
                        case "110": //CTe Denegada - Não sei quando ocorre este, mas descobrir ele no manual então estou incluindo. Wandrey 20/10/2009
                            if (File.Exists(strArquivoNFe))
                            {
                                //Ler o XML para pegar a data de emissão para criar a pasta dos XML´s Denegados
                                var conteudoXMLCTe = new XmlDocument();
                                conteudoXMLCTe.Load(strArquivoNFe);
                                oLerXml.Cte(conteudoXMLCTe);

                                //Mover a NFE da pasta de NFE em processamento para NFe Denegadas
                                TFunctions.MoverArquivo(strArquivoNFe, PastaEnviados.Denegados, oLerXml.oDadosNfe.dEmi);
                                ///
                                /// existe DACTE de CTe denegado???
                                ///
                                try
                                {
                                    var strArquivoDist = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                                        PastaEnviados.Denegados.ToString() + "\\" +
                                        Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi) +
                                        Functions.ExtrairNomeArq(strArquivoNFe, Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML) + Propriedade.ExtRetorno.Den;

                                    TFunctions.ExecutaUniDanfe(strArquivoDist, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
                                }
                                catch (Exception ex)
                                {
                                    Auxiliar.WriteLog("TaskCTeRetRecepcao: " + ex.Message, false);
                                }
                            }

                            if (Empresas.Configuracoes[emp].DocumentosDenegados)
                            {
                                var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                                sendMessageToWhatsApp.AlertNotification("Denegação: " + Convert.ToInt32(strStat).ToString("000") + "-" + xMotivo, "UNINFE - CTe´s estão sendo denegados");
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
                                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(strStat).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - CTe´s estão sendo rejeitados");
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

        #endregion
    }
}