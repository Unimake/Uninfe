using NFe.Components;
using NFe.Settings;
using System;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.MDFe;

namespace NFe.Service
{
    public class TaskMDFeConsNaoEncerrado : TaskAbst
    {
        public TaskMDFeConsNaoEncerrado(string arquivo)
        {
            Servico = Servicos.MDFeConsultaNaoEncerrado;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                var xml = new ConsMDFeNaoEnc();
                xml = Unimake.Business.DFe.Utility.XMLUtility.Deserializar<ConsMDFeNaoEnc>(ConteudoXML);

                var cUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo;
                var versao = xml.Versao;
                var tpAmb = xml.TpAmb;

                var configuracao = new Configuracao
                {
                    TipoDFe = TipoDFe.MDFe,
                    CodigoUF = cUF,
                    TipoAmbiente = tpAmb,
                    CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
                };

                if (ConfiguracaoApp.Proxy)
                {
                    configuracao.HasProxy = true;
                    configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                    configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                    configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
                }

                var consNaoEnc = new Unimake.Business.DFe.Servicos.MDFe.ConsNaoEnc(xml, configuracao);
                consNaoEnc.Executar();

                vStrXmlRetorno = consNaoEnc.RetornoWSString;
                XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.MDFeConsNaoEncerrados).EnvioXML, Propriedade.Extensao(Propriedade.TipoEnvio.MDFeConsNaoEncerrados).RetornoXML);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML,
                            Propriedade.Extensao(Propriedade.TipoEnvio.MDFeConsNaoEncerrados).EnvioXML,
                            Propriedade.ExtRetorno.MDFeConsNaoEnc_ERR, ex);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                    //Wandrey 09/03/2010
                }
            }
            finally
            {
                try
                {
                    //Deletar o arquivo de solicitação do serviço
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML de solicitação do serviço,
                    //infelizmente não posso fazer mais nada, o UniNFe vai tentar mandar
                    //o arquivo novamente para o webservice
                    //Wandrey 09/03/2010
                }
            }
        }
    }
}