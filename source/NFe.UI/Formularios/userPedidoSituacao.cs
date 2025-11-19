using NFe.Components;
using NFe.Service;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Unimake.Business.DFe.Servicos;

using CTeConsStatServ = Unimake.Business.DFe.Xml.CTe.ConsStatServCte;
using CTeStatusServico = Unimake.Business.DFe.Servicos.CTe.StatusServico;
using MDFeConsStatServ = Unimake.Business.DFe.Xml.MDFe.ConsStatServMDFe;
using MDFeStatusServico = Unimake.Business.DFe.Servicos.MDFe.StatusServico;
using NF3eConsStatServ = Unimake.Business.DFe.Xml.NF3e.ConsStatServNF3e;
using NF3eStatusServico = Unimake.Business.DFe.Servicos.NF3e.StatusServico;
using NFComConsStatServ = Unimake.Business.DFe.Xml.NFCom.ConsStatServNFCom;
using NFComStatusServico = Unimake.Business.DFe.Servicos.NFCom.StatusServico;
using NFeConsStatServ = Unimake.Business.DFe.Xml.NFe.ConsStatServ;
using NFeStatusServico = Unimake.Business.DFe.Servicos.NFe.StatusServico;
using NFCeStatusServico = Unimake.Business.DFe.Servicos.NFCe.StatusServico;


namespace NFe.UI
{
    public partial class userPedidoSituacao : UserControl1
    {
        private int Emp;
        private bool todasEmpresas;

        public userPedidoSituacao()
        {
            InitializeComponent();
        }

        public override void UpdateControles()
        {
            base.UpdateControles();

            cbServico.SelectedIndexChanged -= cbServico_SelectedIndexChanged;
            cbEmpresa.SelectedIndexChanged -= cbEmpresa_SelectedIndexChanged;
            try
            {
                cbAmbiente.DataSource = EnumHelper.ToList(typeof(TipoAmbiente), true, true);
                cbAmbiente.DisplayMember = "Value";
                cbAmbiente.ValueMember = "Key";

                var lista = EnumHelper.ToList(typeof(TipoEmissao), true, true, "2,4,5,9");

                cbEmissao.DataSource = lista;
                cbEmissao.DisplayMember = "Value";
                cbEmissao.ValueMember = "Key";

                cbServico.DataSource = uninfeDummy.DatasouceTipoAplicativo(true);
                cbServico.DisplayMember = "Value";
                cbServico.ValueMember = "Key";

                cbEmpresa.DataSource = Auxiliar.CarregaEmpresa(true, true);
                cbEmpresa.ValueMember = "Key";
                cbEmpresa.DisplayMember = NFeStrConstants.Nome;

                comboUf.DisplayMember = "nome";
                comboUf.ValueMember = "valor";
                comboUf.DataSource = Functions.CarregaEstados();

                int posicao = uninfeDummy.xmlParams.ReadValue(GetType().Name, "last_empresa", 0);
                if (posicao > (cbEmpresa.DataSource as System.Collections.ArrayList).Count)
                    posicao = 0;

                cbEmpresa.SelectedIndex = posicao;
                cbVersao.SelectedIndex = 0;
            }
            finally
            {
                cbServico.SelectedIndexChanged += cbServico_SelectedIndexChanged;
                cbEmpresa.SelectedIndexChanged += cbEmpresa_SelectedIndexChanged;

                cbEmpresa_SelectedIndexChanged(null, null);
                if (cbServico.SelectedValue == null)
                {
                    ChangeVersao(Empresas.Configuracoes[0].Servico);
                }
                else
                {
                    ChangeVersao((TipoAplicativo)cbServico.SelectedValue);
                }
            }
        }

        private void cbEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            Emp = -1;
            cbServico.SelectedIndexChanged -= cbServico_SelectedIndexChanged;
            cbServico.Enabled = true;

