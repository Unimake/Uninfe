using NFe.Components;
using NFe.Settings;
using NFe.Validate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Unimake.Business.DFe.Servicos;
namespace NFe.Service
{
    public abstract class TaskAbst
    {
        #region Objetos

        protected Auxiliar oAux = new Auxiliar();
        protected GerarXML oGerarXML = new GerarXML(Thread.CurrentThread);

        #endregion Objetos

        #region Propriedades

        /// <summary>
        /// Conteúdo do XML de retorno do serviço, ou seja, para cada serviço invocado a classe seta neste atributo a string do XML Retornado pelo serviço
        /// </summary>
        public string vStrXmlRetorno { get; set; }

        /// <summary>
        /// Pasta/Nome do arquivo XML contendo os dados a serem enviados (Nota Fiscal, Pedido de Status, Cancelamento, etc...)
        /// </summary>
        private string mNomeArquivoXML;

        public string NomeArquivoXML
        {
            get => mNomeArquivoXML;
            set
            {
                mNomeArquivoXML = value;
                oGerarXML.NomeXMLDadosMsg = value;
            }
        }

        /// <summary>
        /// Conteúdo do XML para que será enviado
        /// </summary>
        public XmlDocument ConteudoXML = new XmlDocument();

        /// <summary>
        /// Serviço que está sendo executado (Envio de Nota, Cancelamento, Consulta, etc...)
        /// </summary>
        private Servicos mServico;

        public Servicos Servico
        {
            get => mServico;
            protected set
            {
                mServico = value;
                oGerarXML.Servico = value;
            }
        }

        /// <summary>
        /// Se o vXmlNFeDadosMsg é um XML
        /// </summary>
        public bool vXmlNfeDadosMsgEhXML    //danasa 12-9-2009
=> Path.GetExtension(NomeArquivoXML).ToLower() == ".xml";

        #endregion Propriedades

        public abstract void Execute();

        #region LerXMLNFe()

        /// <summary>
        /// Le o conteúdo do XML da NFe
        /// </summary>
        /// <param name="conteudoXML">Conteúdo do XML</param>
        /// <returns>Retorna o conteúdo do XML da NFe</returns>
        public DadosNFeClass LerXMLNFe(XmlDocument conteudoXML)
        {
            var lerXML = new LerXML();

            switch (Servico)
            {
                case Servicos.MDFeAssinarValidarEnvioEmLote:
                    lerXML.Mdfe(conteudoXML);
                    break;

                case Servicos.CTeAssinarValidarEnvioEmLote:
                    lerXML.Cte(conteudoXML);
                    break;

                default:
                    lerXML.Nfe(conteudoXML);
                    break;
            }

            return lerXML.oDadosNfe;
        }

        #endregion LerXMLNFe()

        #region AssinarValidarXMLNFe()

        /// <summary>
        /// Assinar e validar o XML da Nota Fiscal Eletrônica e move para a pasta de assinados
        /// </summary>
        public void AssinarValidarXMLNFe()
        {
            var conteudoXML = new XmlDocument();
            try
            {
                conteudoXML.Load(NomeArquivoXML);
            }
            catch
            {
                conteudoXML.LoadXml(File.ReadAllText(NomeArquivoXML, System.Text.Encoding.UTF8));
            }

            ValidarXMLDFe(conteudoXML);

            var sw = File.CreateText(NomeArquivoXML);
            sw.Write(conteudoXML.OuterXml);
            sw.Close();
        }

        #endregion AssinarValidarXMLNFe()

        #region AssinarValidarXMLNFe()

