using Newtonsoft.Json.Linq;
using NFe.Components;
using NFe.Settings;
using System;
using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using Unimake.Business.DFe.Servicos;
namespace NFe.UI.Formularios
{
    [ToolboxItem(false)]
    public partial class userConfiguracao_diversos : MetroFramework.Controls.MetroUserControl
    {
        private ArrayList arrServico = new ArrayList();
        private Empresa empresa;
        private TipoAplicativo servicoCurrent;
        private bool loading;
        private string cnpjCurrent = "";

        public event EventHandler changeEvent;

        public userConfiguracoes uConfiguracoes;
        private ArrayList arrUF, arrMunicipios;

        public userConfiguracao_diversos()
        {
            InitializeComponent();

            loading = true;

            if (!DesignMode)
            {
                cbServico.SelectedIndexChanged -= cbServico_SelectedIndexChanged;
                servicoCurrent = TipoAplicativo.Nulo;

                #region Montar Array DropList da UF

                try
                {
                    arrUF = Functions.CarregaEstados();
                    arrMunicipios = Functions.CarregarMunicipios();
                }
                catch (Exception ex)
                {
                    MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, ex.Message, "");
                }

                #endregion Montar Array DropList da UF

                #region Montar Array DropList do Ambiente

                comboBox_Ambiente.DataSource = EnumHelper.ToList(typeof(TipoAmbiente), true, true);
                comboBox_Ambiente.DisplayMember = "Value";
                comboBox_Ambiente.ValueMember = "Key";
                #endregion Montar Array DropList do Ambiente

                #region Montar array DropList dos tipos de serviços

                cbServico.DataSource = uninfeDummy.DatasouceTipoAplicativo(false);
                cbServico.DisplayMember = "Value";
                cbServico.ValueMember = "Key";
                #endregion Montar array DropList dos tipos de serviços

                #region Montar Array DropList do Tipo de Emissão da NF-e

                comboBox_tpEmis.DataSource = EnumHelper.ToList(typeof(TipoEmissao), true, true);
                comboBox_tpEmis.DisplayMember = "Value";
                comboBox_tpEmis.ValueMember = "Key";
                #endregion Montar Array DropList do Tipo de Emissão da NF-e

                cbServico.SelectedIndexChanged += cbServico_SelectedIndexChanged;
            }
        }