            try
            {
                if (!cbEmpresa.SelectedValue.Equals("Todos"))
                {
                    var list = (cbEmpresa.DataSource as System.Collections.ArrayList)[cbEmpresa.SelectedIndex] as ComboElem;
                    Emp = Empresas.FindConfEmpresaIndex(list.Valor, EnumHelper.StringToEnum<TipoAplicativo>(list.Servico));
                    if (Emp >= 0)
                    {
                        uninfeDummy.xmlParams.WriteValue(GetType().Name, "last_empresa", cbEmpresa.SelectedIndex);
                        uninfeDummy.xmlParams.Save();

                        comboUf.SelectedValue = Functions.CodigoParaUF(Empresas.Configuracoes[Emp].UnidadeFederativaCodigo).Trim();

                        //Posicionar o elemento da combo Ambiente
                        cbAmbiente.SelectedValue = Empresas.Configuracoes[Emp].AmbienteCodigo;

                        //Exibir CNPJ da empresa
                        txtCNPJ.Text = uninfeDummy.FmtCnpjCpf(Empresas.Configuracoes[Emp].CNPJ, true);

                        //Posicionar o elemento da combo tipo de emissão
                        if (Empresas.Configuracoes[Emp].tpEmis == 1 || Empresas.Configuracoes[Emp].tpEmis == 6 ||
                            Empresas.Configuracoes[Emp].tpEmis == 7 || Empresas.Configuracoes[Emp].tpEmis == 8)
                        {
                            cbEmissao.SelectedValue = Empresas.Configuracoes[Emp].tpEmis;
                        }

                        ChangeVersao(Empresas.Configuracoes[Emp].Servico);

                        //Posicionar o elemento da combo tipo de servico
                        if (Empresas.Configuracoes[Emp].Servico != TipoAplicativo.Todos)
                        {
                            cbServico.SelectedValue = (int)Empresas.Configuracoes[Emp].Servico;
                        }
                        else
                        {
                            cbServico.SelectedValue = (int)TipoAplicativo.Nfe;
                            cbServico.Enabled = true;
                        }
                    }
                }
                else
                {
                    comboUf.SelectedValue = string.Empty;
                    txtCNPJ.Text = string.Empty;
                    cbServico.SelectedValue = string.Empty;
                }
            }
            catch (Exception ex)
            {
                cbServico.Enabled = false;
                MetroFramework.MetroMessageBox.Show(uninfeDummy.mainForm, ex.Message, "");
            }
            finally
            {
                cbServico.SelectedIndexChanged += cbServico_SelectedIndexChanged;
                buttonPesquisa.Enabled =
                    cbAmbiente.Enabled =
                    cbEmissao.Enabled =
                    cbVersao.Enabled =
                    cbServico.Enabled =
                    comboUf.Enabled = Emp >= 0;

                if (cbEmpresa.SelectedValue.Equals("Todos"))
                {
                    cbAmbiente.Enabled = true;
                    buttonPesquisa.Enabled = true;
                }
                else
                {
                }
            }
        }

        private void ChangeVersao(TipoAplicativo Servico)
        {
            switch (Servico)
            {
                case TipoAplicativo.Todos:
                    cbVersao.Enabled = cbServico.Enabled = true;
                    cbVersao.Items.Clear();
                    cbVersao.Items.AddRange(new object[] { "4.00", "3.00", "1.00" });
                    cbVersao.SelectedItem = "4.00";
                    break;

                case TipoAplicativo.Nfe:
                    cbVersao.Enabled = true;
                    cbVersao.Items.Clear();
                    cbVersao.Items.AddRange(new object[] { "4.00" });
                    cbVersao.SelectedItem = "4.00";
                    break;

                case TipoAplicativo.NFCe:
                    cbVersao.Enabled = true;
                    cbVersao.Items.Clear();
                    cbVersao.Items.AddRange(new object[] { "4.00" });
                    cbVersao.SelectedItem = "4.00";
                    break;

                case TipoAplicativo.Cte:
                    cbVersao.Enabled = true;
                    cbVersao.Items.Clear();
                    cbVersao.Items.AddRange(new object[] { "4.00" });
                    cbVersao.SelectedItem = "4.00";
                    break;

                case TipoAplicativo.MDFe:
                    cbVersao.Enabled = true;
                    cbVersao.Items.Clear();
                    cbVersao.Items.AddRange(new object[] { "3.00" });
                    cbVersao.SelectedItem = "3.00";
                    break;

                case TipoAplicativo.SATeMFE:
                    cbVersao.Enabled = false;
                    break;

                case TipoAplicativo.NF3e:
                case TipoAplicativo.NFCom:
                    cbVersao.Enabled = true;
                    cbVersao.Items.Clear();
                    cbVersao.Items.AddRange(new object[] { "1.00" });
                    cbVersao.SelectedItem = "1.00";
                    break;
            }
        }

