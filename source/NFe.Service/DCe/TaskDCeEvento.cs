using NFe.Components;
using NFe.Settings;
using System;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.DCe;

namespace NFe.Service
{
    public class TaskDCeEventos : TaskAbst
    {
        public TaskDCeEventos(string arquivo)
        {
            Servico = Servicos.DCeRecepcaoEvento;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new EventoDCe();
                xml = xml.LerXML<EventoDCe>(ConteudoXML);

                var configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = TipoDFe.DCe,
                    TipoEmissao = (Unimake.Business.DFe.Servicos.TipoEmissao.Normal),
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                };

                var recepcaoEvento = new Unimake.Business.DFe.Servicos.DCe.RecepcaoEvento(xml, configuracao);
                recepcaoEvento.Executar();

                ConteudoXML = recepcaoEvento.ConteudoXMLAssinado;
                vStrXmlRetorno = recepcaoEvento.RetornoWSString;

                XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).RetornoXML);

                LerRetornoEvento(emp, recepcaoEvento.Result);

                recepcaoEvento.Dispose();
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioXML, Propriedade.ExtRetorno.Eve_ERR, ex);
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
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML de evento, infelizmente
                    //não posso fazer mais nada, o UniNFe vai tentar mandar o arquivo novamente para o webservice, pois ainda não foi excluido.
                    //Wandrey 09/03/2010
                }
            }
        }

        #endregion Execute

        #region LerRetornoEvento

        private void LerRetornoEvento(int emp, RetEventoDCe retornoEventoDCe)
        {
            var docEventoOriginal = ConteudoXML;
            var autorizou = false;

            var versao = retornoEventoDCe.Versao;
            var infEvento = retornoEventoDCe.InfEvento;

            var cStatCons = infEvento.CStat;
            var xMotivo = string.Empty;

            if (infEvento.XMotivo != null)
            {
                xMotivo = infEvento.XMotivo;
            }

            if (cStatCons == 134 || cStatCons == 135 || cStatCons == 136)
            {
                var chDCe = infEvento.ChDCe;
                var nSeqEvento = Convert.ToInt32("0" + infEvento.NSeqEvento);
                var tpEvento = (int)infEvento.TpEvento;

                var idEventoOriginal = (docEventoOriginal.GetElementsByTagName("infEvento")[0]).Attributes.GetNamedItem(TpcnResources.Id.ToString()).Value;
                var idEventoRetorno = TpcnResources.ID.ToString() + tpEvento + chDCe + nSeqEvento.ToString((idEventoOriginal.Length <= 54 ? "00" : "000"));

                foreach (XmlNode env in docEventoOriginal.GetElementsByTagName("infEvento"))
                {
                    if (idEventoOriginal == idEventoRetorno)
                    {
                        autorizou = true;

                        oGerarXML.XmlDistEventoDCe(emp, chDCe, nSeqEvento.ToString((idEventoOriginal.Length <= 54 ? "00" : "000")), Convert.ToInt32(tpEvento),
                            env.ParentNode.OuterXml, retornoEventoDCe.GerarXML().OuterXml, infEvento.DhRegEvento.DateTime, true, versao);

                        //TODO: Será necessário a implementação no UniDANFE
                        //switch (Convert.ToInt32(tpEvento))
                        //{
                        //    case 110111: //Cancelamento
                        //        try
                        //        {
                        //            TFunctions.ExecutaUniDanfe(oGerarXML.NomeArqGerado, DateTime.Today, Empresas.Configuracoes[emp]);
                        //        }     
                        //        catch (Exception ex)
                        //        {
                        //            Auxiliar.WriteLog("TaskDCeEventos: " + ex.Message, false);
                        //        }
                        //        break;
                        //}

                        break;
                    }
                }
            }
            else
            {
                if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                {
                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(cStatCons).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - Evento da DCe rejeitado");
                }
            }

            if (!autorizou)
            {
                oAux.MoveArqErro(NomeArquivoXML);
            }
        }

        #endregion LerRetornoEvento
    }
}
