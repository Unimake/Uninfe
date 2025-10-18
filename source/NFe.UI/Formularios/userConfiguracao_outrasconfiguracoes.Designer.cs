namespace NFe.UI.Formularios
{
    partial class userConfiguracao_outrasconfiguracoes
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkSalvarXMLDistribuicao = new MetroFramework.Controls.MetroCheckBox();
            this.lbl_udTempoEnvioNFSe = new MetroFramework.Controls.MetroLabel();
            this.udTempoEnvioNFSe = new MetroFramework.Controls.MetroTextBox();
            this.lbl_udVersaoQrCodeNFCe = new MetroFramework.Controls.MetroLabel();
            this.udVersaoQrCodeNFCe = new MetroFramework.Controls.MetroTextBox();
            this.SuspendLayout();
            // 
            // chkSalvarXMLDistribuicao
            // 
            this.chkSalvarXMLDistribuicao.AutoSize = true;
            this.chkSalvarXMLDistribuicao.Location = new System.Drawing.Point(9, 7);
            this.chkSalvarXMLDistribuicao.Name = "chkSalvarXMLDistribuicao";
            this.chkSalvarXMLDistribuicao.Size = new System.Drawing.Size(337, 15);
            this.chkSalvarXMLDistribuicao.TabIndex = 0;
            this.chkSalvarXMLDistribuicao.Text = "Salvar na pasta autorizados somente o XML de distribuição?";
            this.chkSalvarXMLDistribuicao.UseSelectable = true;
            this.chkSalvarXMLDistribuicao.Visible = false;
            this.chkSalvarXMLDistribuicao.CheckedChanged += new System.EventHandler(this.ChkSalvarXMLDistribuicao_CheckedChanged);
            // 
            // lbl_udTempoEnvioNFSe
            // 
            this.lbl_udTempoEnvioNFSe.AutoSize = true;
            this.lbl_udTempoEnvioNFSe.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lbl_udTempoEnvioNFSe.Location = new System.Drawing.Point(9, 84);
            this.lbl_udTempoEnvioNFSe.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.lbl_udTempoEnvioNFSe.Name = "lbl_udTempoEnvioNFSe";
            this.lbl_udTempoEnvioNFSe.Size = new System.Drawing.Size(378, 15);
            this.lbl_udTempoEnvioNFSe.TabIndex = 26;
            this.lbl_udTempoEnvioNFSe.Text = "Intervalo de tempo (em segundos) entre o envio de cada arquivo de NFSe";
            this.lbl_udTempoEnvioNFSe.Visible = false;
            // 
            // udTempoEnvioNFSe
            // 
            this.udTempoEnvioNFSe.Lines = new string[0];
            this.udTempoEnvioNFSe.Location = new System.Drawing.Point(9, 105);
            this.udTempoEnvioNFSe.MaxLength = 4;
            this.udTempoEnvioNFSe.Name = "udTempoEnvioNFSe";
            this.udTempoEnvioNFSe.PasswordChar = '\0';
            this.udTempoEnvioNFSe.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.udTempoEnvioNFSe.SelectedText = "";
            this.udTempoEnvioNFSe.Size = new System.Drawing.Size(74, 22);
            this.udTempoEnvioNFSe.TabIndex = 27;
            this.udTempoEnvioNFSe.UseSelectable = true;
            this.udTempoEnvioNFSe.UseStyleColors = true;
            this.udTempoEnvioNFSe.Visible = false;
            this.udTempoEnvioNFSe.TextChanged += new System.EventHandler(this.udTempoEnvioNFSe_TextChanged);
            // 
            // lbl_udVersaoQrCodeNFCe
            // 
            this.lbl_udVersaoQrCodeNFCe.AutoSize = true;
            this.lbl_udVersaoQrCodeNFCe.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lbl_udVersaoQrCodeNFCe.Location = new System.Drawing.Point(9, 38);
            this.lbl_udVersaoQrCodeNFCe.Margin = new System.Windows.Forms.Padding(3);
            this.lbl_udVersaoQrCodeNFCe.Name = "lbl_udVersaoQrCodeNFCe";
            this.lbl_udVersaoQrCodeNFCe.Size = new System.Drawing.Size(269, 15);
            this.lbl_udVersaoQrCodeNFCe.TabIndex = 28;
            this.lbl_udVersaoQrCodeNFCe.Text = "Versão do QrCode da NFCe (Deixe 0 para o padrão)";
            this.lbl_udVersaoQrCodeNFCe.Visible = false;
            // 
            // udVersaoQrCodeNFCe
            // 
            this.udVersaoQrCodeNFCe.Lines = new string[0];
            this.udVersaoQrCodeNFCe.Location = new System.Drawing.Point(9, 56);
            this.udVersaoQrCodeNFCe.MaxLength = 4;
            this.udVersaoQrCodeNFCe.Name = "udVersaoQrCodeNFCe";
            this.udVersaoQrCodeNFCe.PasswordChar = '\0';
            this.udVersaoQrCodeNFCe.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.udVersaoQrCodeNFCe.SelectedText = "";
            this.udVersaoQrCodeNFCe.Size = new System.Drawing.Size(49, 22);
            this.udVersaoQrCodeNFCe.TabIndex = 29;
            this.udVersaoQrCodeNFCe.UseSelectable = true;
            this.udVersaoQrCodeNFCe.UseStyleColors = true;
            this.udVersaoQrCodeNFCe.Visible = false;
            this.udVersaoQrCodeNFCe.TextChanged += new System.EventHandler(this.udVersaoQrCodeNFCe_TextChanged);
            // 
            // userConfiguracao_outrasconfiguracoes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbl_udVersaoQrCodeNFCe);
            this.Controls.Add(this.udVersaoQrCodeNFCe);
            this.Controls.Add(this.lbl_udTempoEnvioNFSe);
            this.Controls.Add(this.udTempoEnvioNFSe);
            this.Controls.Add(this.chkSalvarXMLDistribuicao);
            this.Name = "userConfiguracao_outrasconfiguracoes";
            this.Size = new System.Drawing.Size(640, 374);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroCheckBox chkSalvarXMLDistribuicao;
        private MetroFramework.Controls.MetroLabel lbl_udTempoEnvioNFSe;
        private MetroFramework.Controls.MetroTextBox udTempoEnvioNFSe;
        private MetroFramework.Controls.MetroLabel lbl_udVersaoQrCodeNFCe;
        private MetroFramework.Controls.MetroTextBox udVersaoQrCodeNFCe;
    }
}