        private void cbServico_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbServico.SelectedValue != null)
            {
                TipoAplicativo servico = (TipoAplicativo)cbServico.SelectedValue;
                ChangeVersao(servico);
            }
        }

        private void buttonPesquisa_Click(object sender, EventArgs e)
        {
            Refresh();
            txtMensagem.Clear();
            metroGridSituacao.Rows.Clear();

            todasEmpresas = cbEmpresa.SelectedValue.Equals("Todos");

            try
            {
                TipoEmissao tpEmis = (TipoEmissao)cbEmissao.SelectedValue;
                int amb = (int)cbAmbiente.SelectedValue;
                string versao = cbVersao.SelectedItem.ToString();
                TipoAplicativo servico = TipoAplicativo.Nulo;
                int cUF = 0;

                if (!todasEmpresas)
                {
                    servico = (TipoAplicativo)cbServico.SelectedValue;
                    cUF = Functions.UFParaCodigo(comboUf.SelectedValue.ToString());

                    switch (servico)
                    {
                        case TipoAplicativo.Cte:
                            if (tpEmis == TipoEmissao.ContingenciaSVCAN)// cbEmissao.SelectedIndex == 4)
                                throw new Exception("CT-e não dispõe do tipo de contingência SVCAN.");
                            break;

                        case TipoAplicativo.Nfe:
                            if (tpEmis == TipoEmissao.ContingenciaSVCSP)// cbEmissao.SelectedIndex == 3)
                                throw new Exception("NF-e não dispõe do tipo de contingência SVCSP.");
                            break;

                        case TipoAplicativo.MDFe:
                            if (tpEmis != TipoEmissao.Normal)
                                throw new Exception("MDF-e só dispõe do tipo de emissão Normal.");
                            break;

                        case TipoAplicativo.NFCe:
                            if (tpEmis != TipoEmissao.Normal)
                                throw new Exception("NFC-e só dispõe do tipo de emissão Normal.");
                            break;
                    }
                }

                Formularios.Wait.Show("Consulta a situação do serviço...");

                if (todasEmpresas)
                {
                    for (int i = 0; i < Empresas.Configuracoes.Count; i++)
                    {
                        int emp = Empresas.FindConfEmpresaIndex(Empresas.Configuracoes[i].CNPJ, Empresas.Configuracoes[i].Servico);

                        servico = Empresas.Configuracoes[emp].Servico;
                        cUF = Empresas.Configuracoes[emp].UnidadeFederativaCodigo;

                        switch (servico)
                        {
                            case TipoAplicativo.Todos:
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.Nfe, amb, cUF, "4.00");
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.NFCe, amb, cUF, "4.00");
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.MDFe, amb, cUF, "3.00");
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.Cte, amb, cUF, "4.00");
                                break;

                            case TipoAplicativo.Nfe:
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.Nfe, amb, cUF, "4.00");
                                break;

                            case TipoAplicativo.Cte:
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.Cte, amb, cUF, "4.00");
                                break;

                            case TipoAplicativo.MDFe:
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.MDFe, amb, cUF, "3.00");
                                break;

                            case TipoAplicativo.NFCe:
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.NFCe, amb, cUF, "4.00");
                                break;

                            case TipoAplicativo.NF3e:
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.NF3e, amb, cUF, "1.00");
                                break;

                            case TipoAplicativo.NFCom:
                                AdicionarConsultaNaGrid(emp, TipoAplicativo.NFCom, amb, cUF, "1.00");
                                break;
                        }
                    }
                }
                else
                {
                    AdicionarConsultaNaGrid(Emp, servico, amb, cUF, versao);
                }
            }
            catch (Exception ex)
            {
                txtMensagem.Text = ex.Message;
            }
            finally
            {
                Formularios.Wait.Close();
            }
        }


        private void AdicionarConsultaNaGrid(int emp, TipoAplicativo servico, int amb, int cUF, string versao)
        {
            var empresa = Empresas.Configuracoes[emp];
            string nomeEmpresa = empresa.Nome;
            string uf = Functions.CodigoParaUF(empresa.UnidadeFederativaCodigo);
            string tipoServico = servico.ToString();

            // Executa a consulta diretamente pela DLL e obtém o resultado
            string result = ExecutarConsulta(emp, servico, amb, cUF, versao);

            metroGridSituacao.Rows.Add(new object[] { nomeEmpresa, uf, tipoServico, result });
            Application.DoEvents();
        }

        private string ExecutarConsulta(int emp, TipoAplicativo servico, int amb, int cUF, string versao)
        {
            try
            {
                var configuracao = todasEmpresas ? ToConfiguracaoPorEmpresa(Empresas.Configuracoes[emp], versao, cUF, amb) :
                                    ToConfiguracao(Empresas.Configuracoes[emp], versao);


                switch (servico)
                {
                    case TipoAplicativo.Nfe:
                        {
                            var consStatServ = new NFeConsStatServ
                            {
                                Versao = versao,
                                TpAmb = (TipoAmbiente)amb,
                                CUF = (UFBrasil)cUF,
                                XServ = "STATUS"
                            };
                            var statusServico = new NFeStatusServico(consStatServ, configuracao);
                            statusServico.Executar();
                            return statusServico.Result.XMotivo;
                        }

                    case TipoAplicativo.NFCe:
                        {
                            var consStatServ = new NFeConsStatServ
                            {
                                Versao = versao,
                                TpAmb = (TipoAmbiente)amb,
                                CUF = (UFBrasil)cUF,
                                XServ = "STATUS"
                            };
                            var statusServico = new NFCeStatusServico(consStatServ, configuracao);
                            statusServico.Executar();
                            return statusServico.Result.XMotivo;
                        }

                    case TipoAplicativo.Cte:
                        {
                            var consStatServ = new CTeConsStatServ
                            {
                                Versao = versao,
                                TpAmb = (TipoAmbiente)amb,
                                CUF = (UFBrasil)cUF,
                                XServ = "STATUS"
                            };
                            var statusServico = new CTeStatusServico(consStatServ, configuracao);
                            statusServico.Executar();
                            return statusServico.Result.XMotivo;
                        }

                    case TipoAplicativo.MDFe:
                        {
                            var consStatServ = new MDFeConsStatServ
                            {
                                Versao = versao,
                                TpAmb = (TipoAmbiente)amb,
                                XServ = "STATUS"
                            };
                            var statusServico = new MDFeStatusServico(consStatServ, configuracao);
                            statusServico.Executar();
                            return statusServico.Result.XMotivo;
                        }

                    case TipoAplicativo.NFCom:
                        {
                            var consStatServ = new NFComConsStatServ
                            {
                                Versao = versao,
                                TpAmb = (TipoAmbiente)amb,
                                XServ = "STATUS"
                            };
                            var statusServico = new NFComStatusServico(consStatServ, configuracao);
                            statusServico.Executar();
                            return statusServico.Result.XMotivo;
                        }

                    case TipoAplicativo.NF3e:
                        {
                            var consStatServ = new NF3eConsStatServ
                            {
                                Versao = versao,
                                TpAmb = (TipoAmbiente)amb,
                                XServ = "STATUS"
                            };
                            var statusServico = new NF3eStatusServico(consStatServ, configuracao);
                            statusServico.Executar();
                            return statusServico.Result.XMotivo;
                        }

                    default:
                        return "Serviço de consulta não disponível para este modelo de documento.";
                }
            }
            catch (Exception ex)
            {
                // Retorna a mensagem de exceção para ser exibida na grid
                return ex.Message;
            }
        }


        private void metroGridSituacao_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            txtMensagem.Text = metroGridSituacao.SelectedRows[0].Cells[columnSitucao.Index].Value.ToString();
        }

        private Configuracao ToConfiguracao(Empresa empresa, string schemaVersao)
        {

            var config = new Configuracao
            {
                TipoDFe = (TipoDFe)MapearServicoParaTipoDFe((TipoAplicativo)cbServico.SelectedValue),
                TipoEmissao = (TipoEmissao)Convert.ToInt32(cbEmissao.SelectedValue),
                CodigoUF = Functions.UFParaCodigo(comboUf.SelectedValue.ToString()),
                TipoAmbiente = (TipoAmbiente)cbAmbiente.SelectedValue,
                SchemaVersao = cbVersao.SelectedItem.ToString(),
                CertificadoDigital = empresa.X509Certificado
            };

            if (!string.IsNullOrEmpty(ConfiguracaoApp.ProxyServidor))
            {
                config.HasProxy = true;
                config.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                config.ProxyPassword = ConfiguracaoApp.ProxySenha;
            }

            return config;
        }

        private Configuracao ToConfiguracaoPorEmpresa(Empresa empresa, string schemaVersao, int cUF, int amb)
        {
            var config = new Configuracao

            {
                TipoDFe = (TipoDFe)MapearServicoParaTipoDFe(empresa.Servico),
                TipoEmissao = (TipoEmissao)empresa.tpEmis,
                CodigoUF = cUF,
                TipoAmbiente = (TipoAmbiente)amb,
                SchemaVersao = schemaVersao,
                CertificadoDigital = empresa.X509Certificado
            };

            if (!string.IsNullOrEmpty(ConfiguracaoApp.ProxyServidor))
            {
                config.HasProxy = true;
                config.ProxyUser = ConfiguracaoApp.ProxyUsuario;
                config.ProxyPassword = ConfiguracaoApp.ProxySenha;
            }

            return config;
        }


        private TipoDFe MapearServicoParaTipoDFe(TipoAplicativo servico)
        {
            switch (servico)
            {
                case TipoAplicativo.Nfe: return TipoDFe.NFe;
                case TipoAplicativo.NFCe: return TipoDFe.NFCe;
                case TipoAplicativo.Cte: return TipoDFe.CTe;
                case TipoAplicativo.MDFe: return TipoDFe.MDFe;
                case TipoAplicativo.NF3e: return TipoDFe.NF3e;
                case TipoAplicativo.NFCom: return TipoDFe.NFCom;
                default: return TipoDFe.Desconhecido;
            }
        }
    }
}