using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFe;

namespace NFe.Service
{
    /// <summary>
    /// Classe para envio de XMLs de inutilização do NFe
    /// </summary>
    public class TaskNFeInutilizacao : TaskAbst
    {
        public TaskNFeInutilizacao(string arquivo)
        {
            Servico = Servicos.NFeInutilizarNumeros;
            NomeArquivoXML = arquivo;
            if (vXmlNfeDadosMsgEhXML)
            {
                ConteudoXML.PreserveWhitespace = false;
                ConteudoXML.Load(arquivo);
            }
        }

        #region Classe com os dados do XML do pedido de inutilização de números de NF

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do pedido de inutilizacao
        /// </summary>
        private DadosPedInut dadosPedInut;

        #endregion Classe com os dados do XML do pedido de inutilização de números de NF

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                dadosPedInut = new DadosPedInut(emp);
                PedInut(emp);

                if (vXmlNfeDadosMsgEhXML)  //danasa 12-9-2009
                {
                    var xml = new InutNFe();
                    xml = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<InutNFe>(ConteudoXML);

                    var configuracao = new Configuracao
                    {
                        TipoDFe = (dadosPedInut.mod == 65 ? TipoDFe.NFCe : TipoDFe.NFe),
                        TipoEmissao = (Unimake.Business.DFe.Servicos.TipoEmissao)dadosPedInut.tpEmis,
                        CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                    };

                    if (ConfiguracaoApp.Proxy)
                    {
                        configuracao.HasProxy = true;
                        configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                        configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                        configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                    }

                    if (dadosPedInut.mod == 65)
                    {
                        var inutilizacao = new Unimake.Business.DFe.Servicos.NFCe.Inutilizacao(xml, configuracao);
                        inutilizacao.Executar();

                        ConteudoXML = inutilizacao.ConteudoXMLAssinado;
                        vStrXmlRetorno = inutilizacao.RetornoWSString;
                    }
                    else
                    {
                        var inutilizacao = new Unimake.Business.DFe.Servicos.NFe.Inutilizacao(xml, configuracao);
                        inutilizacao.Executar();

                        ConteudoXML = inutilizacao.ConteudoXMLAssinado;
                        vStrXmlRetorno = inutilizacao.RetornoWSString;
                    }

                    LerRetornoInut();

                    XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).RetornoXML);
                }
                else
                {
                    var f = Path.GetFileNameWithoutExtension(NomeArquivoXML) + ".xml";

                    if (NomeArquivoXML.IndexOf(Empresas.Configuracoes[emp].PastaValidar, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        f = Path.Combine(Empresas.Configuracoes[emp].PastaValidar, f);
                    }
                    oGerarXML.Inutilizacao(f,
                        dadosPedInut.tpAmb,
                        dadosPedInut.tpEmis,
                        dadosPedInut.cUF,
                        dadosPedInut.ano,
                        dadosPedInut.CNPJ,
                        dadosPedInut.mod,
                        dadosPedInut.serie,
                        dadosPedInut.nNFIni,
                        dadosPedInut.nNFFin,
                        dadosPedInut.xJust,
                        dadosPedInut.versao);
                }
            }
            catch (Exception ex)
            {
                var ExtRet = string.Empty;

                if (vXmlNfeDadosMsgEhXML) //Se for XML
                {
                    ExtRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioXML;
                }
                else //Se for TXT
                {
                    ExtRet = Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioTXT;
                }

                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, ExtRet, Propriedade.ExtRetorno.Inu_ERR, ex);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                    //Wandrey 09/03/2010
                }
            }
            finally
            {
                try
                {
                    if (!vXmlNfeDadosMsgEhXML) //Se for o TXT para ser transformado em XML, vamos excluir o TXT depois de gerado o XML
                    {
                        Functions.DeletarArquivo(NomeArquivoXML);
                    }
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML de inutilização, infelizmente não posso
                    //fazer mais nada. Com certeza o uninfe sendo restabelecido novamente vai tentar enviar o mesmo
                    //xml de inutilização para o SEFAZ. Este erro pode ocorrer por falha no HD, rede, Permissão de pastas, etc. Wandrey 23/03/2010
                }
            }
        }

        #endregion Execute

        #region PedInut()

        /// <summary>
        /// PedInut(string cArquivoXML)
        /// </summary>
        /// <param name="emp">Código da empresa</param>
        private void PedInut(int emp)
        {
            dadosPedInut.tpAmb = Empresas.Configuracoes[emp].AmbienteCodigo;
            dadosPedInut.tpEmis = Empresas.Configuracoes[emp].tpEmis;
            dadosPedInut.versao = "";

            if (Path.GetExtension(NomeArquivoXML).ToLower() == ".txt")
            {
                //      tpAmb|2
                //      tpEmis|1                <<< opcional >>>
                //      cUF|35
                //      ano|08
                //      CNPJ|99999090910270
                //      mod|55
                //      serie|0
                //      nNFIni|1
                //      nNFFin|1
                //      xJust|Teste do WS de Inutilizacao
                //      versao|3.10
                var cLinhas = Functions.LerArquivo(NomeArquivoXML);
                Functions.PopulateClasse(dadosPedInut, cLinhas);
            }
            else
            {
                var InutNFeList = ConteudoXML.GetElementsByTagName("inutNFe");

                foreach (XmlNode InutNFeNode in InutNFeList)
                {
                    var InutNFeElemento = (XmlElement)InutNFeNode;
                    dadosPedInut.versao = InutNFeElemento.Attributes[TpcnResources.versao.ToString()].InnerText;

                    var infInutList = InutNFeElemento.GetElementsByTagName("infInut");

                    foreach (XmlNode infInutNode in infInutList)
                    {
                        var infInutElemento = (XmlElement)infInutNode;
                        Functions.PopulateClasse(dadosPedInut, infInutElemento);

                        if (infInutElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString()).Count != 0)
                        {
                            dadosPedInut.tpEmis = Convert.ToInt16(infInutElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString())[0].InnerText);
                            /// para que o validador não rejeite, excluo a tag <tpEmis>
                            ConteudoXML.DocumentElement["infInut"].RemoveChild(infInutElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString())[0]);
                            /// salvo o arquivo modificado
                            ConteudoXML.Save(NomeArquivoXML);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(dadosPedInut.versao))
            {
                throw new Exception("Inutilização: Versão deve ser informada");
            }
        }

        #endregion PedInut()

        #region LerRetornoInut()

        /// <summary>
        /// Efetua a leitura do XML de retorno do processamento da Inutilização
        /// </summary>
        /// <by>Wandrey Mundin Ferreira</by>
        /// <date>21/04/2009</date>
        private void LerRetornoInut()
        {
            var emp = Empresas.FindEmpresaByThread();

            // vStrXmlRetorno = "<retInutNFe versao=\"3.10\" xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infInut><tpAmb>1</tpAmb><verAplic>SP_NFE_PL_008i2</verAplic><cStat>102</cStat><xMotivo>Inutilização de número homologado</xMotivo><cUF>35</cUF><ano>17</ano><CNPJ>48221139000191</CNPJ><mod>55</mod><serie>1</serie><nNFIni>46066</nNFIni><nNFFin>46066</nNFFin><dhRecbto>2017-03-27T09:58:07-03:00</dhRecbto><nProt>135170189046750</nProt></infInut></retInutNFe>";

            var doc = new XmlDocument();
            doc.Load(Functions.StringXmlToStream(vStrXmlRetorno));

            var retInutNFeList = doc.GetElementsByTagName("retInutNFe");

            foreach (XmlNode retInutNFeNode in retInutNFeList)
            {
                var retInutNFeElemento = (XmlElement)retInutNFeNode;

                var infInutList = retInutNFeElemento.GetElementsByTagName("infInut");

                foreach (XmlNode infInutNode in infInutList)
                {
                    var infInutElemento = (XmlElement)infInutNode;


                    var cStat = string.Empty;
                    var xMotivo = string.Empty;

                    if (infInutElemento.GetElementsByTagName(TpcnResources.cStat.ToString()).Count > 0)
                    {
                        cStat = infInutElemento.GetElementsByTagName(TpcnResources.cStat.ToString())[0].InnerText;
                    }

                    if (infInutElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString()).Count > 0)
                    {
                        xMotivo = infInutElemento.GetElementsByTagName(TpcnResources.xMotivo.ToString())[0].InnerText;
                    }

                    if (cStat == "102") //Inutilização de Número Homologado
                    {
                        var strRetInutNFe = retInutNFeNode.OuterXml;
                        var dataInut = DateTime.Now;
                        oGerarXML.XmlDistInut(ConteudoXML, strRetInutNFe, NomeArquivoXML, dataInut);


                        //Move o arquivo de solicitação do serviço para a pasta de enviados autorizados
                        var sw = File.CreateText(NomeArquivoXML);
                        sw.Write(ConteudoXML.OuterXml);
                        sw.Close();
                        TFunctions.MoverArquivo(NomeArquivoXML, PastaEnviados.Autorizados, dataInut);

                        //Move o arquivo de Distribuição para a pasta de enviados autorizados
                        var strNomeArqProcInutNFe = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                            PastaEnviados.EmProcessamento.ToString() + "\\" +
                            Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedInu).EnvioXML) + Propriedade.ExtRetorno.ProcInutNFe;
                        TFunctions.MoverArquivo(strNomeArqProcInutNFe, PastaEnviados.Autorizados, dataInut);

                        //Evento autorizado sem vinculação do evento à respectiva NF-e
                        try
                        {
                            TFunctions.ExecutaUniDanfe(oGerarXML.NomeArqGerado, DateTime.Today, Empresas.Configuracoes[emp]);
                        }
                        catch (Exception ex)
                        {
                            Auxiliar.WriteLog("TaskInutilizacao: " + ex.Message, false);
                        }
                    }
                    else
                    {
                        //Deletar o arquivo de solicitação do serviço da pasta de envio
                        Functions.DeletarArquivo(NomeArquivoXML);

                        if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                        {
                            var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                            sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(cStat).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - Inutilização NFe/NFCe rejeitada.");
                        }
                    }
                }
            }
        }

        #endregion LerRetornoInut()
    }
}