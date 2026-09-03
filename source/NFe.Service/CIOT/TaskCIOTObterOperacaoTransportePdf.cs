using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using XmlCIOT = Unimake.Business.DFe.Xml.CIOT;

namespace NFe.Service.CIOT
{
    public class TaskCIOTObterOperacaoTransportePdf : TaskCIOTBase
    {
        protected override Propriedade.TipoEnvio TipoEnvioXML => Propriedade.TipoEnvio.CIOTPdf;

        public TaskCIOTObterOperacaoTransportePdf(string arquivo) : base(arquivo)
        {
            Servico = Servicos.CIOTObterOperacaoTransportePdf;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            try
            {
                var xml = new XmlCIOT.ObterOperacaoTransportePdf().LerXML<XmlCIOT.ObterOperacaoTransportePdf>(ConteudoXML);
                using (var servico = new Unimake.Business.DFe.Servicos.CIOT.ObterOperacaoTransportePdf(xml, CriarConfiguracao(emp)))
                {
                    servico.Executar();
                    vStrXmlRetorno = servico.RetornoWSString;

                    if (servico.Result != null && servico.Result.Sucesso)
                    {
                        var nomePDF = Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(TipoEnvioXML).EnvioXML) + "-ret-pdfciot.pdf";
                        servico.GravarPDF(Empresas.Configuracoes[emp].PastaXmlRetorno, nomePDF);
                    }

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
