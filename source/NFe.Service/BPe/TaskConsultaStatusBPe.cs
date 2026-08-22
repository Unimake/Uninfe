using NFe.Components;
using NFe.Settings;
using System;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.BPe;

namespace NFe.Service.BPe
{
    public class TaskConsultaStatusBPe : TaskAbst
    {
        public TaskConsultaStatusBPe(string arquivo)
        {
            Servico = Servicos.BPeStatusServico;
            NomeArquivoXML = arquivo;
            if (vXmlNfeDadosMsgEhXML)
            {
                ConteudoXML.PreserveWhitespace = false;
                ConteudoXML.Load(arquivo);
            }
        }

        private DadosPedSta dadosPedSta;

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            Configuracao configuracao = null;

            try
            {
                dadosPedSta = new DadosPedSta();
                PedSta(emp, dadosPedSta);

                var xml = new ConsStatServBPe();
                xml = xml.LerXML<ConsStatServBPe>(ConteudoXML);

                configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = TipoDFe.BPe,
                    CodigoUF = dadosPedSta.cUF,
                    TipoEmissao = (Unimake.Business.DFe.Servicos.TipoEmissao)dadosPedSta.tpEmis,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    ColetarTelemetriaDisponibilidade = true
                };

                if (ConfiguracaoApp.Proxy)
                {
                    configuracao.HasProxy = true;
                    configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                    configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                    configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                }

                var statusServico = new Unimake.Business.DFe.Servicos.BPe.StatusServico(xml, configuracao);
                statusServico.Executar();

                vStrXmlRetorno = statusServico.RetornoWSString;
                XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).RetornoXML);

                statusServico.Dispose();

                DiagnosticoDisponibilidadeDFeHelper.Gravar(emp, configuracao, NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML);
            }
            catch (Exception ex)
            {
                try
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML,
                        Propriedade.ExtRetorno.Sta_ERR, ex);
                }
                catch
                {
                }

                DiagnosticoDisponibilidadeDFeHelper.Gravar(emp, configuracao, NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML);
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
    }
}
