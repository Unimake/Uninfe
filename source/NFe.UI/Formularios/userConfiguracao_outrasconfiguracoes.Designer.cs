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
            this.chkAtivarPreparacaoTLSAntesEnvioXML = new MetroFramework.Controls.MetroCheckBox();
            this.lblAvisoPreparacaoTLSAntesEnvioXML = new MetroFramework.Controls.MetroLabel();
            this.grpEFrete = new System.Windows.Forms.GroupBox();
            this.lblEFreteIntegrador = new MetroFramework.Controls.MetroLabel();
            this.txtEFreteIntegrador = new MetroFramework.Controls.MetroTextBox();
            this.lblEFreteToken = new MetroFramework.Controls.MetroLabel();
            this.txtEFreteToken = new MetroFramework.Controls.MetroTextBox();
            this.lblEFreteUsuario = new MetroFramework.Controls.MetroLabel();
            this.txtEFreteUsuario = new MetroFramework.Controls.MetroTextBox();
            this.lblEFreteSenha = new MetroFramework.Controls.MetroLabel();
            this.txtEFreteSenha = new MetroFramework.Controls.MetroTextBox();
            this.lblEFreteAutenticacao = new MetroFramework.Controls.MetroLabel();
            this.grpEFrete.SuspendLayout();
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
            this.lbl_udTempoEnvioNFSe.Location = new System.Drawing.Point(9, 160);
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
            this.udTempoEnvioNFSe.Location = new System.Drawing.Point(9, 181);
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
            // chkAtivarPreparacaoTLSAntesEnvioXML
            //
            this.chkAtivarPreparacaoTLSAntesEnvioXML.AutoSize = true;
            this.chkAtivarPreparacaoTLSAntesEnvioXML.Location = new System.Drawing.Point(9, 88);
            this.chkAtivarPreparacaoTLSAntesEnvioXML.Name = "chkAtivarPreparacaoTLSAntesEnvioXML";
            this.chkAtivarPreparacaoTLSAntesEnvioXML.Size = new System.Drawing.Size(287, 15);
            this.chkAtivarPreparacaoTLSAntesEnvioXML.TabIndex = 30;
            this.chkAtivarPreparacaoTLSAntesEnvioXML.Text = "Ativar preparação de TLS antes do envio do XML?";
            this.chkAtivarPreparacaoTLSAntesEnvioXML.UseSelectable = true;
            this.chkAtivarPreparacaoTLSAntesEnvioXML.CheckedChanged += new System.EventHandler(this.chkAtivarPreparacaoTLSAntesEnvioXML_CheckedChanged);
            //
            // lblAvisoPreparacaoTLSAntesEnvioXML
            //
            this.lblAvisoPreparacaoTLSAntesEnvioXML.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lblAvisoPreparacaoTLSAntesEnvioXML.Location = new System.Drawing.Point(9, 111);
            this.lblAvisoPreparacaoTLSAntesEnvioXML.Name = "lblAvisoPreparacaoTLSAntesEnvioXML";
            this.lblAvisoPreparacaoTLSAntesEnvioXML.Size = new System.Drawing.Size(615, 38);
            this.lblAvisoPreparacaoTLSAntesEnvioXML.TabIndex = 31;
            this.lblAvisoPreparacaoTLSAntesEnvioXML.Text = "Atenção: use esta opção somente quando houver problemas de conexão TLS. Ela faz um" +
    "a primeira tentativa para preparar o TLS antes do envio do XML e pode deixar a co" +
    "nexão mais lenta, mesmo que por frações de segundo.";
            this.lblAvisoPreparacaoTLSAntesEnvioXML.Visible = false;
            this.lblAvisoPreparacaoTLSAntesEnvioXML.WrapToLine = true;
            //
            // grpEFrete
            //
            this.grpEFrete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpEFrete.Controls.Add(this.lblEFreteAutenticacao);
            this.grpEFrete.Controls.Add(this.txtEFreteSenha);
            this.grpEFrete.Controls.Add(this.lblEFreteSenha);
            this.grpEFrete.Controls.Add(this.txtEFreteUsuario);
            this.grpEFrete.Controls.Add(this.lblEFreteUsuario);
            this.grpEFrete.Controls.Add(this.txtEFreteToken);
            this.grpEFrete.Controls.Add(this.lblEFreteToken);
            this.grpEFrete.Controls.Add(this.txtEFreteIntegrador);
            this.grpEFrete.Controls.Add(this.lblEFreteIntegrador);
            this.grpEFrete.Location = new System.Drawing.Point(9, 158);
            this.grpEFrete.Name = "grpEFrete";
            this.grpEFrete.Size = new System.Drawing.Size(622, 207);
            this.grpEFrete.TabIndex = 32;
            this.grpEFrete.TabStop = false;
            this.grpEFrete.Text = "Configuração eFrete (CIOT)";
            this.grpEFrete.Visible = false;
            //
            // lblEFreteIntegrador
            //
            this.lblEFreteIntegrador.AutoSize = true;
            this.lblEFreteIntegrador.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lblEFreteIntegrador.Location = new System.Drawing.Point(12, 23);
            this.lblEFreteIntegrador.Name = "lblEFreteIntegrador";
            this.lblEFreteIntegrador.Size = new System.Drawing.Size(61, 15);
            this.lblEFreteIntegrador.TabIndex = 0;
            this.lblEFreteIntegrador.Text = "Integrador";
            //
            // txtEFreteIntegrador
            //
            this.txtEFreteIntegrador.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.txtEFreteIntegrador.Lines = new string[0];
            this.txtEFreteIntegrador.Location = new System.Drawing.Point(12, 41);
            this.txtEFreteIntegrador.MaxLength = 32767;
            this.txtEFreteIntegrador.Name = "txtEFreteIntegrador";
            this.txtEFreteIntegrador.PasswordChar = '\0';
            this.txtEFreteIntegrador.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtEFreteIntegrador.SelectedText = "";
            this.txtEFreteIntegrador.Size = new System.Drawing.Size(288, 23);
            this.txtEFreteIntegrador.TabIndex = 1;
            this.txtEFreteIntegrador.UseSelectable = true;
            this.txtEFreteIntegrador.UseStyleColors = true;
            this.txtEFreteIntegrador.TextChanged += new System.EventHandler(this.EFrete_TextChanged);
            //
            // lblEFreteToken
            //
            this.lblEFreteToken.AutoSize = true;
            this.lblEFreteToken.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lblEFreteToken.Location = new System.Drawing.Point(312, 23);
            this.lblEFreteToken.Name = "lblEFreteToken";
            this.lblEFreteToken.Size = new System.Drawing.Size(38, 15);
            this.lblEFreteToken.TabIndex = 2;
            this.lblEFreteToken.Text = "Token";
            //
            // txtEFreteToken
            //
            this.txtEFreteToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEFreteToken.Lines = new string[0];
            this.txtEFreteToken.Location = new System.Drawing.Point(312, 41);
            this.txtEFreteToken.MaxLength = 32767;
            this.txtEFreteToken.Name = "txtEFreteToken";
            this.txtEFreteToken.PasswordChar = '*';
            this.txtEFreteToken.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtEFreteToken.SelectedText = "";
            this.txtEFreteToken.Size = new System.Drawing.Size(298, 23);
            this.txtEFreteToken.TabIndex = 3;
            this.txtEFreteToken.UseSelectable = true;
            this.txtEFreteToken.UseStyleColors = true;
            this.txtEFreteToken.TextChanged += new System.EventHandler(this.EFrete_TextChanged);
            //
            // lblEFreteUsuario
            //
            this.lblEFreteUsuario.AutoSize = true;
            this.lblEFreteUsuario.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lblEFreteUsuario.Location = new System.Drawing.Point(12, 72);
            this.lblEFreteUsuario.Name = "lblEFreteUsuario";
            this.lblEFreteUsuario.Size = new System.Drawing.Size(45, 15);
            this.lblEFreteUsuario.TabIndex = 4;
            this.lblEFreteUsuario.Text = "Usuário";
            //
            // txtEFreteUsuario
            //
            this.txtEFreteUsuario.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.txtEFreteUsuario.Lines = new string[0];
            this.txtEFreteUsuario.Location = new System.Drawing.Point(12, 90);
            this.txtEFreteUsuario.MaxLength = 32767;
            this.txtEFreteUsuario.Name = "txtEFreteUsuario";
            this.txtEFreteUsuario.PasswordChar = '\0';
            this.txtEFreteUsuario.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtEFreteUsuario.SelectedText = "";
            this.txtEFreteUsuario.Size = new System.Drawing.Size(288, 23);
            this.txtEFreteUsuario.TabIndex = 5;
            this.txtEFreteUsuario.UseSelectable = true;
            this.txtEFreteUsuario.UseStyleColors = true;
            this.txtEFreteUsuario.TextChanged += new System.EventHandler(this.EFrete_TextChanged);
            //
            // lblEFreteSenha
            //
            this.lblEFreteSenha.AutoSize = true;
            this.lblEFreteSenha.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lblEFreteSenha.Location = new System.Drawing.Point(312, 72);
            this.lblEFreteSenha.Name = "lblEFreteSenha";
            this.lblEFreteSenha.Size = new System.Drawing.Size(38, 15);
            this.lblEFreteSenha.TabIndex = 6;
            this.lblEFreteSenha.Text = "Senha";
            //
            // txtEFreteSenha
            //
            this.txtEFreteSenha.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEFreteSenha.Lines = new string[0];
            this.txtEFreteSenha.Location = new System.Drawing.Point(312, 90);
            this.txtEFreteSenha.MaxLength = 32767;
            this.txtEFreteSenha.Name = "txtEFreteSenha";
            this.txtEFreteSenha.PasswordChar = '*';
            this.txtEFreteSenha.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtEFreteSenha.SelectedText = "";
            this.txtEFreteSenha.Size = new System.Drawing.Size(298, 23);
            this.txtEFreteSenha.TabIndex = 7;
            this.txtEFreteSenha.UseSelectable = true;
            this.txtEFreteSenha.UseStyleColors = true;
            this.txtEFreteSenha.TextChanged += new System.EventHandler(this.EFrete_TextChanged);
            //
            // lblEFreteAutenticacao
            //
            this.lblEFreteAutenticacao.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEFreteAutenticacao.FontSize = MetroFramework.MetroLabelSize.Small;
            this.lblEFreteAutenticacao.Location = new System.Drawing.Point(12, 124);
            this.lblEFreteAutenticacao.Name = "lblEFreteAutenticacao";
            this.lblEFreteAutenticacao.Size = new System.Drawing.Size(598, 70);
            this.lblEFreteAutenticacao.TabIndex = 8;
            this.lblEFreteAutenticacao.Text = "Prioridade de autenticação: Token, depois Usuário/Senha e, quando estes não forem informados, Certificado Digital. O Integrador é obrigatório somente para operações eFrete.";
            this.lblEFreteAutenticacao.WrapToLine = true;
            //
            // userConfiguracao_outrasconfiguracoes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpEFrete);
            this.Controls.Add(this.lblAvisoPreparacaoTLSAntesEnvioXML);
            this.Controls.Add(this.chkAtivarPreparacaoTLSAntesEnvioXML);
            this.Controls.Add(this.lbl_udVersaoQrCodeNFCe);
            this.Controls.Add(this.udVersaoQrCodeNFCe);
            this.Controls.Add(this.lbl_udTempoEnvioNFSe);
            this.Controls.Add(this.udTempoEnvioNFSe);
            this.Controls.Add(this.chkSalvarXMLDistribuicao);
            this.Name = "userConfiguracao_outrasconfiguracoes";
            this.Size = new System.Drawing.Size(640, 374);
            this.grpEFrete.ResumeLayout(false);
            this.grpEFrete.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroCheckBox chkSalvarXMLDistribuicao;
        private MetroFramework.Controls.MetroLabel lbl_udTempoEnvioNFSe;
        private MetroFramework.Controls.MetroTextBox udTempoEnvioNFSe;
        private MetroFramework.Controls.MetroLabel lbl_udVersaoQrCodeNFCe;
        private MetroFramework.Controls.MetroTextBox udVersaoQrCodeNFCe;
        private MetroFramework.Controls.MetroCheckBox chkAtivarPreparacaoTLSAntesEnvioXML;
        private MetroFramework.Controls.MetroLabel lblAvisoPreparacaoTLSAntesEnvioXML;
        private System.Windows.Forms.GroupBox grpEFrete;
        private MetroFramework.Controls.MetroLabel lblEFreteIntegrador;
        private MetroFramework.Controls.MetroTextBox txtEFreteIntegrador;
        private MetroFramework.Controls.MetroLabel lblEFreteToken;
        private MetroFramework.Controls.MetroTextBox txtEFreteToken;
        private MetroFramework.Controls.MetroLabel lblEFreteUsuario;
        private MetroFramework.Controls.MetroTextBox txtEFreteUsuario;
        private MetroFramework.Controls.MetroLabel lblEFreteSenha;
        private MetroFramework.Controls.MetroTextBox txtEFreteSenha;
        private MetroFramework.Controls.MetroLabel lblEFreteAutenticacao;
    }
}
