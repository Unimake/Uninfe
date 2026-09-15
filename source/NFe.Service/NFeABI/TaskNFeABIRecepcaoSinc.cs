using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.Service.NFeABI
{
    /// <summary>
    /// Autoriza NF-e ABI síncrona preservando evidências para retomada local.
    /// </summary>
    public class TaskNFeABIRecepcaoSinc : TaskAbst
    {
        internal interface IAutorizacaoNFeABI : IDisposable
        {
            XmlDocument Assinado { get; }
            string Executar();
        }

        internal static Func<XmlDocument, Configuracao, IAutorizacaoNFeABI> CriarAutorizacao = CriarAutorizacaoPadrao;
        internal static Action<string, PastaEnviados, DateTime> MoverArquivo = MoverArquivoPadrao;
        internal static Action<string> PersistirRetorno;
        internal static Action<string> ExcluirArquivo = File.Delete;

        public TaskNFeABIRecepcaoSinc(string arquivo)
        {
            Servico = Servicos.NFeABIAutorizacaoSinc;
            NomeArquivoXML = arquivo;
            ConteudoXML.PreserveWhitespace = false;
            ConteudoXML.Load(arquivo);
        }

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();
            var emProcessamento = Path.Combine(
                Empresas.Configuracoes[emp].PastaXmlEnviado,
                PastaEnviados.EmProcessamento.ToString(),
                Path.GetFileName(NomeArquivoXML));
            Configuracao configuracao = null;
            var preparado = false;
            var transporteIniciado = false;
            var respostaRecebida = false;
            var respostaPersistida = false;
            var categoriaFalha = "Transporte";
            var resultadoOperacao = "Falha";
            try
            {
                ValidarPedido(emp, ConteudoXML);
                if (AdicionarInfRespTec(emp, ConteudoXML))
                {
                    RemoverAssinaturaAnterior(ConteudoXML);
                }

                configuracao = CriarConfiguracao(emp, ConteudoXML);
                var nomeRetorno = Path.Combine(
                    Empresas.Configuracoes[emp].PastaXmlRetorno,
                    Functions.ExtrairNomeArq(
                        NomeArquivoXML,
                        Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML) +
                    Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).RetornoXML);
                var retornoRecuperavel = Path.Combine(
                    Path.GetDirectoryName(emProcessamento),
                    Functions.ExtrairNomeArq(
                        Path.GetFileName(emProcessamento),
                        Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML) +
                    "-ret-nfeabi.xml");

                if (File.Exists(retornoRecuperavel) &&
                    !File.Exists(emProcessamento) &&
                    TentarConcluirCleanupFinal(
                        emp,
                        emProcessamento,
                        retornoRecuperavel,
                        nomeRetorno))
                {
                    resultadoOperacao = "RetornoFiscalRecebido";
                    categoriaFalha = "Nenhuma";
                    return;
                }

                string retorno;
                if (File.Exists(nomeRetorno))
                {
                    if (!File.Exists(emProcessamento))
                    {
                        throw new Exception("Retorno remoto sem XML assinado para retomada local.");
                    }

                    ConteudoXML.Load(emProcessamento);
                    preparado = true;
                    retorno = File.ReadAllText(nomeRetorno);
                    respostaRecebida = true;
                    respostaPersistida = true;
                }
                else if (File.Exists(retornoRecuperavel))
                {
                    if (!File.Exists(emProcessamento))
                    {
                        throw new Exception("Retorno recuperável sem XML assinado para retomada local.");
                    }

                    ConteudoXML.Load(emProcessamento);
                    preparado = true;
                    retorno = File.ReadAllText(retornoRecuperavel);
                    respostaRecebida = true;
                    PersistirRetornoBruto(retorno);
                    respostaPersistida = true;
                }
                else
                {
                    if (File.Exists(emProcessamento))
                    {
                        throw new Exception("Estado remoto da NF-e ABI incerto; aguarde o retorno antes de retransmitir.");
                    }

                    using (var autorizacao = (CriarAutorizacao ?? CriarAutorizacaoPadrao)(ConteudoXML, configuracao))
                    {
                        if (autorizacao == null || autorizacao.Assinado == null)
                        {
                            throw new Exception("Não foi possível preparar o XML assinado da NF-e ABI.");
                        }

                        ConteudoXML = autorizacao.Assinado;
                        SalvarEmProcessamento(emp, emProcessamento);
                        preparado = true;
                        transporteIniciado = true;
                        retorno = autorizacao.Executar();
                    }
                    if (string.IsNullOrWhiteSpace(retorno))
                    {
                        throw new Exception("O serviço de autorização da NF-e ABI não retornou evidências completas.");
                    }

                    respostaRecebida = true;
                    GravarSemSobrescrever(retornoRecuperavel, retorno);
                    PersistirRetornoBruto(retorno);
                    respostaPersistida = true;
                }

                var cStat = ValidarRetornoFiscal(retorno, ConteudoXML);
                if (cStat != "100")
                {
                    TFunctions.MoveArqErro(emProcessamento);
                    if (File.Exists(emProcessamento))
                    {
                        throw new Exception("Não foi possível arquivar a rejeição da NF-e ABI.");
                    }

                    Excluir(NomeArquivoXML);
                    Excluir(retornoRecuperavel);
                    resultadoOperacao = "RetornoFiscalRecebido";
                    categoriaFalha = "Nenhuma";
                    return;
                }

                var protocolo = ObterProtocoloCoerente(retorno, ConteudoXML);
                var proc = CriarProc(ConteudoXML, protocolo);
                var arquivoProc = Path.Combine(
                    Path.GetDirectoryName(emProcessamento),
                    Functions.ExtrairNomeArq(
                        Path.GetFileName(emProcessamento),
                        Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML) +
                    Propriedade.ExtRetorno.ProcNFeABI);
                GravarSemSobrescrever(arquivoProc, proc);
                MoverSeguro(arquivoProc, PastaEnviados.Autorizados);
                MoverSeguro(
                    emProcessamento,
                    Empresas.Configuracoes[emp].SalvarSomenteXMLDistribuicao
                        ? PastaEnviados.Originais
                        : PastaEnviados.Autorizados);
                Excluir(NomeArquivoXML);
                Excluir(retornoRecuperavel);
                resultadoOperacao = "RetornoFiscalRecebido";
                categoriaFalha = "Nenhuma";
            }
            catch (Exception ex)
            {
                categoriaFalha = ClassificarFalha(ex);
                GravarErroSeguro(emp, categoriaFalha);
            }
            finally
            {
                GravarDiagnosticoPassivo(
                    emp,
                    resultadoOperacao,
                    categoriaFalha,
                    preparado,
                    transporteIniciado,
                    respostaRecebida,
                    respostaPersistida);
            }
        }

        private static IAutorizacaoNFeABI CriarAutorizacaoPadrao(XmlDocument documento, Configuracao configuracao)
        {
            return new AutorizacaoNFeABIAdapter(
                new Unimake.Business.DFe.Servicos.NFeABI.AutorizacaoSinc(
                    documento.OuterXml,
                    configuracao));
        }

        private sealed class AutorizacaoNFeABIAdapter : IAutorizacaoNFeABI
        {
            private readonly Unimake.Business.DFe.Servicos.NFeABI.AutorizacaoSinc autorizacao;
            internal AutorizacaoNFeABIAdapter(Unimake.Business.DFe.Servicos.NFeABI.AutorizacaoSinc autorizacao)
            {
                this.autorizacao = autorizacao;
            }

            public XmlDocument Assinado
            {
                get
                {
                    var copia = new XmlDocument();
                    copia.LoadXml(autorizacao.ConteudoXMLAssinado.OuterXml);

                    return copia;
                }
            }

            public string Executar()
            {
                autorizacao.Executar();

                return autorizacao.RetornoWSString;
            }

            public void Dispose()
            {
                autorizacao.Dispose();
            }
        }

        private static Configuracao CriarConfiguracao(int emp, XmlDocument documento)
        {
            var empresa = Empresas.Configuracoes[emp];
            var ide = (XmlElement)documento.GetElementsByTagName("ide")[0];
            var configuracao = new Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = empresa.AtivarPreparacaoTLSAntesEnvioXML,
                TipoDFe = TipoDFe.NFeABI,
                CodigoUF = Convert.ToInt32(ide.GetElementsByTagName("cUF")[0].InnerText),
                TipoAmbiente = (TipoAmbiente)Convert.ToInt32(ide.GetElementsByTagName("tpAmb")[0].InnerText),
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

        private static void ValidarPedido(int emp, XmlDocument xml)
        {
            var ide = xml.GetElementsByTagName("ide");
            if (ide.Count == 0 || ((XmlElement)ide[0]).GetElementsByTagName("tpAmb").Count == 0)
            {
                throw new Exception("Pedido de autorização da NF-e ABI inválido.");
            }

            var ambiente = ((XmlElement)ide[0]).GetElementsByTagName("tpAmb")[0].InnerText;

            if (ambiente != Empresas.Configuracoes[emp].AmbienteCodigo.ToString())
            {
                throw new Exception("O ambiente do pedido da NF-e ABI diverge da configuração da empresa.");
            }

            if (ambiente == "1")
            {
                throw new Exception("O endpoint de produção da NF-e ABI ainda não foi publicado.");
            }

            if (Empresas.Configuracoes[emp].UsaCertificado &&
                Empresas.Configuracoes[emp].X509Certificado == null)
            {
                throw new Exception("Certificado digital não configurado para a autorização da NF-e ABI.");
            }

            if (Empresas.Configuracoes[emp].UsaCertificado &&
                new Unimake.Business.Security.CertificadoDigital().Vencido(
                    Empresas.Configuracoes[emp].X509Certificado))
            {
                throw new Exception("Certificado digital vencido para a autorização da NF-e ABI.");
            }
        }

        private static bool AdicionarInfRespTec(int emp, XmlDocument xml)
        {
            if (xml.GetElementsByTagName("infRespTec").Count > 0)
            {
                return false;
            }

            var empresa = Empresas.Configuracoes[emp];
            if (string.IsNullOrEmpty(empresa.RespTecCNPJ) &&
                string.IsNullOrEmpty(empresa.RespTecXContato) &&
                string.IsNullOrEmpty(empresa.RespTecEmail) &&
                string.IsNullOrEmpty(empresa.RespTecTelefone))
            {
                return false;
            }

            var inf = xml.GetElementsByTagName("infNFeABI")[0];
            var grupo = xml.CreateElement("infRespTec", inf.NamespaceURI);

            AdicionarElemento(xml, grupo, "CNPJ", empresa.RespTecCNPJ);
            AdicionarElemento(xml, grupo, "xContato", empresa.RespTecXContato);
            AdicionarElemento(xml, grupo, "email", empresa.RespTecEmail);
            AdicionarElemento(xml, grupo, "fone", empresa.RespTecTelefone);
            inf.AppendChild(grupo);

            return true;
        }

        private static void AdicionarElemento(XmlDocument xml, XmlElement pai, string nome, string valor)
        {
            if (!string.IsNullOrEmpty(valor))
            {
                var elemento = xml.CreateElement(nome, pai.NamespaceURI);
                elemento.InnerText = valor;
                pai.AppendChild(elemento);
            }
        }

        private static void RemoverAssinaturaAnterior(XmlDocument xml)
        {
            var assinaturas = xml.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
            for (var i = assinaturas.Count - 1; i >= 0; i--)
                assinaturas[i].ParentNode.RemoveChild(assinaturas[i]);
        }

        private void SalvarEmProcessamento(int emp, string arquivo)
        {
            Empresas.Configuracoes[emp].CriarSubPastaEnviado();
            GravarSemSobrescrever(arquivo, ConteudoXML.OuterXml);
        }

        private void PersistirRetornoBruto(string retorno)
        {
            if (PersistirRetorno != null)
            {
                PersistirRetorno(retorno);
            }
            else
            {
                oGerarXML.XmlRetorno(
                    Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).RetornoXML,
                    retorno);
            }
        }

        private static string ValidarRetornoFiscal(string retorno, XmlDocument assinado)
        {
            var x = new XmlDocument();
            x.LoadXml(retorno);

            if (x.DocumentElement == null ||
                x.DocumentElement.LocalName != "retNFeABI" ||
                x.DocumentElement.NamespaceURI != "http://www.portalfiscal.inf.br/nfeabi" ||
                x.DocumentElement.GetAttribute("versao") != "1.00")
            {
                throw new Exception("Retorno técnico da NF-e ABI inválido.");
            }

            var stat = ExigirElementoDireto(x.DocumentElement, "cStat");

            int codigo;
            if (stat.InnerText.Trim().Length != 3 || !int.TryParse(stat.InnerText.Trim(), out codigo))
            {
                throw new Exception("Retorno técnico da NF-e ABI sem cStat conclusivo.");
            }

            ExigirElementoDireto(x.DocumentElement, "verAplic");
            ExigirElementoDireto(x.DocumentElement, "xMotivo");
            ExigirElementoDireto(x.DocumentElement, "cUF");
            ExigirElementoDireto(x.DocumentElement, "dhRecbto");

            var amb = ExigirElementoDireto(x.DocumentElement, "tpAmb");
            var pedidoAmb = ObterElementoPedido(assinado, "tpAmb");

            if (pedidoAmb == null || amb.InnerText.Trim() != pedidoAmb.InnerText.Trim())
            {
                throw new Exception("Ambiente do retorno da NF-e ABI inconsistente.");
            }

            return stat.InnerText.Trim();
        }

        private static XmlElement ObterElementoDireto(XmlElement pai, string nome)
        {
            foreach (XmlNode filho in pai.ChildNodes)
            {
                var elemento = filho as XmlElement;

                if (elemento != null && elemento.LocalName == nome && elemento.NamespaceURI == "http://www.portalfiscal.inf.br/nfeabi")
                {
                    return elemento;
                }
            }

            return null;
        }

        private static XmlElement ExigirElementoDireto(XmlElement pai, string nome)
        {
            var elemento = ObterElementoDireto(pai, nome);
            if (elemento == null || string.IsNullOrWhiteSpace(elemento.InnerText))
            {
                throw new Exception("Retorno técnico da NF-e ABI sem " + nome + ".");
            }

            return elemento;
        }

        private static XmlElement ObterElementoPedido(XmlDocument xml, string nome)
        {
            var elementos = xml.GetElementsByTagName(nome);

            return elementos.Count == 0 ? null : elementos[0] as XmlElement;
        }

        private static XmlElement ObterProtocoloCoerente(string retorno, XmlDocument assinado)
        {
            var x = new XmlDocument();
            x.LoadXml(retorno);
            ValidarRetornoFiscal(retorno, assinado);

            var p = ExigirElementoDireto(x.DocumentElement, "protNFeABI");
            if (p.GetAttribute("versao") != "1.00")
            {
                throw new Exception("Protocolo da NF-e ABI sem versão válida.");
            }

            var infNFe = assinado.GetElementsByTagName("infNFeABI");
            if (infNFe.Count == 0 || infNFe[0].Attributes["Id"] == null)
            {
                throw new Exception("Retorno de autorização sem protocolo.");
            }

            var chave = infNFe[0].Attributes["Id"].Value.Replace("NFeABI", "");
            var infProt = ExigirElementoDireto(p, "infProt");
            var ch = ExigirElementoDireto(infProt, "chNFeABI");
            var stat = ExigirElementoDireto(infProt, "cStat");
            var amb = ExigirElementoDireto(infProt, "tpAmb");
            ExigirElementoDireto(infProt, "verAplic");
            ExigirElementoDireto(infProt, "dhRecbto");
            ExigirElementoDireto(infProt, "xMotivo");
            if (stat.InnerText.Trim() != "100" ||
                ch.InnerText != chave ||
                amb.InnerText.Trim() != ObterElementoPedido(assinado, "tpAmb").InnerText.Trim())
            {
                throw new Exception("Protocolo da NF-e ABI inconsistente.");
            }

            var dig = ExigirElementoDireto(infProt, "digVal");
            var refDig = assinado.GetElementsByTagName("DigestValue");
            if (refDig.Count == 0 || dig.InnerText.Trim() != refDig[0].InnerText.Trim())
            {
                throw new Exception("Digest do protocolo da NF-e ABI inconsistente.");
            }

            return p;
        }

        private static string CriarProc(XmlDocument assinado, XmlElement protocolo)
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<nfeabiProc xmlns=\"http://www.portalfiscal.inf.br/nfeabi\" versao=\"1.00\">" +
                assinado.DocumentElement.OuterXml +
                protocolo.OuterXml +
                "</nfeabiProc>";
        }

        private static void GravarSemSobrescrever(string arquivo, string conteudo)
        {
            if (File.Exists(arquivo) && File.ReadAllText(arquivo) != conteudo)
            {
                throw new Exception("Colisão incompatível de evidência da NF-e ABI.");
            }

            if (!File.Exists(arquivo))
            {
                File.WriteAllText(arquivo, conteudo, new UTF8Encoding(false));
            }
        }

        private void MoverSeguro(string arquivo, PastaEnviados destino)
        {
            if (!File.Exists(arquivo))
            {
                return;
            }

            var data = ObterDataEmissao(ConteudoXML);
            var caminhoDestino = ObterCaminhoDestino(arquivo, destino, data);
            if (File.Exists(caminhoDestino))
            {
                if (File.ReadAllText(arquivo) != File.ReadAllText(caminhoDestino))
                {
                    throw new Exception("Colisão incompatível de evidência da NF-e ABI.");
                }

                File.Delete(arquivo);
                return;
            }

            (MoverArquivo ?? MoverArquivoPadrao)(arquivo, destino, data);

            if (!File.Exists(caminhoDestino) || File.Exists(arquivo))
            {
                throw new Exception("Não foi possível confirmar a movimentação da NF-e ABI.");
            }
        }

        private bool TentarConcluirCleanupFinal(int emp, string emProcessamento, string retornoRecuperavel, string retornoPublico)
        {
            var data = ObterDataEmissao(ConteudoXML);
            var procFinal = Path.Combine(
                Empresas.Configuracoes[Empresas.FindEmpresaByThread()].PastaXmlEnviado,
                PastaEnviados.Autorizados.ToString(),
                Empresas.Configuracoes[Empresas.FindEmpresaByThread()].DiretorioSalvarComo.ToString(data),
                Functions.ExtrairNomeArq(
                    Path.GetFileName(emProcessamento),
                    Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML) +
                Propriedade.ExtRetorno.ProcNFeABI);
            var originalFinal = ObterCaminhoDestino(
                emProcessamento,
                Empresas.Configuracoes[Empresas.FindEmpresaByThread()].SalvarSomenteXMLDistribuicao
                    ? PastaEnviados.Originais
                    : PastaEnviados.Autorizados,
                data);
            var retorno = File.ReadAllText(retornoRecuperavel);
            XmlDocument original = null;
            var autorizado = false;

            if (File.Exists(originalFinal))
            {
                original = CarregarXml(originalFinal);
                autorizado = true;
            }
            else
            {
                var erro = Path.Combine(Empresas.Configuracoes[emp].PastaXmlErro, Path.GetFileName(emProcessamento));
                if (!File.Exists(erro))
                {
                    return false;
                }

                original = CarregarXml(erro);
            }

            var cStat = ValidarRetornoFiscal(retorno, original);
            if (autorizado != (cStat == "100"))
            {
                throw new Exception("Evidência final da NF-e ABI incompatível.");
            }

            if (autorizado)
            {
                if (!File.Exists(procFinal))
                {
                    return false;
                }

                var protocolo = ObterProtocoloCoerente(retorno, original);
                var proc = CarregarXml(procFinal);
                var nfeNoProc = ObterElementoDireto(proc.DocumentElement, "NFeABI");
                var protNoProc = ObterElementoDireto(proc.DocumentElement, "protNFeABI");

                if (proc.DocumentElement.LocalName != "nfeabiProc" ||
                    proc.DocumentElement.NamespaceURI != "http://www.portalfiscal.inf.br/nfeabi" ||
                    nfeNoProc == null ||
                    protNoProc == null ||
                    nfeNoProc.OuterXml != original.DocumentElement.OuterXml ||
                    protNoProc.OuterXml != protocolo.OuterXml)
                {
                    throw new Exception("Evidência final da NF-e ABI incompatível.");
                }
            }

            var pedidoNormalizado = ClonarSemAssinatura(ConteudoXML);
            if (AdicionarInfRespTec(emp, pedidoNormalizado))
            {
                RemoverAssinaturaAnterior(pedidoNormalizado);
            }

            var arquivadoNormalizado = ClonarSemAssinatura(original);
            if (pedidoNormalizado.OuterXml != arquivadoNormalizado.OuterXml)
            {
                if (!File.Exists(retornoPublico))
                {
                    Excluir(retornoRecuperavel);
                    return false;
                }

                throw new Exception("Retorno público pendente de tentativa anterior da NF-e ABI.");
            }
            Excluir(NomeArquivoXML);
            Excluir(retornoRecuperavel);

            return true;
        }

        private static XmlDocument CarregarXml(string arquivo)
        {
            var xml = new XmlDocument
            {
                PreserveWhitespace = false
            };

            xml.Load(arquivo);

            return xml;
        }

        private static XmlDocument ClonarSemAssinatura(XmlDocument origem)
        {
            var clone = new XmlDocument
            {
                PreserveWhitespace = false
            };

            clone.LoadXml(origem.OuterXml);
            RemoverAssinaturaAnterior(clone);

            return clone;
        }

        private static string ObterChave(XmlDocument xml)
        {
            var inf = xml.GetElementsByTagName("infNFeABI");
            if (inf.Count == 0 || inf[0].Attributes["Id"] == null)
            {
                throw new Exception("NF-e ABI sem chave para retomada.");
            }

            return inf[0].Attributes["Id"].Value.Replace("NFeABI", "");
        }

        private static void Excluir(string arquivo)
        {
            if (!File.Exists(arquivo))
            {
                return;
            }

            (ExcluirArquivo ?? File.Delete)(arquivo);

            if (File.Exists(arquivo))
            {
                throw new Exception("Não foi possível concluir o cleanup da NF-e ABI.");
            }
        }

        private string ObterCaminhoDestino(string arquivo, PastaEnviados destino, DateTime data)
        {
            return Path.Combine(
                Empresas.Configuracoes[Empresas.FindEmpresaByThread()].PastaXmlEnviado,
                destino.ToString(),
                Empresas.Configuracoes[Empresas.FindEmpresaByThread()].DiretorioSalvarComo.ToString(data),
                Path.GetFileName(arquivo));
        }

        private static DateTime ObterDataEmissao(XmlDocument xml)
        {
            var dhEmi = xml.GetElementsByTagName("dhEmi");
            DateTime data;
            if (dhEmi.Count == 0 || !DateTime.TryParse(dhEmi[0].InnerText, out data))
            {
                throw new Exception("dhEmi da NF-e ABI ausente ou inválida.");
            }

            return data;
        }

        private static void MoverArquivoPadrao(string arquivo, PastaEnviados destino, DateTime data)
        {
            TFunctions.MoverArquivo(arquivo, destino, data);
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
                mensagem.IndexOf("ssl", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "TLS";
            }

            if (mensagem.IndexOf("dns", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("host", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "DNS";
            }

            if (mensagem.IndexOf("produção", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("ambiente", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("pedido", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Configuracao";
            }

            if (mensagem.IndexOf("persist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("colisão", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("movimenta", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Persistencia";
            }

            if (mensagem.IndexOf("retorno", StringComparison.OrdinalIgnoreCase) >= 0 ||
                mensagem.IndexOf("vazia", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Retorno";
            }

            return "Transporte";
        }

        private void GravarErroSeguro(int emp, string categoria)
        {
            try
            {
                var nome = Functions.ExtrairNomeArq(
                    NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML) +
                    Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).RetornoERR;

                File.WriteAllText(
                    Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, nome),
                    "Falha de " + categoria + " na autorização da NF-e ABI.",
                    new UTF8Encoding(false));
            }
            catch
            {
            }
        }

        private void GravarDiagnosticoPassivo(
            int emp,
            string resultadoOperacao,
            string categoriaFalha,
            bool preparado,
            bool transporteIniciado,
            bool respostaRecebida,
            bool respostaPersistida)
        {
#if _BETA || DEBUG
            try
            {
                var nome = Functions.ExtrairNomeArq(
                    NomeArquivoXML,
                    Propriedade.Extensao(Propriedade.TipoEnvio.NFeABI).EnvioXML) +
                    Propriedade.ExtRetorno.DiagnosticoDisponibilidadeDFe;
                var configuracao = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false)
                };

                using (var w = XmlWriter.Create(
                    Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno, nome),
                    configuracao))
                {
                    w.WriteStartElement("DiagnosticoDisponibilidadeDFe");
                    w.WriteElementString("TipoDFe", "NFeABI");
                    w.WriteElementString("ResultadoOperacao", resultadoOperacao);
                    w.WriteElementString("CategoriaFalha", categoriaFalha);
                    w.WriteElementString("Preparado", preparado ? "Sim" : "Nao");
                    w.WriteElementString("TransporteIniciado", transporteIniciado ? "Sim" : "Nao");
                    w.WriteElementString("RespostaFiscalRecebida", respostaRecebida ? "Sim" : "Nao");
                    w.WriteElementString("RespostaFiscalPersistida", respostaPersistida ? "Sim" : "Nao");
                    w.WriteEndElement();
                }
            }
            catch
            {
            }
#endif
        }
    }
}
