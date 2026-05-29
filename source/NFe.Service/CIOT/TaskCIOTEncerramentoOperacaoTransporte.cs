using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTEncerramentoOperacaoTransporte : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTPedEve;

        public TaskCIOTEncerramentoOperacaoTransporte(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTEncerramentoOperacaoTransporte;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new XmlCIOT.EncerramentoOperacaoTransporte().LerXML<XmlCIOT.EncerramentoOperacaoTransporte>(ConteudoXML);
                using (var encerramento = new Unimake.Business.DFe.Servicos.CIOT.EncerramentoOperacaoTransporte(xml, CriarConfiguracao(emp)))
                {
                    encerramento.Executar();

                    vStrXmlRetorno = encerramento.RetornoWSString;
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
