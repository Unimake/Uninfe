using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFeABI;

namespace NFe.Service.NFeABI
{
    /// <summary>
    /// Consulta o protocolo de uma NF-e ABI pela chave de acesso.
    /// </summary>
    public class TaskConsultaSituacaoNFeABI : TaskAbst
    {
        internal static Func<string, Configuracao, string> ExecutarConsulta = ExecutarConsultaPadrao;

        public TaskConsultaSituacaoNFeABI(string arquivo)
        {
            Servico = Servicos.NFeABIConsultaProtocolo;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var excluirEntrada = false;

            try
            {
                if(ConteudoXML.DocumentElement == null ||
                    ConteudoXML.DocumentElement.LocalName != "consSitNFeABI" ||
                    ConteudoXML.DocumentElement.NamespaceURI != NFeStrConstants.NAME_SPACE_NFEABI)
                {
                    throw new Exception("Pedido de consulta de protocolo da NF-e ABI inválido.");
                }

                var consulta = new ConsSitNFeABI().LerXML<ConsSitNFeABI>(ConteudoXML);
                ValidarPedido(emp, consulta);

                var configuracao = CriarConfiguracao(emp, consulta);
                vStrXmlRetorno = (ExecutarConsulta ?? ExecutarConsultaPadrao)(ConteudoXML.OuterXml, configuracao);
                if(string.IsNullOrWhiteSpace(vStrXmlRetorno))
                {
                    throw new Exception("O serviço de consulta de protocolo da NF-e ABI retornou uma resposta vazia.");
                }

                // O retorno fiscal da DLL é gravado sem desserialização ou reformatação.
                XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).RetornoXML);
                excluirEntrada = true;
            }
            catch(Exception ex)
            {
                GravarErroSeguro(emp, ClassificarFalha(ex));
            }
            finally
            {
                if(excluirEntrada)
                {
                    try { Functions.DeletarArquivo(NomeArquivoXML); }
                    catch { /* O monitor poderá repetir o processamento se a exclusão falhar. */ }
                }
            }
        }

        private static string ExecutarConsultaPadrao(string xmlOriginal, Configuracao configuracao)
        {
            using(var consulta = new Unimake.Business.DFe.Servicos.NFeABI.ConsultaProtocolo(xmlOriginal, configuracao))
            {
                consulta.Executar();
                return consulta.RetornoWSString;
            }
        }

        private static Configuracao CriarConfiguracao(int emp, ConsSitNFeABI consulta)
        {
            var empresa = Empresas.Configuracoes[emp];
            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = empresa.AtivarPreparacaoTLSAntesEnvioXML,
                TipoDFe = TipoDFe.NFeABI,
                CodigoUF = int.Parse(consulta.ChNFeABI.Substring(0, 2)),
                TipoAmbiente = consulta.TpAmb,
                CertificadoDigital = empresa.X509Certificado,
                ColetarTelemetriaDisponibilidade = true
            };

            ConfiguracaoApp.AplicarConfiguracaoProxy(configuracao);
            return configuracao;
        }

        private static void ValidarPedido(int emp, ConsSitNFeABI consulta)
        {
            if(consulta == null || consulta.Versao != "1.00" || consulta.XServ != "CONSULTAR" ||
                string.IsNullOrEmpty(consulta.ChNFeABI) || consulta.ChNFeABI.Length != 44 ||
                consulta.ChNFeABI.Substring(20, 2) != "77")
            {
                throw new Exception("Pedido de consulta de protocolo da NF-e ABI inválido.");
            }

            foreach(var digito in consulta.ChNFeABI)
            {
                if(digito < '0' || digito > '9')
                {
                    throw new Exception("Chave de acesso da NF-e ABI inválida.");
                }
            }

            if((int)consulta.TpAmb != Empresas.Configuracoes[emp].AmbienteCodigo)
            {
                throw new Exception("O ambiente do pedido de consulta da NF-e ABI diverge da configuração da empresa.");
            }

            if(consulta.TpAmb == TipoAmbiente.Producao)
            {
                throw new Exception("O endpoint de produção da NF-e ABI ainda não foi publicado.");
            }

            if(Empresas.Configuracoes[emp].UsaCertificado && Empresas.Configuracoes[emp].X509Certificado == null)
            {
                throw new Exception("Certificado digital não configurado para a consulta de protocolo da NF-e ABI.");
            }

            if(Empresas.Configuracoes[emp].UsaCertificado &&
                new Unimake.Business.Security.CertificadoDigital().Vencido(Empresas.Configuracoes[emp].X509Certificado))
            {
                throw new Exception("Certificado digital vencido para a consulta de protocolo da NF-e ABI.");
            }
        }

        private static string ClassificarFalha(Exception ex)
        {
            var mensagem = ex == null ? string.Empty : ex.Message ?? string.Empty;
            if(mensagem.IndexOf("certificado", StringComparison.OrdinalIgnoreCase) >= 0) return "Certificado";
            if(mensagem.IndexOf("proxy", StringComparison.OrdinalIgnoreCase) >= 0) return "Proxy";
            if(mensagem.IndexOf("tls", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("ssl", StringComparison.OrdinalIgnoreCase) >= 0) return "TLS";
            if(mensagem.IndexOf("dns", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("host", StringComparison.OrdinalIgnoreCase) >= 0) return "DNS";
            if(mensagem.IndexOf("pedido", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("ambiente", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("produção", StringComparison.OrdinalIgnoreCase) >= 0) return "Configuracao";
            if(mensagem.IndexOf("retorno", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("vazia", StringComparison.OrdinalIgnoreCase) >= 0) return "Retorno";
            return "Transporte";
        }

        private void GravarErroSeguro(int emp, string categoria)
        {
            Exception erroSeguro;
            try { throw new Exception("Falha de " + categoria + " na consulta de protocolo da NF-e ABI."); }
            catch(Exception ex) { erroSeguro = ex; }

            try
            {
                TFunctions.GravarArqErroServico(NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML,
                    Propriedade.ExtRetorno.Sit_ERR, erroSeguro,
                    ErroPadrao.ErroNaoDetectado, false);
            }
            catch(Exception ex)
            {
                try
                {
                    var nome = Functions.ExtrairNomeArq(NomeArquivoXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSit).EnvioXML) + Propriedade.ExtRetorno.Sit_ERR;
                    var caminho = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, nome);
                    if(!File.Exists(caminho))
                    {
                        File.WriteAllText(caminho, TFunctions.MontaStringErro(
                            erroSeguro, ErroPadrao.ErroNaoDetectado, "indisponível", "indisponível"),
                            new UTF8Encoding(false));
                    }
                }
                catch(Exception falhaFallback)
                {
                    try { Auxiliar.WriteLog("Falha ao gravar o retorno alternativo da consulta de protocolo da NF-e ABI. " + falhaFallback, false); }
                    catch { }
                }

                try { Auxiliar.WriteLog("Falha ao gravar o retorno de erro da consulta de protocolo da NF-e ABI. " + ex, false); }
                catch { }
            }
        }
    }
}