        /// <summary>
        /// Assinar e validar o XML da Nota Fiscal Eletrônica e move para a pasta de assinados
        /// </summary>
        /// <param name="conteudoXML">Nome da pasta onde está o XML a ser validado e assinado</param>
        /// <returns>true = Conseguiu assinar e validar</returns>
        /// <remarks>
        /// Autor: Wandrey Mundin Ferreira
        /// Data: 03/04/2009
        /// </remarks>
        public void ValidarXMLDFe(XmlDocument conteudoXML)
        {
            var emp = Empresas.FindEmpresaByThread();

            try
            {
                //Fazer uma leitura de algumas tags do XML
                var dadosNFe = LerXMLNFe(conteudoXML);

                var respTecnico = new RespTecnico(Empresas.Configuracoes[emp].RespTecCNPJ,
                    Empresas.Configuracoes[emp].RespTecXContato,
                    Empresas.Configuracoes[emp].RespTecEmail,
                    Empresas.Configuracoes[emp].RespTecTelefone,
                    Empresas.Configuracoes[emp].RespTecIdCSRT,
                    Empresas.Configuracoes[emp].RespTecCSRT);

                respTecnico.AdicionarResponsavelTecnico(conteudoXML);

                var ChaveNfe = dadosNFe.chavenfe;
                var TpEmis = dadosNFe.tpEmis;

                //Inserir NFe no XML de controle do fluxo
                var oFluxoNfe = new FluxoNfe(emp);
                if (oFluxoNfe.NfeExiste(ChaveNfe))
                {
                    Auxiliar.WriteLog($"Já existe um XML com a chave '{ChaveNfe}' em processamento. Aguarde a conclusão antes de tentar gerar o mesmo documento novamente.", false);
                }
                else
                {
                    //Deletar o arquivo XML da pasta de temporários de XML´s com erros se o mesmo existir
                    Functions.DeletarArquivo(Empresas.Configuracoes[emp].PastaXmlErro + "\\" + Path.GetFileName(NomeArquivoXML));
                }

                #region Assinar e validar o XML para manter uma compatibilidade antes de usar a DLL do UNINFE. Wandrey 09/02/2022

                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(conteudoXML.OuterXml);

                    var validarXMLNew = new ValidarXMLNew();

                    switch (Servico)
                    {
                        case Servicos.CTeAssinarValidarEnvioEmLote:
                        case Servicos.NFeAssinarValidarEnvioEmLote:
                            validarXMLNew.Validar(xmlDoc, true, NomeArquivoXML, emp);
                            conteudoXML.Load(NomeArquivoXML);
                            break;

                        default:
                            validarXMLNew.Validar(xmlDoc, false, NomeArquivoXML, emp);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                #endregion

                //Validações gerais
                ValidacoesGeraisXMLNFe(dadosNFe);

                oFluxoNfe.InserirNfeFluxo(ChaveNfe, dadosNFe.mod, NomeArquivoXML);
            }
            catch (Exception ex)
            {
                var exception = ex.GetLastException();

                try
                {
                    var extFinal = Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML;
                    var extErro = Propriedade.ExtRetorno.Nfe_ERR;
                    switch (Servico)
                    {
                        case Servicos.MDFeAssinarValidarEnvioEmLote:
                            extFinal = Propriedade.Extensao(Propriedade.TipoEnvio.MDFe).EnvioXML;
                            extErro = Propriedade.ExtRetorno.MDFe_ERR;
                            break;

                        case Servicos.CTeAssinarValidarEnvioEmLote:
                            extFinal = Propriedade.Extensao(Propriedade.TipoEnvio.CTe).EnvioXML;
                            extErro = Propriedade.ExtRetorno.Cte_ERR;
                            break;
                    }

                    TFunctions.GravarArqErroServico(NomeArquivoXML, extFinal, extErro, exception);
                }
                catch (Exception exx)
                {
                    Auxiliar.WriteLog(exx.Message, true);

                    //Se ocorrer algum erro na hora de tentar gravar o XML de erro para o ERP ou mover o arquivo XML para a pasta de XML com erro, não
                    //vou poder fazer nada, pq foi algum erro de rede, permissão de acesso a pasta ou arquivo, etc.
                    //Wandey 13/03/2010
                }

                throw;
            }
        }

        #endregion AssinarValidarXMLNFe()

        #region ValidacoesGerais()

        /// <summary>
        /// Efetua uma leitura do XML da nota fiscal eletrônica e faz diversas conferências do seu conteúdo e bloqueia se não
        /// estiver de acordo com as configurações do UNINFE
        /// </summary>
        /// <param name="dadosNFe">Objeto com o conteúdo das tags do XML</param>
        /// <returns>true = Validado com sucesso</returns>
        /// <by>Wandrey Mundin Ferreira</by>
        /// <date>16/04/2009</date>
        protected void ValidacoesGeraisXMLNFe(DadosNFeClass dadosNFe)
        {
            var emp = Empresas.FindEmpresaByThread();

            var gException = true;

            var booValido = false;
            var nPos = 0;
            var cTextoErro = "";

            var tpEmis = Convert.ToInt32(dadosNFe.tpEmis);

            switch (Servico)
            {
                case Servicos.MDFeAssinarValidarEnvioEmLote:
                    booValido = true;
                    nPos = 1;
                    goto case Servicos.NFeMontarLoteUma;

                case Servicos.CTeAssinarValidarEnvioEmLote:
                case Servicos.NFeAssinarValidarEnvioEmLote:
                case Servicos.NFeMontarLoteUma:
                    switch (Empresas.Configuracoes[emp].tpEmis)
                    {
                        case (int)TipoEmissao.Normal:
                            switch (tpEmis)
                            {
                                case (int)TipoEmissao.Normal:

                                ///
                                /// Foi emitido em contingencia e agora os quer enviar
                                ///
                                case (int)TipoEmissao.ContingenciaFSIA:
                                case (int)TipoEmissao.ContingenciaFSDA:
                                case (int)TipoEmissao.ContingenciaEPEC:
                                case (int)TipoEmissao.ContingenciaOffLine:
                                    booValido = true;
                                    break;
                            }
                            break;

                        case (int)TipoEmissao.ContingenciaSVCSP:
                            booValido = (tpEmis == (int)TipoEmissao.ContingenciaSVCSP);
                            break;

                        case (int)TipoEmissao.ContingenciaSVCAN:
                            booValido = (tpEmis == (int)TipoEmissao.ContingenciaSVCAN);
                            break;

                        case (int)TipoEmissao.ContingenciaSVCRS:
                            booValido = (tpEmis == (int)TipoEmissao.ContingenciaSVCRS);
                            break;

                        case (int)TipoEmissao.ContingenciaFSIA:
                        case (int)TipoEmissao.ContingenciaEPEC:
                        case (int)TipoEmissao.ContingenciaFSDA:
                        case (int)TipoEmissao.ContingenciaOffLine:

                            //Retorno somente falso mas sem exception para não fazer nada. Wandrey 09/06/2009
                            gException = booValido = false;
                            break;
                    }
                    if (dadosNFe.tpEmis == "3")
                    {
                        throw new Exception("O tipo de emissão 3 passou a ser de uso exclusivo da SEFAZ para emissão da NF-e/NFC-e em regime especial.");
                    }
                    break;
            }
            var cTextoErro2 = "O XML não será enviado e será movido para a pasta de XML com erro para análise.";

            if (!booValido && gException)
            {
                cTextoErro = "O XML está configurado para um tipo de emissão e o UniNFe para outro. " +
                         "XML: " + EnumHelper.GetDescription((TipoEmissao)Enum.ToObject(typeof(TipoEmissao), tpEmis)) +
                         " (tpEmis = " + tpEmis.ToString() + "). " +
                         "UniNFe: " + EnumHelper.GetDescription((TipoEmissao)Enum.ToObject(typeof(TipoEmissao), Empresas.Configuracoes[emp].tpEmis)) +
                         " (tpEmis = " + Empresas.Configuracoes[emp].tpEmis.ToString() + "). " +
                        cTextoErro2;

                throw new Exception(cTextoErro);
            }
            #region Verificar o ambiente da nota com o que está configurado no uninfe. Wandrey 20/08/2014

            if (booValido)
            {
                switch (Empresas.Configuracoes[emp].AmbienteCodigo)
                {
                    case (int)TipoAmbiente.Homologacao:
                        if (Convert.ToInt32(dadosNFe.tpAmb) == (int)TipoAmbiente.Producao)
                        {
                            cTextoErro = "Conteúdo da tag tpAmb do XML está com conteúdo indicando o envio para ambiente de produção e o UniNFe está configurado para ambiente de homologação.";
                            throw new Exception(cTextoErro);
                        }
                        break;

                    case (int)TipoAmbiente.Producao:
                        if (Convert.ToInt32(dadosNFe.tpAmb) == (int)TipoAmbiente.Homologacao)
                        {
                            cTextoErro = "Conteúdo da tag tpAmb do XML está com conteúdo indicando o envio para ambiente de homologação e o UniNFe está configurado para ambiente de produção.";
                            throw new Exception(cTextoErro);
                        }
                        break;
                }
            }

            #endregion Verificar o ambiente da nota com o que está configurado no uninfe. Wandrey 20/08/2014

            #region Verificar se os valores das tag´s que compõe a chave da nfe estão batendo com as informadas na chave

            //Verificar se os valores das tag´s que compõe a chave da nfe estão batendo com as informadas na chave
            if (booValido)
            {
                cTextoErro = string.Empty;

                #region Tag <cUF>

                if (dadosNFe.cUF != dadosNFe.chavenfe.Substring(3 + nPos, 2))
                {
                    cTextoErro += "O código da UF informado na tag <cUF> está diferente do informado na chave da NF-e.\r\n" +
                        "Código da UF informado na tag <cUF>: " + dadosNFe.cUF + "\r\n" +
                        "Código da UF informado na chave da NF-e: " + dadosNFe.chavenfe.Substring(3 + nPos, 2) + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <cUF>

                #region Tag <tpEmis>

                if (dadosNFe.tpEmis != dadosNFe.chavenfe.Substring(37 + nPos, 1))
                {
                    cTextoErro += "O código numérico informado na tag <tpEmis> está diferente do informado na chave da NF-e.\r\n" +
                        "Código numérico informado na tag <tpEmis>: " + dadosNFe.tpEmis + "\r\n" +
                        "Código numérico informado na chave da NF-e: " + dadosNFe.chavenfe.Substring(37 + nPos, 1) + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <tpEmis>

                #region Tag <cNF>

                if (dadosNFe.cNF != dadosNFe.chavenfe.Substring(38 + nPos, 8))
                {
                    cTextoErro += "O código numérico informado na tag <cNF> está diferente do informado na chave da NF-e.\r\n" +
                        "Código numérico informado na tag <cNF>: " + dadosNFe.cNF + "\r\n" +
                        "Código numérico informado na chave da NF-e: " + dadosNFe.chavenfe.Substring(38 + nPos, 8) + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <cNF>

                #region Tag <mod>

                if (dadosNFe.mod != dadosNFe.chavenfe.Substring(23 + nPos, 2))
                {
                    cTextoErro += "O modelo informado na tag <mod> está diferente do informado na chave da NF-e.\r\n" +
                        "Modelo informado na tag <mod>: " + dadosNFe.mod + "\r\n" +
                        "Modelo informado na chave da NF-e: " + dadosNFe.chavenfe.Substring(23 + nPos, 2) + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <mod>

                #region Tag <nNF>

                if (Convert.ToInt32(dadosNFe.nNF) != Convert.ToInt32(dadosNFe.chavenfe.Substring(28 + nPos, 9)))
                {
                    cTextoErro += "O número da NF-e informado na tag <nNF> está diferente do informado na chave da NF-e.\r\n" +
                        "Número da NFe informado na tag <nNF>: " + Convert.ToInt32(dadosNFe.nNF).ToString() + "\r\n" +
                        "Número da NFe informado na chave da NF-e: " + Convert.ToInt32(dadosNFe.chavenfe.Substring(28 + nPos, 9)).ToString() + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <nNF>

                #region Tag <cDV>

                if (dadosNFe.cDV != dadosNFe.chavenfe.Substring(46 + nPos, 1))
                {
                    cTextoErro += "O número do dígito verificador informado na tag <cDV> está diferente do informado na chave da NF-e.\r\n" +
                        "Número do dígito verificador informado na tag <cDV>: " + dadosNFe.cDV + "\r\n" +
                        "Número do dígito verificador informado na chave da NF-e: " + dadosNFe.chavenfe.Substring(46 + nPos, 1) + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <cDV>

                #region Tag <CNPJ> da tag <emit>

                if (string.IsNullOrEmpty(dadosNFe.CNPJ))
                {
                    if (string.IsNullOrEmpty(dadosNFe.CPF))
                    {
                        cTextoErro += "O CNPJ ou CPF do emitente não foi localizado no XML <emit><CNPJ> ou <emit><CPF>.\r\n\r\n";
                        booValido = false;
                    }
                    else if (dadosNFe.CPF != dadosNFe.chavenfe.Substring(12 + nPos, 11))
                    {
                        cTextoErro += "O CPF do emitente informado na tag <emit><CPF> está diferente do informado na chave da NF-e.\r\n" +
                            "CPF do emitente informado na tag <emit><CPF>: " + dadosNFe.CPF + "\r\n" +
                            "CPF do emitente informado na chave da NF-e: " + dadosNFe.chavenfe.Substring(12 + nPos, 11) + "\r\n\r\n";
                        booValido = false;
                    }
                }
                else if (dadosNFe.CNPJ != dadosNFe.chavenfe.Substring(9 + nPos, 14))
                {
                    cTextoErro += "O CNPJ do emitente informado na tag <emit><CNPJ> está diferente do informado na chave da NF-e.\r\n" +
                        "CNPJ do emitente informado na tag <emit><CNPJ>: " + dadosNFe.CNPJ + "\r\n" +
                        "CNPJ do emitente informado na chave da NF-e: " + dadosNFe.chavenfe.Substring(9 + nPos, 14) + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <CNPJ> da tag <emit>

                #region Tag <serie>

                if (Convert.ToInt32(dadosNFe.serie) != Convert.ToInt32(dadosNFe.chavenfe.Substring(25 + nPos, 3)))
                {
                    cTextoErro += "A série informada na tag <serie> está diferente da informada na chave da NF-e.\r\n" +
                        "Série informada na tag <cDV>: " + Convert.ToInt32(dadosNFe.serie).ToString() + "\r\n" +
                        "Série informada na chave da NF-e: " + Convert.ToInt32(dadosNFe.chavenfe.Substring(25 + nPos, 3)).ToString() + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <serie>

                #region Tag <dEmi>

                if (dadosNFe.dEmi.Month.ToString("00") != dadosNFe.chavenfe.Substring(7 + nPos, 2) ||
                    dadosNFe.dEmi.Year.ToString("0000").Substring(2, 2) != dadosNFe.chavenfe.Substring(5 + nPos, 2))
                {
                    cTextoErro += "O ano e mês da emissão informada na tag " + dadosNFe.versao == "2.00" ? "<dEmi> " : "<dhEmi> " + "está diferente da informada na chave da NF-e.\r\n" +
                        "Mês/Ano da data de emissão informada na tag " + dadosNFe.versao == "2.00" ? "<dEmi>: " : "<dhEmi>: " + dadosNFe.dEmi.Month.ToString("00") + "/" + dadosNFe.dEmi.Year.ToString("0000").Substring(2, 2) + "\r\n" +
                        "Mês/Ano informados na chave da NF-e: " + dadosNFe.chavenfe.Substring(7 + nPos, 2) + "/" + dadosNFe.chavenfe.Substring(5 + nPos, 2) + "\r\n\r\n";
                    booValido = false;
                }

                #endregion Tag <dEmi>

                if (!booValido)
                {
                    throw new Exception(cTextoErro);
                }
            }

            #endregion Verificar se os valores das tag´s que compõe a chave da nfe estão batendo com as informadas na chave
        }

        private bool ValidarInformacaoContingencia(DadosNFeClass dadosNFe)
        {
            if (string.IsNullOrEmpty(dadosNFe.dhCont) || string.IsNullOrEmpty(dadosNFe.xJust))
            {
                return false;
            }

            return true;
        }

        #endregion ValidacoesGerais()

        #region LoteNfe()

        /// <summary>
        /// Auxiliar na geração do arquivo XML de Lote de notas fiscais
        /// </summary>
        /// <param name="arquivo">Nome do arquivo XML da NFe</param>
        /// <param name="conteudoXML">Conteúdo do XML</param>
        /// <param name="versaoXml">Versão do XML</param>
        /// <param name="modeloDFe">Modelo do documento fiscal eletrônico</param>
        protected XmlDocument LoteNfe(XmlDocument conteudoXML, string arquivo, string versaoXml, string modeloDFe)
        {
            var arquivos = new List<ArquivoXMLDFe>
            {
                new ArquivoXMLDFe() { NomeArquivoXML = arquivo, ConteudoXML = conteudoXML }
            };

            return oGerarXML.LoteNfe(Servico, arquivos, versaoXml, modeloDFe);
        }

        #endregion LoteNfe()

        #region LoteNfe()

        /// <summary>
        /// Auxliar na geração do arquivo XML de Lote de notas fiscais
        /// </summary>
        /// <param name="arquivosNfe">Lista de arquivos de NFe para montagem do lote de várias NFe</param>
        /// <param name="versaoXml">Versao do Xml de lote</param>
        /// <param name="modeloDFe">Modelo do documento fiscal eletrônico</param>
        /// <date>24/08/2009</date>
        /// <by>Wandrey Mundin Ferreira</by>
        protected XmlDocument LoteNfe(List<ArquivoXMLDFe> arquivosNfe, string versaoXml, string modeloDFe) => oGerarXML.LoteNfe(Servico, arquivosNfe, versaoXml, modeloDFe);

        #endregion LoteNfe()

        #region ProcessaNFeDenegada

        protected void ProcessaNFeDenegada(int emp, LerXML oLerXml, string strArquivoNFe, XmlDocument conteudoXML, string protNFe)
        {
            string strProtNfe;

            if (!File.Exists(strArquivoNFe))
            {
                throw new Exception("Arquivo \"" + strArquivoNFe + "\" não encontrado");
            }

            if (conteudoXML == null)
            {
                conteudoXML = new XmlDocument();
                conteudoXML.Load(strArquivoNFe);
            }
            oLerXml.Nfe(conteudoXML);

            var nomePastaEnviado = Empresas.Configuracoes[emp].PastaXmlEnviado + "\\" +
                PastaEnviados.Denegados.ToString() + "\\" +
                Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi);
            var dArquivo = Path.Combine(nomePastaEnviado, Path.GetFileName(strArquivoNFe).Replace(Propriedade.Extensao(Propriedade.TipoEnvio.NFe).EnvioXML, Propriedade.ExtRetorno.Den));
            var strNomeArqDenegadaNFe = dArquivo;
            var arqDen = dArquivo;

            //danasa 11-4-2012
            var addNFeDen = true;
            if (File.Exists(dArquivo))
            {
                // verifica se a NFe já tem protocolo gravado
                // só para atualizar notas denegadas que ainda não tem o protocolo atualizado
                // e que já estao na pasta de notas denegadas.
                // Para futuras notas denegadas esta propriedade sempre será false
                if (File.ReadAllText(dArquivo).IndexOf("<protNFe>") > 0)
                {
                    addNFeDen = false;
                }
            }
            if (addNFeDen)
            {
                ///
                /// monta o XML de denegacao
                strProtNfe = protNFe;

                ///
                /// gera o arquivo de denegacao na pasta EmProcessamento
                strNomeArqDenegadaNFe = oGerarXML.XmlDistNFe(strArquivoNFe, strProtNfe, Propriedade.ExtRetorno.Den, oLerXml.oDadosNfe.versao);
                if (string.IsNullOrEmpty(strNomeArqDenegadaNFe))
                {
                    throw new Exception("Erro de criação do arquivo de distribuição da nota denegada");
                }

                ///
                /// exclui o XML denegado, se existir
                Functions.DeletarArquivo(dArquivo);

                ///
                /// Move a NFE-denegada da pasta em processamento para NFe Denegadas
                TFunctions.MoverArquivo(strNomeArqDenegadaNFe, PastaEnviados.Denegados, oLerXml.oDadosNfe.dEmi);

                ///
                /// verifica se o arquivo da NFe já existe na pasta denegadas
                dArquivo = Path.Combine(nomePastaEnviado, Path.GetFileName(strArquivoNFe));

                if (!File.Exists(dArquivo))
                {
                    if (!string.IsNullOrEmpty(Empresas.Configuracoes[emp].PastaBackup))
                    {
                        //Criar Pasta do Mês para gravar arquivos enviados
                        var nomePastaBackup = Empresas.Configuracoes[emp].PastaBackup + "\\" +
                                                    PastaEnviados.Denegados + "\\" +
                                                    Empresas.Configuracoes[emp].DiretorioSalvarComo.ToString(oLerXml.oDadosNfe.dEmi);
                        if (!Directory.Exists(nomePastaBackup))
                        {
                            System.IO.Directory.CreateDirectory(nomePastaBackup);
                        }

                        //Se conseguiu criar a pasta ele move o arquivo, caso contrário
                        if (Directory.Exists(nomePastaBackup))
                        {
                            //Mover o arquivo da nota fiscal para a pasta de backup
                            var destinoBackup = Path.Combine(nomePastaBackup, Path.GetFileName(strArquivoNFe));
                            Functions.DeletarArquivo(destinoBackup);
                            File.Copy(strArquivoNFe, destinoBackup);
                        }
                        else
                        {
                            //throw new Exception("Pasta de backup informada nas configurações não existe. (Pasta: " + nomePastaBackup + ")");
                        }
                    }

                    // move o arquivo NFe para a pasta Denegada
                    File.Move(strArquivoNFe, dArquivo);
                }
                else
                {
                    // Como já existe na pasta Enviados\Denegados, só vou excluir da pasta EmProcessamento. Wandrey 22/12/2015
                    Functions.DeletarArquivo(strArquivoNFe);
                }
            }
            try
            {
                TFunctions.ExecutaUniDanfe(arqDen, oLerXml.oDadosNfe.dEmi, Empresas.Configuracoes[emp]);
            }
            catch (Exception ex)
            {
                Auxiliar.WriteLog("ProcessaDenegada: " + ex.Message, false);
            }
        }

        #endregion ProcessaNFeDenegada

        #region XmlRetorno()

        /// <summary>
        /// Auxiliar na geração do arquivo XML de retorno para o ERP quando estivermos utilizando o InvokeMember para chamar o método
        /// </summary>
        /// <param name="pFinalArqEnvio">Final do nome do arquivo de solicitação do serviço.</param>
        /// <param name="pFinalArqRetorno">Final do nome do arquivo que é para ser gravado o retorno.</param>
        /// <date>07/08/2009</date>
        /// <by>Wandrey Mundin Ferreira</by>
        ///
        /// NAO RENOMEAR ou EXCLUIR porque ela é acessada por Invoke
        ///
        public void XmlRetorno(string pFinalArqEnvio, string pFinalArqRetorno) => oGerarXML.XmlRetorno(pFinalArqEnvio, pFinalArqRetorno, vStrXmlRetorno);

        #endregion XmlRetorno()

        #region ValidaEvento

        protected void ValidaEvento(int emp, DadosenvEvento dadosEnvEvento)
        {
            var cErro = "";
            var currentEvento = dadosEnvEvento.eventos[0].tpEvento;
            var ctpEmis = dadosEnvEvento.eventos[0].chNFe.Substring(34, 1);
            foreach (var item in dadosEnvEvento.eventos)
            {
                if (!currentEvento.Equals(item.tpEvento))
                {
                    throw new Exception(string.Format("Não é possivel mesclar tipos de eventos dentro de um mesmo xml/txt de eventos. O tipo de evento neste xml/txt é {0}", currentEvento));
                }

                switch (NFe.Components.EnumHelper.StringToEnum<NFe.ConvertTxt.tpEventos>(currentEvento))
                {
                    case ConvertTxt.tpEventos.tpEvCancelamentoNFe:
                    case ConvertTxt.tpEventos.tpEvCancelamentoSubstituicaoNFCe:
                        if (!ctpEmis.Equals(item.chNFe.Substring(34, 1)))
                        {
                            cErro += "Não é possivel mesclar chaves com tipo de emissão dentro de um mesmo xml/txt de eventos.\r\n";
                        }

                        break;

                    case ConvertTxt.tpEventos.tpEvEPEC:
                        switch (Empresas.Configuracoes[emp].AmbienteCodigo)
                        {
                            case (int)TipoAmbiente.Homologacao:
                                if (Convert.ToInt32(item.tpAmb) == (int)TipoAmbiente.Producao)
                                {
                                    cErro += "Conteúdo da tag tpAmb do XML está com conteúdo indicando o envio para ambiente de produção e o UniNFe está configurado para ambiente de homologação.\r\n";
                                }
                                break;

                            case (int)TipoAmbiente.Producao:
                                if (Convert.ToInt32(item.tpAmb) == (int)TipoAmbiente.Homologacao)
                                {
                                    cErro += "Conteúdo da tag tpAmb do XML está com conteúdo indicando o envio para ambiente de homologação e o UniNFe está configurado para ambiente de produção.\r\n";
                                }
                                break;
                        }
                        var tpEmis = Convert.ToInt32(item.chNFe.Substring(34, 1));
                        if ((TipoEmissao)tpEmis != TipoEmissao.ContingenciaEPEC)
                        {
                            cErro += string.Format("Tipo de emissão no XML deve ser \"{0}\" (tpEmis={1}), mas está informado \"{2}\" (tpEmis={3}).\r\n",
                                         NFe.Components.EnumHelper.GetDescription((TipoEmissao)Enum.ToObject(typeof(TipoEmissao), (int)TipoEmissao.ContingenciaEPEC)),
                                         (int)TipoEmissao.ContingenciaEPEC,
                                         NFe.Components.EnumHelper.GetDescription((TipoEmissao)Enum.ToObject(typeof(TipoEmissao), tpEmis)),
                                         tpEmis);
                        }
                        if ((TipoEmissao)Empresas.Configuracoes[emp].tpEmis != TipoEmissao.ContingenciaEPEC)
                        {
                            cErro += string.Format("Tipo de emissão no Uninfe deve ser \"{0}\" (tpEmis={1}), mas está definido como \"{2}\" (tpEmis={3}).",
                                         NFe.Components.EnumHelper.GetDescription((TipoEmissao)Enum.ToObject(typeof(TipoEmissao), (int)TipoEmissao.ContingenciaEPEC)),
                                         (int)TipoEmissao.ContingenciaEPEC,
                                         NFe.Components.EnumHelper.GetDescription((TipoEmissao)Enum.ToObject(typeof(TipoEmissao), Empresas.Configuracoes[emp].tpEmis)),
                                         Empresas.Configuracoes[emp].tpEmis);
                        }
                        break;
                }
            }
            if (cErro != "")
            {
                throw new Exception(cErro);
            }
        }

        #endregion ValidaEvento

        #region PedSta

        protected virtual void PedSta(int emp, DadosPedSta dadosPedSta)
        {
            dadosPedSta.tpAmb = 0;
            dadosPedSta.cUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo;
            dadosPedSta.tpEmis = Empresas.Configuracoes[emp].tpEmis;
            dadosPedSta.versao = "";

            ///
            /// danasa 12-9-2009
            ///
            if (Path.GetExtension(NomeArquivoXML).ToLower() == ".txt")
            {
                // tpEmis|1						<<< opcional >>>
                // tpAmb|1
                // cUF|35
                // versao|3.10
                var cLinhas = Functions.LerArquivo(NomeArquivoXML);
                Functions.PopulateClasse(dadosPedSta, cLinhas);
            }
            else
            {
                var isMDFe = false;

                var consStatServList = ConteudoXML.GetElementsByTagName("consStatServCTe");
                if (consStatServList.Count == 0)
                {
                    consStatServList = ConteudoXML.GetElementsByTagName("consStatServMDFe");
                    if (consStatServList.Count == 0)
                    {
                        consStatServList = ConteudoXML.GetElementsByTagName("consStatServ");

                        if (consStatServList.Count == 0)
                        {
                            consStatServList = ConteudoXML.GetElementsByTagName("consStatServNF3e");

                            if (consStatServList.Count == 0)
                            {
                                consStatServList = ConteudoXML.GetElementsByTagName("consStatServNFCom");
                            }
                        }
                    }
                    else
                    {
                        isMDFe = true;
                    }
                }

                foreach (XmlNode consStatServNode in consStatServList)
                {
                    var consStatServElemento = (XmlElement)consStatServNode;

                    dadosPedSta.tpAmb = Convert.ToInt32("0" + consStatServElemento.GetElementsByTagName(TpcnResources.tpAmb.ToString())[0].InnerText);
                    dadosPedSta.versao = consStatServElemento.Attributes[TpcnResources.versao.ToString()].InnerText;

                    if (consStatServElemento.GetElementsByTagName(TpcnResources.cUF.ToString()).Count != 0)
                    {
                        dadosPedSta.cUF = Convert.ToInt32("0" + consStatServElemento.GetElementsByTagName(TpcnResources.cUF.ToString())[0].InnerText);

                        if (isMDFe)
                        {
                            // para que o validador não rejeite, excluo a tag <cUF>
                            ConteudoXML.DocumentElement.RemoveChild(consStatServElemento.GetElementsByTagName(TpcnResources.cUF.ToString())[0]);
                        }
                    }

                    if (consStatServElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString()).Count != 0)
                    {
                        dadosPedSta.tpEmis = Convert.ToInt16(consStatServElemento.GetElementsByTagName(NFe.Components.TpcnResources.tpEmis.ToString())[0].InnerText);

                        // para que o validador não rejeite, excluo a tag <tpEmis>
                        ConteudoXML.DocumentElement.RemoveChild(consStatServElemento.GetElementsByTagName(NFe.Components.TpcnResources.tpEmis.ToString())[0]);
                    }

                    if (consStatServElemento.GetElementsByTagName(TpcnResources.mod.ToString()).Count != 0)
                    {
                        dadosPedSta.mod = consStatServElemento.GetElementsByTagName(TpcnResources.mod.ToString())[0].InnerText;

                        /// para que o validador não rejeite, excluo a tag <mod>
                        ConteudoXML.DocumentElement.RemoveChild(consStatServElemento.GetElementsByTagName(TpcnResources.mod.ToString())[0]);
                    }
                }
            }
        }

        #endregion PedSta

        #region PedSit

        protected virtual void PedSit(int emp, DadosPedSit dadosPedSit)
        {
            dadosPedSit.tpAmb = Empresas.Configuracoes[emp].AmbienteCodigo;
            dadosPedSit.chNFe = string.Empty;

            var consSitNFeList = ConteudoXML.GetElementsByTagName("consSitCTe");
            if (consSitNFeList.Count == 0)
            {
                consSitNFeList = ConteudoXML.GetElementsByTagName("consSitMDFe");
            }
            foreach (XmlNode consSitNFeNode in consSitNFeList)
            {
                var consSitNFeElemento = (XmlElement)consSitNFeNode;

                dadosPedSit.versao = consSitNFeElemento.Attributes[TpcnResources.versao.ToString()].Value;
                dadosPedSit.tpAmb = Convert.ToInt32("0" + consSitNFeElemento.GetElementsByTagName(TpcnResources.tpAmb.ToString())[0].InnerText);
                dadosPedSit.chNFe = Functions.LerTag(consSitNFeElemento, TpcnResources.chCTe.ToString(), "") +
                                    Functions.LerTag(consSitNFeElemento, TpcnResources.chMDFe.ToString(), "");

                if (consSitNFeElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString()).Count != 0)
                {
                    dadosPedSit.tpEmis = Convert.ToInt16(consSitNFeElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString())[0].InnerText);

                    /// para que o validador não rejeite, excluo a tag <tpEmis>
                    ConteudoXML.DocumentElement.RemoveChild(consSitNFeElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString())[0]);
                }
            }
        }

        #endregion PedSit

        #region EnvEvento

        protected virtual void EnvEvento(int emp, DadosenvEvento dadosEnvEvento)
        {
            //<?xml version="1.0" encoding="UTF-8"?>
            //<envEvento versao="1.00" xmlns="http://www.portalfiscal.inf.br/nfe">
            //  <idLote>000000000015255</idLote>
            //  <evento versao="1.00" xmlns="http://www.portalfiscal.inf.br/nfe">
            //    <infEvento Id="ID1101103511031029073900013955001000000001105112804108">
            //      <cOrgao>35</cOrgao>
            //      <tpAmb>2</tpAmb>
            //      <CNPJ>10290739000139</CNPJ>
            //      <chNFe>35110310290739000139550010000000011051128041</chNFe>
            //      <dhEvento>2011-03-03T08:06:00-03:00</dhEvento>
            //      <tpEvento>110110</tpEvento>
            //      <nSeqEvento>8</nSeqEvento>
            //      <verEvento>1.00</verEvento>
            //      <detEvento versao="1.00">
            //          <descEvento>Carta de Correção</descEvento>
            //          <xCorrecao>Texto de teste para Carta de Correção. Conteúdo do campo xCorrecao.</xCorrecao>
            //          <xCondUso>A Carta de Correção é disciplinada pelo § 1º-A do art. 7º do Convênio S/N, de 15 de dezembro de 1970 e pode ser utilizada para regularização de erro ocorrido na emissão de documento fiscal, desde que o erro não esteja relacionado com: I - as variáveis que determinam o valor do imposto tais como: base de cálculo, alíquota, diferença de preço, quantidade, valor da operação ou da prestação; II - a correção de dados cadastrais que implique mudança do remetente ou do destinatário; III - a data de emissão ou de saída.</xCondUso>
            //      </detEvento>
            //    </infEvento>
            //  </evento>
            //</envEvento>

            var doSave = false;

            var envEventoList = ConteudoXML.GetElementsByTagName("infEvento");

            foreach (XmlNode envEventoNode in envEventoList)
            {
                var envEventoElemento = (XmlElement)envEventoNode;

                dadosEnvEvento.eventos.Add(new Evento()
                {
                    tpEvento = Functions.LerTag(envEventoElemento, TpcnResources.tpEvento.ToString(), false),
                    tpAmb = Convert.ToInt32("0" + Functions.LerTag(envEventoElemento, TpcnResources.tpAmb.ToString(), false)),
                    cOrgao = Convert.ToInt32("0" + Functions.LerTag(envEventoElemento, TpcnResources.cOrgao.ToString(), false)),
                    nSeqEvento = Convert.ToInt32("0" + Functions.LerTag(envEventoElemento, TpcnResources.nSeqEvento.ToString(), false))
                });
                dadosEnvEvento.eventos[dadosEnvEvento.eventos.Count - 1].chNFe =
                    Functions.LerTag(envEventoElemento, TpcnResources.chNFe.ToString(), "") +
                    Functions.LerTag(envEventoElemento, TpcnResources.chMDFe.ToString(), "") +
                    Functions.LerTag(envEventoElemento, TpcnResources.chCTe.ToString(), "");

                dadosEnvEvento.eventos[dadosEnvEvento.eventos.Count - 1].tpEmis =
                    Convert.ToInt16(dadosEnvEvento.eventos[dadosEnvEvento.eventos.Count - 1].chNFe.Substring(34, 1));

                if (envEventoElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString()).Count != 0)
                {
                    var node = envEventoElemento.GetElementsByTagName(TpcnResources.tpEmis.ToString())[0];

                    dadosEnvEvento.eventos[dadosEnvEvento.eventos.Count - 1].tpEmis = Convert.ToInt16("0" + node.InnerText);

                    /// para que o validador não rejeite, excluo a tag <tpEmis>
                    envEventoNode.RemoveChild(node);
                    doSave = true;
                }
            }

            /// salvo o arquivo modificado
            if (doSave)
            {
                ConteudoXML.Save(NomeArquivoXML);
            }
        }

        #endregion EnvEvento

        /// <summary>
        /// Busca arquivos na pasta temp do envio individual ou em lote, comparando com a chave para descobrir o nome original do arquivo enviado.
        /// </summary>
        /// <param name="pasta">Pasta que é para procurar o arquivo</param>
        /// <param name="chave">Chave do DFe</param>
        /// <param name="tagID">Nome da tag que tem o ID/Chave do DFe</param>
        /// <param name="tagPrincipal">Nome da tag Principal do XML (CTe, NFe, MDFe)</param>
        /// <returns>Nome do arquivo enviado</returns>
        protected string NomeArquivoXMLTemp(string pasta, string chave, string tagPrincipal, string tagID)
        {
            var nomeArqNFe = string.Empty;

            var dirInfo = new DirectoryInfo(pasta);
            foreach (var file in dirInfo.GetFiles())
            {
                var achou = false;
                var doc = new XmlDocument();
                doc.Load(Path.Combine(file.FullName));

                var nodeListNFe = ConteudoXML.GetElementsByTagName(tagPrincipal);

                foreach (var nodeNFe in nodeListNFe)
                {
                    var xmlElementNFe = (XmlElement)nodeNFe;
                    if ((XmlElement)xmlElementNFe.GetElementsByTagName(tagID)[0] != null)
                    {
                        if (((XmlElement)xmlElementNFe.GetElementsByTagName(tagID)[0]).GetAttribute("Id") == chave)
                        {
                            nomeArqNFe = file.Name;
                            achou = true;
                        }

                        break;
                    }
                }

                if (achou)
                {
                    break;
                }
            }

            return nomeArqNFe;
        }

        /// <summary>
        /// Remove o arquivo XML da pasta temp da pasta de envio
        /// </summary>
        /// <param name="strArquivoNFe">FullPath do arquivo que está na pasta EmProcessamento</param>
        /// <param name="emp">Empresa</param>
        protected void RemoverArqTemp(string arquivoDFe, int emp)
        {
            var arqTemp = Path.Combine(Empresas.Configuracoes[emp].PastaXmlEnvio + "\\temp", Path.GetFileName(arquivoDFe));

            if (File.Exists(arqTemp))
            {
                try
                {
                    File.Delete(arqTemp);
                }
                catch (Exception ex)
                {
                    Auxiliar.WriteLog("Erro ao excluir o arquivo da nota na pasta temporário: " + arqTemp + " - " + ex.Message, true);
                }
            }
        }

    }
}