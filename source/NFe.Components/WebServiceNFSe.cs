using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unimake.Business.DFe.Servicos;
namespace NFe.Components
{
    public class PadroesDataSource
    {
        public string fromDescription { get; set; }
        public string fromType { get; set; }
    }

    public class WebServiceNFSe
    {
        private static List<PadroesDataSource> _PadroesUnicoWSDLDataSource = null;

        public static List<PadroesDataSource> PadraoNFSeUnicoWSDLDataSource
        {
            get
            {
                if (_PadroesUnicoWSDLDataSource == null)
                {
                    Array arr = Enum.GetValues(typeof(PadraoNFSe));
                    _PadroesUnicoWSDLDataSource = new List<PadroesDataSource>();
                    foreach (PadraoNFSe type in arr)
                        switch (type)
                        {
                            case PadraoNFSe.None:
                            case PadraoNFSe.GINFES:
                            case PadraoNFSe.EQUIPLANO:
                            case PadraoNFSe.ABASE:
                                _PadroesUnicoWSDLDataSource.Add(new PadroesDataSource { fromType = type.ToString(), fromDescription = EnumHelper.GetEnumItemDescription(type) });
                                break;
                        }
                }
                return _PadroesUnicoWSDLDataSource.OrderBy(p => p.fromDescription).ToList();
            }
        }

        public static PadraoNFSe GetPadraoFromString(string padrao)
        {
            try
            {
                return (PadraoNFSe)EnumHelper.StringToEnum<PadraoNFSe>(padrao);
            }
            catch
            {
                return PadraoNFSe.None;
            }
        }        
    }
}