        public void Populate(Empresa empresa, bool novaempresa)
        {
            loading = true;
            try
            {
                uninfeDummy.ClearControls(this, true, false);

                this.empresa = empresa;

                Configurar(empresa, novaempresa);

                if (empresa.Servico == TipoAplicativo.Nfse)
                {
                    comboBox_UF.DataSource = arrMunicipios;
                }
                else
                {
                    comboBox_UF.DataSource = arrUF;
                }

                comboBox_UF.DisplayMember = NFeStrConstants.Nome;
                comboBox_UF.ValueMember = "Codigo";

                cnpjCurrent = edtCNPJ.Text = empresa.CNPJ;
                edtNome.Text = empresa.Nome;

                if (!string.IsNullOrEmpty(empresa.Documento))
                {
                    if (empresa.Documento.Equals("CPF"))
                    {
                        var temp = (edtCNPJ.Text.ToString().Substring(11, edtCNPJ.Text.Length - 11)).Trim();
                        if (!string.IsNullOrWhiteSpace(temp))
                        {
                            temp = temp.Insert(0, " - ");
                            edtCNPJ.Text = edtCNPJ.Text.Substring(0, 11);
                        }

                        edtCNPJ.Text = ((CPF)edtCNPJ.Text).ToString();

                        if (!string.IsNullOrWhiteSpace(temp))
                        {
                            edtCNPJ.Text += temp;
                        }

                    }
                    else if (empresa.Documento.Equals("CEI"))
                    {
                        edtCNPJ.Text = ((CEI)edtCNPJ.Text).ToString();
                    }
                    else if (empresa.Documento.Equals("CAEPF"))
                    {
                        edtCNPJ.Text = Convert.ToInt64(edtCNPJ.Text).ToString(@"000\.000\.000\/000\-00");
                    }
                    else
                    {
                        edtCNPJ.Text = ((CNPJ)edtCNPJ.Text).ToString();
                    }
                }
                else
                {
                    if (empresa?.CNPJ?.Length == 11)
                    {
                        edtCNPJ.Text = ((CPF)edtCNPJ.Text).ToString();
                    }
                    else if (empresa?.CNPJ?.Length == 12)
                    {
                        edtCNPJ.Text = ((CEI)edtCNPJ.Text).ToString();
                    }
                    else
                    {
                        edtCNPJ.Text = ((CNPJ)edtCNPJ.Text).ToString();
                    }
                }

                comboBox_tpEmis.SelectedValue = this.empresa.tpEmis;
                comboBox_Ambiente.SelectedValue = this.empresa.AmbienteCodigo;
                comboBox_UF.SelectedValue = this.empresa.UnidadeFederativaCodigo;
                cbServico.SelectedValue = (int)this.empresa.Servico;

                if (empresa.Servico == TipoAplicativo.Nfse && this.empresa.UnidadeFederativaCodigo == 0)
                {
                    comboBox_UF.SelectedIndex = 0;
                }

                checkBoxRetornoNFETxt.Checked = this.empresa.GravarRetornoTXTNFe;
                checkBoxGravarEventosDeTerceiros.Checked = this.empresa.GravarEventosDeTerceiros;
                checkBoxGravarEventosNaPastaEnviadosNFe.Checked = this.empresa.GravarEventosNaPastaEnviadosNFe;
                checkBoxGravarEventosCancelamentoNaPastaEnviadosNFe.Checked = this.empresa.GravarEventosCancelamentoNaPastaEnviadosNFe;
                checkBoxArqNSU.Checked = this.empresa.ArqNSU;
                checkBoxRpsSincAssincTHEMA.Checked = this.empresa.RpsSincAssincTHEMA;

                edtIdentificadorCSC.Text = this.empresa.IdentificadorCSC;
                edtTokenCSC.Text = this.empresa.TokenCSC;

                cboDiretorioSalvarComo.Text = this.empresa.DiretorioSalvarComo;
                udDiasLimpeza.Text = this.empresa.DiasLimpeza.ToString();
                udTempoConsulta.Text = this.empresa.TempoConsulta.ToString();

                txtSenhaWS.Text = this.empresa.SenhaWS;
                txtUsuarioWS.Text = this.empresa.UsuarioWS;

                if (empresa.UnidadeFederativaCodigo.Equals(4205407))
                {
                    var result = empresa.RecuperarConfiguracaoNFSeSoftplan(empresa.CNPJ);

                    txtClienteID.Text = result.ClientID;
                    txtClientSecret.Text = result.ClientSecret;
                    empresa.ClientID = result.ClientID;
                    empresa.ClientSecret = result.ClientSecret;
                    empresa.TokenNFse = result.TokenNFse;
                    empresa.TokenNFSeExpire = result.TokenNFSeExpire;
                }

                if (empresa.UnidadeFederativaCodigo.Equals(5107925))
                {
                    var result = empresa.RecuperarConfiguracaoNFSeSoftplan(empresa.CNPJ);

                    txtClienteID.Text = result.ClientID;
                    txtClientSecret.Text = result.ClientSecret;
                    empresa.ClientID = result.ClientID;
                    empresa.ClientSecret = result.ClientSecret;
                }

                HabilitaUsuarioSenhaWS(this.empresa.UnidadeFederativaCodigo, this.empresa.Servico);
                servicoCurrent = this.empresa.Servico;

                HabilitaOpcaoCompactar(this.empresa.Servico == TipoAplicativo.Nfe);

                edtCNPJ.ReadOnly = !string.IsNullOrEmpty(empresa.CNPJ);
                cbServico.Enabled = !edtCNPJ.ReadOnly;

                if (this.empresa.Servico != TipoAplicativo.Nfse && !novaempresa)
                {
                    cbServico.Enabled = true;
                }

                if (this.empresa.Servico.Equals(TipoAplicativo.Nfe) ||
                    this.empresa.Servico.Equals(TipoAplicativo.NFCe) ||
                    this.empresa.Servico.Equals(TipoAplicativo.MDFe) ||
                    this.empresa.Servico.Equals(TipoAplicativo.Cte) ||
                    this.empresa.Servico.Equals(TipoAplicativo.Todos) ||
                    this.empresa.Servico.Equals(TipoAplicativo.NF3e) ||
                    this.empresa.Servico.Equals(TipoAplicativo.NFCom))
                {
                    checkBoxValidarDigestValue.Checked = this.empresa.CompararDigestValueDFeRetornadoSEFAZ;
                }

                if (this.empresa.Servico.Equals(TipoAplicativo.Nfe) ||
                    this.empresa.Servico.Equals(TipoAplicativo.NFCe) ||
                    this.empresa.Servico.Equals(TipoAplicativo.MDFe) ||
                    this.empresa.Servico.Equals(TipoAplicativo.Cte) ||
                    this.empresa.Servico.Equals(TipoAplicativo.Todos) ||
                    this.empresa.Servico.Equals(TipoAplicativo.NF3e) ||
                    this.empresa.Servico.Equals(TipoAplicativo.NFCom))
                {
                    checkBoxGravarWarnings.Checked = this.empresa.GravarWarnings;
                }
            }
            finally
            {
                loading = false;
                cbServico_SelectedIndexChanged(null, null);
                comboBox_UF_SelectedIndexChanged(null, null);
            }
        }

