using NFe.Components;
using NFe.Settings;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Unimake.Business.DFe.Servicos;
using Unimake.Business.DFe.Xml.NFSe.NACIONAL;

namespace NFe.Service.NFSe
{
    public class TaskNFSeCancelar : TaskAbst
    {
        #region Private Fields

        /// <summary>
        /// Esta herança que deve ser utilizada fora da classe para obter os valores das tag´s do pedido de cancelamento
        /// </summary>
        private DadosPedCanNfse oDadosPedCanNfse;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Fazer a leitura do conteúdo do XML de cancelamento de NFS-e e disponibilizar conteúdo em um objeto para analise
        /// </summary>
        /// <param name="arquivoXML">Arquivo XML que é para efetuar a leitura</param>
        private void PedCanNfse(int emp, string arquivoXML)
        {
        }

        #endregion Private Methods

        #region Public Methods

        public override void Execute()
        {
            var emp = Empresas.FindEmpresaByThread();

            //Definir o serviço que será executado para a classe
            Servico = Servicos.NFSeCancelar;

            try
            {
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" +
                                         Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML) + Propriedade.ExtRetorno.CanNfse_ERR);
                Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + NomeArquivoXML);

                oDadosPedCanNfse = new DadosPedCanNfse(emp);
                PedCanNfse(emp, NomeArquivoXML);
                var padraoNFSe = Functions.BuscaPadraoNFSe(oDadosPedCanNfse.cMunicipio);

                ExecuteDLL(emp, oDadosPedCanNfse.cMunicipio, padraoNFSe);

