using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTConsultarSituacaoTransportador : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTConsultar;

        public TaskCIOTConsultarSituacaoTransportador(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTConsultarSituacaoTransportador;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new XmlCIOT.ConsultarSituacaoTransportador().LerXML<XmlCIOT.ConsultarSituacaoTransportador>(ConteudoXML);
                var consulta = new Unimake.Business.DFe.Servicos.CIOT.ConsultarSituacaoTransportador(xml, CriarConfiguracao(emp));
                consulta.Executar();

                vStrXmlRetorno = consulta.RetornoWSString;
                GravarRetorno();
                consulta.Dispose();
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
