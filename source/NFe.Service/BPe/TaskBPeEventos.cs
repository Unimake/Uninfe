using NFe.Components;
using NFe.Settings;
using System;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.BPe;

namespace NFe.Service.BPe
{
    public class TaskBPeEventos : TaskAbst
    {
        public TaskBPeEventos(string arquivo)
        {
            Servico = Servicos.BPeRecepcaoEvento;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new EventoBPe();
                xml = xml.LerXML<EventoBPe>(ConteudoXML);

                var configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = TipoDFe.BPe,
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

                var recepcaoEvento = new Unimake.Business.DFe.Servicos.BPe.RecepcaoEvento(xml, configuracao);
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
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedEve).EnvioXML, Propriedade.ExtRetorno.Eve_ERR, ex);
                }
                catch
                {
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
                }
            }
        }

        private void LerRetornoEvento(int emp, RetEventoBPe retornoEventoBPe)
        {
            var docEventoOriginal = ConteudoXML;
            var autorizou = false;

            var versao = retornoEventoBPe.Versao;
            var infEvento = retornoEventoBPe.InfEvento;

            var cStatCons = infEvento.CStat;
            var xMotivo = string.Empty;

            if (infEvento.XMotivo != null)
            {
                xMotivo = infEvento.XMotivo;
            }

            if (cStatCons == 134 || cStatCons == 135 || cStatCons == 136)
            {
                var chBPe = infEvento.ChBPe;
                var nSeqEvento = Convert.ToInt32("0" + infEvento.NSeqEvento);
                var tpEvento = (int)infEvento.TpEvento;

                var idEventoOriginal = (docEventoOriginal.GetElementsByTagName("infEvento")[0]).Attributes.GetNamedItem(TpcnResources.Id.ToString()).Value;
                var idEventoRetorno = TpcnResources.ID.ToString() + tpEvento + chBPe + nSeqEvento.ToString((idEventoOriginal.Length <= 54 ? "00" : "000"));

                foreach (XmlNode env in docEventoOriginal.GetElementsByTagName("infEvento"))
                {
                    if (idEventoOriginal == idEventoRetorno)
                    {
                        autorizou = true;

                        oGerarXML.XmlDistEventoBPe(emp, chBPe, nSeqEvento.ToString((idEventoOriginal.Length <= 54 ? "00" : "000")), tpEvento,
                            env.ParentNode.OuterXml, retornoEventoBPe.GerarXML().OuterXml, infEvento.DhRegEvento.DateTime, true, versao);

                        break;
                    }
                }
            }
            else
            {
                if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                {
                    var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                    sendMessageToWhatsApp.AlertNotification("Rejeição: " + Convert.ToInt32(cStatCons).ToString("000") + "-" + xMotivo.Trim(), "UNINFE - Evento do BPe rejeitado");
                }
            }

            if (!autorizou)
            {
                oAux.MoveArqErro(NomeArquivoXML);
            }
        }
    }
}
