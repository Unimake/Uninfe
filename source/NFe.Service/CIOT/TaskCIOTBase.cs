using NFe.Components;
using NFe.Settings;
using System;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.CIOT
{
    public abstract class TaskCIOTBase : TaskAbst
    {
        protected TaskCIOTBase(string arquivo)
        {
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        protected abstract Propriedade.TipoEnvio TipoEnvioXML { get; }

        protected Configuracao CriarConfiguracao(int emp)
        {
            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                TipoDFe = TipoDFe.CIOT,
                TipoEmissao = Unimake.Business.DFe.Servicos.TipoEmissao.Normal,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoUF = (int)UFBrasil.AN,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado
            };

            if (ConfiguracaoApp.Proxy)
            {
                configuracao.HasProxy = true;
                configuracao.ProxyAutoDetect = ConfiguracaoApp.DetectarConfiguracaoProxyAuto;
                configuracao.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                configuracao.ProxyPassword = ConfiguracaoApp.ProxySenha;
            }

            return configuracao;
        }

        protected void GravarRetorno()
        {
            oGerarXML.XmlRetorno(Propriedade.Extensao(TipoEnvioXML).EnvioXML, Propriedade.Extensao(TipoEnvioXML).RetornoXML, vStrXmlRetorno);
        }

        protected void GravarErro(Exception ex)
        {
            try
            {
                TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(TipoEnvioXML).EnvioXML, Propriedade.Extensao(TipoEnvioXML).RetornoERR, ex);
            }
            catch { }
        }

        protected void DeletarArquivo()
        {
            try
            {
                Functions.DeletarArquivo(NomeArquivoXML);
            }
            catch { }
        }
    }
}
