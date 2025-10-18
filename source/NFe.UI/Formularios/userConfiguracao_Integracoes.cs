using System;
using System.ComponentModel;
using System.Windows.Forms;
using NFe.Components;

namespace NFe.UI.Formularios
{
    [ToolboxItem(false)]
    public partial class userConfiguracao_Integracoes : MetroFramework.Controls.MetroUserControl
    {
        public userConfiguracao_Integracoes()
        {
            InitializeComponent();
        }

        public event EventHandler changeEvent;
        private NFe.Settings.Empresa empresa;

        public void Populate(NFe.Settings.Empresa empresa)
        {
            uninfeDummy.ClearControls(this, true, false);
            this.empresa = empresa;
            txt_AppID.Text = empresa.AppID;
            txt_Secret.Text = empresa.Secret;
            txt_AppID_UMessenger.Text = empresa.AppID_UMessenger;
            txt_Secret_UMessenger.Text = empresa.Secret_UMessenger;
            txt_NumeroUMessenger.Text = empresa.NumeroUMessenger;
            txt_AppID_UMessenger.Text = empresa.AppID_UMessenger;
            txt_Secret_UMessenger.Text = empresa.Secret_UMessenger;
            cbox_DocumentosDenegados.Checked = empresa.DocumentosDenegados;
            cbox_DocumentosRejeitados.Checked = empresa.DocumentosRejeitados;
            cbox_ErrosUniNFe.Checked = empresa.ErrosUniNFe;
            cbox_MesmosDados.Checked = empresa.MesmosDadosEb_Mb;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public void Validar()
        {
            this.empresa.AppID = txt_AppID.Text;
            this.empresa.Secret = txt_Secret.Text;
            this.empresa.AppID_UMessenger = txt_AppID_UMessenger.Text;
            this.empresa.Secret_UMessenger = txt_Secret_UMessenger.Text;
            this.empresa.AppID_UMessenger = txt_AppID_UMessenger.Text;
            this.empresa.Secret_UMessenger = txt_Secret_UMessenger.Text;
            this.empresa.NumeroUMessenger = txt_NumeroUMessenger.Text;
            this.empresa.ErrosUniNFe = cbox_ErrosUniNFe.Checked;
            this.empresa.MesmosDadosEb_Mb = cbox_MesmosDados.Checked;
            this.empresa.DocumentosDenegados = cbox_DocumentosDenegados.Checked;
            this.empresa.DocumentosRejeitados = cbox_DocumentosRejeitados.Checked;

        }
        private void Validate(object sender, EventArgs e)
        {
            if (txt_AppID.Text.Length > 50)
            {
                throw new Exception($"O AppID deve conter no máximo 50 caracteres");
            } 
            else if (txt_Secret.Text.Length > 50)
            {
                throw new Exception($"O Secret deve conter no máximo 50 caracteres");
            }

            if (this.changeEvent != null)
                this.changeEvent(sender, e);
        }

        public void FocusFirstControl()
        {
            Timer t = new Timer();
            t.Interval = 50;
            t.Tick += (sender, e) =>
            {
                ((Timer)sender).Stop();
                ((Timer)sender).Dispose();
            };
            t.Start();
        }

        private void CheckChangedUMessenger(object sender, EventArgs e)
        {
            if (cbox_ErrosUniNFe.Checked || cbox_DocumentosDenegados.Checked || cbox_DocumentosRejeitados.Checked)
            {
                lbl_NumeroUMessenger.Visible = true;
                txt_NumeroUMessenger.Visible = true;
                txt_NumeroUMessenger.Enabled = true;
                txt_NumeroUMessenger.Focus();
            }
            else
            {
                lbl_NumeroUMessenger.Visible = false;
                txt_NumeroUMessenger.Visible = false;
                txt_NumeroUMessenger.Enabled = false;
                txt_NumeroUMessenger.Text = string.Empty;
            }
        }

        private void CheckChanged_MesmosDados(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                txt_AppID_UMessenger.Enabled = false;
                txt_Secret_UMessenger.Enabled = false;

                txt_AppID_UMessenger.Text = string.Empty;
                txt_Secret_UMessenger.Text = string.Empty;

            }
            else
            {
                txt_AppID_UMessenger.Enabled = true;
                txt_Secret_UMessenger.Enabled = true;
            }
        }
    }
}
