using NFe.Components;
using NFe.Settings;
using System;
using System.IO;

namespace NFe.Service.BPe
{
    public class TaskBPeTARecepcao : TaskBPeRecepcao
    {
        public TaskBPeTARecepcao(string arquivo)
            : base(arquivo)
        {
            Servico = Servicos.BPeTAAutorizacao;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);
            Unimake.Business.DFe.Servicos.BPe.AutorizacaoBPeTA autorizacao = null;

            try
            {
                var xmlBPeTA = new Unimake.Business.DFe.Xml.BPeTA.BPeTA();
                xmlBPeTA = xmlBPeTA.LerXML<Unimake.Business.DFe.Xml.BPeTA.BPeTA>(ConteudoXML);

                if (xmlBPeTA.InfBPe.InfRespTec == null)
                {
                    if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecCNPJ) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecEmail) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecTelefone) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecXContato))
                    {
                        xmlBPeTA.InfBPe.InfRespTec = new Unimake.Business.DFe.Xml.BPeTM.InfRespTec
                        {
                            CNPJ = Empresas.Configuracoes[emp].RespTecCNPJ,
                            Email = Empresas.Configuracoes[emp].RespTecEmail,
                            Fone = Empresas.Configuracoes[emp].RespTecTelefone,
                            XContato = Empresas.Configuracoes[emp].RespTecXContato
                        };
                    }
                }

                autorizacao = new Unimake.Business.DFe.Servicos.BPe.AutorizacaoBPeTA(xmlBPeTA, CriarConfiguracao(emp));
                autorizacao.Executar();

                ConteudoXML = autorizacao.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "BPeTA");

                vStrXmlRetorno = autorizacao.RetornoWSString;

                if (autorizacao.Result.CStat == 100)
                {
                    FinalizarBPe(vStrXmlRetorno, emp, Propriedade.TipoEnvio.BPeTA, Propriedade.ExtRetorno.ProcBPeTA, "BPeTA");
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacao.Result.CStat.ToString("000") + "-" + autorizacao.Result.XMotivo.Trim(), "UNINFE - BPe TA´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.BPeTA).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.BPeTA).RetornoXML, vStrXmlRetorno);

                if (File.Exists(NomeArquivoXML))
                {
                    File.Delete(NomeArquivoXML);
                }
            }
            catch (Exception ex)
            {
                GravarErroEnvio(arqEmProcessamento, Propriedade.TipoEnvio.BPeTA, ex);
            }
            finally
            {
                if (autorizacao != null)
                {
                    autorizacao.Dispose();
                }
            }
        }
    }
}
