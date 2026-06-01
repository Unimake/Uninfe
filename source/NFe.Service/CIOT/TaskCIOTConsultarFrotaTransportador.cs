using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTConsultarFrotaTransportador : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTConsultar;

        public TaskCIOTConsultarFrotaTransportador(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTConsultarFrotaTransportador;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new XmlCIOT.ConsultarFrotaTransportador().LerXML<XmlCIOT.ConsultarFrotaTransportador>(ConteudoXML);
                using (var consulta = new Unimake.Business.DFe.Servicos.CIOT.ConsultarFrotaTransportador(xml, CriarConfiguracao(emp)))
                {
                    consulta.Executar();

                    vStrXmlRetorno = consulta.RetornoWSString;
                    GravarRetorno();
                }
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
