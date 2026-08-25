using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTGravarProprietario : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTCadastro;
        public TaskCIOTGravarProprietario(string arquivo) : base(arquivo) { Servico = Servicos.CIOTGravarProprietario; }
        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            try
            {
                var xml = new XmlCIOT.GravarProprietario().LerXML<XmlCIOT.GravarProprietario>(ConteudoXML);
                using (var servico = new Unimake.Business.DFe.Servicos.CIOT.GravarProprietario(xml, CriarConfiguracao(emp)))
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