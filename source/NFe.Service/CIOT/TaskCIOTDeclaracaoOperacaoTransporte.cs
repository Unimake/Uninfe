using NFe.Components;
using NFe.Settings;
using System;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTDeclaracaoOperacaoTransporte : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOT;

        public TaskCIOTDeclaracaoOperacaoTransporte(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTDeclaracaoOperacaoTransporte;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new XmlCIOT.DeclaracaoOperacaoTransporte().LerXML<XmlCIOT.DeclaracaoOperacaoTransporte>(ConteudoXML);
                var declaracao = new Unimake.Business.DFe.Servicos.CIOT.DeclaracaoOperacaoTransporte(xml, CriarConfiguracao(emp));
                declaracao.Executar();

                vStrXmlRetorno = declaracao.RetornoWSString;
                GravarRetorno();
                declaracao.Dispose();
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
