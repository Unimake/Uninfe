using NFe.Components;
using NFe.Settings;
using System;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFeABI;
using StatusServico = Unimake.Business.DFe.Servicos.NFeABI.StatusServico;

namespace NFe.Service.NFeABI
{
    public class TaskConsultaStatusNFeABI : TaskAbst
    {
        // Permite substituir somente a execução remota nos testes.
        internal static Action<StatusServico> ExecutarConsulta;

        public TaskConsultaStatusNFeABI(string arquivo)
        {
            Servico = Servicos.NFeABIStatusServico;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            Configuracao configuracao = null;

            try
            {
                var xml = new ConsStatServNFeABI().LerXML<ConsStatServNFeABI>(ConteudoXML);
                configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = TipoDFe.NFeABI,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                    ColetarTelemetriaDisponibilidade = true
                };
                ConfiguracaoApp.AplicarConfiguracaoProxy(configuracao);

                using(var consulta = new StatusServico(xml, configuracao))
                {
                    if(ExecutarConsulta == null) consulta.Executar();
                    else ExecutarConsulta(consulta);

                    vStrXmlRetorno = consulta.RetornoWSString;
                    XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).RetornoXML);
                }
            }
            catch(Exception ex)
            {
                try
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML,
                        Propriedade.ExtRetorno.Sta_ERR, ex);
                }
                catch(Exception erro)
                {
                    Auxiliar.WriteLog("Falha ao gravar o retorno de erro do status da NF-e ABI. " + erro, false);
                }
            }
            finally
            {
                DiagnosticoDisponibilidadeDFeHelper.Gravar(emp, configuracao, NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML);
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch(Exception ex)
                {
                    Auxiliar.WriteLog("Falha ao excluir o pedido de status da NF-e ABI. " + ex, false);
                }
            }
        }
    }
}
