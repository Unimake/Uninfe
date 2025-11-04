using System;
using System.Xml;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NFe.Components
{
	public class XMLIniFile
	{
        public enum Store { SameElement, MultiElements };

        public XmlDocument xmldoc{get; private set;}
		private string xmlfilename = "";
		private string aDocElementname = "Principal";
		private int aDocElementVersion = 1;
		private Store aXmlFormat = Store.SameElement;
		private bool Modified = true;
        public string namespaceURI { get; set; }
      
		const string stxmlini_ValueNotExists = "Value '{0} - {1}' not exists!";
		const string stxmlini_PathNotExists = "Path '{0}' not exists!";
		const string stxmlini_CannotCreatePath = "Cannot create '{0}'";
		const string stxmlini_Version = "Version";

		public XMLIniFile(string filename)
		{
			xmlfilename = filename;
			xmldoc = new XmlDocument();
			DocElementname = "Principal";
			DocElementVersion = 1;
			aXmlFormat = Store.SameElement;
			try
			{
				xmldoc.Load(filename);
			}
			catch{}
			Modified = false;
		}

        ~XMLIniFile()
        {
            this.Save();
        }

		#region Property

		public string DocElementname
		{
			get{return aDocElementname;}
			set{aDocElementname = value;}
		}

		public int DocElementVersion
		{
			get{return aDocElementVersion;}
			set{aDocElementVersion = value;}
		}

		#endregion

		#region Save Functions

		public void Save()
		{
            if (this.xmlfilename == "")
                return;

			try
			{
				if (Modified)
					xmldoc.Save(this.xmlfilename);

                Modified = false;
			}
			catch//( Exception ex )
			{
				//MessageBox.Show( "Save: "+ex.Message );
			}
		}

		#endregion

		
        #region Delete Functions
       
		public void DeleteValue(string Path, string ValueSection)   //ok
		{
			XmlNode n = GetPathNode(Path, false);
			if (n != null)
			{
				//MessageBox.Show(Path+"\n\r"+ValueSection);
				if (n.Attributes != null && n.Attributes.GetNamedItem( ValueSection )!=null)
				{
					n.Attributes.RemoveNamedItem(ValueSection);
                    this.Modified = true;
					return;
				}
				for(int i = 0; i < n.ChildNodes.Count; ++i)
					if (n.ChildNodes[i].LocalName == ValueSection)
					{
						XmlNode d = n.ChildNodes[i];
                        n.RemoveChild(d);
                        this.Modified = true;
						return;
					}
			}
		}

		#endregion

		#region Read Functions

        public Int32 ReadValue(string Path, string ValueSection, Int32 Default)
		{
            try
            {
                if (ValueExists(Path, ValueSection))
                {
                    var val = ReadThisValue(Path, ValueSection);
                    if (val != "")
                    {
                        return Convert.ToInt32(val);
                    }
                }
            }
            catch
            {
            }

		    return Default;
		}
		public string ReadValue(string Path, string ValueSection, string Default)
		{
            try
            {
                if (ValueExists(Path, ValueSection))
                    return ReadThisValue(Path, ValueSection);
            }
            catch 
            {
            }

			return Default;
		}

        protected string ReadThisValue(string Path, string ValueSection)	//OK
		{
			XmlNode n = GetPathNode(Path, false);
			try
			{
				if (n != null)
				{
					if (n.Attributes.GetNamedItem( ValueSection )!=null)
					{
						n = n.Attributes.GetNamedItem( ValueSection );
						return n.Value;
					}
					for(int i=0; i < n.ChildNodes.Count; ++i)
						if (n.ChildNodes[i].LocalName == ValueSection)
						{
							return n.ChildNodes[i].InnerText;
						}
				}
			}
			catch
			{
			}

			return "";
		}

        #endregion

        #region Write Functions

        public void WriteValue(string Path, string ValueSection, double Value)	//ok
		{
			WriteThisValue( Translate(Path), Translate(ValueSection), Convert.ToString(Value) );//.Replace(',','.') );
		}
        public void WriteValue(string Path, string ValueSection, string Value)	//ok
        {
            WriteThisValue(Translate(Path), Translate(ValueSection), Value);
        }

        protected void WriteThisValue(string Path, string ValueSection, string Value)
		{
			try
			{
				XmlNode node = GetPathNode(Path, true);
				if (node!=null)
				{
					if (node.Attributes.GetNamedItem( ValueSection )!=null)
					{
						node = node.Attributes.GetNamedItem( ValueSection );
						node.Value = Value;
					}
					else
					{
						XmlNode xmlInf;
						XmlAttribute xmlSection;

						switch(aXmlFormat)
						{
							case Store.SameElement:
							{
								//
								// cria um elemento no mesmo path+item
								// 
								xmlSection = xmldoc.CreateAttribute(ValueSection);
								xmlSection.Value = Value;
								node.Attributes.Append(xmlSection);
								break;
							}

							case Store.MultiElements:
							{
								//
								// cria um elemento no path com niveis
								// 
								for(int i=0; i<node.ChildNodes.Count; ++i)
									if (node.ChildNodes[i].LocalName==ValueSection)
									{
										node.ChildNodes[i].InnerText = Value;
										return;
									}
								xmlInf = xmldoc.CreateElement("", ValueSection, "");
                                if (!string.IsNullOrEmpty(Value))
								    xmlInf.InnerText = Value;
								node.AppendChild(xmlInf);
								break;
							}
						}
					}
                    Modified = true;
                }
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message+"\n\r"+String.Format("N„o pode salvar '{0} - {1} = {2}'", Path, ValueSection, Value));
			}
		}

        #endregion

        #region Misc Functions

        public void CenterForm(Form xform)
        {
            //xform.Location = new Point((SystemInformation.VirtualScreen.Width - xform.Size.Width) / 2, (SystemInformation.VirtualScreen.Height - xform.Size.Height) / 2);

            Screen screen = Screen.FromControl(xform);
            Rectangle workingArea = screen.WorkingArea;
            xform.Location = new Point()
            {
                X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - xform.Width) / 2),
                Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - xform.Height) / 2)
            };
        }

        public bool LoadForm(Form xform, string aSection, bool forcaleitura)
		{
			string section = xform.Name + aSection;

            if (!this.ValueExists(section, "top") && !this.ValueExists(section, "WindowState"))
			{
                if (xform.StartPosition == FormStartPosition.Manual && xform.MdiParent == null)
                {
                    CenterForm(xform);
                }
				return false;
			}
			else
			{
                if (this.ValueExists(section, "WindowState"))
                {
                    switch (this.ReadValue(section, "WindowState", 0))
                    {
                        case 2: 
                            xform.WindowState = FormWindowState.Maximized; 
                            break;
                        default: 
                            xform.WindowState = FormWindowState.Normal; 
                            break;
                    }
                }
				if (xform.WindowState == FormWindowState.Normal || forcaleitura)
				{
					int left	= this.ReadValue(section, "left",xform.Location.X);//Left);
					int top		= this.ReadValue(section, "top", xform.Location.Y);//Top);

					int width  = xform.Size.Width, 
						height = xform.Size.Height;

					switch (xform.FormBorderStyle)
					{
						case FormBorderStyle.Sizable:
						case FormBorderStyle.SizableToolWindow:
                        case FormBorderStyle.None:
							width	= this.ReadValue(section, "width",	xform.Size.Width);
							height	= this.ReadValue(section, "height", xform.Size.Height);
							break;
					}

					if (xform.MdiParent==null)
					{
                        if ((height + top) > SystemInformation.VirtualScreen.Height || top < 0)
							top = 0;
						if ((width + left) > SystemInformation.VirtualScreen.Width || left < 0)
							left = 0;
					}
					xform.StartPosition = FormStartPosition.Manual;
					xform.Location = new Point(left, top);
					xform.Size = new Size(width, height);
				}
				return true;
			}
		}
        
        public void SaveForm(Form xform, string aSection)
		{
            if (xform == null) return;
            if (xform.Name == "") return;

			string section = xform.Name + aSection;

			if (xform.WindowState != FormWindowState.Maximized)
			{
				this.WriteValue(section, "left",	xform.Location.X);
				this.WriteValue(section, "top",		xform.Location.Y);
				switch (xform.FormBorderStyle)
				{
					case FormBorderStyle.Sizable:
					case FormBorderStyle.SizableToolWindow:
                    case FormBorderStyle.None:
						this.WriteValue(section, "width",	xform.Size.Width);
						this.WriteValue(section, "height",	xform.Size.Height);
						break;
					default:
						this.DeleteValue(section, "width");
						this.DeleteValue(section, "height");
						break;
				}
				this.DeleteValue(section, "WindowState");
			}
			else
			{
				this.DeleteValue(section, "left");
				this.DeleteValue(section, "top");
				this.DeleteValue(section, "width");
				this.DeleteValue(section, "height");
				this.WriteValue(section, "WindowState",(int)xform.WindowState);
			}
		}

        protected void CheckInitialized()	//ok
		{
			if (xmldoc.DocumentElement == null)
			{
				if (DocElementname.Length==0)
					DocElementname = "Principal";

				Modified = true;

                XmlDeclaration node;
                node = xmldoc.CreateXmlDeclaration("1.0", "iso-8859-1", "yes");
                xmldoc.InsertBefore(node, xmldoc.DocumentElement);
				//XmlNode childNode = xmldoc.AppendChild(node);
                XmlNode xmlInf = xmldoc.CreateElement(DocElementname, this.namespaceURI);
                xmldoc.AppendChild(xmlInf);

				//XmlNode xmlInf = xmldoc.CreateElement("", DocElementname, "");
                if (DocElementVersion > 0)
                {
                    XmlAttribute xmlVersion = xmldoc.CreateAttribute("Version");
                    xmlVersion.Value = Convert.ToString(DocElementVersion);
                    xmlInf.Attributes.Append(xmlVersion);
                    xmldoc.AppendChild(xmlInf);
                }
			}
		}

        protected string ConvertToOEM(string FBuffer)
        {
            const string FAnsi = (" ·ÈÌÛ˙¡…Õ”⁄Á«‡ËÏÚ˘¿»Ã“Ÿ„ı√’∫™ß—‚‰ÂÍÎÔÓƒ≈Ù˚ˇ÷‹Ò¸¬");
            const string FOEM = (" aeiouAEIOUcCaeiouAEIOUaoAOoa.NaaaeeiiAAouyOUnuA");
            int L, P;
            char X;
            string result = "";

            for (L = 0; L < FBuffer.Length; ++L)
            {
                X = (char)FBuffer[L];
                P = FAnsi.IndexOf(X);
                if (P >= 0) X = FOEM[P];
                result += X;
            }
            return result;
        }

        [System.Diagnostics.DebuggerHidden()]
        protected string Translate(string Value)	// OK
		{
			int I;
			string T;
			string result;
			char X;

			result = "";
			T = ConvertToOEM(Value);
			try
			{
				result = XmlConvert.VerifyName(T);
			}
			catch
			{
				for (I=0; I < T.Length; ++I)
				{
					X = T[I];
					if ((X=='\\') || (((X>='a') && (X<='z')) || ((X>='A') && (X<='Z')) || ((X>='0') && (X<='9'))))
						result += X;
					else
						result += '_';
				}
                result = result.Replace(' ', '_');
			}
			return result;
		}

		protected XmlNode _FindNode(XmlNode _RootNode, string _Nodename)	//OK
		{
			XmlNode result = _RootNode.SelectSingleNode(_Nodename);
			if (result==null)
			{
				for (int i = 0; i < _RootNode.ChildNodes.Count; ++i)
					if (_RootNode.ChildNodes[i].NodeType == XmlNodeType.Element)
						if ( _RootNode.ChildNodes[i].LocalName == _Nodename) 
							return _RootNode.ChildNodes[i];
				return null;
			}
			return result;
		}

		protected ArrayList ParseString(string s)	// OK
		{
			ArrayList slist = new ArrayList();
			string[] a = new string[0];
			a = s.Split('\\');
			for (int c1 = 0; c1 < a.Length; ++c1)
				if (a[c1].ToString().Trim()!="")
					slist.Add( a[c1] );

			return slist;
		}

		public XmlNode GetPathNode(string NodePath, bool CanCreate)
		{
			XmlNode lnode;
			XmlNode Result = null;
			ArrayList s1 = new ArrayList();

			try
			{
                CheckInitialized();
                Result = xmldoc.DocumentElement;

				s1 = ParseString( Translate(NodePath) );
				for (int i = 0; i < s1.Count; ++i)
				{
					lnode = Result;
					Result = _FindNode(lnode, Translate(s1[i].ToString()));
					if (Result == null)// || this.ReplaceValue)
						if (CanCreate)
						{
							XmlNode xmlProp = xmldoc.CreateElement("", Translate(s1[i].ToString()), "");
							Result = lnode.AppendChild(xmlProp);
						}
						else 
						{
							Result = null;
							break;
						}
				}
			}
			catch
			{
                Result = null;
			}
			return Result;
		}

		public bool ValueExists(string Path, string ValueSection)	// OK
		{
			XmlNode n = GetPathNode(Path, false);
			if (n!=null)
			{
				if ((n.ChildNodes.Count>0) && (n.HasChildNodes))
					for (int i=0; i<n.ChildNodes.Count; ++i)
						if (n.ChildNodes[i].LocalName == ValueSection)
							return true;

				return n.Attributes.GetNamedItem( ValueSection ) != null;
			}
			return false;
		}

		#endregion
    }
}
