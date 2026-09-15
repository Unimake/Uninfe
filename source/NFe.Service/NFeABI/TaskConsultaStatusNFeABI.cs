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
    /// Processa o pedido de StatusServico da NF-e ABI sem aplicar o contrato PedSta legado.
    /// </summary>
    public class TaskConsultaStatusNFeABI : TaskAbst
    {
        internal static Func<string, Configuracao, string> ExecutarConsulta = ExecutarConsultaPadrao;

        public TaskConsultaStatusNFeABI(string arquivo)
        {
            Servico = Servicos.NFeABIStatusServico;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            Configuracao configuracao = null;
            var excluirEntrada = false;
            var resultadoOperacao = "Falha";
            var categoriaFalha = "Processamento";
            var cStat = string.Empty;

            try
            {
                var consulta = new ConsStatServNFeABI().LerXML<ConsStatServNFeABI>(ConteudoXML);
                ValidarPedido(emp, consulta);

                configuracao = CriarConfiguracao(emp, consulta);
                vStrXmlRetorno = (ExecutarConsulta ?? ExecutarConsultaPadrao)(ConteudoXML.OuterXml, configuracao);
                if (string.IsNullOrWhiteSpace(vStrXmlRetorno))
                {
                    throw new Exception("O serviço de status da NF-e ABI retornou uma resposta vazia.");
                }

                // O XML devolvido pela DLL é o retorno fiscal original; não desserializar nem reformatar.
                XmlRetorno(Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).RetornoXML);

                cStat = ObterCStat(vStrXmlRetorno);
                resultadoOperacao = "RetornoFiscalRecebido";
                categoriaFalha = "Nenhuma";
                excluirEntrada = true;
            }
            catch (Exception ex)
            {
                categoriaFalha = ClassificarFalha(ex);
                GravarErroSeguro(emp, categoriaFalha);
            }
            finally
            {
                GravarDiagnosticoPassivo(emp, resultadoOperacao, categoriaFalha, cStat);

                if (excluirEntrada)
                {
                    try
                    {
                        Functions.DeletarArquivo(NomeArquivoXML);
                    }
                    catch
                    {
                        // Mantém o comportamento de retry do monitor quando o arquivo estiver bloqueado.
                    }
                }
            }
        }

        private static string ExecutarConsultaPadrao(string xmlOriginal, Configuracao configuracao)
        {
            using (var statusServico = new Unimake.Business.DFe.Servicos.NFeABI.StatusServico(xmlOriginal, configuracao))
            {
                statusServico.Executar();
                return statusServico.RetornoWSString;
            }
        }

        private static Configuracao CriarConfiguracao(int emp, ConsStatServNFeABI consulta)
        {
            var empresa = Empresas.Configuracoes[emp];
            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = empresa.AtivarPreparacaoTLSAntesEnvioXML,
                TipoDFe = TipoDFe.NFeABI,
                CodigoUF = (int)consulta.CUF,
                TipoAmbiente = consulta.TpAmb,
                CertificadoDigital = empresa.X509Certificado,
                ColetarTelemetriaDisponibilidade = true
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

        private static void ValidarPedido(int emp, ConsStatServNFeABI consulta)
        {
            if (consulta == null || consulta.Versao != "1.00" || consulta.XServ != "STATUS")
            {
                throw new Exception("Pedido de status da NF-e ABI inválido.");
            }

            if ((int)consulta.TpAmb != Empresas.Configuracoes[emp].AmbienteCodigo)
            {
                throw new Exception("O ambiente do pedido de status da NF-e ABI diverge da configuração da empresa.");
            }

            if (consulta.TpAmb == TipoAmbiente.Producao)
            {
                throw new Exception("O endpoint de produção da NF-e ABI ainda não foi publicado.");
            }

            if (Empresas.Configuracoes[emp].UsaCertificado && Empresas.Configuracoes[emp].X509Certificado == null)
            {
                throw new Exception("Certificado digital não configurado para a consulta de status da NF-e ABI.");
            }

            if (Empresas.Configuracoes[emp].UsaCertificado &&
                new Unimake.Business.Security.CertificadoDigital().Vencido(Empresas.Configuracoes[emp].X509Certificado))
            {
                throw new Exception("Certificado digital vencido para a consulta de status da NF-e ABI.");
            }
        }

        private void GravarDiagnosticoPassivo(int emp, string resultadoOperacao, string categoriaFalha, string cStat)
        {
#if _BETA || DEBUG
            if (emp < 0 || emp >= Empresas.Configuracoes.Count)
            {
                return;
            }

            try
            {
                var nomeArquivo = Functions.ExtrairNomeArq(NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML) +
                    Propriedade.ExtRetorno.DiagnosticoDisponibilidadeDFe;
                var caminhoArquivo = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, nomeArquivo);

                using (var writer = XmlWriter.Create(caminhoArquivo, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("DiagnosticoDisponibilidadeDFe");
                    writer.WriteElementString("TipoDFe", TipoDFe.NFeABI.ToString());
                    writer.WriteElementString("ResultadoOperacao", resultadoOperacao);
                    writer.WriteElementString("CategoriaFalha", categoriaFalha);
                    if (!string.IsNullOrWhiteSpace(cStat))
                    {
                        writer.WriteElementString("CStat", cStat);
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch
            {
                // Diagnóstico é complementar e nunca pode alterar o retorno ERP.
            }
#endif
        }

        private static string ClassificarFalha(Exception ex)
        {
            var mensagem = ex == null ? string.Empty : ex.Message ?? string.Empty;
            if (mensagem.IndexOf("certificado", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Certificado";
            }

            if (mensagem.IndexOf("proxy", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Proxy";
            }

            if (mensagem.IndexOf("tls", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("ssl", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("canal seguro", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "TLS";
            }

            if (mensagem.IndexOf("dns", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("nome remoto", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("host", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "DNS";
            }

            if (mensagem.IndexOf("produção", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("ambiente", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("pedido", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("configuração", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Configuracao";
            }

            if (mensagem.IndexOf("vazia", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("retorno", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Retorno";
            }

            return "Transporte";
        }

        private static Exception CriarExcecaoSegura(string categoriaFalha)
        {
            try { throw new Exception("Falha de " + categoriaFalha + " na consulta de status da NF-e ABI."); }
            catch (Exception ex) { return ex; }
        }

        private void GravarErroSeguro(int emp, string categoriaFalha)
        {
            var erroSeguro = CriarExcecaoSegura(categoriaFalha);
            try
            {
                TFunctions.GravarArqErroServico(NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML,
                    Propriedade.ExtRetorno.Sta_ERR, erroSeguro);
            }
            catch
            {
                try
                {
                    var nome = Functions.ExtrairNomeArq(NomeArquivoXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.PedSta).EnvioXML) + Propriedade.ExtRetorno.Sta_ERR;
                    File.WriteAllText(Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, nome), erroSeguro.Message);
                }
                catch
                {
                    // A falha na gravação do .ERR não pode interromper o monitor.
                }
            }
        }

        private static string ObterCStat(string xmlRetorno)
        {
            try
            {
                var xml = new XmlDocument();
                xml.LoadXml(xmlRetorno);
                var elemento = xml.GetElementsByTagName("cStat");
                var valor = elemento.Count == 0 ? string.Empty : elemento[0].InnerText.Trim();
                int codigo;
                return valor.Length == 3 && int.TryParse(valor, out codigo) && codigo >= 100 && codigo <= 999
                    ? valor
                    : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
