using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.CTeSimp;

namespace NFe.Service
{
    public class TaskCTeRecepcaoSimp : TaskCTeRecepcaoSinc
    {
        public TaskCTeRecepcaoSimp(string arquivo) : base(arquivo) => Servico = Servicos.CTeEnviarSimp;

        #region Execute

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            var arqEmProcessamento = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" + PastaEnviados.EmProcessamento.ToString() + "\\" + (new FileInfo(NomeArquivoXML).Name);

            try
            {
                var xmlCTe = new CTeSimp();
                xmlCTe = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<CTeSimp>(ConteudoXML);

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

                var autorizacaoSimp = new Unimake.Business.DFe.Servicos.CTe.AutorizacaoSimp(xmlCTe, configuracao);
                autorizacaoSimp.Executar();

                ConteudoXML = autorizacaoSimp.ConteudoXMLAssinado;

                SalvarArquivoEmProcessamento(emp, arqEmProcessamento, "CTeSimp");

                vStrXmlRetorno = autorizacaoSimp.RetornoWSString;

                #region Código utilizado para testes -> Não apague

                //
                //Não apage o código abaixo, comentado. Utilizo ele para simular autorizações do CTe para facilitar testes.
                //
                //vStrXmlRetorno = "<retCTeSimp xmlns=\"http://www.portalfiscal.inf.br/cte\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>17</cUF><verAplic>RS20240829125422</verAplic><cStat>100</cStat><xMotivo>Autorizado uso do CTe</xMotivo><protCTe versao=\"3.00\" xmlns=\"http://www.portalfiscal.inf.br/cte\"><infProt><tpAmb>1</tpAmb><verAplic>PR-v3_1_25</verAplic><chCTe>41201280568835000181570010000004841004185096</chCTe><dhRecbto>2020-12-04T08:00:44-03:00</dhRecbto><nProt>141200136850558</nProt><digVal>VFHL16ctT5MqBqQld/8D9CwBbIA=</digVal><cStat>100</cStat><xMotivo>Autorizado o uso do CT-e</xMotivo></infProt></protCTe></retCTeSimp>";
                //
                //if (autorizacaoSimp.Result.CStat == 213 || autorizacaoSimp.Result.CStat == 100)
                //{
                //    FinalizarCTeSincrono(vStrXmlRetorno, emp);
                //}

                #endregion

                if (autorizacaoSimp.Result.CStat == 104 || autorizacaoSimp.Result.CStat == 100)
                {
                    FinalizarCTeSincrono(vStrXmlRetorno, emp);
                }
                else
                {
                    oAux.MoveArqErro(arqEmProcessamento);

                    if (Empresas.Configuracoes[emp].DocumentosRejeitados)
                    {
                        var sendMessageToWhatsApp = new SendMessageToWhatsApp(emp);
                        sendMessageToWhatsApp.AlertNotification("Rejeição: " + autorizacaoSimp.Result.CStat.ToString("000") + "-" + autorizacaoSimp.Result.XMotivo, "UNINFE - CTe´s estão sendo rejeitados");
                    }
                }

                oGerarXML.XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedRec).RetornoXML, vStrXmlRetorno);

                if (File.Exists(NomeArquivoXML))
                {
                    File.Delete(NomeArquivoXML);
                }
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

        #endregion
    }
}