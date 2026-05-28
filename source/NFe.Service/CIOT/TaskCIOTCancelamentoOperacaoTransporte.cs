using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTCancelamentoOperacaoTransporte : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTPedEve;

        public TaskCIOTCancelamentoOperacaoTransporte(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTCancelamentoOperacaoTransporte;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new XmlCIOT.CancelamentoOperacaoTransporte().LerXML<XmlCIOT.CancelamentoOperacaoTransporte>(ConteudoXML);
                var cancelamento = new Unimake.Business.DFe.Servicos.CIOT.CancelamentoOperacaoTransporte(xml, CriarConfiguracao(emp));
                cancelamento.Executar();

                vStrXmlRetorno = cancelamento.RetornoWSString;
                GravarRetorno();
                cancelamento.Dispose();
            }
            catch (Exception ex)
            {
                GravarErro(ex);
            }
            finally
            {
                DeletarArquivo();
            }
        }
    }
}
