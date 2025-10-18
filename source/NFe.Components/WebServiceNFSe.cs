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

        public static void SalvarXMLMunicipios(string uf, string cidade, int codigomunicipio, string padrao, bool forcaAtualizacao)
        {
            try
            {
                if (uf != null)
                {
                    Municipio mun = Propriedade.Municipios.FirstOrDefault(w => w.CodigoMunicipio == codigomunicipio);

                    var geral = PadraoNFSeUnicoWSDLDataSource.FirstOrDefault(w => w.fromType == GetPadraoFromString(padrao).ToString());

                    if (geral != null && mun != null)
                    {
                        Propriedade.Municipios.Remove(mun);
                    }
                    else
                    {
                        if (padrao != PadraoNFSe.None.ToString())
                        {
                            if (mun != null)
                            {
                                ///
                                /// É o mesmo padrão definido?
                                /// O parâmetro "forcaAtualizacao" é "true" somente quando vier da aba "Municipios definidos"
                                /// Desde que o datagrid atualiza automaticamente o membro "padrao" da classe "Municipio" quando ele é alterado.
                                if (mun.PadraoStr == padrao && !forcaAtualizacao)
                                    return;

                                mun.Padrao = GetPadraoFromString(padrao);
                                mun.PadraoStr = padrao;
                            }
                            else
                                Propriedade.Municipios.Add(new Municipio(codigomunicipio, uf, cidade.Trim(), GetPadraoFromString(padrao)));
                        }
                    }
                }
                if (System.IO.File.Exists(Propriedade.NomeArqXMLMunicipios))
                {
                    // Faz uma cópia por segurança
                    if (System.IO.File.Exists(Propriedade.NomeArqXMLMunicipios + ".bck"))
                        System.IO.File.Delete(Propriedade.NomeArqXMLMunicipios + ".bck");
                    System.IO.File.Copy(Propriedade.NomeArqXMLMunicipios, Propriedade.NomeArqXMLMunicipios + ".bck");
                }

                /*
                <nfes_municipios>
                    <Registro ID="4125506" Nome="São José dos Pinais - PR" Padrao="GINFES" />
                </nfes_municipios>
                 */

                var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));
                XElement elementos = new XElement("nfes_municipios");
                var r = (from ss in Propriedade.Municipios orderby ss.Nome select ss);
                foreach (Municipio item in r)
                {
                    elementos.Add(new XElement(NFeStrConstants.Registro,
                                    new XAttribute(TpcnResources.ID.ToString(), item.CodigoMunicipio.ToString()),
                                    new XAttribute(NFeStrConstants.Nome, item.Nome.Trim()),
                                    new XAttribute(NFeStrConstants.Padrao, item.PadraoStr)));
                }
                xml.Add(elementos);
                xml.Save(Propriedade.NomeArqXMLMunicipios);
            }
            catch (Exception ex)
            {
                // Recupera a cópia feita se houve erro na criação do XML de munícipios
                if (System.IO.File.Exists(Propriedade.NomeArqXMLMunicipios + ".bck"))
                    Functions.Move(Propriedade.NomeArqXMLMunicipios + ".bck", Propriedade.NomeArqXMLMunicipios);
                throw ex;
            }
        }

        /// <summary>
        /// Responsável pela gravação do arquivo de munícipios, caso não exista
        /// </summary>
        public static void SalvarXMLMunicipios()
        {
            if (!System.IO.File.Exists(Propriedade.NomeArqXMLMunicipios) &&
                System.IO.File.Exists(Propriedade.NomeArqXMLWebService_NFSe))
            {
                var xml = new XDocument(new XDeclaration("1.0", "utf-8", null));
                XElement elementos = new XElement("nfes_municipios");

                XElement axml = XElement.Load(Propriedade.NomeArqXMLWebService_NFSe);
                var s = (from p in axml.Descendants(NFeStrConstants.Estado)
                         where (string)p.Attribute(TpcnResources.UF.ToString()) != "XX"
                         orderby p.Attribute(NFeStrConstants.Nome).Value
                         select p);
                foreach (var item in s)
                {
                    string padrao = PadraoNFSe.None.ToString();
                    if (item.Attribute(NFeStrConstants.Padrao) != null)
                        padrao = item.Attribute(NFeStrConstants.Padrao).Value;

                    if (padrao != PadraoNFSe.None.ToString())
                    {
                        string ID = item.Attribute(TpcnResources.ID.ToString()).Value;
                        string Nome = item.Attribute(NFeStrConstants.Nome).Value;
                        string UF = item.Attribute(TpcnResources.UF.ToString()).Value;

                        elementos.Add(new XElement(NFeStrConstants.Registro,
                                        new XAttribute(TpcnResources.ID.ToString(), ID),
                                        new XAttribute(NFeStrConstants.Nome, Nome.Trim()),
                                        new XAttribute(NFeStrConstants.Padrao, padrao)));
                    }
                }
                if (!elementos.IsEmpty)
                {
                    xml.Add(elementos);
                    xml.Save(Propriedade.NomeArqXMLMunicipios);
                }
            }
        }
    }
}