       public bool Validar(bool exibeerro, bool novaempresa)
        {
            var cnpj = this.edtCNPJ.Text.RemoveChars('/', '-', ',', '.',' '); // (string)Functions.OnlyNumbers(edtCNPJ.Text, ".-/");



            if (Convert.ToInt32("0" + udTempoConsulta.Text) < 2 || Convert.ToInt32("0" + udTempoConsulta.Text) > 15)
            {
                throw new Exception(lbl_udTempoConsulta.Text + " inválido");
            }

            if (comboBox_UF.SelectedValue == null)
            {
                throw new Exception(labelUF.Text + " deve ser informado");
            }

            ValidadeCNPJ(true);

            if (string.IsNullOrEmpty(edtNome.Text))
            {
                throw new Exception("Nome da empresa deve ser informado");
            }

            if (servicoCurrent != (TipoAplicativo)cbServico.SelectedValue && !novaempresa && exibeerro)
            {
                if ((TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Nfse)
                {
                    throw new Exception("Não pode mudar para esse tipo de serviço (NFSe)");
                }

                if ((TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.SATeMFE)
                {
                    throw new Exception("Não pode mudar para esse tipo de serviço (SAT)");
                }

                var e = Empresas.FindConfEmpresa(cnpj, (TipoAplicativo)cbServico.SelectedValue);
                if (e != null)
                {
                    throw new Exception("A empresa '" + e.Nome + "' já está monitorando esse tipo de serviço");
                }

                if (MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, "Confirma a alteração do tipo de serviço?", "",
                                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return false;
                }
            }

            switch ((TipoAplicativo)cbServico.SelectedValue)
            {
                case TipoAplicativo.NFCe:
                    if (!string.IsNullOrEmpty(edtIdentificadorCSC.Text) && string.IsNullOrEmpty(edtTokenCSC.Text))
                    {
                        throw new Exception("É obrigatório informar o IDToken quando informado o CSC.");
                    }
                    else if (string.IsNullOrEmpty(edtIdentificadorCSC.Text) && !string.IsNullOrEmpty(edtTokenCSC.Text))
                    {
                        throw new Exception("É obrigatório informar o CSC quando informado o IDToken.");
                    }

                    break;
            }

            empresa.AmbienteCodigo = (int)comboBox_Ambiente.SelectedValue;
            empresa.CNPJ = cnpj;
            empresa.ArqNSU = checkBoxArqNSU.Checked;
            empresa.DiasLimpeza = Math.Abs(Convert.ToInt32("0" + udDiasLimpeza.Text));
            empresa.DiretorioSalvarComo = cboDiretorioSalvarComo.Text;
            empresa.GravarRetornoTXTNFe = checkBoxRetornoNFETxt.Checked;
            empresa.GravarEventosDeTerceiros = checkBoxGravarEventosDeTerceiros.Checked;
            empresa.GravarEventosNaPastaEnviadosNFe = checkBoxGravarEventosNaPastaEnviadosNFe.Checked;
            empresa.GravarEventosCancelamentoNaPastaEnviadosNFe = checkBoxGravarEventosCancelamentoNaPastaEnviadosNFe.Checked;
            empresa.Nome = edtNome.Text;
            empresa.TempoConsulta = Math.Abs(Convert.ToInt32("0" + udTempoConsulta.Text));
            empresa.tpEmis = (int)comboBox_tpEmis.SelectedValue;
            empresa.UnidadeFederativaCodigo = (int)comboBox_UF.SelectedValue;
            empresa.Servico = (TipoAplicativo)cbServico.SelectedValue;
            empresa.SenhaWS = txtSenhaWS.Text;
            empresa.UsuarioWS = txtUsuarioWS.Text;
            empresa.IdentificadorCSC = edtIdentificadorCSC.Text;
            empresa.TokenCSC = edtTokenCSC.Text;
            empresa.CompararDigestValueDFeRetornadoSEFAZ = checkBoxValidarDigestValue.Checked;
            empresa.RpsSincAssincTHEMA = checkBoxRpsSincAssincTHEMA.Checked;
            empresa.GravarWarnings = checkBoxGravarWarnings.Checked;

            if (edtCodMun.Text.Equals("4205407"))
            {
                if (string.IsNullOrEmpty(txtUsuarioWS.Text) ||
                string.IsNullOrEmpty(txtSenhaWS.Text) ||
                string.IsNullOrEmpty(txtClienteID.Text) ||
                string.IsNullOrEmpty(txtClientSecret.Text))
                {
                    throw new Exception("As seguintes informações tem que estarem todas informadas: Usuário, Senha, ClientID e ClientSecret");
                }

                
                empresa.SalvarConfiguracoesNFSeSoftplan(txtUsuarioWS.Text,
                                                        txtSenhaWS.Text,
                                                        txtClienteID.Text,
                                                        txtClientSecret.Text,
                                                        edtCNPJ.Text, null, null);
            }

            return true;
        }

        private void HabilitaOpcaoCompactar(bool ativar)
        {
            if (empresa.Servico == TipoAplicativo.Nfse)
            {
                ativar = false;
            }
        }

        private void udDiasLimpeza_KeyPress(object sender, KeyPressEventArgs e) => e.Handled = !char.IsNumber(e.KeyChar);

        private void cbServico_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
            {
                return;
            }

            grpQRCode.Visible = (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.NFCe ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Nfe ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Todos;

            checkBoxValidarDigestValue.Visible = (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.NFCe ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Nfe ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Cte ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.MDFe ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Todos ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.NF3e ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.NFCom;

            checkBoxGravarWarnings.Visible = (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.NFCe ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Nfe ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Cte ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.MDFe ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Todos ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.NF3e ||
                                     (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.NFCom;

            if ((TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Nfe ||
                (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.NFCe ||
                (TipoAplicativo)cbServico.SelectedValue == TipoAplicativo.Todos)
            {
                HabilitaOpcaoCompactar(true);
            }
            else
            {
                HabilitaOpcaoCompactar(false);
            }
            if (changeEvent != null)
            {
                changeEvent(sender, e);
            }
        }

        private void HabilitaUsuarioSenhaWS(int ufCod, TipoAplicativo servico = TipoAplicativo.Nulo)
        {
            if (servico == TipoAplicativo.Nfse)
            {
                var padraoNfse = Functions.BuscaPadraoNFSe(ufCod);

                if (padraoNfse == PadraoNFSe.IPM || padraoNfse == PadraoNFSe.ISSONLINE_ASSESSORPUBLICO || padraoNfse == PadraoNFSe.SIMPLISS ||
                    padraoNfse == PadraoNFSe.FIORILLI || padraoNfse == PadraoNFSe.SMARAPD || padraoNfse == PadraoNFSe.EL ||
                    padraoNfse == PadraoNFSe.ADM_SISTEMAS || padraoNfse == PadraoNFSe.MEMORY || padraoNfse == PadraoNFSe.MODERNIZACAO_PUBLICA ||
                    padraoNfse == PadraoNFSe.WEBFISCO || padraoNfse == PadraoNFSe.IIBRASIL || padraoNfse == PadraoNFSe.CENTI ||
                    padraoNfse == PadraoNFSe.FINTEL || padraoNfse == PadraoNFSe.SIGISSWEB || padraoNfse == PadraoNFSe.SIMPLE ||
                    padraoNfse == PadraoNFSe.CONAM || padraoNfse == PadraoNFSe.HM2SOLUCOES || padraoNfse == PadraoNFSe.SINSOFT ||
                    padraoNfse == PadraoNFSe.GIAP)

                {
                    lbl_UsuarioWS.Visible = true;
                    txtUsuarioWS.Visible = true;
                    lbl_SenhaWS.Visible = true;
                    txtSenhaWS.Visible = true;
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                }
                else if (padraoNfse == PadraoNFSe.SOFTPLAN)
                {
                    lblClienteID.Visible = true;
                    lblClientSecret.Visible = true;
                    txtClienteID.Visible = true;
                    txtClientSecret.Visible = true;
                    lbl_UsuarioWS.Visible = true;
                    txtUsuarioWS.Visible = true;
                    lbl_SenhaWS.Visible = true;
                    txtSenhaWS.Visible = true;
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                }
                else if (padraoNfse == PadraoNFSe.BAUHAUS || padraoNfse == PadraoNFSe.TRIBUTUS)
                {
                    lbl_SenhaWS.Visible = true;
                    txtSenhaWS.Visible = true;
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                }
                else if (padraoNfse == PadraoNFSe.THEMA) {
                    checkBoxRpsSincAssincTHEMA.Visible = true;
                }
                else
                {
                    lbl_UsuarioWS.Visible = false;
                    txtUsuarioWS.Visible = false;
                    lbl_SenhaWS.Visible = false;
                    txtSenhaWS.Visible = false;
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                }
            }
            else
            { 
                lbl_SenhaWS.Visible = false;
                txtSenhaWS.Visible = false;
            }
        }

        private void comboBox_UF_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
            {
                return;
            }

            // danasa 1-2012
            try
            {
                var xuf = comboBox_UF.SelectedValue;

                edtCodMun.Text = xuf.ToString();

                edtPadrao.Text = EnumHelper.GetEnumItemDescription(Functions.BuscaPadraoNFSe(Convert.ToInt32(xuf)));
                HabilitaUsuarioSenhaWS(Convert.ToInt32(edtCodMun.Text), TipoAplicativo.Nfse);

            }
            catch
            {
                HabilitaUsuarioSenhaWS(-1);
                edtCodMun.Text = edtPadrao.Text = "Indefinido";
            }
            if (changeEvent != null)
            {
                changeEvent(sender, e);
            }
        }

        private void comboBox_Ambiente_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (changeEvent != null)
            {
                changeEvent(sender, e);
            }
        }

        public bool ValidadeCNPJ(bool istrow = false) => true;

        private void edtCNPJ_Leave(object sender, EventArgs e) => ValidadeCNPJ();

        private void edtCNPJ_Enter(object sender, EventArgs e)
        {
        }

        private void comboBox_UF_DropDownClosed(object sender, EventArgs e)
        {
            if (empresa.Servico == TipoAplicativo.Nfse)
            {
                comboBox_UF.DropDownWidth = comboBox_UF.Width;
            }
        }

        private void comboBox_UF_DropDown(object sender, EventArgs e)
        {
            if (empresa.Servico == TipoAplicativo.Nfse)
            {
                comboBox_UF.DropDownWidth = 300;
            }
        }

        private void txtUsuarioWS_TextChanged(object sender, EventArgs e)
        {
            if (changeEvent != null)
            {
                changeEvent(sender, e);
            }
        }

        private void txtSenhaWS_TextChanged(object sender, EventArgs e)
        {
            if (changeEvent != null)
            {
                changeEvent(sender, e);
            }
        }

        private void txtClienteID_TextChanged(object sender, EventArgs e)
        {
            if (changeEvent != null)
            {
                changeEvent(sender, e);
            }
        }

        private void txtClientSecret_TextChanged(object sender, EventArgs e)
        {
            if (changeEvent != null)
            {
                changeEvent(sender, e);
            }
        }

        private void checkBoxRpsSincAssincTHEMA_CheckedChanged(object sender, EventArgs e)
        {
             if (changeEvent != null)
             {
                changeEvent(sender, e);
             }
        }

        private void checkBoxGravarWarnings_CheckedChanged(object sender, EventArgs e)
        {
            if (changeEvent != null)
            {
                changeEvent(sender, e);
            }
        }

        private void edtPadrao_Click(object sender, EventArgs e)
        {

        }

        private void edtCodMun_Click(object sender, EventArgs e)
        {

        }

        private void txtClienteID_Click(object sender, EventArgs e)
        {

        }

        private void Configurar(Empresa empresa, bool novaempresa)
        {
            switch (empresa.Servico)
            {
                case TipoAplicativo.Nfse:
                    labelUF.Visible = true;
                    labelUF.Text = "Município";
                    comboBox_UF.Visible = true;
                    lbl_CodMun.Visible = true;
                    edtCodMun.Visible = true;
                    edtPadrao.Visible = true;
                    lbl_Padrao.Visible = true;
                    cboDiretorioSalvarComo.Visible = false;
                    lbl_DiretorioSalvarComo.Visible = false;
                    comboBox_tpEmis.Visible = false;
                    metroLabel11.Visible = false;
                    checkBoxGravarEventosNaPastaEnviadosNFe.Visible = false;
                    checkBoxRetornoNFETxt.Visible = false;
                    checkBoxGravarEventosDeTerceiros.Visible = false;
                    checkBoxGravarEventosCancelamentoNaPastaEnviadosNFe.Visible = false;
                    udTempoConsulta.Visible = lbl_udTempoConsulta.Visible = false;
                    comboBox_Ambiente.Visible = true;
                    checkBoxArqNSU.Visible = false;
                    checkBoxValidarDigestValue.Visible = false;
                    lbl_udDiasLimpeza.Location = new System.Drawing.Point(3, 247);
                    udDiasLimpeza.Location = new System.Drawing.Point(3, 266);
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                    checkBoxGravarWarnings.Visible = false;
                    break;

                case TipoAplicativo.SATeMFE:
                    labelUF.Visible = true;
                    labelUF.Text = "Unidade Federativa (UF)";
                    comboBox_UF.Visible = true;
                    cboDiretorioSalvarComo.Visible = false;
                    lbl_DiretorioSalvarComo.Visible = false;
                    comboBox_tpEmis.Visible = false;
                    metroLabel11.Visible = false;
                    checkBoxGravarEventosNaPastaEnviadosNFe.Visible = false;
                    checkBoxRetornoNFETxt.Visible = false;
                    checkBoxGravarEventosDeTerceiros.Visible = false;
                    checkBoxGravarEventosCancelamentoNaPastaEnviadosNFe.Visible = false;
                    udTempoConsulta.Visible = lbl_udTempoConsulta.Visible = false;
                    metroLabel10.Visible = false;
                    comboBox_Ambiente.Visible = false;
                    lbl_CodMun.Visible = false;
                    edtCodMun.Visible = false;
                    edtPadrao.Visible = false;
                    lbl_Padrao.Visible = false;
                    lblClienteID.Visible = false;
                    lblClientSecret.Visible = false;
                    txtClienteID.Visible = false;
                    txtClientSecret.Visible = false;
                    checkBoxArqNSU.Visible = false;
                    checkBoxValidarDigestValue.Visible = false;
                    lbl_udDiasLimpeza.Location = new System.Drawing.Point(3, 147);
                    udDiasLimpeza.Location = new System.Drawing.Point(3, 166);
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                    checkBoxGravarWarnings.Visible = false;
                    break;

                case TipoAplicativo.EFDReinf:
                case TipoAplicativo.eSocial:
                case TipoAplicativo.EFDReinfeSocial:
                    comboBox_UF.Visible = false;
                    comboBox_tpEmis.Visible = false;
                    udTempoConsulta.Visible = false;
                    checkBoxGravarEventosCancelamentoNaPastaEnviadosNFe.Visible = false;
                    checkBoxGravarEventosDeTerceiros.Visible = false;
                    checkBoxGravarEventosNaPastaEnviadosNFe.Visible = false;
                    checkBoxRetornoNFETxt.Visible = false;
                    grpQRCode.Visible = false;
                    metroLabel11.Visible = false;
                    lbl_udTempoConsulta.Visible = false;
                    labelUF.Visible = false;
                    lbl_CodMun.Visible = false;
                    edtCodMun.Visible = false;
                    edtPadrao.Visible = false;
                    lbl_Padrao.Visible = false;
                    lblClienteID.Visible = false;
                    lblClientSecret.Visible = false;
                    txtClienteID.Visible = false;
                    txtClientSecret.Visible = false;
                    checkBoxArqNSU.Visible = false;
                    checkBoxValidarDigestValue.Visible = false;
                    lbl_udDiasLimpeza.Location = new System.Drawing.Point(3, 147);
                    udDiasLimpeza.Location = new System.Drawing.Point(3, 166);
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                    checkBoxGravarWarnings.Visible = false;
                    break;

                case TipoAplicativo.GNREeDARE:
                    labelUF.Visible = true;
                    labelUF.Text = "Unidade Federativa (UF)";
                    comboBox_UF.Visible = true;
                    cboDiretorioSalvarComo.Visible = true;
                    lbl_DiretorioSalvarComo.Visible = true;
                    comboBox_tpEmis.Visible = false;
                    udTempoConsulta.Visible = false;
                    checkBoxGravarEventosCancelamentoNaPastaEnviadosNFe.Visible = false;
                    checkBoxGravarEventosDeTerceiros.Visible = false;
                    checkBoxGravarEventosNaPastaEnviadosNFe.Visible = false;
                    checkBoxRetornoNFETxt.Visible = false;
                    grpQRCode.Visible = false;
                    metroLabel11.Visible = false;
                    lbl_udTempoConsulta.Visible = false;
                    lbl_CodMun.Visible = false;
                    edtCodMun.Visible = false;
                    edtPadrao.Visible = false;
                    lbl_Padrao.Visible = false;
                    checkBoxArqNSU.Visible = false;
                    checkBoxValidarDigestValue.Visible = false;
                    lbl_udDiasLimpeza.Location = new System.Drawing.Point(3, 147);
                    udDiasLimpeza.Location = new System.Drawing.Point(3, 166);
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                    checkBoxGravarWarnings.Visible = false;
                    break;

                case TipoAplicativo.NF3e:
                case TipoAplicativo.NFCom:
                    comboBox_UF.Visible = true;
                    comboBox_tpEmis.Visible = true;
                    udTempoConsulta.Visible = false;
                    checkBoxGravarEventosCancelamentoNaPastaEnviadosNFe.Visible = true;
                    checkBoxGravarEventosDeTerceiros.Visible = false;
                    checkBoxGravarEventosNaPastaEnviadosNFe.Visible = true;
                    checkBoxRetornoNFETxt.Visible = false;
                    grpQRCode.Visible = false;
                    metroLabel11.Visible = true;
                    lbl_udTempoConsulta.Visible = false;
                    labelUF.Visible = false;
                    lbl_CodMun.Visible = false;
                    edtCodMun.Visible = false;
                    edtPadrao.Visible = false;
                    lbl_Padrao.Visible = false;
                    lblClienteID.Visible = false;
                    lblClientSecret.Visible = false;
                    txtClienteID.Visible = false;
                    txtClientSecret.Visible = false;
                    checkBoxArqNSU.Visible = false;
                    checkBoxValidarDigestValue.Visible = true;
                    lbl_udDiasLimpeza.Location = new System.Drawing.Point(3, 147);
                    udDiasLimpeza.Location = new System.Drawing.Point(3, 166);
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                    checkBoxGravarWarnings.Visible = true;
                    break;

                default:
                    labelUF.Visible = true;
                    labelUF.Text = "Unidade Federativa (UF)";
                    comboBox_UF.Visible = true;
                    cboDiretorioSalvarComo.Visible = true;
                    lbl_DiretorioSalvarComo.Visible = true;
                    comboBox_tpEmis.Visible = true;
                    metroLabel11.Visible = true;
                    checkBoxGravarEventosNaPastaEnviadosNFe.Visible = true;
                    checkBoxRetornoNFETxt.Visible = true;
                    checkBoxGravarEventosDeTerceiros.Visible = true;
                    checkBoxGravarEventosCancelamentoNaPastaEnviadosNFe.Visible = true;
                    udTempoConsulta.Visible = lbl_udTempoConsulta.Visible = true;
                    metroLabel10.Visible = true;
                    comboBox_Ambiente.Visible = true;
                    lbl_CodMun.Visible = false;
                    edtCodMun.Visible = false;
                    edtPadrao.Visible = false;
                    lbl_Padrao.Visible = false;
                    grpQRCode.Visible = true;
                    edtTokenCSC.Visible = true;
                    edtIdentificadorCSC.Visible = true;
                    metroLabel2.Visible = true;
                    metroLabel1.Visible = true;
                    lblClienteID.Visible = false;
                    lblClientSecret.Visible = false;
                    txtClienteID.Visible = false;
                    txtClientSecret.Visible = false;
                    checkBoxArqNSU.Visible = true;
                    checkBoxValidarDigestValue.Visible = true;
                    lbl_udDiasLimpeza.Location = new System.Drawing.Point(3, 147);
                    udDiasLimpeza.Location = new System.Drawing.Point(3, 166);
                    checkBoxRpsSincAssincTHEMA.Visible = false;
                    checkBoxGravarWarnings.Visible = true;
                    break;
            }
        }
    }
}