
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NFe.Components
{
    #region SubPastas da pasta de enviados

    /// <summary>
    /// SubPastas da pasta de XML´s enviados para os webservices
    /// </summary>
    public enum PastaEnviados
    {
        EmProcessamento,
        Autorizados,
        Denegados,
        Originais
    }

    #endregion SubPastas da pasta de enviados

    #region Servicos

    /// <summary>
    /// Serviços executados pelo Aplicativo
    /// </summary>
    public enum Servicos
    {
        #region NFe

        /// <summary>
        /// Consulta status serviço NFe
        /// </summary>
        NFeConsultaStatusServico,

        /// <summary>
        /// Somente converter TXT da NFe para XML de NFe
        /// </summary>
        NFeConverterTXTparaXML,

        /// <summary>
        /// Envia os lotes de NFe para os webservices
        /// </summary>
        NFeEnviarLote,

        /// <summary>
        /// Envia XML de Inutilização da NFe
        /// </summary>
        NFeInutilizarNumeros,

        /// <summary>
        /// Assinar e montar lote de uma NFe
        /// </summary>
        NFeMontarLoteUma,

        /// <summary>
        /// Consulta situação da NFe
        /// </summary>
        NFePedidoConsultaSituacao,

        /// <summary>
        /// Consulta recibo do lote nfe
        /// </summary>
        NFePedidoSituacaoLote,

        #region Eventos NFe

        /// <summary>
        /// Enviar XML Evento - Cancelamento
        /// </summary>
        EventoCancelamento,

        /// <summary>
        /// Enviar XML Evento - Carta de Correção
        /// </summary>
        EventoCCe,

        /// <summary>
        /// Enviar um evento de EPEC
        /// </summary>
        EventoEPEC,

        /// <summary>
        /// Enviar um evento de manifestação
        /// </summary>
        EventoManifestacaoDest,

        /// <summary>
        /// Enviar XML de Evento NFe
        /// </summary>
        EventoRecepcao,

        /// <summary>
        /// Enviar XML de Evento do Comprovante de Entrega da NFe
        /// </summary>
        EventoCompEntregaNFe,

        /// <summary>
        /// Enviar XML de Evento de Cancelamento do Comprovante de Entrega da NFe
        /// </summary>
        EventoCancCompEntregaNFe,

        /// <summary>
        /// Enviar XML de Evento de Conciliação Financeira da NFe/NFCe
        /// </summary>
        EventoConciliacaoFinanceiraNFe,

        /// <summary>
        /// Enviar XML de Evento de Cancelamento do Evento de Conciliação Financeira da NFe/NFCe
        /// </summary>
        EventoCancelamentoConciliacaoFinanceiraNFe,

        #endregion Eventos NFe

        /// <summary>
        /// Assinar e validar um XML de NFe no envio em Lote
        /// </summary>
        NFeAssinarValidarEnvioEmLote,

        /// <summary>
        /// Monta chave de acesso
        /// </summary>
        NFeGerarChave,

        /// <summary>
        /// Assinar e montar lote de várias NFe
        /// </summary>
        NFeMontarLoteVarias,

        #endregion NFe

        #region NFCe

        /// <summary>
        /// Download de XML da NFCe
        /// </summary>
        NFCeDownloadXML,

        /// <summary>
        /// Consultar Lista de Chaves da NFCe
        /// </summary>
        NFCeConsultaChaves,

        #endregion NFCE

        #region CTe

        /// <summary>
        /// Assinar e validar um XML de CTe no envio em Lote
        /// </summary>
        CTeAssinarValidarEnvioEmLote,

        /// <summary>
        /// Consulta Status Serviço CTe
        /// </summary>
        CTeConsultaStatusServico,

        /// <summary>
        /// Envia o CTe para os webservices Sincrono
        /// </summary>
        CTeEnviarSinc,

        /// <summary>
        /// Envia o CTe Simplificado para os webservices Sincrono
        /// </summary>
        CTeEnviarSimp,


        /// <summary>
        /// Consulta situação da CTe
        /// </summary>
        CTePedidoConsultaSituacao,

        /// <summary>
        /// Enviar XML Evento CTe
        /// </summary>
        CTeRecepcaoEvento,

        /// <summary>
        /// Enviar XML de distribuição de DFe de interesses de autores (CTe)
        /// </summary>
        CTeDistribuicaoDFe,

        /// <summary>
        /// Enviar XML de CTe modelo 67
        /// </summary>
        CteRecepcaoOS,

        #endregion CTe

        #region NFSe

        /// <summary>
        /// Cancelar NFS-e
        /// </summary>
        [Description("Cancelar NFS-e")]
        NFSeCancelar,

        /// <summary>
        /// Consultar NFS-e por Data
        /// </summary>
        [Description("Consultar NFS-e por Data")]
        NFSeConsultar,

        /// <summary>
        /// Consultar lote RPS
        /// </summary>
        [Description("ConsultarLoteRPS")]
        NFSeConsultarLoteRps,

        /// <summary>
        /// Consultar NFS-e por RPS
        /// </summary>
        [Description("Consultar NFS-e por RPS")]
        NFSeConsultarPorRps,

        /// <summary>
        /// Consultar Situação do lote RPS NFS-e
        /// </summary>
        [Description("Consultar Situação do lote RPS NFS-e")]
        NFSeConsultarSituacaoLoteRps,

        /// <summary>
        /// Consultar a URL de visualização da NFSe
        /// </summary>
        [Description("Consultar a URL de Visualização da NFS-e")]
        NFSeConsultarURL,

        /// <summary>
        /// Consultar a URL de visualização da NFSe
        /// </summary>
        [Description("Consultar a URL de Visualização da NFS-e com a Série")]
        NFSeConsultarURLSerie,

        /// <summary>
        /// Enviar Lote RPS NFS-e
        /// </summary>
        [Description("Enviar Lote RPS NFS-e ")]
        NFSeRecepcionarLoteRps,

        /// <summary>
        /// Enviar Lote RPS NFS-e de forma sincrona
        /// Criado inicialmente para ser utilizado para o padrão BHIss, pois é necessario utilizar a recepção de lote das duas formas.
        /// </summary>
        [Description("Enviar Lote RPS NFS-e Sincrono")]
        NFSeRecepcionarLoteRpsSincrono,

        /// <summary>
        /// Enviar Lote RPS NFS-e de forma sincrona
        /// Criado inicialmente para ser utilizado para o padrão BHIss, pois é necessario utilizar a recepção de lote das duas formas.
        /// </summary>
        [Description("Enviar Lote RPS NFS-e Sincrono")]
        NFSeGerarNfse,

        /// <summary>
        /// Consulta da imagem de uma NFS-e em formato PNG
        /// Criado inicialmente para ser utilizado para o padrão INFISC para a Prefeitura de Caxias do Sul - RS
        /// </summary>
        [Description("Consulta da imagem de uma NFS-e em formato PNG")]
        NFSeConsultarNFSePNG,

        /// <summary>
        /// Consulta da imagem de uma NFS-e em formato
        /// Criado inicialmente para ser utilizado para o padrão INFISC para a Prefeitura de Caxias do Sul - RS
        /// </summary>
        [Description("Inutilização de uma NFS-e")]
        NFSeInutilizarNFSe,

        /// <summary>
        /// Consulta da imagem de uma NFS-e em formato PDF
        /// Criado inicialmente para ser utilizado para o padrão INFISC para a Prefeitura de Caxias do Sul - RS
        /// </summary>
        [Description("Consulta da imagem de uma NFS-e em formato PDF")]
        NFSeConsultarNFSePDF,

        /// <summary>
        /// Baixar o XML da NFSe
        /// </summary>
        [Description("Obter o XML da NFS-e")]
        NFSeObterNotaFiscal,

        /// <summary>
        /// Consulta Sequencia do Lote da Nota RPS
        /// </summary>
        [Description("Consulta Sequencia do Lote da Nota RPS")]
        NFSeConsultaSequenciaLoteNotaRPS,

        /// <summary>
        /// Substituir NFS-e
        /// </summary>
        [Description("Substituir NFS-e")]
        NFSeSubstituirNfse,

        /// <summary>
        /// Consultar Status NFS-e
        /// </summary>
        [Description("Consultar Status da NFS-e")]
        NFSeConsultarStatusNota,

        // <summary>
        /// Consultar as notas fiscais de serviço recebidas
        /// </summary>
        [Description("Consultar NFS-e recebidas")]
        NFSeConsultarNFSeRecebidas,

        /// <summary>
        /// Consultar as notas fiscais de serviço tomados
        /// </summary>
        [Description("Consultar NFS-e tomados")]
        NFSeConsultarNFSeTomados,

        // <summary>
        /// Consultar as notas fiscais de serviço emitidas
        /// </summary>
        [Description("Consultar NFS-e emitidas")]
        NFSeConsultarNFSeEmitidas,

        // <summary>
        /// Consultar a situacaodo pedido de cancelamento
        /// </summary>
        [Description("Consultar Situacaodo Cancelamento da NFS-e")]
        NFSeConsultarSituacaoCancelamento,

        ///<summary>
        ///Consultar Convênio Municipal da NFSe NACIONAL
        /// </summary>
        [Description("Consultar Convênio Municipal da NFSe NACIONAL")]
        NFSeConsultarConvenioMunicipal,

        ///<summary>
        ///Consultar Alíquotas Municipais da NFSe NACIONAL
        /// </summary>
        [Description("Consultar Alíquotas Municipais da NFSe NACIONAL")]
        NFSeConsultarAliquotasMunicipais,

        ///<summary>
        ///Consultar Histórico de Alíquotas Municipais da NFSe NACIONAL
        /// </summary>
        [Description("Consultar Histórico de Alíquotas Municipais da NFSe NACIONAL")]
        NFSeConsultarHistoricoAliquotasMunicipais,

        ///<summary>
        ///Consultar Regimes Especiais Municipais da NFSe NACIONAL
        /// </summary>
        [Description("Consultar Regimes Especiais Municipais da NFSe NACIONAL")]
        NFSeConsultarRegimesEspeciaisMunicipais,

        ///<summary>
        ///Consultar Retenções Municipais da NFSe NACIONAL
        /// </summary>
        [Description("Consultar Retenções Municipais da NFSe NACIONAL")]
        NFSeConsultarRetencoesMunicipais,

        ///<summary>
        ///Consultar Benefícios Municipais da NFSe NACIONAL
        /// </summary>
        [Description("Consultar Benefícios Municipais da NFSe NACIONAL")]
        NFSeConsultarBeneficioMunicipal,

        ///<summary>
        ///Recepcionar Eventos Diversos da NFSe NACIONAL
        /// </summary>
        [Description("Recepcionar Eventos Diversos da NFSe NACIONAL")]
        NFSeRecepcionarEventosDiversos,

        ///<summary>
        ///Consutlar Eventos Diversos da NFSe NACIONAL
        /// </summary>
        [Description("Consutlar Eventos Diversos da NFSe NACIONAL")]
        NFSeConsultarEventosDiversos,

        #endregion NFSe

        #region CFSe

        /// <summary>
        /// Enviar Lote CFS-e
        /// </summary>
        [Description("Enviar Lote CFS-e")]
        RecepcionarLoteCfse,

        /// <summary>
        /// Enviar Lote CFS-e Sincrono
        /// </summary>
        [Description("Enviar Lote CFS-e")]
        RecepcionarLoteCfseSincrono,

        /// <summary>
        /// Cancelar CFS-e
        /// </summary>
        [Description("Enviar Cancelamento CFS-e")]
        CancelarCfse,

        /// <summary>
        /// Consultar Lote CFS-e
        /// </summary>
        [Description("Enviar consulta do lote CFS-e")]
        ConsultarLoteCfse,

        /// <summary>
        /// Consultar CFS-e
        /// </summary>
        [Description("Enviar consulta do CFS-e")]
        ConsultarCfse,

        /// <summary>
        /// Configurar/Ativar Terminal CFS-e
        /// </summary>
        [Description("Enviar XML de configuração/ativação de terminal CFS-e")]
        ConfigurarTerminalCfse,

        /// <summary>
        /// Informar terminal CFS-e em manutenção
        /// </summary>
        [Description("Enviar XML para informar que o terminal de CFS-e está em manutenção")]
        EnviarInformeManutencaoCfse,

        /// <summary>
        /// Informar data sem movimento de CFS-e
        /// </summary>
        [Description("Enviar XML para informar que não teve movimento de CFS-e no dia")]
        InformeTrasmissaoSemMovimentoCfse,

        /// <summary>
        /// Consulta dados cadastro terminal CFS-e
        /// </summary>
        [Description("Enviar XML para consultar dados cadastros terminal CFS-e")]
        ConsultarDadosCadastroCfse,

        #endregion CFSe

        #region MDFe

        /// <summary>
        /// Assinar e validar um XML de MDFe no envio em Lote
        /// </summary>
        MDFeAssinarValidarEnvioEmLote,

        /// <summary>
        /// Consulta MDFe nao encerrados
        /// </summary>
        MDFeConsultaNaoEncerrado,

        /// <summary>
        /// Consulta Status Serviço MDFe
        /// </summary>
        MDFeConsultaStatusServico,

        /// <summary>
        /// Envia o MDFe para os webservices Sincrono
        /// </summary>
        MDFeEnviarSinc,

        /// <summary>
        /// Consulta situação da MDFe
        /// </summary>
        MDFePedidoConsultaSituacao,

        /// <summary>
        /// Enviar XML Evento MDFe
        /// </summary>
        MDFeRecepcaoEvento,

        #endregion MDFe

        #region Serviços em comum NFe, CTe, MDFe e NFSe

        /// <summary>
        /// Valida e envia o XML de pedido de Consulta do Cadastro do Contribuinte para o webservice
        /// </summary>
        ConsultaCadastroContribuinte,

        /// <summary>
        /// Efetua verificações nas notas em processamento para evitar algumas falhas e perder retornos de autorização de notas
        /// </summary>
        EmProcessamento,

        /// <summary>
        /// Somente assinar e validar o XML
        /// </summary>
        AssinarValidar,

        #endregion Serviços em comum NFe, CTe, MDFe e NFSe

        #region Serviços gerais

        /// <summary>
        /// Consultar Informações Gerais do UniNFe
        /// </summary>
        UniNFeConsultaInformacoes,

        /// <summary>
        /// Solicitar ao UniNFe que altere suas configurações
        /// </summary>
        UniNFeAlterarConfiguracoes,

        /// <summary>
        /// Efetua uma limpeza das pastas que recebem arquivos temporários
        /// </summary>
        UniNFeLimpezaTemporario,

        /// <summary>
        /// Consultas efetuadas pela pasta GERAL.
        /// </summary>
        UniNFeConsultaGeral,

        /// <summary>
        /// Consulta Certificados Instalados na estação do UniNFe.
        /// </summary>
        UniNFeConsultaCertificados,

        /// <summary>
        /// Força atualizar o aplicativo UniNFe
        /// </summary>
        UniNFeUpdate,

        #endregion Serviços gerais

        #region Impressao do DANFE

        DANFEImpressao,
        DANFEImpressao_Contingencia,

        #endregion Impressao do DANFE

        #region Impressao do relatorio de e-mails do DANFE

        DANFERelatorio,

        #endregion Impressao do relatorio de e-mails do DANFE

        DFeEnviar,

        #region CCG

        /// <summary>
        /// /// Consulta Centralizada de Código GTIN (CCG)
        /// </summary>
        CCGConsGTIN,

        #endregion

        #region EFDReinf

        RecepcaoLoteReinf,
        ConsultasReinf,
        ConsultaLoteAssincReinf,
        ConsultaFechamento2099Reinf,

        #endregion EFDReinf

        #region eSocial

        RecepcaoLoteeSocial,
        ConsultarLoteeSocial,
        ConsultarIdentificadoresEventoseSocial,
        DownloadEventoseSocial,

        #endregion eSocial

        #region GNRE

        /// <summary>
        /// Envio da consulta do resultado do lote da GNRE pelo número do recibo
        /// </summary>
        ConsultaResultadoLoteGNRE,

        /// <summary>
        /// Envio do lote da GNRE
        /// </summary>
        LoteRecepcaoGNRE,

        /// <summary>
        /// Envio da consulta das configurações UF da GNRE
        /// </summary>
        ConsultaConfigUfGNRE,

        #endregion

        #region PIX

        /// <summary>
        /// Sinalizar recebimento com PIX para o banco
        /// </summary>
        PIXCobrancaCreateRequest,

        /// <summary>
        /// Consultar PIX para ver se já foi ou não recebido (TxId)
        /// </summary>
        PIXGetRequest,

        /// <summary>
        /// Consultar extrato de PIX recebidos
        /// </summary>
        PIXConsultaRequest,

        /// <summary>
        /// uMessenger
        /// </summary>
        UMessenger,

        #endregion

        #region eBoleto

        BoletoRegistrar,
        BoletoCancelar,
        BoletoConsultar,
        BoletoAlterarVencto,
        BoletoEnviarInstrucao,
        BoletoInformarPagto,

        #endregion

        /// <summary>
        /// Enviar arquivos para FTP
        /// </summary>
        EnviarFTP,

        #region DARE

        RecepcaoDARE,
        ConsultaReceitasDARE,

        #endregion DARE

        #region NF3e

        /// <summary>
        /// Consulta status serviço NF3e
        /// </summary>
        NF3eStatusServico,

        /// <summary>
        /// Consulta protocolo da NF3e
        /// </summary>
        NF3eConsultaProtocolo,

        /// <summary>
        /// Consulta recibo NF3e
        /// </summary>
        NF3eConsultaRecibo,

        /// <summary>
        /// Envio de Eventos da NF3e
        /// </summary>
        NF3eRecepcaoEvento,

        /// <summary>
        /// Envio do XML de NF3e
        /// </summary>
        NF3eAutorizacaoSinc,

        #endregion NF3e

        #region NFCom

        /// <summary>
        /// Consulta procolo da NFCom
        /// </summary>
        NFComConsultaProtocolo,

        /// <summary>
        /// Consulta status serviço NFCom
        /// </summary>
        NFComStatusServico,

        /// <summary>
        /// Envio de Eventos da NFCom
        /// </summary>
        NFComRecepcaoEvento,

        /// <summary>
        /// Envio do XML de NFCom
        /// </summary>
        NFComAutorizacaoSinc,


        #endregion NFCom

        /// <summary>
        /// Nulo / Nenhum serviço em execução
        /// </summary>
        Nulo
    }

    #endregion Servicos

    #region TipoAplicativo

    public enum TipoAplicativo
    {
        /// <summary>
        /// Aplicativo ou serviços para processamento dos XMLs da NF-e, NFC-e, GNRE e DARE
        /// </summary>
        [Description("NF-e, NFC-e, GNRE e DARE")]
        Nfe = 0,

        /// <summary>
        /// Aplicativo ou serviços para processamento dos XMLs do CT-e, GNRE e DARE
        /// </summary>
        [Description("CT-e, GNRE e DARE")]
        Cte = 1,

        /// <summary>
        /// Aplicativo ou servicos para processamento dos XMLs da NFS-e
        /// </summary>
        [Description("NFS-e")]
        Nfse = 2,

        /// <summary>
        /// Aplicativo ou serviços para processamento dos XMLs do MDF-e
        /// </summary>
        [Description("MDF-e")]
        MDFe = 3,

        /// <summary>
        /// Aplicativo ou serviços para processamento dos XMLs da NFC-e
        /// </summary>
        [Description("NFC-e")]
        NFCe = 4,

        /// <summary>
        /// Aplicativo ou serviços para processamento dos XMLs do EFD Reinf
        /// </summary>
        [Description("EFD Reinf")]
        EFDReinf = 6,

        /// <summary>
        /// Aplicativo ou serviços para processamento dos XMLs do eSocial
        /// </summary>
        [Description("eSocial")]
        eSocial = 7,

        /// <summary>
        /// Aplicativo ou seviços para processamento dos XMLs de EFD Reinf e eSocial
        /// </summary>
        [Description("EFD Reinf e eSocial")]
        EFDReinfeSocial = 8,

        /// <summary>
        /// Aplicativo ou serviços para processamento dos XMLs da GNRE e do DARE
        /// </summary>
        [Description("GNRE e DARE")]
        GNREeDARE = 9,

        /// <summary>
        /// Aplicativo ou serviços para processamentos dos XMLs de todos os DFEs (exceto NFSe)
        /// </summary>
        [Description("NF-e, NFC-e, NF3-e, NFCom, CT-e, MDF-e, GNRE, DARE e EFD Reinf e eSocial")]
        Todos = 10,

        /// <summary>
        /// Aplicativo ou serviços para processamentos dos XMLs da NF3e
        /// </summary>
        [Description("NF3-e")]
        NF3e = 11,

        /// <summary>
        /// Aplicativo ou serviços para processamentos dos XMLs da NFCom
        /// </summary>
        [Description("NFCom")]
        NFCom = 12,

        [Description("")]
        Nulo = 100
    }

    #endregion TipoAplicativo

    /// <summary>
    /// Regime tributação ISSQN
    /// </summary>
    public enum RegTribISSQN
    {
        [Description("1 - Micro Empresa Municipal")]
        MicroEmpresaMunicipal = 1,

        [Description("2 - Estimativa")]
        Estimativa = 2,

        [Description("3 - Sociedade de Profissionais")]
        SociedadeDeProfissionais = 3,

        [Description("4 - Cooperativa")]
        Cooperativa = 4,

        [Description("5 - Micro Empresário Individual (MEI)")]
        MicroEmpresarioIndividual = 5
    }

    /// <summary>
    /// Informa se o Desconto sobre
    /// subtotal deve ser rateado entre
    /// os itens sujeitos à tributação pelo ISSQN.
    /// </summary>
    public enum IndRatISSQN
    {
        [Description("Sim")]
        S,

        [Description("Não")]
        N
    }

    #region Erros Padrões

    public enum ErroPadrao
    {
        ErroNaoDetectado = 0,
        FalhaInternet = 1,
        FalhaEnvioXmlWS = 2,
        CertificadoVencido = 3,
        FalhaEnvioXmlNFeWS = 5,
        CertificadoNaoEncontrado = 6,
        ValidarXML = 7
    }

    #endregion Erros Padrões

    #region EnumHelper

    /*
ComboBox combo = new ComboBox();
combo.DataSource = EnumHelper.ToList(typeof(SimpleEnum));
combo.DisplayMember = "Value";
combo.ValueMember = "Key";

        foreach (string value in Enum.GetNames(typeof(Model.TipoCampanhaSituacao)))
        {
            Model.TipoCampanhaSituacao stausEnum = (Model.TipoCampanhaSituacao)Enum.Parse(typeof(Model.TipoCampanhaSituacao), value);
            Console.WriteLine(" Description: " + value+"  "+ Model.EnumHelper.GetDescription(stausEnum));
        }

 */

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class AttributeTipoAplicacao : Attribute
    {
        private TipoAplicativo aplicacao;

        public TipoAplicativo Aplicacao
        {
            get
            {
                return this.aplicacao;
            }
        }

        public AttributeTipoAplicacao(TipoAplicativo aplicacao)
            : base()
        {
            this.aplicacao = aplicacao;
        }
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EnumDescriptionAttribute : Attribute
    {
        private string description;

        public string Description
        {
            get
            {
                return this.description;
            }
        }

        public EnumDescriptionAttribute(string description)
            : base()
        {
            this.description = description;
        }
    }

    /// <summary>
    /// Classe com metodos para serem utilizadas nos Enuns
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Retorna a description do enum
        /// </summary>
        /// <param name="value">Enum para buscar a description</param>
        /// <returns>Retorna a description do enun</returns>
        /*public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }*/

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name, true);
        }

        /// <summary>
        /// Gets the <see cref="DescriptionAttribute"/> of an <see cref="Enum"/> type value.
        /// </summary>
        /// <param name="value">The <see cref="Enum"/> type value.</param>
        /// <returns>A string containing the text of the <see cref="DescriptionAttribute"/>.</returns>
        public static string GetDescription(Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string description = value.ToString();
            FieldInfo fieldInfo = value.GetType().GetField(description);
            EnumDescriptionAttribute[] attributes = (EnumDescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                description = attributes[0].Description;
            }
            else
            {
                return GetEnumItemDescription(value);
                //DescriptionAttribute[] dattributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                //if (dattributes != null && dattributes.Length > 0)
                //description = dattributes[0].Description;
            }
            return description;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static string GetEnumItemDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        ///  Converts the <see cref="Enum"/> type to an <see cref="IList"/> compatible object.
        /// </summary>
        /// <param name="type">The <see cref="Enum"/> type.</param>
        /// <returns>An <see cref="IList"/> containing the enumerated type value and description.</returns>
        public static IList ToList(Type type, bool returnInt, bool excluibrancos)
        {
            return ToList(type, returnInt, excluibrancos, "");
        }

        public static IList ToList(Type type, bool returnInt, bool excluibrancos, string eliminar)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            ArrayList list = new ArrayList();
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                string _descr = GetDescription(value);
                if (excluibrancos && string.IsNullOrEmpty(_descr)) continue;

                if (eliminar.IndexOf(Convert.ToInt32(value).ToString()) != -1) continue;

                if (returnInt)
                    list.Add(new KeyValuePair<int, string>(Convert.ToInt32(value), _descr));
                else
                    list.Add(new KeyValuePair<Enum, string>(value, _descr));
            }

            return list;
        }

        public static IList ToStrings(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            ArrayList list = new ArrayList();
            Array enumValues = Enum.GetValues(type);

            foreach (Enum value in enumValues)
            {
                list.Add(GetDescription(value));
            }

            return list;
        }
    }

    #endregion EnumHelper
}