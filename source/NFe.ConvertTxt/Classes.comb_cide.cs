using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NFe.ConvertTxt
{
    /// <summary>
    /// Comb
    /// </summary>
    public class Comb
    {
        public int cProdANP;
        public string descANP;
        public double pGLP;
        public double pGNn;
        public double pGNi;
        public double vPart;
        public double pMixGN;
        public string CODIF;
        public double qTemp;
        public string UFCons;
        public CIDE CIDE;
        public Encerrante encerrante;
        public double pBio;
        public List<OrigComb> origComb;

        public Comb()
        {
            origComb = new List<OrigComb>();
        }
    }

    /// <summary>
    /// CIDE
    /// </summary>
    public struct CIDE
    {
        public double qBCprod;
        public double vAliqProd;
        public double vCIDE;
    }

    public struct Encerrante 
    {
        public int nBico;
        public int nBomba;
        public int nTanque;
        public string vEncIni;
        public string vEncFin;
    }

    public class OrigComb
    {
        public int indImport;
        public int cUFOrig;
        public double pOrig;
    }
}
