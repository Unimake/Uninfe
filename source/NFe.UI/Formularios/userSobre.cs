using NFe.Components;
using NFe.Settings;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace NFe.UI
{
    public partial class userSobre : UserControl1
    {
        public userSobre() => InitializeComponent();

        public override void UpdateControles()
        {
            base.UpdateControles();

            textBox_versao.Text = Propriedade.Versao;
            textbox_Compilacao.Text = "4.8.1";
            textBox_versaofw.Text = Environment.Version.ToString();

            lblDescricaoAplicacao.Text = Propriedade.DescricaoAplicacao;
            lblNomeAplicacao.Text = Propriedade.NomeAplicacao;
#if _BETA
            lblNomeAplicacao.Text += " - Versão BETA";
#endif
            labelTitle.Text = "Sobre o " + Propriedade.NomeAplicacao;

            //Atualizar o texto da licença de uso
            textBox_licenca.Text = "GNU General Public License\r\n\r\n";
            textBox_licenca.Text += Propriedade.NomeAplicacao + " - " + Propriedade.DescricaoAplicacao + "\r\n";
            textBox_licenca.Text += string.Format("Copyright (C) 2008-{0} {1}", DateTime.Today.Year, ConfiguracaoApp.NomeEmpresa) + "\r\n\r\n";
            textBox_licenca.Text += "Este programa é software livre; você pode redistribuí-lo e/ou modificá-lo sob os termos da Licença Pública Geral GNU, conforme publicada pela Free Software Foundation; tanto a versão 2 da Licença como (a seu critério) qualquer versão mais nova.\r\n\r\n";
            textBox_licenca.Text += "Este programa é distribuído na expectativa de ser útil, mas SEM QUALQUER GARANTIA; sem mesmo a garantia implícita de COMERCIALIZAÇÃO ou de ADEQUAÇÃO A QUALQUER PROPÓSITO EM PARTICULAR. Consulte a Licença Pública Geral GNU para obter mais detalhes.\r\n\r\n";
            textBox_licenca.Text += "Você deve ter recebido uma cópia da Licença Pública Geral GNU junto com este programa; se não, escreva para a Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA     02111-1307, USA ou consulte a licença oficial em http://www.gnu.org/licenses/.";

            textBox_DataUltimaModificacao.Text = Propriedade.DataHoraUltimaModificacaoAplicacao;

            lblEmpresa.Text = ConfiguracaoApp.NomeEmpresa;
            linkLabelSite.Visible = !string.IsNullOrEmpty(ConfiguracaoApp.Site);
            linkLabelSiteProduto.Visible = !string.IsNullOrEmpty(ConfiguracaoApp.SiteProduto);

            linkLabelSite.Text = ConfiguracaoApp.Site;
            linkLabelSiteProduto.Text = ConfiguracaoApp.SiteProduto;

            var elapsedDays = ConfiguracaoApp.ExecutionTime.Elapsed.Days + " dias ininterruptos.";

            if (ConfiguracaoApp.ExecutionTime.Elapsed.Days < 1)
            {
                elapsedDays = ConfiguracaoApp.ExecutionTime.Elapsed.Hours + " horas ininterruptas.";
            }

            if (ConfiguracaoApp.ExecutionTime.Elapsed.Hours < 1)
            {
                elapsedDays = "A menos de uma hora.";
            }

            txtElapsedDays.Text = elapsedDays;
        }

        private void linkLabelSite_Click(object sender, EventArgs e)
        {
            try
            {
                var url = (sender as MetroFramework.Controls.MetroLink).Text;
                if (url.Contains("@"))
                {
                    url = "mailto:" + url;
                }
                else
                {
                    url = "http://" + url;
                }

                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, ex.Message, "");
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Propriedade.UrlManualUniNFe);
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void userSobre_Load(object sender, EventArgs e)
        {
        }

        #region Form para gerar empresas

        private void button1_Click(object sender, EventArgs e)
        {
            GravarArqEmpresas();

            textBox2.Clear();
            MessageBox.Show("Empresas geradas com sucesso!");
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e) => IntNumero(e);

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        #endregion Form para gerar empresas

        #region Método para digitar somente números
        public static void IntNumero(KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }
        #endregion Método para digitar somente números

        #region Gravar várias empresas 

        public void GravarArqEmpresas()
        {
            var quantidade = Convert.ToInt32(textBox2.Text);

            var empresasele = new XElement("Empresa");
            var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));

            for (var i = 0; i <= quantidade - 1; i++)
            {
                Thread.Sleep(100);
                var cnpj = GerarCNPJ();
                var servico = TipoAplicativo.Todos;
                var nome = "Teste " + cnpj;

                empresasele.Add(new XElement(NFe.Components.NFeStrConstants.Registro,
                                new XAttribute(NFe.Components.TpcnResources.CNPJ.ToString(), cnpj),
                                new XAttribute(NFe.Components.NFeStrConstants.Servico, ((int)servico).ToString()),
                                new XElement(NFe.Components.NFeStrConstants.Nome, nome.Trim())));

                var pasta = Path.Combine(Propriedade.PastaExecutavel, cnpj);
                if (!Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }

                var fileName = Path.Combine(pasta, Propriedade.NomeArqConfig);

                var xmlConfigEmp = "<Empresa xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><PastaXmlEnvio>C:\\Projetos\\UniNFe\\trunk\\fontes\\uninfe\\bin\\Release\\06117473000150\\Envio</PastaXmlEnvio><PastaXmlRetorno>C:\\Projetos\\UniNFe\\trunk\\fontes\\uninfe\\bin\\Release\\06117473000150\\Retorno</PastaXmlRetorno><PastaXmlEnviado>C:\\Projetos\\UniNFe\\trunk\\fontes\\uninfe\\bin\\Release\\06117473000150\\Enviado</PastaXmlEnviado><PastaXmlErro>C:\\Projetos\\UniNFe\\trunk\\fontes\\uninfe\\bin\\Release\\06117473000150\\Erro</PastaXmlErro><PastaBackup>C:\\Projetos\\UniNFe\\trunk\\fontes\\uninfe\\bin\\Release\\06117473000150\\Backup</PastaBackup><PastaXmlEmLote>C:\\Projetos\\UniNFe\\trunk\\fontes\\uninfe\\bin\\Release\\06117473000150\\EnvioEmLote</PastaXmlEmLote><PastaValidar>C:\\Projetos\\UniNFe\\trunk\\fontes\\uninfe\\bin\\Release\\06117473000150\\Validar</PastaValidar><PastaDanfeMon /><PastaDownloadNFeDest>C:\\Projetos\\UniNFe\\trunk\\fontes\\uninfe\\bin\\Release\\06117473000150\\DownloadNFe</PastaDownloadNFeDest><CNPJ>06117473000150</CNPJ><Nome>Teste</Nome><UnidadeFederativaCodigo>41</UnidadeFederativaCodigo><AmbienteCodigo>2</AmbienteCodigo><tpEmis>1</tpEmis><UsaCertificado>true</UsaCertificado><CertificadoInstalado>false</CertificadoInstalado><CertificadoArquivo>C:\\Projetos\\Unimake_PV.pfx</CertificadoArquivo><CertificadoSenha>rY2bU5McRj8anl0BPZoaog==</CertificadoSenha><CertificadoPIN /><Certificado /><CertificadoDigitalThumbPrint /><GravarRetornoTXTNFe>false</GravarRetornoTXTNFe><DiasLimpeza>10</DiasLimpeza><TempoConsulta>2</TempoConsulta><UsuarioWS /><SenhaWS /><Servico>Todos</Servico><CompararDigestValueDFeRetornadoSEFAZ>false</CompararDigestValueDFeRetornadoSEFAZ><Documento>CNPJ</Documento><SalvarSomenteXMLDistribuicao>false</SalvarSomenteXMLDistribuicao><CriaPastasAutomaticamente>true</CriaPastasAutomaticamente><GravarEventosNaPastaEnviadosNFe>false</GravarEventosNaPastaEnviadosNFe><GravarEventosCancelamentoNaPastaEnviadosNFe>false</GravarEventosCancelamentoNaPastaEnviadosNFe><GravarEventosDeTerceiros>false</GravarEventosDeTerceiros><ArqNSU>false</ArqNSU><IndSinc>true</IndSinc><IndSincNFCe>true</IndSincNFCe><IndSincMDFe>false</IndSincMDFe><PastaExeUniDanfe /><PastaConfigUniDanfe /><ConfiguracaoDanfe /><ConfiguracaoCCe /><XMLDanfeMonNFe>false</XMLDanfeMonNFe><XMLDanfeMonProcNFe>false</XMLDanfeMonProcNFe><XMLDanfeMonDenegadaNFe>false</XMLDanfeMonDenegadaNFe><EmailDanfe /><AdicionaEmailDanfe>true</AdicionaEmailDanfe><IdentificadorCSC /><TokenCSC /><diretorioSalvarComo>AM</diretorioSalvarComo><FTPPastaAutorizados /><FTPPastaRetornos /><FTPNomeDoServidor /><FTPNomeDoUsuario /><FTPSenha /><FTPPorta>21</FTPPorta><FTPAtivo>false</FTPAtivo><FTPPassivo>false</FTPPassivo><FTPGravaXMLPastaUnica>false</FTPGravaXMLPastaUnica><UtilizaConversaoCFe>false</UtilizaConversaoCFe><NumeroCaixa>001</NumeroCaixa><RegTribISSQNSAT>MicroEmpresaMunicipal</RegTribISSQNSAT><IndRatISSQNSAT>S</IndRatISSQNSAT><RespTecCNPJ /><RespTecXContato /><RespTecEmail /><RespTecTelefone /><RespTecIdCSRT /><RespTecCSRT /></Empresa>";
                xmlConfigEmp = xmlConfigEmp.Replace("06117473000150", cnpj);

                var doc = new XmlDocument();
                doc.LoadXml(xmlConfigEmp);
                doc.Save(fileName);
            }

            xml.Add(empresasele);
            xml.Save(Propriedade.NomeArqEmpresas);
        }

        #endregion Gravar várias empresas 

        #region Cálculos para gerar CNPJ

        private string GerarCNPJ()
        {
            int soma = 0, resto = 0, numero4;

            string cnpj, numero, numero1, numero2, numero3;

            var verificador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var verificador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            //Gerar número aleatório
            var gera = new Random();
            var aleatorio = gera.Next(10000000, 99999999).ToString();

            numero1 = "0";
            numero2 = "0";
            numero3 = "0";
            numero4 = (gera.Next()) % 9;

            numero = numero1 + numero2 + numero3 + numero4;

            cnpj = aleatorio + numero;

            //Gerar primeiro dígito verificador
            for (var i = 0; i < 12; i++)
            {
                soma += int.Parse(cnpj[i].ToString()) * verificador1[i];
            }

            resto = soma % 11;
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            var digito = resto.ToString();
            cnpj += digito;

            //Gerar segundo dígito verificador
            soma = 0;

            for (var i = 0; i < 13; i++)
            {
                soma += int.Parse(cnpj[i].ToString()) * verificador2[i];
            }

            resto = soma % 11;

            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            var digito2 = resto.ToString();
            cnpj += digito2;

            return cnpj;
        }

        #endregion Cálculos para gerar CNPJ
    }
}
