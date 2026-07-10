using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.BPe
{
    public class TaskBPeTMRecepcao : TaskBPeRecepcao
    {
        public TaskBPeTMRecepcao(string arquivo)
            : base(arquivo)
        {
            Servico = Servicos.BPeTMAutorizacao;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);
            Unimake.Business.DFe.Servicos.BPe.AutorizacaoBPeTM autorizacao = null;

            try
            {
                var xmlBPeTM = new Unimake.Business.DFe.Xml.BPeTM.BPeTM();
                xmlBPeTM = xmlBPeTM.LerXML<Unimake.Business.DFe.Xml.BPeTM.BPeTM>(ConteudoXML);

                if (xmlBPeTM.InfBPe.InfRespTec == null)
                {
                    if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecCNPJ) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecEmail) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecTelefone) ||
                        !string.IsNullOrEmpty(Empresas.Configuracoes[emp].RespTecXContato))
                    {
                        xmlBPeTM.InfBPe.InfRespTec = new Unimake.Business.DFe.Xml.BPeTM.InfRespTec
                        {
                            CNPJ = Empresas.Configuracoes[emp].RespTecCNPJ,
                            Email = Empresas.Configuracoes[emp].RespTecEmail,
                            Fone = Empresas.Configuracoes[emp].RespTecTelefone,
                            XContato = Empresas.Configuracoes[emp].RespTecXContato
                        };
                    }
                }

                autorizacao = new Unimake.Business.DFe.Servicos.BPe.AutorizacaoBPeTM(xmlBPeTM, CriarConfiguracao(emp));
                autorizacao.Executar();

                ConteudoXML = autorizacao.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "BPeTM");

                vStrXmlRetorno = autorizacao.RetornoWSString;

                if (autorizacao.Result.CStat == 100)
                {
                    FinalizarBPe(vStrXmlRetorno, emp, Propriedade.TipoEnvio.BPeTM, Propriedade.ExtRetorno.ProcBPeTM, "BPeTM");
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacao.Result.CStat.ToString("000") + "-" + autorizacao.Result.XMotivo.Trim(), "UNINFE - BPe TM´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.BPeTM).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.BPeTM).RetornoXML, vStrXmlRetorno);

                if (File.Exists(NomeArquivoXML))
                {
                    File.Delete(NomeArquivoXML);
                }
            }
            catch (Exception ex)
            {
                GravarErroEnvio(arqEmProcessamento, Propriedade.TipoEnvio.BPeTM, ex);
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
