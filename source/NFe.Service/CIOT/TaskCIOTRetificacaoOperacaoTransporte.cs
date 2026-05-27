using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTRetificacaoOperacaoTransporte : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTPedEve;

        public TaskCIOTRetificacaoOperacaoTransporte(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTRetificacaoOperacaoTransporte;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new XmlCIOT.RetificacaoOperacaoTransporte().LerXML<XmlCIOT.RetificacaoOperacaoTransporte>(ConteudoXML);
                var retificacao = new Unimake.Business.DFe.Servicos.CIOT.RetificacaoOperacaoTransporte(xml, CriarConfiguracao(emp));
                retificacao.Executar();

                vStrXmlRetorno = retificacao.RetornoWSString;
                GravarRetorno();
                retificacao.Dispose();
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
