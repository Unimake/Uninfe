using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTConsultarCIOTGerado : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTConsultar;

        public TaskCIOTConsultarCIOTGerado(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTConsultarCIOTGerado;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new XmlCIOT.ConsultarCIOTGerado().LerXML<XmlCIOT.ConsultarCIOTGerado>(ConteudoXML);
                using (var consulta = new Unimake.Business.DFe.Servicos.CIOT.ConsultarCIOTGerado(xml, CriarConfiguracao(emp)))
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
