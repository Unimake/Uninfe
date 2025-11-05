using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFe.Components;
using NFe.Settings;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace NFe.Service
{
    public class TaskLayouts : TaskAbst
    {
        public override void Execute()
        {
            Exception exx = null;
            var uext = NFe.Components.Propriedade.Extensao(Components.Propriedade.TipoEnvio.pedLayouts);
            string ExtRetorno = (this.NomeArquivoXML.ToLower().EndsWith(".xml") ? uext.EnvioXML : uext.EnvioTXT);
            string finalArqErro = uext.EnvioXML.Replace(".xml", ".err");

            string pastaRetorno = Propriedade.PastaGeralRetorno;
            string arqRetorno = pastaRetorno + "\\" + Functions.ExtrairNomeArq(this.NomeArquivoXML, null) + Propriedade.Extensao(Propriedade.TipoEnvio.pedLayouts).RetornoXML;

            Functions.DeletarArquivo(arqRetorno);
            Functions.DeletarArquivo(Propriedade.PastaGeralRetorno + "\\" + Functions.ExtrairNomeArq(this.NomeArquivoXML, null) + finalArqErro);

            //Document document = new Document();
            try
            {
                CriaPDFLayout(arqRetorno);
            }
            catch (DocumentException de)
            {
                exx = de;
            }
            catch (IOException ioe)
            {
                exx = ioe;
            }
            catch(Exception ex)
            {
                exx = ex;
            }
            finally
            {
                Functions.DeletarArquivo(this.NomeArquivoXML);

                if (exx != null)
                {
                    Functions.DeletarArquivo(arqRetorno);

                    try
                    {
                        NFe.Service.TFunctions.GravarArqErroServico(this.NomeArquivoXML, ExtRetorno, finalArqErro, exx);
                    }
                    catch { }
                }
            }
        }

        public void CriaPDFLayout(string arqRetorno)
        {
                using (Document document = new Document(PageSize.A4, 20f, 20f, 80f, 10f))
                {
                    var pdfWriter = PdfWriter.GetInstance(document, new FileStream(arqRetorno, FileMode.Create));

                    pdfWriter.PageEvent = new ITextEvents();

                    document.AddTitle("Layout UniNFe");
                    document.AddSubject("Layout de arquivos texto");
                    document.AddCreator(Propriedade.DescricaoAplicacao);
                    document.AddAuthor(ConfiguracaoApp.NomeEmpresa);

                    //document.Header = header;

                    // landscape
                    document.SetPageSize(PageSize.A4.Rotate());

                    document.Open();

                    PdfPTable table = null;// new PdfPTable(3);

                    int n = 0;
                    foreach (Propriedade.TipoEnvio item in Enum.GetValues(typeof(Propriedade.TipoEnvio)))
                    {
                    if ((n % 16) == 0 || table == null)
                        {
                            if (table != null)
                            {
                                document.Add(table);
                                document.NewPage();
                            }

                            document.Add(new Phrase("Extensões permitidas" + (n > 0 ? "..." : ""), new Font(Font.FontFamily.HELVETICA, 18)));

                            table = new PdfPTable(3);
                            table.HorizontalAlignment = 0;
                            table.TotalWidth = document.PageSize.GetRight(41);
                            table.LockedWidth = true;
                            table.PaddingTop = 2;
                            table.SetWidths(new float[] { 40f, 40f, 100f });

                            table.AddCell(this.titulo("Envios"));
                            table.AddCell(this.titulo("Retornos"));
                            table.AddCell(this.titulo("Descrição"));
                        }
                        ++n;
                        var EXT = Propriedade.Extensao(item);
                        table.AddCell(this.texto(this.fmtname(EXT.EnvioXML) + this.fmtname(EXT.EnvioTXT," ou ")));
                        table.AddCell(this.texto(this.fmtname(EXT.RetornoXML) + this.fmtname(EXT.RetornoTXT," ou ")));
                        table.AddCell(this.texto(EXT.descricao));
                    }
                    if (table != null)
                    {
                        document.Add(table);
                        document.NewPage();
                    }
                    ///
                    /// Layout NFe/NFCe
                    /// 

                string old = "";

                    n = 0;
                    table = null;
                    //document.Add(new Phrase("Layout de notas NF-e/NFc-e", new Font(Font.FontFamily.HELVETICA, 18)));
                    foreach (KeyValuePair<string, string> key in new NFe.ConvertTxt.ConversaoTXT().LayoutTXT)
                    {
                    if (old == key.Value.ToString().Substring(1)) continue;

                    old = key.Value.ToString().Substring(1);

                        if (key.Key.Contains("_"))
                            ///
                            /// só considera o layout da versao >= 3.1
                            if (key.Key.Contains("_200")) continue;

                    if ((n % 20) == 0 || table == null)
                        {
                            if (table != null)
                            {
                                document.Add(table);
                                document.NewPage();
                            }
                            document.Add(new Phrase("Layout de notas NF-e/NFc-e" + (n>0 ? "...":""), new Font(Font.FontFamily.HELVETICA, 18)));
                            table = new PdfPTable(2);
                            table.AddCell(this.titulo("Segmento"));
                            table.AddCell(this.titulo("Formato"));
                            table.HorizontalAlignment = 0;
                            table.TotalWidth = document.PageSize.GetRight(41);
                            table.LockedWidth = true;
                            table.PaddingTop = 2;
                        table.SetWidths(new float[] { 15f, 100f });
                        }
                        ++n;

                            table.AddCell(this.texto(key.Key));
                        table.AddCell(this.texto(key.Value.ToString().Substring(1)));
                    }
                    if (table != null)
                    {
                        document.Add(table);
                        document.NewPage();
                    }

                    document.Close();
                }
            }

        string fmtname(string value, string prefix = "")
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.StartsWith("-")) return prefix + "???" + value;
            return prefix + value;
        }
        
        PdfPCell titulo(string value)
        {
            PdfPCell cell = new PdfPCell(new Phrase(value, FontFactory.GetFont(FontFactory.COURIER, 10, Font.BOLD)));
            cell.BackgroundColor = new BaseColor(0xC0, 0xC0, 0xC0);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            return cell;
        }
        
        PdfPCell texto(string value)
        {
            PdfPCell cell = new PdfPCell(new Phrase(value, FontFactory.GetFont(FontFactory.COURIER, 10, Font.NORMAL)));
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            return cell;
        }
    }
    public class ITextEvents : PdfPageEventHelper
    {
        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        #region Fields
        private string _header;
        #endregion

    }
}
