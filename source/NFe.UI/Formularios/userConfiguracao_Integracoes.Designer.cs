namespace NFe.UI.Formularios
{
    partial class userConfiguracao_Integracoes
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
            this.components = new System.ComponentModel.Container();
            this.lbl_Secret = new MetroFramework.Controls.MetroLabel();
            this.txt_Secret = new MetroFramework.Controls.MetroTextBox();
            this.lbl_AppID = new MetroFramework.Controls.MetroLabel();
            this.txt_AppID = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.txt_Secret_UMessenger = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.txt_AppID_UMessenger = new MetroFramework.Controls.MetroTextBox();
            this.lbl_UMessenger = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Info_UMessenger = new System.Windows.Forms.ToolTip(this.components);
            this.info_Ebank_Pix = new System.Windows.Forms.ToolTip(this.components);
            this.cbox_DocumentosRejeitados = new MetroFramework.Controls.MetroCheckBox();
            this.cbox_DocumentosDenegados = new MetroFramework.Controls.MetroCheckBox();
            this.cbox_ErrosUniNFe = new MetroFramework.Controls.MetroCheckBox();
            this.cbox_MesmosDados = new MetroFramework.Controls.MetroCheckBox();
            this.lbl_NumeroUMessenger = new System.Windows.Forms.Label();
            this.txt_NumeroUMessenger = new System.Windows.Forms.MaskedTextBox();
            this.SuspendLayout();
            // 
            // lbl_Secret
            // 
            this.lbl_Secret.AutoSize = true;
            this.lbl_Secret.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lbl_Secret.Location = new System.Drawing.Point(497, 52);
            this.lbl_Secret.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbl_Secret.Name = "lbl_Secret";
            this.lbl_Secret.Size = new System.Drawing.Size(38, 15);
            this.lbl_Secret.TabIndex = 4;
            this.lbl_Secret.Text = "Secret";
            // 
            // txt_Secret
            // 
            this.txt_Secret.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_Secret.Lines = new string[0];
            this.txt_Secret.Location = new System.Drawing.Point(497, 80);
            this.txt_Secret.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_Secret.MaxLength = 32767;
            this.txt_Secret.Name = "txt_Secret";
            this.txt_Secret.PasswordChar = '*';
            this.txt_Secret.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txt_Secret.SelectedText = "";
            this.txt_Secret.Size = new System.Drawing.Size(231, 34);
            this.txt_Secret.TabIndex = 44;
            this.txt_Secret.UseSelectable = true;
            this.txt_Secret.UseStyleColors = true;
            this.txt_Secret.TextChanged += new System.EventHandler(this.Validate);
            // 
            // lbl_AppID
            // 
            this.lbl_AppID.AutoSize = true;
            this.lbl_AppID.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lbl_AppID.Location = new System.Drawing.Point(15, 52);
            this.lbl_AppID.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbl_AppID.Name = "lbl_AppID";
            this.lbl_AppID.Size = new System.Drawing.Size(40, 15);
            this.lbl_AppID.TabIndex = 41;
            this.lbl_AppID.Text = "AppID";
            // 
            // txt_AppID
            // 
            this.txt_AppID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_AppID.Lines = new string[0];
            this.txt_AppID.Location = new System.Drawing.Point(15, 80);
            this.txt_AppID.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_AppID.MaxLength = 32767;
            this.txt_AppID.Name = "txt_AppID";
            this.txt_AppID.PasswordChar = '*';
            this.txt_AppID.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txt_AppID.SelectedText = "";
            this.txt_AppID.Size = new System.Drawing.Size(231, 34);
            this.txt_AppID.TabIndex = 42;
            this.txt_AppID.UseSelectable = true;
            this.txt_AppID.UseStyleColors = true;
            this.txt_AppID.TextChanged += new System.EventHandler(this.Validate);
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.FontSize = MetroFramework.MetroLabelSize.Small;
            this.metroLabel1.Location = new System.Drawing.Point(497, 185);
            this.metroLabel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(38, 15);
            this.metroLabel1.TabIndex = 45;
            this.metroLabel1.Text = "Secret";
            // 
            // txt_Secret_UMessenger
            // 
            this.txt_Secret_UMessenger.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_Secret_UMessenger.Lines = new string[0];
            this.txt_Secret_UMessenger.Location = new System.Drawing.Point(497, 212);
            this.txt_Secret_UMessenger.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_Secret_UMessenger.MaxLength = 32767;
            this.txt_Secret_UMessenger.Name = "txt_Secret_UMessenger";
            this.txt_Secret_UMessenger.PasswordChar = '*';
            this.txt_Secret_UMessenger.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txt_Secret_UMessenger.SelectedText = "";
            this.txt_Secret_UMessenger.Size = new System.Drawing.Size(231, 34);
            this.txt_Secret_UMessenger.TabIndex = 48;
            this.txt_Secret_UMessenger.UseSelectable = true;
            this.txt_Secret_UMessenger.UseStyleColors = true;
            this.txt_Secret_UMessenger.TextChanged += new System.EventHandler(this.Validate);
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.FontSize = MetroFramework.MetroLabelSize.Small;
            this.metroLabel2.Location = new System.Drawing.Point(15, 185);
            this.metroLabel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(40, 15);
            this.metroLabel2.TabIndex = 46;
            this.metroLabel2.Text = "AppID";
            // 
            // txt_AppID_UMessenger
            // 
            this.txt_AppID_UMessenger.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_AppID_UMessenger.Lines = new string[0];
            this.txt_AppID_UMessenger.Location = new System.Drawing.Point(15, 212);
            this.txt_AppID_UMessenger.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_AppID_UMessenger.MaxLength = 32767;
            this.txt_AppID_UMessenger.Name = "txt_AppID_UMessenger";
            this.txt_AppID_UMessenger.PasswordChar = '*';
            this.txt_AppID_UMessenger.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txt_AppID_UMessenger.SelectedText = "";
            this.txt_AppID_UMessenger.Size = new System.Drawing.Size(231, 34);
            this.txt_AppID_UMessenger.TabIndex = 47;
            this.txt_AppID_UMessenger.UseSelectable = true;
            this.txt_AppID_UMessenger.UseStyleColors = true;
            this.txt_AppID_UMessenger.TextChanged += new System.EventHandler(this.Validate);
            // 
            // lbl_UMessenger
            // 
            this.lbl_UMessenger.AutoSize = true;
            this.lbl_UMessenger.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.lbl_UMessenger.Location = new System.Drawing.Point(19, 154);
            this.lbl_UMessenger.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_UMessenger.Name = "lbl_UMessenger";
            this.lbl_UMessenger.Size = new System.Drawing.Size(138, 25);
            this.lbl_UMessenger.TabIndex = 49;
            this.lbl_UMessenger.Text = "u-Messenger";
            this.Info_UMessenger.SetToolTip(this.lbl_UMessenger, "uMessenger é uma API para envio de mensagens via WhatsApp.");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.label1.Location = new System.Drawing.Point(19, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 25);
            this.label1.TabIndex = 50;
            this.label1.Text = "e-bank";
            this.info_Ebank_Pix.SetToolTip(this.label1, "O Ebank é uma API para o consumo de serviços de Boleto, PIX, Extratos, Pagamentos" +
        ", etc.");
            // 
            // cbox_DocumentosRejeitados
            // 
            this.cbox_DocumentosRejeitados.AutoSize = true;
            this.cbox_DocumentosRejeitados.Location = new System.Drawing.Point(15, 311);
            this.cbox_DocumentosRejeitados.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbox_DocumentosRejeitados.Name = "cbox_DocumentosRejeitados";
            this.cbox_DocumentosRejeitados.Size = new System.Drawing.Size(316, 15);
            this.cbox_DocumentosRejeitados.TabIndex = 55;
            this.cbox_DocumentosRejeitados.Text = "Enviar alerta de documentos rejeitados para WhatsApp?";
            this.cbox_DocumentosRejeitados.UseSelectable = true;
            this.cbox_DocumentosRejeitados.CheckedChanged += new System.EventHandler(this.CheckChangedUMessenger);
            this.cbox_DocumentosRejeitados.CheckStateChanged += new System.EventHandler(this.Validate);
            // 
            // cbox_DocumentosDenegados
            // 
            this.cbox_DocumentosDenegados.AutoSize = true;
            this.cbox_DocumentosDenegados.Location = new System.Drawing.Point(15, 343);
            this.cbox_DocumentosDenegados.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbox_DocumentosDenegados.Name = "cbox_DocumentosDenegados";
            this.cbox_DocumentosDenegados.Size = new System.Drawing.Size(323, 15);
            this.cbox_DocumentosDenegados.TabIndex = 56;
            this.cbox_DocumentosDenegados.Text = "Enviar alerta de documentos denegados para WhatsApp?";
            this.cbox_DocumentosDenegados.UseSelectable = true;
            this.cbox_DocumentosDenegados.CheckedChanged += new System.EventHandler(this.CheckChangedUMessenger);
            this.cbox_DocumentosDenegados.CheckStateChanged += new System.EventHandler(this.Validate);
            // 
            // cbox_ErrosUniNFe
            // 
            this.cbox_ErrosUniNFe.AutoSize = true;
            this.cbox_ErrosUniNFe.Location = new System.Drawing.Point(15, 375);
            this.cbox_ErrosUniNFe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbox_ErrosUniNFe.Name = "cbox_ErrosUniNFe";
            this.cbox_ErrosUniNFe.Size = new System.Drawing.Size(333, 15);
            this.cbox_ErrosUniNFe.TabIndex = 57;
            this.cbox_ErrosUniNFe.Text = "Enviar alerta de erros ocorridos no UniNFe para WhatsApp?";
            this.cbox_ErrosUniNFe.UseSelectable = true;
            this.cbox_ErrosUniNFe.CheckedChanged += new System.EventHandler(this.CheckChangedUMessenger);
            this.cbox_ErrosUniNFe.CheckStateChanged += new System.EventHandler(this.Validate);
            // 
            // cbox_MesmosDados
            // 
            this.cbox_MesmosDados.AutoSize = true;
            this.cbox_MesmosDados.Location = new System.Drawing.Point(15, 279);
            this.cbox_MesmosDados.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cbox_MesmosDados.Name = "cbox_MesmosDados";
            this.cbox_MesmosDados.Size = new System.Drawing.Size(301, 15);
            this.cbox_MesmosDados.TabIndex = 58;
            this.cbox_MesmosDados.Text = "Utilizar oAppID e Secret do Ebank para o uMessenger";
            this.cbox_MesmosDados.UseSelectable = true;
            this.cbox_MesmosDados.CheckedChanged += new System.EventHandler(this.CheckChanged_MesmosDados);
            this.cbox_MesmosDados.CheckStateChanged += new System.EventHandler(this.Validate);
            // 
            // lbl_NumeroUMessenger
            // 
            this.lbl_NumeroUMessenger.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lbl_NumeroUMessenger.Location = new System.Drawing.Point(594, 311);
            this.lbl_NumeroUMessenger.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbl_NumeroUMessenger.Name = "lbl_NumeroUMessenger";
            this.lbl_NumeroUMessenger.Size = new System.Drawing.Size(150, 23);
            this.lbl_NumeroUMessenger.TabIndex = 59;
            this.lbl_NumeroUMessenger.Text = "Numero para envio";
            this.lbl_NumeroUMessenger.Visible = false;
            // 
            // txt_NumeroUMessenger
            // 
            this.txt_NumeroUMessenger.Enabled = false;
            this.txt_NumeroUMessenger.Location = new System.Drawing.Point(587, 346);
            this.txt_NumeroUMessenger.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txt_NumeroUMessenger.Name = "txt_NumeroUMessenger";
            this.txt_NumeroUMessenger.Size = new System.Drawing.Size(156, 26);
            this.txt_NumeroUMessenger.TabIndex = 61;
            this.txt_NumeroUMessenger.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            this.txt_NumeroUMessenger.Visible = false;
            this.txt_NumeroUMessenger.TextChanged += new System.EventHandler(this.Validate);
            // 
            // userConfiguracao_Integracoes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.txt_NumeroUMessenger);
            this.Controls.Add(this.lbl_NumeroUMessenger);
            this.Controls.Add(this.cbox_MesmosDados);
            this.Controls.Add(this.cbox_ErrosUniNFe);
            this.Controls.Add(this.cbox_DocumentosDenegados);
            this.Controls.Add(this.cbox_DocumentosRejeitados);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_UMessenger);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.txt_Secret_UMessenger);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.txt_AppID_UMessenger);
            this.Controls.Add(this.lbl_Secret);
            this.Controls.Add(this.txt_Secret);
            this.Controls.Add(this.lbl_AppID);
            this.Controls.Add(this.txt_AppID);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "userConfiguracao_Integracoes";
            this.Size = new System.Drawing.Size(966, 755);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MetroFramework.Controls.MetroLabel lbl_Secret;
        private MetroFramework.Controls.MetroTextBox txt_Secret;
        private MetroFramework.Controls.MetroLabel lbl_AppID;
        private MetroFramework.Controls.MetroTextBox txt_AppID;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroTextBox txt_Secret_UMessenger;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroTextBox txt_AppID_UMessenger;
        private System.Windows.Forms.Label lbl_UMessenger;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip Info_UMessenger;
        private System.Windows.Forms.ToolTip info_Ebank_Pix;
        private MetroFramework.Controls.MetroCheckBox cbox_DocumentosRejeitados;
        private MetroFramework.Controls.MetroCheckBox cbox_DocumentosDenegados;
        private MetroFramework.Controls.MetroCheckBox cbox_ErrosUniNFe;
        private MetroFramework.Controls.MetroCheckBox cbox_MesmosDados;
        private System.Windows.Forms.Label lbl_NumeroUMessenger;
        private System.Windows.Forms.MaskedTextBox txt_NumeroUMessenger;
    }
}
