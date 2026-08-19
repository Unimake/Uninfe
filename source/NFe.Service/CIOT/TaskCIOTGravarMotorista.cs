using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTGravarMotorista : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTCadastro;
        public TaskCIOTGravarMotorista(string arquivo) : base(arquivo) { Servico = Servicos.CIOTGravarMotorista; }
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            try
            {
                var xml = new XmlCIOT.GravarMotorista().LerXML<XmlCIOT.GravarMotorista>(ConteudoXML);
                using (var servico = new Unimake.Business.DFe.Servicos.CIOT.GravarMotorista(xml, CriarConfiguracao(emp)))
                {
                    servico.Executar();
                    vStrXmlRetorno = servico.RetornoWSString;
                    GravarRetorno();
                }
            }
            catch (Exception ex) { GravarErro(ex); }
            finally { DeletarArquivo(); }
        }
    }
}