using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTConsultarExcecao : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTConsultar;

        public TaskCIOTConsultarExcecao(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTConsultarExcecao;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new XmlCIOT.ConsultarExcecao().LerXML<XmlCIOT.ConsultarExcecao>(ConteudoXML);
                var consulta = new Unimake.Business.DFe.Servicos.CIOT.ConsultarExcecao(xml, CriarConfiguracao(emp));
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
