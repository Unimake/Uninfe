using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTGravarVeiculo : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTCadastro;
        public TaskCIOTGravarVeiculo(string arquivo) : base(arquivo) { Servico = Servicos.CIOTGravarVeiculo; }
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            try
            {
                var xml = new XmlCIOT.GravarVeiculo().LerXML<XmlCIOT.GravarVeiculo>(ConteudoXML);
                using (var servico = new Unimake.Business.DFe.Servicos.CIOT.GravarVeiculo(xml, CriarConfiguracao(emp)))
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