                AnalisarRetorno(vStrXmlRetorno, padraoNFSe, emp);
            }
            catch (Exception ex)
            {
                try
                {
                    //Gravar o arquivo de erro de retorno para o ERP, caso ocorra
                    TFunctions.GravarArqErroServico(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML, Propriedade.ExtRetorno.CanNfse_ERR, ex);
                }
                catch
                {
                    //Se falhou algo na hora de gravar o retorno .ERR (de erro) para o ERP, infelizmente não posso fazer mais nada.
                    //Wandrey 31/08/2011
                }
            }
            finally
            {
                try
                {
                    Functions.DeletarArquivo(NomeArquivoXML);
                }
                catch
                {
                    //Se falhou algo na hora de deletar o XML de cancelamento de NFe, infelizmente
                    //não posso fazer mais nada, o UniNFe vai tentar mandar o arquivo novamente para o webservice, pois ainda não foi excluido.
                    //Wandrey 31/08/2011
                }
            }
        }

        /// <summary>
        /// Analisar o XML retornado se for ambiente nacional e se o evento  tiver sido autorizado vamos salvar o XML na pasta autorizados
        /// </summary>
        /// <param name="vStrXmlRetorno">XML Retornado</param>
        /// <param name="padraoNFSe">Padrão da NFSe</param>
        /// <param name="emp">Codigo da empresa</param>
        private void AnalisarRetorno(string vStrXmlRetorno, PadraoNFSe padraoNFSe, int emp)
        {
            if (padraoNFSe != PadraoNFSe.NACIONAL)
            {
                return;
            }

            var pastaEnviado = Empresas.Configuracoes[emp].PastaXmlEnviado;
            if (string.IsNullOrWhiteSpace(pastaEnviado))
            {
                return;
            }

            try
            {
                var autorizou = false;
                var doc = new XmlDocument();
                doc.LoadXml(vStrXmlRetorno);
                var eventoNodes = doc.GetElementsByTagName("evento");

                foreach (XmlElement evento in eventoNodes)
                {
                    var infEvento = evento["infEvento"] as XmlElement;
                    if (infEvento == null)
                    {
                        continue;
                    }

                    autorizou = true;

                    var dhProcTexto = infEvento?["dhProc"]?.InnerText;
                    if (string.IsNullOrWhiteSpace(dhProcTexto))
                    {
                        continue;
                    }

                    if (!DateTimeOffset.TryParseExact(dhProcTexto, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dhProcOffset))
                    {
                        continue;
                    }

                    var dhProc = dhProcOffset.DateTime;
                    var id = infEvento.GetAttribute("Id");
                    if (string.IsNullOrWhiteSpace(id) || id.Length <= 3)
                    {
                        continue;
                    }

                    var nameArq = id.Substring(3) + "-proceventonfse.xml";
                    var pathFile = Path.Combine(pastaEnviado, PastaEnviados.Autorizados.ToString(), Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(dhProc), nameArq);

                    var dir = Path.GetDirectoryName(pathFile);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (!File.Exists(pathFile))
                    {
                        File.WriteAllText(pathFile, vStrXmlRetorno);
                    }
                }

                if (!autorizou)
                {
                    Functions.Move(NomeArquivoXML, Path.Combine(Empresas.Configuracoes[emp].PastaXmlErro, Path.GetFileName(NomeArquivoXML))); //Move o arquivo para a pasta de erro
                }
            }
            catch (Exception ex)
            {
                // Logar erro se necessário, mas não lançar para não interromper o fluxo
                Auxiliar.WriteLog($"Erro ao salvar o XML do Evento da NFSe autorizado: {ex.Message}", false);
            }
        }

        #endregion Public Methods

        /// <summary>
        /// Executa o serviço utilizando a DLL do UniNFe.
        /// </summary>
        /// <param name="emp">Empresa que está enviando o XML</param>
        /// <param name="municipio">Código do município para onde será enviado o XML</param>
        /// <param name="padraoNFSe">Padrão do munípio para NFSe</param>
        private void ExecuteDLL(int emp, int municipio, PadraoNFSe padraoNFSe)
        {
            var conteudoXML = new XmlDocument();
            conteudoXML.Load(NomeArquivoXML);

            var finalArqEnvio = Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML;
            var finalArqRetorno = Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).RetornoXML;
            var resolucao = ResolucaoCentralizadaNFSe.Resolver(conteudoXML, padraoNFSe, municipio);
            var versaoXML = resolucao.Versao;
            var servico = resolucao.Servico;

            Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlRetorno + "\\" + Functions.ExtrairNomeArq(NomeArquivoXML, finalArqEnvio) + Functions.ExtractExtension(finalArqRetorno) + ".err");

            var configuracao = new Unimake.Business.DFe.Servicos.Configuracao
            {
                PrepararConexaoTLSAntesDoEnvio = Empresas.Configuracoes[emp].AtivarPreparacaoTLSAntesEnvioXML,
                TipoDFe = Unimake.Business.DFe.Servicos.TipoDFe.NFSe,
                CertificadoDigital = Empresas.Configuracoes[emp].X509Certificado,
                TipoAmbiente = (Unimake.Business.DFe.Servicos.TipoAmbiente)Empresas.Configuracoes[emp].AmbienteCodigo,
                CodigoMunicipio = TFunctions.DefiniMunicioPadrao(padraoNFSe, municipio),
                Servico = servico,
                SchemaVersao = versaoXML,
                MunicipioToken = Empresas.Configuracoes[emp].SenhaWS,
                TokenSoap = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioSenha = Empresas.Configuracoes[emp].SenhaWS,
                MunicipioUsuario = Empresas.Configuracoes[emp].UsuarioWS
            };

            if (padraoNFSe == PadraoNFSe.WEBFISCO)
            {
                XmlElement root = conteudoXML.DocumentElement;
                XmlNode firstElement = root.FirstChild;
                XmlNode tagUsuario = conteudoXML.CreateElement("usuario");
                XmlNode tagSenha = conteudoXML.CreateElement("pass");

                tagUsuario.InnerText = configuracao.MunicipioUsuario;
                tagSenha.InnerText = configuracao.MunicipioSenha;
                root.InsertBefore(tagUsuario, firstElement);
                root.InsertBefore(tagSenha, firstElement);

                conteudoXML.AppendChild(root);
            }

            switch (servico)
            {
                case Unimake.Business.DFe.Servicos.Servico.NFSeCancelarNfse:
                    var cancelarNfse = new Unimake.Business.DFe.Servicos.NFSe.CancelarNfse(conteudoXML, configuracao);
                    cancelarNfse.Executar();
                    vStrXmlRetorno = cancelarNfse.RetornoWSString;

                    cancelarNfse.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeCancelamentoNfe:
                    var cancelamentoNfe = new Unimake.Business.DFe.Servicos.NFSe.CancelamentoNfe(conteudoXML, configuracao);
                    cancelamentoNfe.Executar();
                    vStrXmlRetorno = cancelamentoNfe.RetornoWSString;

                    cancelamentoNfe.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeCancelaNota:
                    var cancelaNota = new Unimake.Business.DFe.Servicos.NFSe.CancelaNota(conteudoXML, configuracao);
                    cancelaNota.Executar();
                    vStrXmlRetorno = cancelaNota.RetornoWSString;

                    cancelaNota.Dispose();
                    break;

                case Unimake.Business.DFe.Servicos.Servico.NFSeCancelarNotaFiscal:
                    var cancelarNotaFiscal = new Unimake.Business.DFe.Servicos.NFSe.CancelarNotaFiscal(conteudoXML, configuracao);
                    cancelarNotaFiscal.Executar();
                    vStrXmlRetorno = cancelarNotaFiscal.RetornoWSString;

                    cancelarNotaFiscal.Dispose();
                    break;
            }


            XmlRetorno(finalArqEnvio, finalArqRetorno);

            /// grava o arquivo no FTP
            var filenameFTP = Path.Combine(Empresas.Configuracoes[emp].PastaXmlRetorno,
                Functions.ExtrairNomeArq(NomeArquivoXML, Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).EnvioXML) + Propriedade.Extensao(Propriedade.TipoEnvio.PedCanNFSe).RetornoXML);

            if (File.Exists(filenameFTP))
            {
                new GerarXML(emp).XmlParaFTP(emp, filenameFTP);
            }
        }


    }
}
