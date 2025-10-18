using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NFe.Components;
using NFe.Settings;
using System.Runtime.CompilerServices;

namespace NFe.UI.Formularios
{
    public partial class userConfiguracao_outrasconfiguracoes : MetroFramework.Controls.MetroUserControl
    {
        public event EventHandler changeEvent;


        private Settings.Empresa empresa;
        public userConfiguracao_outrasconfiguracoes()
        {
            InitializeComponent();
        }

        public void Populate(Settings.Empresa empresa)
        {
            this.empresa = empresa;
            uninfeDummy.ClearControls(this, true, false);

            if (empresa.Servico == TipoAplicativo.Nfse)
            {
                udTempoEnvioNFSe.Visible = true;
                lbl_udTempoEnvioNFSe.Visible = true;
                lbl_udVersaoQrCodeNFCe.Visible = false;
                udVersaoQrCodeNFCe.Visible = false;
                chkSalvarXMLDistribuicao.Visible = false;
            }
            else if (empresa.Servico == TipoAplicativo.Nfe || empresa.Servico == TipoAplicativo.NFCe || empresa.Servico == TipoAplicativo.Todos)
            {
                chkSalvarXMLDistribuicao.Visible = true;
                lbl_udVersaoQrCodeNFCe.Visible = true;
                udVersaoQrCodeNFCe.Visible = true;
                lbl_udTempoEnvioNFSe.Visible = false;
                udTempoEnvioNFSe.Visible = false;
            }
            else
            {
                chkSalvarXMLDistribuicao.Visible = true;
                udTempoEnvioNFSe.Visible = false;
                lbl_udTempoEnvioNFSe.Visible = false;
                lbl_udVersaoQrCodeNFCe.Visible = false;
                udVersaoQrCodeNFCe.Visible = false;
            }

            chkSalvarXMLDistribuicao.Checked = empresa.SalvarSomenteXMLDistribuicao;
            udTempoEnvioNFSe.Text = empresa.TempoEnvioNFSe.ToString();
            udVersaoQrCodeNFCe.Text = empresa.VersaoQRCodeNFCe.ToString();
        }

        public void Validar(bool salvando = true)
        {
            empresa.SalvarSomenteXMLDistribuicao = chkSalvarXMLDistribuicao.Checked;
            empresa.TempoEnvioNFSe = Convert.ToInt32(udTempoEnvioNFSe.Text);
            empresa.VersaoQRCodeNFCe = Convert.ToInt32(udVersaoQrCodeNFCe.Text);
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

        private void ChkSalvarXMLDistribuicao_CheckedChanged(object sender, EventArgs e)
        {
            changeEvent?.Invoke(sender, e);
        }

        private void udTempoEnvioNFSe_TextChanged(object sender, EventArgs e)
        {
            changeEvent?.Invoke(sender, e);
        }

        private void udVersaoQrCodeNFCe_TextChanged(object sender, EventArgs e)
        {
            changeEvent?.Invoke(sender, e);
        }
    }
}