using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFeABI;
using RecepcaoEvento = Unimake.Business.DFe.Servicos.NFeABI.RecepcaoEvento;

namespace NFe.Service.NFeABI
{
    public class TaskNFeABIEventos : TaskAbst
    {
        internal static Action<RecepcaoEvento> ExecutarRecepcao;

        public TaskNFeABIEventos(string arquivo)
        {
            Servico = Servicos.NFeABIRecepcaoEvento;
            NomeArquivoXML = arquivo;
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var empresa = Empresas.Configuracoes[emp];
            var extensao = Propriedade.Extensao(Propriedade.TipoEnvio.PedEve);

            try
            {
                ConteudoXML.PreserveWhitespace = true;
                ConteudoXML.Load(NomeArquivoXML);
                var ns = new XmlNamespaceManager(ConteudoXML.NameTable);
                ns.AddNamespace("abi", NFeStrConstants.NAME_SPACE_NFEABI);
                var ambiente = ConteudoXML.SelectSingleNode("/abi:evento/abi:infEvento/abi:tpAmb", ns);
                if(!int.TryParse(ambiente?.InnerText, out var tpAmb) || tpAmb != empresa.AmbienteCodigo)
                {
                    throw new InvalidOperationException("O tpAmb do evento da NF-e ABI deve corresponder ao ambiente configurado para a empresa.");
                }

                var configuracao = new Configuracao
                {
                    PrepararConexaoTLSAntesDoEnvio = empresa.AtivarPreparacaoTLSAntesEnvioXML,
                    TipoDFe = TipoDFe.NFeABI,
                    CertificadoDigital = empresa.X509Certificado,
                    ColetarTelemetriaDisponibilidade = true
                };
                ConfiguracaoApp.AplicarConfiguracaoProxy(configuracao);

                using(var recepcao = new RecepcaoEvento(ConteudoXML.OuterXml, configuracao))
                {
                    if(ExecutarRecepcao == null) recepcao.Executar();
                    else ExecutarRecepcao(recepcao);

                    ConteudoXML = recepcao.ConteudoXMLAssinado;
                    vStrXmlRetorno = recepcao.RetornoWSString;
                    if(string.IsNullOrWhiteSpace(vStrXmlRetorno))
                    {
                        throw new InvalidOperationException("O serviço de recepção de eventos da NF-e ABI não retornou XML.");
                    }

                    var info = recepcao.Result.InfEvento;
                    if(info == null || info.CStat <= 0)
                    {
                        throw new InvalidOperationException("O retorno da recepção de eventos da NF-e ABI não contém um status válido.");
                    }

                    if(info.CStat == 135 || info.CStat == 136)
                    {
                        var evento = recepcao.EventoNFeABI;
                        ValidarProtocolo(evento.InfEvento, info);
                        XmlRetorno(extensao.EnvioXML, extensao.RetornoXML);
                        ArquivarEvento(emp, evento, info, recepcao.RetornoWSXML);
                        Functions.DeletarArquivo(NomeArquivoXML);
                        return;
                    }

                    XmlRetorno(extensao.EnvioXML, extensao.RetornoXML);
                    TFunctions.MoveArqErro(NomeArquivoXML);
                    if(empresa.DocumentosRejeitados)
                    {
                        new SendMessageToWhatsApp(emp).AlertNotification(
                            "Rejeição: " + info.CStat.ToString("000") + "-" + info.XMotivo?.Trim(),
                            "UNINFE - Evento da NF-e ABI rejeitado");
                    }
                }
            }
            catch(Exception ex)
            {
                try
                {
                    TFunctions.GravarArqErroServico(NomeArquivoXML, extensao.EnvioXML, Propriedade.ExtRetorno.Eve_ERR, ex);
                }
                catch(Exception erro)
                {
                    Auxiliar.WriteLog("Falha ao gravar o retorno de erro do evento da NF-e ABI. " + erro, false);
                }
            }
        }

        private static void ValidarProtocolo(InfEventoNFeABI enviado, InfRetEventoNFeABI recebido)
        {
            if(recebido.ChNFeABI != enviado.ChNFeABI || recebido.TpEvento != enviado.TpEvento ||
                recebido.NSeqEvento != enviado.NSeqEvento || recebido.TpAmb != enviado.TpAmb ||
                string.IsNullOrWhiteSpace(recebido.NProt) || recebido.DhRegEvento == default)
            {
                throw new InvalidOperationException("O protocolo retornado não corresponde ao evento da NF-e ABI enviado ou está incompleto.");
            }
        }

        private void ArquivarEvento(int emp, EventoNFeABI evento, InfRetEventoNFeABI recebido, XmlDocument xmlRetorno)
        {
            var empresa = Empresas.Configuracoes[emp];
            var nome = new ProcEventoNFeABI { EventoNFeABI = evento }.NomeArquivoDistribuicao;
            var caminhoRelativo = Path.Combine(PastaEnviados.Autorizados.ToString(),
                empresa.DiretorioSalvarComo.ToString(recebido.DhRegEvento.DateTime), nome);
            var processado = CriarProcessado(ConteudoXML.DocumentElement, xmlRetorno.DocumentElement, evento.Versao);

            var arquivo = Path.Combine(empresa.PastaXmlEnviado, caminhoRelativo);
            GravarProcessado(arquivo, processado);
            if(!string.IsNullOrEmpty(empresa.PastaBackup))
            {
                GravarProcessado(Path.Combine(empresa.PastaBackup, caminhoRelativo), processado);
            }

            oGerarXML.XmlParaFTP(emp, arquivo);
        }

        private static XmlDocument CriarProcessado(XmlElement evento, XmlElement retorno, string versao)
        {
            var xml = new XmlDocument { PreserveWhitespace = true };
            var raiz = xml.CreateElement("procEventoNFeABI", NFeStrConstants.NAME_SPACE_NFEABI);
            raiz.SetAttribute("versao", versao);
            xml.AppendChild(raiz);

            // O XSD do processado usa eventoNFeABI para o mesmo conteúdo do pedido assinado.
            var registro = xml.CreateElement(evento.Prefix, "eventoNFeABI", evento.NamespaceURI);
            foreach(XmlAttribute atributo in evento.Attributes)
            {
                registro.Attributes.Append((XmlAttribute)xml.ImportNode(atributo, true));
            }
            foreach(XmlNode filho in evento.ChildNodes)
            {
                registro.AppendChild(xml.ImportNode(filho, true));
            }

            raiz.AppendChild(registro);
            raiz.AppendChild(xml.ImportNode(retorno, true));
            return xml;
        }

        private static void GravarProcessado(string arquivo, XmlDocument processado)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(arquivo));
            if(!File.Exists(arquivo))
            {
                var temporario = arquivo + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try
                {
                    processado.Save(temporario);
                    File.Move(temporario, arquivo);
                }
                catch(IOException) when(File.Exists(arquivo))
                {
                    // Outra execução já concluiu o arquivamento do mesmo evento.
                }
                finally
                {
                    File.Delete(temporario);
                }
            }
        }
    }
}
