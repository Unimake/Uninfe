using NFe.Components;
using NFe.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Unimake.Business.DFe.Servicos;

namespace NFe.ConvertTxt
{
    public class ConversaoTXT
    {
        #region --- public properties

        public Dictionary<string, string> LayoutTXT
        {
            get
            {
                if (_LayoutTXT == null)
                {
                    _LayoutTXT = new Dictionary<string, string>();

                    /// "A"
                    _LayoutTXT.Add("A", prefix + "A|versao|Id|");
                    /// "B"
                    _LayoutTXT.Add("B_23", prefix + "B|cUF|cNF|NatOp|mod|serie|nNF|dhEmi|dhSaiEnt|tpNF|idDest|cMunFG|TpImp|TpEmis|cDV|TpAmb|FinNFe|indFinal|indPres|ProcEmi|VerProc|dhCont|xJust|");
                    _LayoutTXT.Add("B_24", prefix + "B|cUF|cNF|NatOp|mod|serie|nNF|dhEmi|dhSaiEnt|tpNF|idDest|cMunFG|TpImp|TpEmis|cDV|TpAmb|FinNFe|indFinal|indPres|indIntermed|ProcEmi|VerProc|dhCont|xJust|");
                    _LayoutTXT.Add("B_27", prefix + "B|cUF|cNF|NatOp|mod|serie|nNF|dhEmi|dhSaiEnt|tpNF|idDest|cMunFG|cMunFGIBS|TpImp|TpEmis|cDV|TpAmb|FinNFe|tpNFDebito|tpNFCredito|indFinal|indPres|indIntermed|ProcEmi|VerProc|dhCont|xJust|");
                    _LayoutTXT.Add("B_28", prefix + "B|cUF|cNF|NatOp|mod|serie|nNF|dhEmi|dhSaiEnt|dPrevEntrega|tpNF|idDest|cMunFG|cMunFGIBS|TpImp|TpEmis|cDV|TpAmb|FinNFe|tpNFDebito|tpNFCredito|indFinal|indPres|indIntermed|ProcEmi|VerProc|dhCont|xJust|");
                    _LayoutTXT.Add("B13", prefix + "B13|refNFe|");
                    _LayoutTXT.Add("BA02", prefix + "BA02|refNFe|");
                    _LayoutTXT.Add("BA03", prefix + "BA03|cUF|AAMM|CNPJ|mod|serie|nNF|");
                    _LayoutTXT.Add("BA10", prefix + "BA10|cUF|AAMM|IE|mod|serie|nNF|refCTe|");
                    _LayoutTXT.Add("B20A", prefix + "B20a|cUF|AAMM|IE|mod|serie|nNF|");
                    _LayoutTXT.Add("B20D", prefix + "B20d|CNPJ|");
                    _LayoutTXT.Add("BA13", prefix + "BA13|CNPJ|");
                    _LayoutTXT.Add("B20E", prefix + "B20e|CPF|");
                    _LayoutTXT.Add("BA14", prefix + "BA14|CPF|");
                    _LayoutTXT.Add("B20I", prefix + "B20i|refCTe|");
                    _LayoutTXT.Add("BA19", prefix + "BA19|refCTe|");
                    _LayoutTXT.Add("B20J", prefix + "B20j|mod|nECF|nCOO|");
                    _LayoutTXT.Add("BA20", prefix + "BA20|mod|nECF|nCOO|");
                    _LayoutTXT.Add("B31", prefix + "B31|tpEnteGov|pRedutor|tpOperGov|"); // ide->gCompraGov
                    _LayoutTXT.Add("BB01", prefix + "BB01|refNFe|"); // ide->gPagAntecipado
                    /// "C"
                    _LayoutTXT.Add("C", prefix + "C|xNome|xFant|IE|IEST|IM|CNAE|CRT|");
                    _LayoutTXT.Add("C02", prefix + "C02|CNPJ|");
                    _LayoutTXT.Add("C02A", prefix + "C02a|CPF|");
                    _LayoutTXT.Add("C05", prefix + "C05|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|CEP|cPais|xPais|fone|");
                    /// "D"
                    _LayoutTXT.Add("D", prefix + "D|CNPJ|xOrgao|matr|xAgente|fone|UF|nDAR|dEmi|vDAR|repEmi|dPag|");
                    /// "E"
                    _LayoutTXT.Add("E_400", prefix + "E|xNome|indIEDest|IE|ISUF|IM|email|");
                    _LayoutTXT.Add("E02", prefix + "E02|CNPJ|");
                    _LayoutTXT.Add("E03", prefix + "E03|CPF|");
                    _LayoutTXT.Add("E03A", prefix + "E03a|idEstrangeiro|");
                    _LayoutTXT.Add("E05", prefix + "E05|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|CEP|cPais|xPais|fone|");
                    /// "F"
                    _LayoutTXT.Add("F", prefix + "F|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|");
                    _LayoutTXT.Add("F_16", prefix + "F|CNPJ_CPF|xNome|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|CEP|cPais|xPais|fone|email|IE|");
                    _LayoutTXT.Add("F02", prefix + "F02|CNPJ|");
                    _LayoutTXT.Add("F02A", prefix + "F02a|CPF|");
                    /// "G"
                    _LayoutTXT.Add("G", prefix + "G|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|");
                    _LayoutTXT.Add("G_16", prefix + "G|CNPJ_CPF|xNome|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|CEP|cPais|xPais|fone|email|IE|");
                    _LayoutTXT.Add("G02", prefix + "G02|CNPJ|");
                    _LayoutTXT.Add("G02A", prefix + "G02a|CPF|");
                    _LayoutTXT.Add("G51", prefix + "G51|CNPJ|");
                    _LayoutTXT.Add("GA02", prefix + "GA02|CNPJ|");
                    _LayoutTXT.Add("G52", prefix + "G52|CPF|");
                    _LayoutTXT.Add("GA03", prefix + "GA03|CPF|");
                    /// "H"
                    _LayoutTXT.Add("H", prefix + "H|nItem|infAdProd|");
                    /// "I"
                    _LayoutTXT.Add("I_28", prefix + "I|cProd|cEAN|XProd|NCM|NVE|CEST|indEscala|CNPJFab|cBenef|EXTIPI|CFOP|UCom|QCom|VUnCom|VProd|CEANTrib|UTrib|QTrib|VUnTrib|VFrete|VSeg|VDesc|vOutro|indTot|xPed|nItemPed|nFCI|");
                    _LayoutTXT.Add("I_30", prefix + "I|cProd|cEAN|cBarra|XProd|NCM|NVE|CEST|indEscala|CNPJFab|cBenef|EXTIPI|CFOP|UCom|QCom|VUnCom|VProd|CEANTrib|cBarraTrib|UTrib|QTrib|VUnTrib|VFrete|VSeg|VDesc|vOutro|indTot|xPed|nItemPed|nFCI|");

                    _LayoutTXT.Add("I05G", prefix + "I05g|cCredPresumido|pCredPresumido|vCredPresumido|");

                    _LayoutTXT.Add("I05A", prefix + "I05a|NVE|");
                    _LayoutTXT.Add("I05K", prefix + "I05k|tpCredPresIBSZFM|"); // prod->tpCredPresIBSZFM (IBSCBS)
                    _LayoutTXT.Add("I05W", prefix + "I05w|CEST|");
                    _LayoutTXT.Add("I05W_4", prefix + "I05w|CEST|indEscala|CNPJFab|");
                    _LayoutTXT.Add("I05C", prefix + "I05c|CEST|");
                    _LayoutTXT.Add("I05C_4", prefix + "I05c|CEST|indEscala|CNPJFab|");
                    _LayoutTXT.Add("I17", prefix + "I17|indBemMovelUsado|"); // prod->indBemMovelUsado (IBSCBS)
                    _LayoutTXT.Add("I18_400_12", prefix + "I18|nDI|dDI|xLocDesemb|UFDesemb|dDesemb|tpViaTransp|vAFRMM|tpIntermedio|CNPJ|UFTerceiro|cExportador|");
                    _LayoutTXT.Add("I18_400_13", prefix + "I18|nDI|dDI|xLocDesemb|UFDesemb|dDesemb|tpViaTransp|vAFRMM|tpIntermedio|CNPJ|CPF|UFTerceiro|cExportador|");
                    _LayoutTXT.Add("I25_400", prefix + "I25|NAdicao|NSeqAdic|CFabricante|VDescDI|nDraw|");
                    _LayoutTXT.Add("I50", prefix + "I50|nDraw|");
                    _LayoutTXT.Add("I52", prefix + "I52|nRE|chNFe|qExport|");
                    _LayoutTXT.Add("I80", prefix + "I80|nLote|qLote|dFab|dVal|cAgreg|");
                    _LayoutTXT.Add("IRT", prefix + "IRT|CNPJ|xContato|email|fone|idCSRT|hashCSRT|");
                    /// "J"
                    _LayoutTXT.Add("J", prefix + "J|tpOp|Chassi|CCor|XCor|Pot|cilin|pesoL|pesoB|NSerie|TpComb|NMotor|CMT|Dist|anoMod|anoFab|tpPint|tpVeic|espVeic|VIN|condVeic|cMod|cCorDENATRAN|lota|tpRest|");
                    _LayoutTXT.Add("JA", prefix + "JA|tpOp|Chassi|CCor|XCor|Pot|cilin|pesoL|pesoB|NSerie|TpComb|NMotor|CMT|Dist|anoMod|anoFab|tpPint|tpVeic|espVeic|VIN|condVeic|cMod|cCorDENATRAN|lota|tpRest|");
                    /// "K"
                    _LayoutTXT.Add("K", prefix + "K|nLote|qLote|dFab|dVal|vPMC|");
                    _LayoutTXT.Add("K_3", prefix + "K|cProdANVISA|vPMC|");
                    _LayoutTXT.Add("K_4", prefix + "K|cProdANVISA|xMotivoIsencao|vPMC|");
                    /// "L"
                    _LayoutTXT.Add("L", prefix + "L|tpArma|nSerie|nCano|descr|");

                    _LayoutTXT.Add("LA_10", prefix + "LA|cProdANP|descANP|pGLP|pGNn|pGNi|vPart|CODIF|qTemp|UFCons|");
                    _LayoutTXT.Add("L01_10", prefix + "L01|cProdANP|descANP|pGLP|pGNn|pGNi|vPart|CODIF|qTemp|UFCons|");
                    _LayoutTXT.Add("LA_11", prefix + "LA|cProdANP|descANP|pGLP|pGNn|pGNi|vPart|CODIF|qTemp|UFCons|pBio|");
                    _LayoutTXT.Add("L01_11", prefix + "L01|cProdANP|descANP|pGLP|pGNn|pGNi|vPart|CODIF|qTemp|UFCons|pBio|");

                    _LayoutTXT.Add("LA1", prefix + "LA1|nBico|nBomba|nTanque|vEncIni|vEncFin|");
                    _LayoutTXT.Add("LA07", prefix + "LA07|qBCProd|vAliqProd|vCIDE|");
                    _LayoutTXT.Add("LA18", prefix + "LA18|indImport|cUFOrig|pOrig|");

                    _LayoutTXT.Add("L105", prefix + "L105|qBCProd|vAliqProd|vCIDE|");
                    _LayoutTXT.Add("LB", prefix + "LB|nRECOPI|");
                    _LayoutTXT.Add("L109", prefix + "L109|nRECOPI|");
                    /// "M"
                    _LayoutTXT.Add("M", prefix + "M|vTotTrib|");
                    /// "N"
                    _LayoutTXT.Add("N02_400", prefix + "N02|Orig|CST|modBC|vBC|pICMS|vICMS|pFCP|vFCP|");

                    _LayoutTXT.Add("N02A", prefix + "N02A|Orig|CST|qBCMono|adRemICMS|vICMSMono|");

                    _LayoutTXT.Add("N03_19", prefix + "N03|Orig|CST|modBC|vBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|");
                    _LayoutTXT.Add("N03_21", prefix + "N03|Orig|CST|modBC|vBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSSTDeson|motDesICMSST|");

                    _LayoutTXT.Add("N03A", prefix + "N03A|Orig|CST|qBCMono|adRemICMS|vICMSMono|qBCMonoReten|adRemICMSReten|vICMSMonoReten|pRedAdRem|motRedAdRem|");

                    _LayoutTXT.Add("N04_400_13", prefix + "N04|orig|CST|modBC|pRedBC|vBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|vICMSDeson|motDesICMS|");
                    _LayoutTXT.Add("N04_400_14", prefix + "N04|orig|CST|modBC|pRedBC|vBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|vICMSDeson|motDesICMS|indDeduzDeson|");
                    _LayoutTXT.Add("N05_400_14", prefix + "N05|orig|CST|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSDeson|motDesICMS|");
                    _LayoutTXT.Add("N05_400_15", prefix + "N05|orig|CST|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSDeson|motDesICMS|indDeduzDeson|");

                    _LayoutTXT.Add("N06_400_3", prefix + "N06|orig|CST|");
                    _LayoutTXT.Add("N06_400_5", prefix + "N06|orig|CST|vICMSDeson|motDesICMS|");
                    _LayoutTXT.Add("N06_400_6", prefix + "N06|orig|CST|vICMSDeson|motDesICMS|indDeduzDeson|");

                    _LayoutTXT.Add("N07_14", prefix + "N07|orig|CST|modBC|pRedBC|vBC|pICMS|vICMSOp|pDif|vICMSDif|vICMS|vBCFCP|pFCP|vFCP|");
                    _LayoutTXT.Add("N07_17", prefix + "N07|orig|CST|modBC|pRedBC|vBC|pICMS|vICMSOp|pDif|vICMSDif|vICMS|vBCFCP|pFCP|vFCP|pFCPDif|vFCPDif|vFCPEfet|");
                    _LayoutTXT.Add("N07_18", prefix + "N07|orig|CST|modBC|pRedBC|cBenefRBC|vBC|pICMS|vICMSOp|pDif|vICMSDif|vICMS|vBCFCP|pFCP|vFCP|pFCPDif|vFCPDif|vFCPEfet|");
                    _LayoutTXT.Add("N07A", prefix + "N07A|orig|CST|qBCMono|adRemICMS|vICMSMonoOp|pDif|vICMSMonoDif|vICMSMono|");

                    _LayoutTXT.Add("N08_400_9", prefix + "N08|Orig|CST|vBCSTRet|pST|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|");
                    _LayoutTXT.Add("N08_400_10", prefix + "N08|Orig|CST|vBCSTRet|pST|vICMSSubstituto|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|");
                    _LayoutTXT.Add("N08_400_13", prefix + "N08|Orig|CST|vBCSTRet|pST|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|pRedBCEfet|vBCEfet|pICMSEfet|vICMSEfet|");
                    _LayoutTXT.Add("N08_400_14", prefix + "N08|Orig|CST|vBCSTRet|pST|vICMSSubstituto|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|pRedBCEfet|vBCEfet|pICMSEfet|vICMSEfet|");
                    _LayoutTXT.Add("N08A", prefix + "N08A|Orig|CST|qBCMonoRet|adRemICMSRet|vICMSMonoRet|");

                    _LayoutTXT.Add("N09_22", prefix + "N09|orig|CST|modBC|pRedBC|vBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSDeson|motDesICMS|");
                    _LayoutTXT.Add("N09_24", prefix + "N09|orig|CST|modBC|pRedBC|vBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSDeson|motDesICMS|vICMSSTDeson|motDesICMSST|");
                    _LayoutTXT.Add("N09_25", prefix + "N09|orig|CST|modBC|pRedBC|vBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSDeson|motDesICMS|vICMSSTDeson|motDesICMSST|indDeduzDeson|");

                    _LayoutTXT.Add("N10_22", prefix + "N10|orig|CST|modBC|vBC|pRedBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSDeson|motDesICMS|");
                    _LayoutTXT.Add("N10_24", prefix + "N10|orig|CST|modBC|vBC|pRedBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSDeson|motDesICMS|vICMSSTDeson|motDesICMSST|");
                    _LayoutTXT.Add("N10_25", prefix + "N10|orig|CST|modBC|vBC|pRedBC|pICMS|vICMS|vBCFCP|pFCP|vFCP|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|vICMSDeson|motDesICMS|vICMSSTDeson|motDesICMSST|indDeduzDeson|");

                    _LayoutTXT.Add("N10A_400_16", prefix + "N10a|orig|CST|modBC|vBC|pRedBC|pICMS|vICMS|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|pBCOp|UFST|");
                    _LayoutTXT.Add("N10A_400_19", prefix + "N10a|orig|CST|modBC|vBC|pRedBC|pICMS|vICMS|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|pBCOp|UFST|");

                    _LayoutTXT.Add("N10B", prefix + "N10b|orig|CST|vBCSTRet|vICMSSTRet|vBCSTDest|vICMSSTDest|");
                    _LayoutTXT.Add("N10B_16", prefix + "N10b|orig|CST|vBCSTRet|pST|vICMSSubstituto|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|vBCSTDest|vICMSSTDest|pRedBCEfet|vBCEfet|pICMSEfet|vICMSEfet|");
                    _LayoutTXT.Add("N10C", prefix + "N10c|orig|CSOSN|pCredSN|vCredICMSSN|");
                    _LayoutTXT.Add("N10D", prefix + "N10d|orig|CSOSN|");
                    _LayoutTXT.Add("N10E_400", prefix + "N10e|orig|CSOSN|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|pCredSN|vCredICMSSN|");
                    _LayoutTXT.Add("N10F_400", prefix + "N10f|orig|CSOSN|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|");
                    _LayoutTXT.Add("N10G_400_5", prefix + "N10g|orig|CSOSN|vBCSTRet|vICMSSTRet|");
                    _LayoutTXT.Add("N10G_400_9", prefix + "N10g|orig|CSOSN|vBCSTRet|pST|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|");
                    _LayoutTXT.Add("N10G_400_10", prefix + "N10g|orig|CSOSN|vBCSTRet|pST|vICMSSubstituto|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|");
                    _LayoutTXT.Add("N10G_400_13", prefix + "N10g|orig|CSOSN|vBCSTRet|pST|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|pRedBCEfet|vBCEfet|pICMSEfet|vICMSEfet|");
                    _LayoutTXT.Add("N10G_400_14", prefix + "N10g|orig|CSOSN|vBCSTRet|pST|vICMSSubstituto|vICMSSTRet|vBCFCPSTRet|pFCPSTRet|vFCPSTRet|pRedBCEfet|vBCEfet|pICMSEfet|vICMSEfet|");
                    _LayoutTXT.Add("N10H_400", prefix + "N10h|orig|CSOSN|modBC|vBC|pRedBC|pICMS|vICMS|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|pCredSN|vCredICMSSN|");
                    _LayoutTXT.Add("NA_400", prefix + "NA|vBCUFDest|vBCFCPUFDest|pFCPUFDest|pICMSUFDest|pICMSInter|pICMSInterPart|vFCPUFDest|vICMSUFDest|vICMSUFRemet|");
                    /// "O"
                    _LayoutTXT.Add("O_400", prefix + "O|CNPJProd|cSelo|qSelo|cEnq|");
                    _LayoutTXT.Add("O07", prefix + "O07|CST|vIPI|");

                    _LayoutTXT.Add("O08", prefix + "O08|CST|");
                    _LayoutTXT.Add("O10", prefix + "O10|vBC|pIPI|");
                    _LayoutTXT.Add("O11_400", prefix + "O11|qUnid|vUnid|vIPI|");
                    /// "P":
                    _LayoutTXT.Add("P", prefix + "P|vBC|vDespAdu|vII|vIOF|");
                    /// "Q":
                    _LayoutTXT.Add("Q02", prefix + "Q02|CST|VBC|PPIS|VPIS|");
                    _LayoutTXT.Add("Q03", prefix + "Q03|CST|QBCProd|VAliqProd|VPIS|");
                    _LayoutTXT.Add("Q04", prefix + "Q04|CST|");
                    _LayoutTXT.Add("Q05", prefix + "Q05|CST|vPIS|");
                    _LayoutTXT.Add("Q07_400", prefix + "Q07|vBC|pPIS|vPIS|");
                    _LayoutTXT.Add("Q10", prefix + "Q10|qBCProd|vAliqProd|");
                    /// "R":
                    _LayoutTXT.Add("R", prefix + "R|vPIS|"); //ok
                    _LayoutTXT.Add("R02", prefix + "R02|vBC|pPIS|");

                    _LayoutTXT.Add("R04_4", prefix + "R04|qBCProd|vAliqProd|vPIS|");
                    _LayoutTXT.Add("R04_5", prefix + "R04|qBCProd|vAliqProd|vPIS|indSomaPISST|");

                    /// "S"
                    _LayoutTXT.Add("S02", prefix + "S02|CST|vBC|pCOFINS|vCOFINS|");
                    _LayoutTXT.Add("S03", prefix + "S03|CST|QBCProd|VAliqProd|VCOFINS|");
                    _LayoutTXT.Add("S04", prefix + "S04|CST|");
                    _LayoutTXT.Add("S05", prefix + "S05|CST|VCOFINS|");
                    _LayoutTXT.Add("S07", prefix + "S07|VBC|PCOFINS|");
                    _LayoutTXT.Add("S09", prefix + "S09|QBCProd|VAliqProd|");
                    /// "T":
                    _LayoutTXT.Add("T", prefix + "T|VCOFINS|"); //ok
                    _LayoutTXT.Add("T02", prefix + "T02|VBC|PCOFINS|");

                    _LayoutTXT.Add("T04_4", prefix + "T04|QBCProd|VAliqProd|vCOFINS|");
                    _LayoutTXT.Add("T04_5", prefix + "T04|QBCProd|VAliqProd|vCOFINS|indSomaCOFINSST|");

                    /// "U":
                    _LayoutTXT.Add("U_400", prefix + "U|VBC|VAliq|VISSQN|CMunFG|CListServ|vDeducao|vOutro|vDescIncond|vDescCond|vISSRet|indISS|cServico|cMun|cPais|nProcesso|indIncentivo|");
                    _LayoutTXT.Add("UA", prefix + "UA|pDevol|vIPIDevol|");
                    _LayoutTXT.Add("UB01", prefix + "UB01|CSTIS|cClassTribIS|vBCIS|pIS|pISEspec|uTrib|qTrib|vIS|"); //IS
                    _LayoutTXT.Add("UB12", prefix + "UB12|CST|cClassTrib|"); //IBSCBS
                    _LayoutTXT.Add("UB15", prefix + "UB15|vBC|vIBS|"); //IBSCBS->gIBSCBS

                    _LayoutTXT.Add("UB17", prefix + "UB17|pIBSUF|pDif|vDif|vDevTrib|pRedAliq|pAliqEfet|vIBSUF|"); //IBSCBS->gIBSCBS->gIBSUF
                    _LayoutTXT.Add("UB36", prefix + "UB36|pIBSMun|pDif|vDif|vDevTrib|pRedAliq|pAliqEfet|vIBSMun|"); //IBSCBS->gIBSCBS->gIBSMun
                    _LayoutTXT.Add("UB55", prefix + "UB55|pCBS|pDif|vDif|vDevTrib|pRedAliq|pAliqEfet|vCBS|"); //IBSCBS->gIBSCBS->gCBS

                    _LayoutTXT.Add("UB68", prefix + "UB68|CSTReg|cClassTribReg|pAliqEfetRegIBSUF|vTribRegIBSUF|pAliqEfetRegIBSMun|vTribRegIBSMun|pAliqEfetRegCBS|vTribRegCBS|"); //IBSCBS->gIBSCBS->gTribRegular

                    _LayoutTXT.Add("UB82", prefix + "UB82|pAliqIBSUF|vTribIBSUF|pAliqIBSMun|vTribIBSMun|pAliqCBS|vTribCBS|"); //IBSCBS->gIBSCBS->gTribCompraGov

                    _LayoutTXT.Add("UB84", prefix + "UB84|vTotIBSMonoItem|vTotCBSMonoItem|"); //IBSCBS->gIBSCBSMono

                    _LayoutTXT.Add("UB85", prefix + "UB85|qBCMono|adRemIBS|adRemCBS|vIBSMono|vCBSMono|"); //IBSCBS->gIBSCBSMono->gMonoPadrao
                    _LayoutTXT.Add("UB91", prefix + "UB91|qBCMonoReten|adRemIBSReten|vIBSMonoReten|adRemCBSReten|vCBSMonoReten|"); //IBSCBS->gIBSCBSMono->gMonoReten
                    _LayoutTXT.Add("UB95", prefix + "UB95|qBCMonoRet|adRemIBSRet|vIBSMonoRet|adRemCBSRet|vCBSMonoRet|"); //IBSCBS->gIBSCBSMono->gMonoRet
                    _LayoutTXT.Add("UB100", prefix + "UB100|pDifIBS|vIBSMonoDif|pDifCBS|vCBSMonoDif|"); //IBSCBS->gIBSCBSMono->gMonoDif

                    _LayoutTXT.Add("UB106", prefix + "UB106|vIBS|vCBS|"); //IBSCBS->gTransfCred

                    _LayoutTXT.Add("UB14A", prefix + "UB14a|indDoacao|");//IBSCBS->indDoacao

                    _LayoutTXT.Add("UB112", prefix + "UB112|competApur|vIBS|vCBS|");

                    _LayoutTXT.Add("UB116", prefix + "UB116|vIBSEstCred|vCBSEstCred|");

                    _LayoutTXT.Add("UB120", prefix + "UB120|vBCCredPres|cCredPres|");

                    _LayoutTXT.Add("UB123", prefix + "UB123|pCredPres|vCredPres|vCredPresCondSus|");

                    _LayoutTXT.Add("UB127", prefix + "UB127|pCredPres|vCredPres|vCredPresCondSus|");

                    _LayoutTXT.Add("UB131", prefix + "UB131|competApur|tpCredPresIBSZFM|vCredPresIBSZFM|");


                    /// "V":
                    _LayoutTXT.Add("VA02", prefix + "VA02|XCampo|XTexto|");
                    _LayoutTXT.Add("VA05", prefix + "VA05|XCampo|XTexto|");
                    _LayoutTXT.Add("VB01", prefix + "VB01|vItem|");
                    _LayoutTXT.Add("VC01", prefix + "VC01|chaveAcesso|nItem|");

                    /// "W"
                    _LayoutTXT.Add("W02_400_17", prefix + "W02|vBC|vICMS|vICMSDeson|vBCST|vST|vProd|vFrete|vSeg|vDesc|vII|vIPI|vPIS|vCOFINS|vOutro|vNF|vTotTrib|");
                    _LayoutTXT.Add("W02_400_20", prefix + "W02|vBC|vICMS|vICMSDeson|vFCPUFDest|vICMSUFDest|vICMSUFRemet|vBCST|vST|vProd|vFrete|vSeg|vDesc|vII|vIPI|vPIS|vCOFINS|vOutro|vNF|vTotTrib|");
                    _LayoutTXT.Add("W02_400_21", prefix + "W02|vBC|vICMS|vICMSDeson|vFCPUFDest|vICMSUFDest|vICMSUFRemet||vBCST|vST|vProd|vFrete|vSeg|vDesc|vII|vIPI|vPIS|vCOFINS|vOutro|vNF|vTotTrib|");
                    _LayoutTXT.Add("W02_400_24", prefix + "W02|vBC|vICMS|vICMSDeson|vFCP|vFCPUFDest|vICMSUFDest|vICMSUFRemet|vBCST|vST|vFCPST|vFCPSTRet|vProd|vFrete|vSeg|vDesc|vII|vIPI|vIPIDevol|vPIS|vCOFINS|vOutro|vNF|vTotTrib|");
                    _LayoutTXT.Add("W02_400_30", prefix + "W02|vBC|vICMS|vICMSDeson|vFCP|vFCPUFDest|vICMSUFDest|vICMSUFRemet|vBCST|vST|vFCPST|vFCPSTRet|qBCMono|vICMSMono|qBCMonoReten|vICMSMonoReten|qBCMonoRet|vICMSMonoRet|vProd|vFrete|vSeg|vDesc|vII|vIPI|vIPIDevol|vPIS|vCOFINS|vOutro|vNF|vTotTrib|");
                    ///
                    /// criada duas entradas porque acho que a Sefaz cometeu um erro, colocando um pipe em branco
                    _LayoutTXT.Add("W04", prefix + "W04|vICMSUFDest|vICMSUFRemet|vFCPUFDest|");
                    _LayoutTXT.Add("W17_400", prefix + "W17|VServ|VBC|VISS|VPIS|VCOFINS|dCompet|vDeducao|vOutro|vDescIncond|vDescCond|vISSRet|cRegTrib|");
                    _LayoutTXT.Add("W23", prefix + "W23|VRetPIS|VRetCOFINS|VRetCSLL|VBCIRRF|VIRRF|VBCRetPrev|VRetPrev|");

                    _LayoutTXT.Add("W31", prefix + "W31|vIS|"); //ISTot -> vIS
                    _LayoutTXT.Add("W34", prefix + "W34|vBCIBSCBS|"); //IBSCBSTot -> vBCIBSCBS
                    _LayoutTXT.Add("W36", prefix + "W36|vIBS|vCredPres|vCredPresCondSus|"); //IBSCBSTot -> gIBS
                    _LayoutTXT.Add("W37", prefix + "W37|vDif|vDevTrib|vIBSUF|"); //IBSCBSTot -> gIBS -> gIBSUF
                    _LayoutTXT.Add("W42", prefix + "W42|vDif|vDevTrib|vIBSMun|"); //IBSCBSTot -> gIBS -> gIBSMun
                    _LayoutTXT.Add("W50", prefix + "W50|vDif|vDevTrib|vCBS|vCredPres|vCredPresCondSus|"); //IBSCBSTot -> gCBS
                    _LayoutTXT.Add("W57", prefix + "W57|vIBSMono|vCBSMono|vIBSMonoReten|vCBSMonoReten|vIBSMonoRet|vCBSMonoRet|"); //IBSCBSTot -> gMono
                    _LayoutTXT.Add("W59E", prefix + "W59e|vIBSEstCred|vCBSEstCred|"); 
                    _LayoutTXT.Add("W60", prefix + "W60|vNFTot|"); //Total -> vNFTot

                    /// "X":
                    _LayoutTXT.Add("X", prefix + "X|modFrete|");
                    _LayoutTXT.Add("X03", prefix + "X03|xNome|IE|xEnder|xMun|UF|");
                    _LayoutTXT.Add("X04", prefix + "X04|CNPJ|");
                    _LayoutTXT.Add("X05", prefix + "X05|CPF|");
                    _LayoutTXT.Add("X11", prefix + "X11|VServ|VBCRet|PICMSRet|VICMSRet|CFOP|CMunFG|");
                    _LayoutTXT.Add("X18", prefix + "X18|Placa|UF|RNTC|");
                    _LayoutTXT.Add("X22_400", prefix + "X22|Placa|UF|RNTC|vagao|balsa|");
                    _LayoutTXT.Add("X26", prefix + "X26|QVol|Esp|Marca|NVol|PesoL|PesoB|");
                    _LayoutTXT.Add("X33", prefix + "X33|NLacre|");
                    /// "Y":
                    _LayoutTXT.Add("Y02", prefix + "Y02|NFat|VOrig|VDesc|VLiq|");
                    _LayoutTXT.Add("Y07", prefix + "Y07|NDup|DVenc|VDup|");

                    _LayoutTXT.Add("YA_9", prefix + "YA|indPag|tPag|xPag|vPag|CNPJ|tBand|cAut|tpIntegra|");
                    _LayoutTXT.Add("YA_14", prefix + "YA|indPag|tPag|xPag|vPag|dPag|CNPJPag|UFPag|CNPJ|tBand|cAut|tpIntegra|CNPJReceb|idTermPag|");

                    _LayoutTXT.Add("YA04", prefix + "YA04|tpIntegra|");
                    _LayoutTXT.Add("YA04A", prefix + "YA04a|tpIntegra|");
                    _LayoutTXT.Add("YA09", prefix + "YA09|vTroco|");

                    _LayoutTXT.Add("YB", prefix + "YB|CNPJ|idCadIntTran|");

                    /// "Z":
                    _LayoutTXT.Add("Z", prefix + "Z|InfAdFisco|InfCpl|");
                    _LayoutTXT.Add("Z04", prefix + "Z04|XCampo|XTexto|");
                    _LayoutTXT.Add("Z07", prefix + "Z07|XCampo|XTexto|");
                    _LayoutTXT.Add("Z10", prefix + "Z10|NProc|IndProc|");
                    _LayoutTXT.Add("Z10_4", prefix + "Z10|NProc|IndProc|tpAto|");
                    _LayoutTXT.Add("ZA_400", prefix + "ZA|UFSaidaPais|xLocExporta|xLocDespacho|");
                    _LayoutTXT.Add("ZA01_400", prefix + "ZA01|UFSaidaPais|xLocExporta|xLocDespacho|");
                    _LayoutTXT.Add("ZB", prefix + "ZB|XNEmp|XPed|XCont|");
                    _LayoutTXT.Add("ZC", prefix + "ZC|safra|ref|qTotMes|qTotAnt|qTotGer|vFor|vTotDed|vLiqFor|");
                    _LayoutTXT.Add("ZC01", prefix + "ZC01|safra|ref|qTotMes|qTotAnt|qTotGer|vFor|vTotDed|vLiqFor|");
                    _LayoutTXT.Add("ZC04", prefix + "ZC04|dia|qtde|");
                    _LayoutTXT.Add("ZC10", prefix + "ZC10|xDed|vDed|");
                    _LayoutTXT.Add("ZD", prefix + "ZD|CNPJ|xContato|email|fone|idCSRT|hashCSRT|");

                    _LayoutTXT.Add("ZF02", prefix + "ZF02|nReceituario|CPFRespTec|"); // agropecuario->defensivo
                    _LayoutTXT.Add("ZF04", prefix + "ZF04|tpGuia|UFGuia|serieGuia|nGuia|"); // agropecuario->guiaTransito
                }
                return _LayoutTXT;
            }
        }

        public List<txtTOxmlClassRetorno> cRetorno = null;
        public string cMensagemErro { get; private set; }

        #endregion

        #region -- private proprieties

        private Dictionary<string, string> _LayoutTXT = null;
        private NFe NFe = null;
        private string FSegmento;
        private int LinhaLida;
        private string Registro;
        private string layout;
        private string chave;
        private const string prefix = "§";
        /// <summary>
        /// conteudo do arquivo de cada nota
        /// </summary>
        private Dictionary<int, List<string>> xConteudoArquivo;

        private string ArqTXT;

        #endregion

        public ConversaoTXT()
        {
            this.xConteudoArquivo = new Dictionary<int, List<string>>();
            this.cRetorno = new List<txtTOxmlClassRetorno>();
            this.cMensagemErro = "";
            this.LinhaLida = 0;
        }

        /// <summary>
        /// CarregarArquivo
        /// </summary>
        private bool CarregarArquivo(string cArquivo)
        {
            ArqTXT = cArquivo;

            if (File.Exists(cArquivo))
            {
                TextReader txt = new StreamReader(cArquivo, Encoding.Default, true);
                try
                {
                    int nNota = -1;
                    string cLinhaTXT = txt.ReadLine();
                    if (cLinhaTXT != null)
                    {
                        if (!cLinhaTXT.StartsWith("NOTAFISCAL") && !cLinhaTXT.StartsWith("NOTA FISCAL"))
                        {
                            this.cMensagemErro = " Conteúdo da primeira linha do arquivo deve ser 'NOTAFISCAL'";
                        }
                        cLinhaTXT = txt.ReadLine();
                        this.LinhaLida = 1;
                    }
                    while (cLinhaTXT != null)
                    {
                        ++LinhaLida;

                        if (cLinhaTXT.Trim().Length > 0)
                        {
                            if (cLinhaTXT.StartsWith("A|"))
                            {
                                ++nNota;
                                xConteudoArquivo.Add(nNota, new List<string>());
                            }
                            List<string> temp;
                            xConteudoArquivo.TryGetValue(nNota, out temp);
                            temp.Add(prefix + cLinhaTXT.Trim() + (!cLinhaTXT.TrimEnd().EndsWith("|") ? "|" : ""));
                        }
                        cLinhaTXT = txt.ReadLine();
                    }
                }
                catch (IOException ex)
                {
                    this.cMensagemErro += ex.Message;
                }
                catch (Exception ex)
                {
                    this.cMensagemErro += ex.Message;
                }
                finally
                {
                    txt.Close();
                }
            }
            else
                this.cMensagemErro = "Arquivo [" + cArquivo + "] não encontrado";

            return ((this.xConteudoArquivo.Count == 0 || !string.IsNullOrEmpty(this.cMensagemErro)) ? false : true);
        }

        /// <summary>
        /// Converter
        /// </summary>
        public bool Converter(string cArquivo, string cFolderDestino)
        {
            cRetorno.Clear();

            if (this.CarregarArquivo(cArquivo))
            {
                this.LinhaLida = 0;
                foreach (List<string> content in this.xConteudoArquivo.Values)
                {
                    NFe = null;
                    NFe = new NFe();
                    bool houveErro = false;

                    foreach (string xContent in content)
                    {
                        houveErro = false;
                        ++this.LinhaLida;
                        try
                        {
                            ///
                            /// processa o TXT
                            /// 
                            this.LerRegistro(xContent);
                        }
                        catch (Exception ex)
                        {
                            houveErro = true;
                            if (!string.IsNullOrEmpty(this.layout))
                                this.cMensagemErro += "Layout: " + this.layout.Replace(prefix, "") + Environment.NewLine;
                            this.cMensagemErro += "Linha lida: " + (this.LinhaLida + 1).ToString() + Environment.NewLine +
                                                    "Conteudo: " + xContent.Substring(1) + Environment.NewLine +
                                                    ex.Message + Environment.NewLine;
                        }
                    }


                    if (!houveErro && this.cMensagemErro == "")
                    {
                        NFeW nfew = new NFeW();
                        try
                        {
                            nfew.cMensagemErro = this.cMensagemErro;
                            ///
                            /// gera o XML da nota
                            /// 
                            nfew.GerarXml(NFe, cFolderDestino, cArquivo);
                            if (nfew.cFileName != "")
                            {
                                ///
                                /// Adiciona o XML na lista de arquivos convertidos
                                /// 
                                this.cRetorno.Add(new txtTOxmlClassRetorno(nfew.cFileName, NFe.infNFe.ID, NFe.ide.nNF, NFe.ide.serie));
                            }
                        }
                        catch (Exception ex)
                        {
                            nfew.cMensagemErro += ex.Message;
                        }
                        this.cMensagemErro = nfew.cMensagemErro;
                    }

                    if (this.cMensagemErro != "")
                    {
                        ///
                        /// exclui os arquivos gerados
                        /// 
                        foreach (txtTOxmlClassRetorno txtClass in this.cRetorno)
                        {
                            string dArquivo = txtClass.XMLFileName;
                            if (File.Exists(dArquivo))
                            {
                                FileInfo fi = new FileInfo(dArquivo);
                                fi.Delete();
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(this.cMensagemErro))
                {
                    this.cMensagemErro += "----------------------" + Environment.NewLine;
                    this.cMensagemErro += "Para gerar o layout em TXT da NFe/NFCe, grave um arquivo com o nome 'uninfe-layout.txt' ou 'uninfe-layout.xml' com conteudo vazio na pasta 'geral' do Uninfe.";
                    this.cMensagemErro += "----------------------" + Environment.NewLine;
                }
                return string.IsNullOrEmpty(this.cMensagemErro);
            }
            return false;
        }

        /// <summary>
        /// getDateTime
        /// </summary>
        public DateTime getDateTime(TpcnTipoCampo Tipo, string value)
        {
            if (string.IsNullOrEmpty(value))
                return DateTime.MinValue;

            try
            {
                int _ano = Convert.ToInt16(value.Substring(0, 4));
                int _mes = Convert.ToInt16(value.Substring(5, 2));
                int _dia = Convert.ToInt16(value.Substring(8, 2));
                if (Tipo == TpcnTipoCampo.tcDatHor && value.Contains(":"))
                {
                    int _hora = Convert.ToInt16(value.Substring(11, 2));
                    int _min = Convert.ToInt16(value.Substring(14, 2));
                    int _seg = Convert.ToInt16(value.Substring(17, 2));
                    return new DateTime(_ano, _mes, _dia, _hora, _min, _seg);
                }
                return new DateTime(_ano, _mes, _dia);
            }
            catch
            {
                throw new Exception("Data inválida do conteudo [" + value + "]");
            }
        }

        /// <summary>
        /// getDateTime2
        /// </summary>
        public DateTime getDate2(TpcnTipoCampo Tipo, string value)
        {
            if (string.IsNullOrEmpty(value))
                return DateTime.MinValue;

            if (value.Contains("-"))
                return this.getDateTime(Tipo, value);

            try
            {
                int _ano = Convert.ToInt16(value.Substring(0, 4));
                int _mes = Convert.ToInt16(value.Substring(4, 2));
                int _dia = Convert.ToInt16(value.Substring(6, 2));
                return new DateTime(_ano, _mes, _dia);
            }
            catch
            {
                throw new Exception("Data inválida do conteudo [" + value + "]");
            }
        }

        /// <summary>
        /// getTime
        /// </summary>
        private DateTime getTime(string value)
        {
            if (string.IsNullOrEmpty(value))
                return DateTime.MinValue;

            try
            {
                int _hora = Convert.ToInt16(value.Substring(0, 2));
                int _min = Convert.ToInt16(value.Substring(3, 2));
                int _seg = Convert.ToInt16(value.Substring(6, 2));
                return new DateTime(1, 1, 1, _hora, _min, _seg);
            }
            catch
            {
                throw new Exception("Hora inválida do conteudo [" + value + "]");
            }
        }

        /// <summary>
        /// RetornarConteudoTag
        /// </summary>
        private string RetornarConteudoTag(string TAG, bool trim, ObOp optional)
        {
            ///
            /// "§B14|cUF|AAMM|CNPJ|Mod|serie|nNF"); //ok
            /// 
            /// se a tag a ser consulta é CNPJ, então é verificada no layout quantos pipes existem até ela.
            /// neste caso no comando abaixo será retornado "§B14|cUF|AAMM|" existindo 3 pipes para pegar
            /// o valor do retorno
            /// 
            if (string.IsNullOrEmpty(layout)) throw new Exception("Layout para o segmento '" + this.FSegmento + "' não encontrado");
            if (!layout.StartsWith(prefix)) layout = prefix + layout;
            if (!layout.EndsWith("|")) layout += "|";
            string fValue = layout.Substring(0, layout.ToUpper().IndexOf("|" + TAG.ToUpper().Trim() + "|") + 1);
            if (fValue == "")
                if (optional == ObOp.Obrigatorio)
                    throw new Exception("Segmento: " + this.FSegmento + " - Tag: " + TAG + " não encontrada");
                else
                    return "";

            string[] pipes = fValue.Split(new char[] { '|' });
            int j = pipes.GetLength(0) - 1;
            if (j >= 0)
            {
                ///
                /// qual a posicao do conteudo do registro lido
                /// 
                string[] dados = this.Registro.Split(new char[] { '|' });
                try
                {
                    if (trim)
                        return dados[j].TrimStart().TrimEnd();
                    else
                        return dados[j];
                }
                catch
                {
                    return "";
                }
            }
            else
                return "";
        }

        /// <summary>
        /// SomenteNumeros
        /// </summary>
        private string SomenteNumeros(string entrada)
        {
            if (string.IsNullOrEmpty(entrada)) return "";

            StringBuilder saida = new StringBuilder(entrada.Length);
            foreach (char c in entrada)
            {
                if (char.IsDigit(c))
                {
                    saida.Append(c);
                }
            }
            return saida.ToString();
        }

        /// <summary>
        /// LerCampo
        /// </summary>

        private double LerDouble(TpcnTipoCampo Tipo, TpcnResources tag, ObOp optional, int maxLength, bool returnNull) =>
            (double)LerCampo(Tipo, tag, optional, 0, maxLength, true, returnNull);

        private double LerDouble(TpcnTipoCampo Tipo, TpcnResources tag, ObOp optional, int maxLength) =>
            (double)this.LerCampo(Tipo, tag, optional, 0, maxLength, true, false);
        private decimal LerDecimal(TpcnTipoCampo Tipo, TpcnResources tag, ObOp optional, int maxLength) =>
            (decimal)this.LerCampo(Tipo, tag.ToString(), optional, 0, maxLength, true, false);

        private double LerDouble(TpcnTipoCampo Tipo, TpcnResources tag, ObOp optional, int minLength, int maxLength) =>
            (double)this.LerCampo(Tipo, tag, optional, minLength, maxLength, true, false);

        private Int32 LerInt32(string tag, ObOp optional, int minLength, int maxLength) =>
            (Int32)this.LerCampo(TpcnTipoCampo.tcInt, tag, optional, minLength, maxLength, true, false);

        private Int32 LerInt32(TpcnResources tag, ObOp optional, int minLength, int maxLength) =>
            (Int32)this.LerCampo(TpcnTipoCampo.tcInt, tag, optional, minLength, maxLength, true, false);

        private Int32 LerInt32(TpcnResources tag, ObOp optional, int minLength, int maxLength, bool returnNull) =>
            (Int32)this.LerCampo(TpcnTipoCampo.tcInt, tag, optional, minLength, maxLength, true, returnNull);

        private string LerString(string tag, ObOp optional, int minLength, int maxLength) =>
            (string)this.LerCampo(TpcnTipoCampo.tcStr, tag, optional, minLength, maxLength, true, false);

        private string LerString(TpcnResources tag, ObOp optional, int minLength, int maxLength) =>
            (string)this.LerCampo(TpcnTipoCampo.tcStr, tag, optional, minLength, maxLength, true, false);

        private string LerString(TpcnResources tag, ObOp optional, int minLength, int maxLength, bool trim) =>
            (string)this.LerCampo(TpcnTipoCampo.tcStr, tag, optional, minLength, maxLength, trim, false);

        private object LerCampo(TpcnTipoCampo Tipo, TpcnResources tag, ObOp optional, int minLength, int maxLength, bool trim, bool returnNull)
            => LerCampo(Tipo, tag.ToString(), optional, minLength, maxLength, trim, returnNull);

        private object LerCampo(TpcnTipoCampo Tipo, string /*TpcnResources*/ tag, ObOp optional, int minLength, int maxLength, bool trim, bool returnNull)
        {
            int nDecimais = 0;
            string ConteudoTag = "";
            try
            {
                ConteudoTag = RetornarConteudoTag(tag.ToString(), trim, optional);

                if (ConteudoTag != "")
                    if (ConteudoTag.StartsWith(prefix))
                        ConteudoTag = "";

                if (string.IsNullOrEmpty(ConteudoTag) && (tag.ToString() == "cEAN" || tag.ToString() == "cEANTrib"))
                    return ConteudoTag = "SEM GTIN";

                int len = ConteudoTag.Length;
                if (len == 0 && (optional == ObOp.Opcional || optional == ObOp.None))
                {
                }
                else
                {
                    switch (Tipo)
                    {
                        case TpcnTipoCampo.tcHor:
                            maxLength = minLength = 8; //hh:mm:ss
                            break;
                        case TpcnTipoCampo.tcDatYYYY_MM_DD:
                            maxLength = minLength = 10; //yyyy-MM-dd
                            break;
                        case TpcnTipoCampo.tcDatYYYYMMDD:
                            maxLength = minLength = 8; //yyyyMMdd
                            break;
                        case TpcnTipoCampo.tcDatHor:
                            maxLength = minLength = 19; //aaaa-mm-dd hh:mm:ss
                            break;
                        default:
                            if (Tipo >= TpcnTipoCampo.tcDouble2 && Tipo <= TpcnTipoCampo.tcDouble10)
                            {
                                nDecimais = (int)Tipo;
                            }
                            else if (Tipo == TpcnTipoCampo.tcDec4)
                            {
                                nDecimais = 4;
                            }
                            else if (Tipo == TpcnTipoCampo.tcDec10)
                            {
                                nDecimais = 10;
                            }

                            break;
                    }

                    if (len == 0 && minLength > 0)
                    {
                        this.cMensagemErro += "Layout: " + this.layout.Replace(prefix, "") + Environment.NewLine;
                        this.cMensagemErro += string.Format("Segmento [{0}]: tag <{1}> deve ser informada.\r\n" +
                                                            "\tLinha: {2}: Conteudo do segmento: {3}",
                                                            this.FSegmento, tag.ToString(), this.LinhaLida + 1, this.Registro.Substring(1)) + Environment.NewLine;
                    }
                    else
                    {
                        switch (Tipo)
                        {
                            case TpcnTipoCampo.tcDouble2:
                            case TpcnTipoCampo.tcDouble3:
                            case TpcnTipoCampo.tcDouble4:
                            case TpcnTipoCampo.tcDouble5:
                            case TpcnTipoCampo.tcDouble6:
                            case TpcnTipoCampo.tcDouble7:
                            case TpcnTipoCampo.tcDouble8:
                            case TpcnTipoCampo.tcDouble9:
                            case TpcnTipoCampo.tcDouble10:
                            case TpcnTipoCampo.tcDec4:
                            case TpcnTipoCampo.tcDec10:
                                //quando numerico do tipo double não consiste o tamanho minimo nem maximo
                                break;
                            default:
                                if ((len > maxLength || len < minLength) && (maxLength + minLength > 0))
                                {
                                    this.cMensagemErro += "Layout: " + this.layout.Replace(prefix, "") + Environment.NewLine;
                                    this.cMensagemErro += string.Format("Segmento [{0}]: tag <{1}> deve ter seu tamanho entre {2} e {3}. Conteudo: {4}" +
                                                            "\r\n\tLinha: {5}: Conteudo do segmento: {6}",
                                                            this.FSegmento, tag.ToString(), minLength, maxLength, ConteudoTag, this.LinhaLida + 1, this.Registro.Substring(1)) + Environment.NewLine;
                                }
                                break;
                        }
                    }
                }

                if (optional == ObOp.Obrigatorio || ((optional == ObOp.Opcional || optional == ObOp.None) && len != 0))
                {
                    switch (Tipo)
                    {
                        case TpcnTipoCampo.tcDouble2:
                        case TpcnTipoCampo.tcDouble3:
                        case TpcnTipoCampo.tcDouble4:
                        case TpcnTipoCampo.tcDouble5:
                        case TpcnTipoCampo.tcDouble6:
                        case TpcnTipoCampo.tcDouble7:
                        case TpcnTipoCampo.tcDouble8:
                        case TpcnTipoCampo.tcDouble9:
                        case TpcnTipoCampo.tcDouble10:
                        case TpcnTipoCampo.tcDec4:
                        case TpcnTipoCampo.tcDec10:
                            {
                                int pos = ConteudoTag.IndexOf(".") + 1;
                                int ndec = (pos > 1 ? ConteudoTag.Substring(pos).Length : 0);
                                if (pos >= 1)
                                {
                                    string xdec = ConteudoTag.Substring(pos);
                                    //
                                    // ajusta o numero de casas decimais
                                    while (ndec > nDecimais)
                                    {
                                        if (xdec.Substring(ndec - 1, 1) == "0")
                                            --ndec;
                                        else
                                            break;
                                    }

                                    if (ndec > nDecimais)
                                    {
                                        this.cMensagemErro += "Layout: " + this.layout.Replace(prefix, "") + Environment.NewLine;
                                        this.cMensagemErro += string.Format("Segmento [{0}]: tag <{1}> número de casas decimais deve ser de {2} e existe(m) {3}" +
                                                                            "\r\n\tLinha: {4}: Conteudo do segmento: {5}",
                                                                            this.FSegmento, tag.ToString(), nDecimais, ndec, this.LinhaLida + 1, this.Registro.Substring(1)) + Environment.NewLine;
                                    }
                                }
                                else
                                    ndec = nDecimais;

                                #region -- atribui o numero de casas decimais que serão gravadas

                                if (ndec < (int)TpcnTipoCampo.tcDouble2 || ndec > (int)TpcnTipoCampo.tcDouble10)
                                    ndec = (int)TpcnTipoCampo.tcDouble2;

                                TpcnTipoCampo tipo = (TpcnTipoCampo)ndec;

                                if (tag == TpcnResources.vUnCom.ToString())
                                {
                                    NFe.det[NFe.det.Count - 1].Prod.vUnCom_Tipo = tipo;
                                }

                                if (tag == TpcnResources.vUnTrib.ToString())
                                {
                                    NFe.det[NFe.det.Count - 1].Prod.vUnTrib_Tipo = tipo;
                                }

                                if (tag == TpcnResources.qTotMes.ToString())
                                {
                                    NFe.cana.qTotMes_Tipo = tipo;
                                }

                                if (tag == TpcnResources.qTotAnt.ToString())
                                {
                                    NFe.cana.qTotAnt_Tipo = tipo;
                                }

                                if (tag == TpcnResources.qTotGer.ToString())
                                {
                                    NFe.cana.qTotGer_Tipo = tipo;
                                }

                                if (tag == TpcnResources.qtde.ToString())
                                {
                                    NFe.cana.fordia[NFe.cana.fordia.Count - 1].qtde_Tipo = tipo;
                                }

                                #endregion
                            }
                            break;
                    }
                }

                switch (Tipo)
                {
                    case TpcnTipoCampo.tcDatYYYYMMDD:
                        return this.getDate2(Tipo, ConteudoTag);

                    case TpcnTipoCampo.tcDatYYYY_MM_DD:
                    case TpcnTipoCampo.tcDatHor:
                        return this.getDateTime(Tipo, ConteudoTag);

                    case TpcnTipoCampo.tcHor:
                        return this.getTime(ConteudoTag);

                    case TpcnTipoCampo.tcDouble2:
                    case TpcnTipoCampo.tcDouble3:
                    case TpcnTipoCampo.tcDouble4:
                    case TpcnTipoCampo.tcDouble5:
                    case TpcnTipoCampo.tcDouble6:
                    case TpcnTipoCampo.tcDouble7:
                    case TpcnTipoCampo.tcDouble8:
                    case TpcnTipoCampo.tcDouble9:
                    case TpcnTipoCampo.tcDouble10:
                    case TpcnTipoCampo.tcDec4:
                    case TpcnTipoCampo.tcDec10:
                        if (string.IsNullOrEmpty(ConteudoTag) && returnNull)
                        {
                            return -9.99;
                        }
                        else
                        {
                            if (Tipo == TpcnTipoCampo.tcDec4 || Tipo == TpcnTipoCampo.tcDec10)
                            {
                                return Convert.ToDecimal("0" + ConteudoTag.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
                            }
                            else
                            {
                                return Convert.ToDouble("0" + ConteudoTag.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
                            }
                        }

                    case TpcnTipoCampo.tcInt:
                        if (string.IsNullOrEmpty(ConteudoTag) && returnNull)
                            return 100;
                        else
                            return Convert.ToInt32("0" + SomenteNumeros(ConteudoTag));

                    default:
                        return (trim ? ConteudoTag.Trim() : ConteudoTag);
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(layout))
                    this.cMensagemErro += "Layout: " + this.layout.Replace(prefix, "") + Environment.NewLine;
                this.cMensagemErro += string.Format("Segmento [{0}]: tag <{1}> Conteudo: {2}\r\n" +
                                                    "\tLinha: {3}: Conteudo do segmento: {4}\r\n\tMensagem de erro: {5}",
                                                    this.FSegmento, tag.ToString(), ConteudoTag, this.LinhaLida + 1, this.Registro.Substring(1),
                                                    ex.Message) + Environment.NewLine;
                switch (Tipo)
                {
                    case TpcnTipoCampo.tcHor:
                    case TpcnTipoCampo.tcDatYYYY_MM_DD:
                    case TpcnTipoCampo.tcDatYYYYMMDD:
                    case TpcnTipoCampo.tcDatHor:
                        return DateTime.MinValue;

                    case TpcnTipoCampo.tcDouble2:
                    case TpcnTipoCampo.tcDouble3:
                    case TpcnTipoCampo.tcDouble4:
                    case TpcnTipoCampo.tcDouble5:
                    case TpcnTipoCampo.tcDouble6:
                    case TpcnTipoCampo.tcDouble7:
                    case TpcnTipoCampo.tcDouble8:
                    case TpcnTipoCampo.tcDouble9:
                    case TpcnTipoCampo.tcDouble10:
                    case TpcnTipoCampo.tcDec4:
                    case TpcnTipoCampo.tcDec10:
                        return 0.0;

                    case TpcnTipoCampo.tcInt:
                        return 0;

                    default:
                        return "";
                }
            }
        }

        private int CasasDecimais75
        {
            get
            {
                return (double)NFe.infNFe.Versao >= 3.10 ? 7 : 5;
            }
        }
        private TpcnTipoCampo TipoCampo42
        {
            get
            {
                return (double)NFe.infNFe.Versao >= 3.10 ? TpcnTipoCampo.tcDouble4 : TpcnTipoCampo.tcDouble2;
            }
        }

        /// <summary>
        /// LerRegistro
        /// </summary>
        private void LerRegistro(string aRegistro)
        {
            if (aRegistro.StartsWith("*")) return;

            int lenPipesRegistro = aRegistro.Split(new char[] { '|' }).Length - 1;
            int nProd = NFe.det.Count - 1;
            this.Registro = aRegistro;
            this.FSegmento = this.Registro.Substring(1, this.Registro.IndexOf("|") - 1);
#if DEBUG
            Console.WriteLine("Segmento lido: {0} - linha: {1} - Pipes: {2}", FSegmento, this.LinhaLida + 1, lenPipesRegistro);
#endif
            try
            {
                /// Exemplo: "B_310"
                layout = this.LayoutTXT[this.FSegmento.ToUpper() + "_" +
                                        NFe.infNFe.Versao.ToString("0.00").Replace(".", "").Replace(",", "")].ToString();
            }
            catch
            {
                try
                {
                    /// Exemplo: "W02_310_21"
                    layout = this.LayoutTXT[this.FSegmento.ToUpper() + "_" +
                                            NFe.infNFe.Versao.ToString("0.00").Replace(".", "").Replace(",", "") + "_" +
                                            lenPipesRegistro.ToString()].ToString();
                }
                catch
                {
                    try
                    {
                        /// Exemplo: "I_23"
                        layout = this.LayoutTXT[this.FSegmento.ToUpper() + "_" +
                                                lenPipesRegistro.ToString()].ToString();
                    }
                    catch
                    {
                        try
                        {
                            layout = this.LayoutTXT[this.FSegmento.ToUpper()].ToString();
                        }
                        catch
                        {
                            layout = null;
                            ///
                            /// OPS!!! cometemos um erro
                            /// 
                            foreach (KeyValuePair<string, string> k in this._LayoutTXT)
                            {
                                if (k.Value.Substring(1, k.Value.IndexOf('|')).Equals(this.FSegmento + "|", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (k.Key.Contains("_400") && NFe.infNFe.Versao != (decimal)4) continue;
                                    if (k.Key.Contains("_310") && NFe.infNFe.Versao != (decimal)3.1) continue;
                                    if (k.Key.Contains("_200") && NFe.infNFe.Versao != (decimal)2) continue;

                                    if (k.Key.Contains("_310_"))
                                    {
                                        string kk = this.FSegmento.ToUpper() + "_310_" + lenPipesRegistro.ToString();
                                        if (!k.Key.Equals(kk)) continue;
                                    }
                                    if (k.Key.Contains("_400_"))
                                    {
                                        string kk = this.FSegmento.ToUpper() + "_400_" + lenPipesRegistro.ToString();
                                        if (!k.Key.Equals(kk)) continue;
                                    }
                                    layout = k.Value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            switch (this.FSegmento.ToUpper())
            {
                case "A":
                    double v = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.versao, ObOp.Opcional, 6);
                    this.chave = this.LerString(TpcnResources.ID, ObOp.Opcional, 0, 47);
                    this.chave = this.SomenteNumeros(this.chave);
                    if (!string.IsNullOrEmpty(this.chave) && this.chave.Length != 44)
                    {
                        throw new Exception("Chave de acesso inválida no segmento A");
                    }

                    NFe.infNFe.Versao = (v > 0 ? Convert.ToDecimal(v) : 2);
                    break;

                case "B":
                    ///
                    /// Grupo da TAG <ide>
                    /// 
                    #region -- <ide>

                    NFe.ide.cUF = this.LerInt32(TpcnResources.cUF, ObOp.Obrigatorio, 2, 2);
                    NFe.ide.cNF = this.LerInt32(TpcnResources.cNF, ObOp.Opcional, 8, 8);
                    NFe.ide.natOp = this.LerString(TpcnResources.natOp, ObOp.Obrigatorio, 1, 60);
                    NFe.ide.mod = (TpcnMod)this.LerInt32(TpcnResources.mod, ObOp.Obrigatorio, 2, 2);
                    NFe.ide.serie = this.LerInt32(TpcnResources.serie, ObOp.Obrigatorio, 1, 3);
                    NFe.ide.nNF = this.LerInt32(TpcnResources.nNF, ObOp.Obrigatorio, 1, 9);
                    if (NFe.infNFe.Versao >= 3)
                    {
                        NFe.ide.dhEmi = this.LerString(TpcnResources.dhEmi, ObOp.Obrigatorio, 19, 25);
                        NFe.ide.dhSaiEnt = this.LerString(TpcnResources.dhSaiEnt, ObOp.Opcional, 0, 25);
                        NFe.ide.idDest = (TpcnDestinoOperacao)this.LerInt32(TpcnResources.idDest, ObOp.Obrigatorio, 1, 1);

                        if (string.IsNullOrEmpty(NFe.ide.dhEmi) || Convert.ToDateTime(NFe.ide.dhEmi).Year == 1 ||
                            NFe.ide.dhEmi.EndsWith("00:00"))
                            throw new Exception("Data de emissão da nota inválida");

                        if (!string.IsNullOrEmpty(NFe.ide.dhSaiEnt))
                            if (Convert.ToDateTime(NFe.ide.dhSaiEnt).Year == 1)
                                throw new Exception("Data de saida da nota inválida");
                    }
                    else
                    {
                        NFe.ide.dEmi = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dEmi, ObOp.Obrigatorio, 10, 10, true, false);
                        NFe.ide.dSaiEnt = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dSaiEnt, ObOp.Opcional, 10, 10, true, false);
                        NFe.ide.hSaiEnt = (DateTime)this.LerCampo(TpcnTipoCampo.tcHor, TpcnResources.hSaiEnt, ObOp.Opcional, 8, 8, true, false);
                    }
                    NFe.ide.tpNF = (TpcnTipoNFe)this.LerInt32(TpcnResources.tpNF, ObOp.Obrigatorio, 1, 1);
                    NFe.ide.cMunFG = this.LerInt32(TpcnResources.cMunFG, ObOp.Obrigatorio, 7, 7);
                    NFe.ide.cMunFGIBS = this.LerInt32(TpcnResources.cMunFGIBS, ObOp.Opcional, 7, 7);
                    NFe.ide.tpImp = (TpcnTipoImpressao)this.LerInt32(TpcnResources.tpImp, ObOp.Obrigatorio, 1, 1);
                    NFe.ide.tpEmis = (TipoEmissao)this.LerInt32(TpcnResources.tpEmis, ObOp.Obrigatorio, 1, 1);
                    NFe.ide.cDV = this.LerInt32(TpcnResources.cDV, ObOp.Opcional, 1, 1);
                    NFe.ide.tpAmb = (TipoAmbiente)this.LerInt32(TpcnResources.tpAmb, ObOp.Obrigatorio, 1, 1);
                    NFe.ide.finNFe = (TpcnFinalidadeNFe)this.LerInt32(TpcnResources.finNFe, ObOp.Obrigatorio, 1, 1);
                    NFe.ide.tpNFDebito = (TpcnTipoNFDebito)this.LerInt32(TpcnResources.tpNFDebito, ObOp.Opcional, 2, 2);
                    NFe.ide.tpNFCredito = (TpcnTipoNFCredito)this.LerInt32(TpcnResources.tpNFCredito, ObOp.Opcional, 2, 2);
                    NFe.ide.indFinal = (TpcnConsumidorFinal)this.LerInt32(TpcnResources.indFinal, ObOp.Obrigatorio, 1, 1);
                    NFe.ide.indPres = (TpcnPresencaComprador)this.LerInt32(TpcnResources.indPres, ObOp.Obrigatorio, 1, 1);

                    NFe.ide.indIntermed = TpcnIntermediario.NaoInserirTagNoXML;
                    if (lenPipesRegistro >= 24)
                    {
                        NFe.ide.indIntermed = (TpcnIntermediario)this.LerInt32(TpcnResources.indIntermed, ObOp.Opcional, 1, 1, true);
                    }

                    NFe.ide.procEmi = (TpcnProcessoEmissao)this.LerInt32(TpcnResources.procEmi, ObOp.Obrigatorio, 1, 1);
                    NFe.ide.verProc = this.LerString(TpcnResources.verProc, ObOp.Obrigatorio, 1, 20);
                    NFe.ide.dhCont = this.LerString(TpcnResources.dhCont, ObOp.Opcional, 0, 25);
                    NFe.ide.xJust = this.LerString(TpcnResources.xJust, ObOp.Opcional, 15, 256);

                    if(lenPipesRegistro >= 28)
                    {
                        NFe.ide.dPrevEntrega = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dPrevEntrega, ObOp.Opcional,8, 10, true, false);
                    }
                        

                    if (!string.IsNullOrEmpty(this.chave))
                    {
                        if (NFe.ide.cNF == 0)
                            NFe.ide.cNF = Convert.ToInt32(this.chave.Substring(35, 8));

                        if (NFe.ide.cDV == 0)
                            NFe.ide.cDV = Convert.ToInt32(this.chave.Substring(this.chave.Length - 1, 1));
                    }
                    break;
                #endregion

                case "B13":
                case "BA02":
                    ///
                    /// Grupo da TAG <ide><NFref><refNFe>
                    ///
                    #region <ide><NFref><refNFe>

                    NFe.ide.NFref.Add(new NFref(this.LerString(TpcnResources.refNFe, ObOp.Obrigatorio, 44, 44), null));

                    #endregion
                    break;

                case "B14":
                case "BA03":
                    ///
                    /// Grupo da TAG <ide><NFref><RefNF>
                    ///
                    #region <ide><NFref><RefNF>
                    {
                        NFref item = new NFref();
                        item.refNF = new refNF();

                        item.refNF.cUF = this.LerInt32(TpcnResources.cUF, ObOp.Obrigatorio, 2, 2);
                        item.refNF.AAMM = this.LerString(TpcnResources.AAMM, ObOp.Obrigatorio, 4, 4);
                        item.refNF.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14);
                        item.refNF.mod = this.LerString(TpcnResources.mod, ObOp.Obrigatorio, 2, 2);
                        item.refNF.serie = this.LerInt32(TpcnResources.serie, ObOp.Obrigatorio, 1, 3);
                        item.refNF.nNF = this.LerInt32(TpcnResources.nNF, ObOp.Obrigatorio, 1, 9);

                        NFe.ide.NFref.Add(item);
                    }
                    #endregion
                    break;

                case "BA10":
                case "B20A":
                    #region B20a | BA10
                    {
                        NFref item = new NFref();
                        item.refNFP = new refNFP();
                        item.refNFP.cUF = this.LerInt32(TpcnResources.cUF, ObOp.Obrigatorio, 2, 2);
                        item.refNFP.AAMM = this.LerString(TpcnResources.AAMM, ObOp.Obrigatorio, 4, 4);
                        item.refNFP.IE = this.LerString(TpcnResources.IE, ObOp.Obrigatorio, 1, 14);
                        item.refNFP.mod = this.LerString(TpcnResources.mod, ObOp.Obrigatorio, 2, 2);
                        item.refNFP.serie = this.LerInt32(TpcnResources.serie, ObOp.Obrigatorio, 1, 3);
                        item.refNFP.nNF = this.LerInt32(TpcnResources.nNF, ObOp.Obrigatorio, 1, 9);
                        if (FSegmento.ToUpper().Equals("BA10"))
                        {
                            item.refCTe = this.LerString(TpcnResources.refCTe, ObOp.Opcional, 44, 44);
                        }
                        NFe.ide.NFref.Add(item);
                    }
                    #endregion
                    break;

                case "B20D":
                case "BA13":
                    if (NFe.ide.NFref.Count == 0 || (NFe.ide.NFref.Count > 0 && NFe.ide.NFref[NFe.ide.NFref.Count - 1].refNFP == null))
                        throw new Exception(FSegmento.ToUpper().Equals("B20D") ? "Segmento B20d sem segmento B20A" : "Segmento BA13 sem segmento BA10");
                    NFe.ide.NFref[NFe.ide.NFref.Count - 1].refNFP.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14);
                    break;

                case "B20E":
                case "BA14":
                    if (NFe.ide.NFref.Count == 0 || (NFe.ide.NFref.Count > 0 && NFe.ide.NFref[NFe.ide.NFref.Count - 1].refNFP == null))
                        throw new Exception(FSegmento.ToUpper().Equals("B20E") ? "Segmento B20e sem segmento B20A" : "Segmento BA14 sem segmento BA10");
                    NFe.ide.NFref[NFe.ide.NFref.Count - 1].refNFP.CPF = this.LerString(TpcnResources.CPF, ObOp.Obrigatorio, 11, 11);
                    break;

                case "BA19":
                case "B20I":
                    //layout = "§BA19|refCTe"; //ok
                    NFe.ide.NFref.Add(new NFref(null, LerString(TpcnResources.refCTe, ObOp.Obrigatorio, 44, 44)));
                    break;

                case "B20J":
                case "BA20":
                    //layout = prefix + this.FSegmento + "|mod|nECF|nCOO"; //ok
                    {
                        NFref item = new NFref();
                        item.refECF = new refECF();
                        item.refECF.mod = this.LerString(TpcnResources.mod, ObOp.Obrigatorio, 2, 2);
                        item.refECF.nECF = this.LerInt32(TpcnResources.nECF, ObOp.Obrigatorio, 1, 3);
                        item.refECF.nCOO = this.LerInt32(TpcnResources.nCOO, ObOp.Obrigatorio, 1, 6);
                        NFe.ide.NFref.Add(item);
                    }
                    break;

                case "B31":
                    //layout = B31|tpEnteGov|pRedutor|tpOperGov|
                    NFe.ide.gCompraGov.tpEnteGov = (TpcnTipoEnteGovernamental)this.LerInt32(TpcnResources.tpEnteGov, ObOp.Obrigatorio, 1, 1);
                    NFe.ide.gCompraGov.pRedutor = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pRedutor, ObOp.Obrigatorio, 7);
                    NFe.ide.gCompraGov.tpOperGov = (TpcnTipoOperacaoEnteGovernamental)this.LerInt32(TpcnResources.tpOperGov, ObOp.Obrigatorio, 1, 1);
                    break;

                case "BB01":
                    //layout = BB01|refNFe|

                    NFe.ide.gPagAntecipado.refNFe.Add(this.LerString(TpcnResources.refNFe, ObOp.Obrigatorio, 1, 99));
                    break;

                case "C":
                    //layout = "§C|xNome|xFant|IE|IEST|IM|CNAE|CRT"; //okz
                    ///
                    /// Grupo da TAG <emit>
                    ///
                    #region <emit>

                    NFe.emit.xNome = this.LerString(TpcnResources.xNome, ObOp.Obrigatorio, 2, 60);
                    NFe.emit.xFant = this.LerString(TpcnResources.xFant, ObOp.Opcional, 1, 60);
                    NFe.emit.IE = this.LerString(TpcnResources.IE, ObOp.Opcional, 0, 14);
                    NFe.emit.IEST = this.LerString(TpcnResources.IEST, ObOp.Opcional, 2, 14);
                    NFe.emit.IM = this.LerString(TpcnResources.IM, ObOp.Opcional, 1, 15);
                    NFe.emit.CNAE = this.LerString(TpcnResources.CNAE, ObOp.Opcional, 7, 7);
                    NFe.emit.CRT = (TpcnCRT)this.LerInt32(TpcnResources.CRT, ObOp.Obrigatorio, 1, 1);

                    #endregion
                    break;

                case "C02":
                    //layout = "§C02|CNPJ"; //ok
                    NFe.emit.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14);
                    break;

                case "C02A":
                    //layout = "§C02A|CPF"; //ok
                    NFe.emit.CPF = this.LerString(TpcnResources.CPF, ObOp.Obrigatorio, 11, 11);
                    break;

                case "C05":
                    //layout = "§C05|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|CEP|cPais|xPais|fone"; //ok
                    ///
                    /// Grupo da TAG <emit><EnderEmit>
                    /// 
                    #region <emit><EnderEmit>

                    NFe.emit.enderEmit.xLgr = this.LerString(TpcnResources.xLgr, ObOp.Obrigatorio, 1, 60);
                    NFe.emit.enderEmit.nro = this.LerString(TpcnResources.nro, ObOp.Obrigatorio, 1, 60);
                    NFe.emit.enderEmit.xCpl = this.LerString(TpcnResources.xCpl, ObOp.Opcional, 1, 60);
                    NFe.emit.enderEmit.xBairro = this.LerString(TpcnResources.xBairro, ObOp.Obrigatorio, 2, 60);
                    NFe.emit.enderEmit.cMun = this.LerInt32(TpcnResources.cMun, ObOp.Obrigatorio, 7, 7);
                    NFe.emit.enderEmit.xMun = this.LerString(TpcnResources.xMun, ObOp.Obrigatorio, 2, 60);
                    NFe.emit.enderEmit.UF = this.LerString(TpcnResources.UF, ObOp.Obrigatorio, 2, 2);
                    NFe.emit.enderEmit.CEP = this.LerInt32(TpcnResources.CEP, ObOp.Opcional, 0, 8);
                    NFe.emit.enderEmit.cPais = this.LerInt32(TpcnResources.cPais, ObOp.Obrigatorio, 4, 4);
                    NFe.emit.enderEmit.xPais = this.LerString(TpcnResources.xPais, ObOp.Opcional, 1, 60);
                    NFe.emit.enderEmit.fone = this.LerString(TpcnResources.fone, ObOp.Opcional, 6, 14);

                    #endregion
                    break;

                case "D":
                    //layout = "§D|CNPJ|xOrgao|matr|xAgente|fone|UF|nDAR|dEmi|vDAR|repEmi|dPag"; //ok
                    ///
                    /// Grupo da TAG <avulsa>
                    /// 
                    #region <avulsa>

                    NFe.avulsa.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14);
                    NFe.avulsa.xOrgao = this.LerString(TpcnResources.xOrgao, ObOp.Obrigatorio, 1, 60);
                    NFe.avulsa.matr = this.LerString(TpcnResources.matr, ObOp.Obrigatorio, 1, 60);
                    NFe.avulsa.xAgente = this.LerString(TpcnResources.xAgente, ObOp.Obrigatorio, 1, 60);
                    NFe.avulsa.fone = this.LerString(TpcnResources.fone, ObOp.Obrigatorio, 6, 14);
                    NFe.avulsa.UF = this.LerString(TpcnResources.UF, ObOp.Obrigatorio, 2, 2);
                    NFe.avulsa.nDAR = this.LerString(TpcnResources.nDAR, ObOp.Obrigatorio, 1, 60);
                    NFe.avulsa.dEmi = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dEmi, ObOp.Obrigatorio, 10, 10, true, false);
                    NFe.avulsa.vDAR = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDAR, ObOp.Obrigatorio, 15);
                    NFe.avulsa.repEmi = this.LerString(TpcnResources.repEmi, ObOp.Obrigatorio, 1, 60);
                    NFe.avulsa.dPag = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dPag, ObOp.Opcional, 10, 10, true, false);

                    #endregion
                    break;

                case "E":
                    //layout = (NFe.infNFe.Versao >= 3 ? "§E|xNome|indIEDest|IE|ISUF|IM|email" : "§E|xNome|IE|ISUF|email");
                    ///
                    /// Grupo da TAG <dest>
                    /// 
                    #region <dest>

                    NFe.dest.xNome = this.LerString(TpcnResources.xNome, (NFe.infNFe.Versao >= 3 && NFe.ide.mod != TpcnMod.modNFe ? ObOp.Opcional : ObOp.Obrigatorio), 2, 60);
                    if (NFe.infNFe.Versao >= 3)
                        NFe.dest.indIEDest = (TpcnindIEDest)this.LerInt32(TpcnResources.indIEDest, ObOp.Opcional, 0, 1);
                    NFe.dest.IE = this.LerString(TpcnResources.IE, ObOp.Opcional, 0, 14);
                    NFe.dest.ISUF = this.LerString(TpcnResources.ISUF, ObOp.Opcional, 8, 9);
                    if (NFe.infNFe.Versao >= 3)
                        NFe.dest.IM = this.LerString(TpcnResources.IM, ObOp.Opcional, 1, 15);
                    NFe.dest.email = this.LerString(TpcnResources.email, ObOp.Opcional, 1, 60);

                    #endregion
                    break;

                case "E02":
                    //layout = "§E02|CNPJ"; //ok
                    NFe.dest.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Opcional, 14, 14);
                    break;

                case "E03":
                    //layout = "§E03|CPF"; //ok
                    if (NFe.ide.mod == TpcnMod.modNFCe && NFe.infNFe.Versao >= 3) //nfc-e
                        NFe.dest.CPF = this.LerString(TpcnResources.CPF, ObOp.Opcional, 11, 11);
                    else
                        NFe.dest.CPF = this.LerString(TpcnResources.CPF, ObOp.Obrigatorio, 11, 11);
                    break;

                case "E03A":
                    //layout = "§E03a|idEstrangeiro"; //ok
                    NFe.dest.idEstrangeiro = this.LerString(TpcnResources.idEstrangeiro, ObOp.Opcional, 5, 20);
                    if (string.IsNullOrEmpty(NFe.dest.idEstrangeiro) && string.IsNullOrEmpty(NFe.dest.CPF))
                        NFe.dest.idEstrangeiro = "NAO GERAR TAG";
                    break;

                case "E05":
                    //layout = "§E05|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|CEP|cPais|xPais|fone"; //ok
                    ///
                    /// Grupo da TAG <dest><EnderDest>
                    /// 
                    #region <dest><EnderDest>
                    NFe.dest.enderDest.xLgr = this.LerString(TpcnResources.xLgr, ObOp.Obrigatorio, 1, 60);
                    NFe.dest.enderDest.nro = this.LerString(TpcnResources.nro, ObOp.Obrigatorio, 1, 60);
                    NFe.dest.enderDest.xCpl = this.LerString(TpcnResources.xCpl, ObOp.Opcional, 1, 60);
                    NFe.dest.enderDest.xBairro = this.LerString(TpcnResources.xBairro, ObOp.Obrigatorio, 1, 60);
                    NFe.dest.enderDest.cMun = this.LerInt32(TpcnResources.cMun, ObOp.Obrigatorio, 7, 7);
                    NFe.dest.enderDest.xMun = this.LerString(TpcnResources.xMun, ObOp.Obrigatorio, 2, 60);
                    NFe.dest.enderDest.UF = this.LerString(TpcnResources.UF, ObOp.Obrigatorio, 2, 2);
                    NFe.dest.enderDest.CEP = this.LerInt32(TpcnResources.CEP, ObOp.Opcional, 0, 8);
                    NFe.dest.enderDest.cPais = this.LerInt32(TpcnResources.cPais, ObOp.Obrigatorio, 2, 4);
                    NFe.dest.enderDest.xPais = this.LerString(TpcnResources.xPais, ObOp.Opcional, 2, 60);
                    NFe.dest.enderDest.fone = this.LerString(TpcnResources.fone, ObOp.Opcional, 6, 14);
                    #endregion
                    break;

                case "F":
                    //layout = "§F|xLgr|nro|xCpl|xBairro|cMun|xMun|UF"; //ok
                    //layout = "§F|CNPJ_CPF|xNome|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|CEP|cPais|xPais|fone|email|IE|"
                    ///
                    /// Grupo da TAG <retirada> 
                    /// 
                    {
                        #region <retirada>
                        bool novo;
                        if ((novo = lenPipesRegistro == 16 || NFe.infNFe.Versao >= 4))
                        {
                            NFe.retirada.CNPJ = this.LerString(TpcnResources.CNPJ_CPF, ObOp.Opcional, 0, 0);
                            if (!string.IsNullOrEmpty(NFe.retirada.CNPJ) && NFe.retirada.CNPJ.Length == 11)
                            {
                                NFe.retirada.CPF = NFe.retirada.CNPJ;
                                NFe.retirada.CNPJ = "";
                            }
                            NFe.retirada.xNome = this.LerString(TpcnResources.xNome, ObOp.Opcional, 2, 60);
                        }
                        NFe.retirada.xLgr = this.LerString(TpcnResources.xLgr, ObOp.Obrigatorio, 1, 60);
                        NFe.retirada.nro = this.LerString(TpcnResources.nro, ObOp.Obrigatorio, 1, 60);
                        NFe.retirada.xCpl = this.LerString(TpcnResources.xCpl, ObOp.Opcional, 1, 60);
                        NFe.retirada.xBairro = this.LerString(TpcnResources.xBairro, ObOp.Obrigatorio, 1, 60);
                        NFe.retirada.cMun = this.LerInt32(TpcnResources.cMun, ObOp.Obrigatorio, 7, 7);
                        NFe.retirada.xMun = this.LerString(TpcnResources.xMun, ObOp.Obrigatorio, 2, 60);
                        NFe.retirada.UF = this.LerString(TpcnResources.UF, ObOp.Obrigatorio, 2, 2);
                        if (novo)
                        {
                            NFe.retirada.CEP = this.LerString(TpcnResources.CEP, ObOp.Opcional, 8, 8);
                            NFe.retirada.cPais = this.LerInt32(TpcnResources.cPais, ObOp.Opcional, 4, 4);
                            NFe.retirada.xPais = this.LerString(TpcnResources.xPais, ObOp.Opcional, 2, 60);
                            NFe.retirada.fone = this.LerString(TpcnResources.fone, ObOp.Opcional, 6, 14);
                            NFe.retirada.email = this.LerString(TpcnResources.email, ObOp.Opcional, 1, 60);
                            NFe.retirada.IE = this.LerString(TpcnResources.IE, ObOp.Opcional, 2, 14);
                        }
                        #endregion
                    }
                    break;

                case "F02":
                    //layout = "§F02|CNPJ"; //ok
                    NFe.retirada.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14);
                    break;

                case "F02A":
                    //layout = "§F02a|CPF"; //ok
                    NFe.retirada.CPF = this.LerString(TpcnResources.CPF, ObOp.Obrigatorio, 11, 11);
                    break;

                case "G":
                    //layout = "§G|xLgr|nro|xCpl|xBairro|cMun|xMun|UF"; //ok
                    //layout = "§G|CNPJ_CPF|xNome|xLgr|nro|xCpl|xBairro|cMun|xMun|UF|CEP|cPais|xPais|fone|email|IE|"
                    ///
                    /// Grupo da TAG <entrega>
                    /// 
                    {
                        #region <entrega>
                        bool novo;
                        if ((novo = lenPipesRegistro == 16 || NFe.infNFe.Versao >= 4))
                        {
                            NFe.entrega.CNPJ = this.LerString(TpcnResources.CNPJ_CPF, ObOp.Opcional, 0, 0);
                            if (!string.IsNullOrEmpty(NFe.entrega.CNPJ) && NFe.entrega.CNPJ.Length == 11)
                            {
                                NFe.entrega.CPF = NFe.entrega.CNPJ;
                                NFe.entrega.CNPJ = "";
                            }
                            NFe.entrega.xNome = this.LerString(TpcnResources.xNome, ObOp.Opcional, 2, 60);
                        }
                        NFe.entrega.xLgr = this.LerString(TpcnResources.xLgr, ObOp.Obrigatorio, 1, 60);
                        NFe.entrega.nro = this.LerString(TpcnResources.nro, ObOp.Obrigatorio, 1, 60);
                        NFe.entrega.xCpl = this.LerString(TpcnResources.xCpl, ObOp.Opcional, 1, 60);
                        NFe.entrega.xBairro = this.LerString(TpcnResources.xBairro, ObOp.Obrigatorio, 1, 60);
                        NFe.entrega.cMun = this.LerInt32(TpcnResources.cMun, ObOp.Obrigatorio, 7, 7);
                        NFe.entrega.xMun = this.LerString(TpcnResources.xMun, ObOp.Obrigatorio, 2, 60);
                        NFe.entrega.UF = this.LerString(TpcnResources.UF, ObOp.Obrigatorio, 2, 2);
                        if (novo)
                        {
                            NFe.entrega.CEP = this.LerString(TpcnResources.CEP, ObOp.Opcional, 8, 8);
                            NFe.entrega.cPais = this.LerInt32(TpcnResources.cPais, ObOp.Opcional, 4, 4);
                            NFe.entrega.xPais = this.LerString(TpcnResources.xPais, ObOp.Opcional, 2, 60);
                            NFe.entrega.fone = this.LerString(TpcnResources.fone, ObOp.Opcional, 6, 14);
                            NFe.entrega.email = this.LerString(TpcnResources.email, ObOp.Opcional, 1, 60);
                            NFe.entrega.IE = this.LerString(TpcnResources.IE, ObOp.Opcional, 2, 14);
                        }
                        #endregion
                    }
                    break;

                case "G02":
                    //layout = prefix + this.FSegmento + "|CNPJ"; //ok
                    NFe.entrega.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14);
                    break;

                case "G02A":
                    //layout = prefix + this.FSegmento + "|CPF"; //ok
                    NFe.entrega.CPF = this.LerString(TpcnResources.CPF, ObOp.Obrigatorio, 11, 11);
                    break;

                case "G51":
                case "GA02":
                    //layout = prefix + this.FSegmento + "|CNPJ"; //ok
                    NFe.autXML.Add(new autXML { CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14) });
                    break;

                case "G52":
                case "GA03":
                    //layout = prefix + this.FSegmento + "|CPF"; //ok
                    NFe.autXML.Add(new autXML { CPF = this.LerString(TpcnResources.CPF, ObOp.Obrigatorio, 11, 11) });
                    break;

                ///
                /// Grupo da TAG <det>
                /// 
                case "H":
                    //layout = "§H|nItem|infAdProd"; //ok
                    NFe.det.Add(new Det());
                    nProd = NFe.det.Count - 1;
                    NFe.det[nProd].Prod.nItem = this.LerInt32(TpcnResources.NItem, ObOp.Obrigatorio, 1, 3);
                    NFe.det[nProd].infAdProd = this.LerString(TpcnResources.infAdProd, ObOp.Opcional, 0, 500, false);
                    break;

                case "I":
                    ///
                    /// Grupo da TAG <det><prod>
                    /// 
                    #region <det><prod>

                    NFe.det[nProd].Prod.cProd = this.LerString(TpcnResources.cProd, ObOp.Obrigatorio, 1, 60);
                    NFe.det[nProd].Prod.cEAN = this.LerString(TpcnResources.cEAN, ObOp.Obrigatorio, 0, 14);

                    if (lenPipesRegistro >= 30)
                    {
                        NFe.det[nProd].Prod.cBarra = this.LerString(TpcnResources.cBarra, ObOp.Opcional, 0, 30);
                    }

                    NFe.det[nProd].Prod.xProd = this.LerString(TpcnResources.xProd, ObOp.Obrigatorio, 1, 120);
                    NFe.det[nProd].Prod.NCM = this.LerString(TpcnResources.NCM, ObOp.Obrigatorio, 2, 8);
                    NFe.det[nProd].Prod.NVE = this.LerString(TpcnResources.NVE, ObOp.Opcional, 0, 6);
                    NFe.det[nProd].Prod.CEST = this.LerInt32(TpcnResources.CEST, ObOp.Opcional, 0, 7);

                    switch (this.LerString(TpcnResources.indEscala, ObOp.Opcional, 1, 1))
                    {
                        case "S":
                            NFe.det[nProd].Prod.indEscala = TpcnIndicadorEscala.ieSomaTotalNFe;
                            break;
                        case "N":
                            NFe.det[nProd].Prod.indEscala = TpcnIndicadorEscala.ieNaoSomaTotalNFe;
                            break;
                        default:
                            NFe.det[nProd].Prod.indEscala = TpcnIndicadorEscala.ieNenhum;
                            break;
                    }

                    NFe.det[nProd].Prod.CNPJFab = this.LerString(TpcnResources.CNPJFab, ObOp.Opcional, 0, 14);
                    NFe.det[nProd].Prod.cBenef = this.LerString(TpcnResources.cBenef, ObOp.Opcional, 0, 10);
                    NFe.det[nProd].Prod.EXTIPI = this.LerString(TpcnResources.EXTIPI, ObOp.Opcional, 2, 3);
                    NFe.det[nProd].Prod.CFOP = this.LerString(TpcnResources.CFOP, ObOp.Obrigatorio, 4, 4);
                    NFe.det[nProd].Prod.uCom = this.LerString(TpcnResources.uCom, ObOp.Obrigatorio, 1, 6);
                    NFe.det[nProd].Prod.qCom = this.LerDecimal(TpcnTipoCampo.tcDec4, TpcnResources.qCom, ObOp.Obrigatorio, 11);
                    NFe.det[nProd].Prod.vUnCom = this.LerDecimal(TpcnTipoCampo.tcDec10, TpcnResources.vUnCom, ObOp.Opcional, 21);
                    NFe.det[nProd].Prod.vProd = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vProd, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Prod.cEANTrib = this.LerString(TpcnResources.cEANTrib, ObOp.Obrigatorio, 0, 14);

                    if (lenPipesRegistro >= 30)
                    {
                        NFe.det[nProd].Prod.cBarraTrib = this.LerString(TpcnResources.cBarraTrib, ObOp.Opcional, 0, 30);
                    }

                    NFe.det[nProd].Prod.uTrib = this.LerString(TpcnResources.uTrib, ObOp.Obrigatorio, 1, 6);
                    NFe.det[nProd].Prod.qTrib = this.LerDecimal(TpcnTipoCampo.tcDec4, TpcnResources.qTrib, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Prod.vUnTrib = this.LerDecimal(TpcnTipoCampo.tcDec10, TpcnResources.vUnTrib, ObOp.Obrigatorio, 21);
                    NFe.det[nProd].Prod.vFrete = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFrete, ObOp.Opcional, 15);
                    NFe.det[nProd].Prod.vSeg = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vSeg, ObOp.Opcional, 15);
                    NFe.det[nProd].Prod.vDesc = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDesc, ObOp.Opcional, 15);
                    NFe.det[nProd].Prod.vOutro = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vOutro, ObOp.Opcional, 15);
                    NFe.det[nProd].Prod.indTot = (TpcnIndicadorTotal)this.LerInt32(TpcnResources.indTot, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Prod.xPed = this.LerString(TpcnResources.xPed, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Prod.nItemPed = this.LerString(TpcnResources.nItemPed, ObOp.Opcional, 0, 6);
                    NFe.det[nProd].Prod.nFCI = this.LerString(TpcnResources.nFCI, ObOp.Opcional, 0, 255);
                    NFe.det[nProd].Imposto.ISSQN.cSitTrib = string.Empty;

                    #endregion
                    break;

                case "I05G":
                    var credPresumido = new CredPresumido();
                    credPresumido.cCredPresumido = this.LerString(TpcnResources.cCredPresumido, ObOp.Obrigatorio, 8, 10);
                    credPresumido.pCredPresumido = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pCredPresumido, ObOp.Obrigatorio, 8);
                    credPresumido.vCredPresumido = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPresumido, ObOp.Obrigatorio, 16);

                    NFe.det[nProd].Prod.credPresumido.Add(credPresumido);
                    break;

                case "I05A":
                    NFe.det[nProd].Prod.NVE = this.LerString(TpcnResources.NVE, ObOp.Opcional, 0, 6);
                    break;

                case "I05K":
                    NFe.det[nProd].Prod.tpCredPresIBSZFM = this.LerString(TpcnResources.tpCredPresIBSZFM, ObOp.Opcional, 1, 1);
                    break;

                case "I05C":
                case "I05W":
                    NFe.det[nProd].Prod.CEST = this.LerInt32(TpcnResources.CEST, ObOp.Opcional, 0, 7);
                    if (NFe.infNFe.Versao >= 4 && lenPipesRegistro == 4)
                    {
                        NFe.det[nProd].Prod.indEscala = (TpcnIndicadorEscala)this.LerInt32(TpcnResources.indEscala, ObOp.Opcional, 1, 1);
                        NFe.det[nProd].Prod.CNPJFab = this.LerString(TpcnResources.CNPJFab, ObOp.Opcional, 0, 14);
                    }
                    break;

                case "I17":
                    NFe.det[nProd].Prod.indBemMovelUsado = (TpcnIndicadorBemMovelUsado)this.LerInt32(TpcnResources.indBemMovelUsado, ObOp.Opcional, 1, 1);
                    break;

                case "I18":
                    ///
                    /// Grupo da TAG <det><prod><DI>
                    /// 
                    #region <det><prod><DI>

                    DI diItem = new DI();

                    diItem.nDI = this.LerString(TpcnResources.nDI, ObOp.Obrigatorio, 1, 12);
                    diItem.dDI = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dDI, ObOp.Obrigatorio, 10, 10, true, false);
                    diItem.xLocDesemb = this.LerString(TpcnResources.xLocDesemb, ObOp.Obrigatorio, 1, 60);
                    diItem.UFDesemb = this.LerString(TpcnResources.UFDesemb, ObOp.Obrigatorio, 2, 2);
                    diItem.dDesemb = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dDesemb, ObOp.Obrigatorio, 10, 10, true, false);
                    if (NFe.infNFe.Versao >= 3)
                    {
                        diItem.tpViaTransp = (TpcnTipoViaTransp)this.LerInt32(TpcnResources.tpViaTransp, ObOp.Opcional, 1, 2);
                        diItem.vAFRMM = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vAFRMM, ObOp.Opcional, 15);
                        diItem.tpIntermedio = (TpcnTipoIntermedio)this.LerInt32(TpcnResources.tpIntermedio, ObOp.Opcional, 1, 1);
                        diItem.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Opcional, 14, 14);

                        if (lenPipesRegistro >= 13)
                        {
                            diItem.CPF = this.LerString(TpcnResources.CPF, ObOp.Opcional, 11, 11);
                        }

                        diItem.UFTerceiro = this.LerString(TpcnResources.UFTerceiro, ObOp.Opcional, 2, 2);
                    }
                    diItem.cExportador = this.LerString(TpcnResources.cExportador, ObOp.Obrigatorio, 1, 60);
                    NFe.det[nProd].Prod.DI.Add(diItem);
                    #endregion
                    break;

                case "I25":
                    ///
                    /// Grupo da TAG <det><prod><DI><adi> 
                    /// 
                    #region <det><prod><DI><adi>

                    Adi adiItem = new Adi();

                    adiItem.nAdicao = this.LerInt32(TpcnResources.nAdicao, ObOp.Obrigatorio, 1, 3);
                    adiItem.nSeqAdi = this.LerInt32(TpcnResources.nSeqAdic, ObOp.Obrigatorio, 1, 3);
                    adiItem.cFabricante = this.LerString(TpcnResources.cFabricante, ObOp.Obrigatorio, 1, 60);
                    adiItem.vDescDI = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDescDI, ObOp.Opcional, 15);
                    if (NFe.infNFe.Versao >= 3)
                        adiItem.nDraw = this.LerString(TpcnResources.nDraw, ObOp.Opcional, 0, 11);

                    NFe.det[nProd].Prod.DI[NFe.det[nProd].Prod.DI.Count - 1].adi.Add(adiItem);
                    #endregion
                    break;

                case "I50":
                    #region <det><prod><detExport>
                    NFe.det[nProd].Prod.detExport.Add(new detExport { nDraw = this.LerString(TpcnResources.nDraw, ObOp.Opcional, 0, 11) });
                    #endregion
                    break;

                case "I52":
                    #region <det><prod><detExport><exportInd>
                    NFe.det[nProd].Prod.detExport[NFe.det[nProd].Prod.detExport.Count - 1].exportInd.nRE = this.LerString(TpcnResources.nRE, ObOp.Opcional, 1, 12);
                    NFe.det[nProd].Prod.detExport[NFe.det[nProd].Prod.detExport.Count - 1].exportInd.chNFe = this.LerString(TpcnResources.chNFe, ObOp.Obrigatorio, 44, 44);
                    NFe.det[nProd].Prod.detExport[NFe.det[nProd].Prod.detExport.Count - 1].exportInd.qExport = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qExport, ObOp.Obrigatorio, 1, 15);
                    #endregion
                    break;

                case "I80":
                    #region <det><prod><rastro>
                    var r = new Rastro();
                    r.nLote = this.LerString(TpcnResources.nLote, ObOp.Obrigatorio, 1, 20);
                    r.qLote = this.LerDouble(TpcnTipoCampo.tcDouble3, TpcnResources.qLote, ObOp.Obrigatorio, 11);
                    r.dFab = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dFab, ObOp.Obrigatorio, 10, 10, true, false);
                    r.dVal = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dVal, ObOp.Obrigatorio, 10, 10, true, false);
                    r.cAgreg = this.LerString(TpcnResources.cAgreg, ObOp.Opcional, 0, 20);
                    NFe.det[nProd].Prod.rastro.Add(r);
                    #endregion
                    break;

                case "IRT":
                    //layout = "IRT|CNPJ|xContato|email|fone|idCSRT|hashCSRT|"
                    NFe.resptecnico.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14);
                    NFe.resptecnico.xContato = this.LerString(nameof(RespTecnico.xContato), ObOp.Obrigatorio, 2, 60);
                    NFe.resptecnico.email = this.LerString(TpcnResources.email, ObOp.Obrigatorio, 2, 60);
                    NFe.resptecnico.fone = this.LerString(TpcnResources.fone, ObOp.Obrigatorio, 6, 14);
                    NFe.resptecnico.idCSRT = this.LerInt32(nameof(RespTecnico.idCSRT), ObOp.Opcional, 2, 2);
                    NFe.resptecnico.hashCSRT = this.LerString(nameof(RespTecnico.hashCSRT), ObOp.Opcional, 28, 28);
                    break;

                case "J":
                case "JA":
                    ///
                    /// Grupo da TAG <det><prod><veicProd>
                    /// 
                    #region <det><prod><veicProd>

                    NFe.det[nProd].Prod.veicProd.tpOp = this.LerString(TpcnResources.tpOp, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Prod.veicProd.chassi = this.LerString(TpcnResources.chassi, ObOp.Obrigatorio, 17, 17);
                    NFe.det[nProd].Prod.veicProd.cCor = this.LerString(TpcnResources.cCor, ObOp.Obrigatorio, 1, 4);
                    NFe.det[nProd].Prod.veicProd.xCor = this.LerString(TpcnResources.xCor, ObOp.Obrigatorio, 1, 40);
                    NFe.det[nProd].Prod.veicProd.pot = this.LerString(TpcnResources.pot, ObOp.Obrigatorio, 1, 4);
                    NFe.det[nProd].Prod.veicProd.cilin = this.LerString(TpcnResources.cilin, ObOp.Obrigatorio, 1, 4);
                    NFe.det[nProd].Prod.veicProd.pesoL = this.LerString(TpcnResources.pesoL, ObOp.Obrigatorio, 1, 9);
                    NFe.det[nProd].Prod.veicProd.pesoB = this.LerString(TpcnResources.pesoB, ObOp.Obrigatorio, 1, 9);
                    NFe.det[nProd].Prod.veicProd.nSerie = this.LerString(TpcnResources.nSerie, ObOp.Obrigatorio, 1, 9);
                    NFe.det[nProd].Prod.veicProd.tpComb = this.LerString(TpcnResources.tpComb, ObOp.Obrigatorio, 1, 2);
                    NFe.det[nProd].Prod.veicProd.nMotor = this.LerString(TpcnResources.nMotor, ObOp.Obrigatorio, 1, 21);
                    NFe.det[nProd].Prod.veicProd.CMT = this.LerString(TpcnResources.CMT, ObOp.Obrigatorio, 1, 9);
                    NFe.det[nProd].Prod.veicProd.dist = this.LerString(TpcnResources.dist, ObOp.Obrigatorio, 1, 4);
                    NFe.det[nProd].Prod.veicProd.anoMod = this.LerInt32(TpcnResources.anoMod, ObOp.Obrigatorio, 4, 4);
                    NFe.det[nProd].Prod.veicProd.anoFab = this.LerInt32(TpcnResources.anoFab, ObOp.Obrigatorio, 4, 4);
                    NFe.det[nProd].Prod.veicProd.tpPint = this.LerString(TpcnResources.tpPint, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Prod.veicProd.tpVeic = this.LerInt32(TpcnResources.tpVeic, ObOp.Obrigatorio, 1, 2);
                    NFe.det[nProd].Prod.veicProd.espVeic = this.LerInt32(TpcnResources.espVeic, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Prod.veicProd.VIN = this.LerString(TpcnResources.VIN, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Prod.veicProd.condVeic = this.LerString(TpcnResources.condVeic, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Prod.veicProd.cMod = this.LerString(TpcnResources.cMod, ObOp.Obrigatorio, 1, 6);
                    NFe.det[nProd].Prod.veicProd.cCorDENATRAN = this.LerInt32(TpcnResources.cCorDENATRAN, ObOp.Obrigatorio, 1, 2);
                    NFe.det[nProd].Prod.veicProd.lota = this.LerInt32(TpcnResources.lota, ObOp.Obrigatorio, 1, 3);
                    NFe.det[nProd].Prod.veicProd.tpRest = this.LerInt32(TpcnResources.tpRest, ObOp.Obrigatorio, 1, 1);


                    #endregion
                    break;

                case "K":
                    ///
                    /// Grupo da TAG <det><prod><med>
                    /// 
                    #region <det><prod><med>
                    Med medItem = new Med();
                    if (NFe.infNFe.Versao >= 4)
                    {
                        medItem.cProdANVISA = LerString(TpcnResources.cProdANVISA, ObOp.Obrigatorio, 1, 13);
                        medItem.xMotivoIsencao = LerString(nameof(medItem.xMotivoIsencao), ObOp.Opcional, 1, 255);
                    }
                    else
                    {
                        medItem.nLote = LerString(TpcnResources.nLote, ObOp.Obrigatorio, 1, 20);
                        medItem.qLote = LerDouble(TpcnTipoCampo.tcDouble3, TpcnResources.qLote, ObOp.Obrigatorio, 11);
                        medItem.dFab = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dFab, ObOp.Obrigatorio, 10, 10, true, false);
                        medItem.dVal = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dVal, ObOp.Obrigatorio, 10, 10, true, false);
                    }
                    medItem.vPMC = LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPMC, ObOp.Obrigatorio, 15);

                    NFe.det[nProd].Prod.med.Add(medItem);
                    #endregion
                    break;

                case "L":
                    //layout = "§L|tpArma|nSerie|nCano|descr"; //ok
                    ///
                    /// Grupo da TAG <det><prod><arma>
                    /// 
                    #region <det><prod><arma>
                    Arma armaItem = new Arma();

                    armaItem.tpArma = (TpcnTipoArma)this.LerInt32(TpcnResources.tpArma, ObOp.Obrigatorio, 1, 1);
                    armaItem.nSerie = LerString(TpcnResources.nSerie, ObOp.Obrigatorio, 1, (double)NFe.infNFe.Versao >= 3.10 ? 15 : 9);
                    armaItem.nCano = LerString(TpcnResources.nCano, ObOp.Obrigatorio, 1, (double)NFe.infNFe.Versao >= 3.10 ? 15 : 9);
                    armaItem.descr = LerString(TpcnResources.descr, ObOp.Obrigatorio, 1, 256);

                    NFe.det[nProd].Prod.arma.Add(armaItem);
                    #endregion
                    break;

                case "LA":
                case "L01":
                    ///
                    /// Grupo da TAG <det><prod><comb>
                    /// 
                    #region <det><prod><comb>
                    NFe.det[nProd].Prod.comb = new Comb();

                    NFe.det[nProd].Prod.comb.cProdANP = this.LerInt32(TpcnResources.cProdANP, ObOp.Obrigatorio, 9, 9);
                    if (NFe.infNFe.Versao >= 3 && NFe.infNFe.Versao < 4)
                        NFe.det[nProd].Prod.comb.pMixGN = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pMixGN, ObOp.Opcional, 6);

                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Prod.comb.descANP = this.LerString(TpcnResources.descANP, ObOp.Opcional, 2, 295);
                        NFe.det[nProd].Prod.comb.pGLP = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pGLP, ObOp.Opcional, 16);
                        NFe.det[nProd].Prod.comb.pGNn = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pGNn, ObOp.Opcional, 16);
                        NFe.det[nProd].Prod.comb.pGNi = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pGNi, ObOp.Opcional, 16);
                        NFe.det[nProd].Prod.comb.vPart = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPart, ObOp.Opcional, 16);
                    }
                    NFe.det[nProd].Prod.comb.CODIF = this.LerString(TpcnResources.CODIF, ObOp.Opcional, 0, 21);
                    NFe.det[nProd].Prod.comb.qTemp = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qTemp, ObOp.Opcional, 16);
                    NFe.det[nProd].Prod.comb.UFCons = this.LerString(TpcnResources.UFCons, ObOp.Obrigatorio, 2, 2);

                    if (lenPipesRegistro >= 11)
                    {
                        NFe.det[nProd].Prod.comb.pBio = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pBio, ObOp.Opcional, 15);
                    }

                    #endregion
                    break;

                case "LA1":
                    //layout = prefix + this.FSegmento + "|nBico|nBomba|nTanque|vEncIni|vEncFin"; //ok
                    ///
                    /// Grupo da TAG <det><prod><comb><encerrante>
                    /// 
                    NFe.det[nProd].Prod.comb.encerrante.nBico = this.LerInt32(TpcnResources.nBico, ObOp.Obrigatorio, 1, 3);
                    NFe.det[nProd].Prod.comb.encerrante.nBomba = this.LerInt32(TpcnResources.nBomba, ObOp.Opcional, 0, 3);
                    NFe.det[nProd].Prod.comb.encerrante.nTanque = this.LerInt32(TpcnResources.nTanque, ObOp.Obrigatorio, 1, 3);
                    NFe.det[nProd].Prod.comb.encerrante.vEncIni = this.LerString(TpcnResources.vEncIni, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Prod.comb.encerrante.vEncFin = this.LerString(TpcnResources.vEncFin, ObOp.Obrigatorio, 1, 15);
                    break;


                case "LA18":
                    //layout = prefix + this.FSegmento + "|indImport|cUFOrig|pOrig";
                    ///
                    /// Grupo da TAG <det><prod><comb><origComb>
                    /// 
                    #region <det><prod><comb><origComb>

                    NFe.det[nProd].Prod.comb.origComb.Add(new OrigComb());
                    NFe.det[nProd].Prod.comb.origComb[NFe.det[nProd].Prod.comb.origComb.Count - 1].indImport = this.LerInt32(TpcnResources.indImport, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Prod.comb.origComb[NFe.det[nProd].Prod.comb.origComb.Count - 1].cUFOrig = this.LerInt32(TpcnResources.cUFOrig, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Prod.comb.origComb[NFe.det[nProd].Prod.comb.origComb.Count - 1].pOrig = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pOrig, ObOp.Obrigatorio, 15);
                    #endregion

                    break;

                case "LA07":
                case "L105":
                    //layout = prefix + this.FSegmento + "|qBCProd|vAliqProd|vCIDE"; 
                    ///
                    /// Grupo da TAG <det><prod><comb><CIDE>
                    /// 
                    #region <det><prod><comb><CIDE>
                    NFe.det[nProd].Prod.comb.CIDE.qBCprod = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCProd, ObOp.Obrigatorio, 16);
                    NFe.det[nProd].Prod.comb.CIDE.vAliqProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vAliqProd, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Prod.comb.CIDE.vCIDE = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCIDE, ObOp.Obrigatorio, 15);
                    #endregion
                    break;


                case "LB":
                case "L109":
                    //layout = prefix + this.FSegmento + "|nRECOPI"; //ok
                    NFe.det[nProd].Prod.nRECOPI = this.LerString(TpcnResources.nRECOPI, ObOp.Opcional, 20, 20);
                    break;

                case "M":
                    //layout = prefix + "M|vTotTrib|"; //ok   
                    NFe.det[nProd].Imposto.vTotTrib = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTotTrib, ObOp.Opcional, 15);
                    break;

                /*
                 * 

            case "M02":
                layout = "§M02|vTotTrib"; //ok   
                NFe.det[nProd].Imposto.vTotTrib = this.LerDouble(TpcnTipoCampo.tcDec2, TpcnResources.vTotTrib, ObOp.Opcional, 15);
                break;
                 * 
                 */

                ///
                /// Grupo da TAG <det><imposto><ICMS>
                /// 
                case "N02":
                    #region ICMS00

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.modBC = (TpcnDeterminacaoBaseIcms)this.LerInt32(TpcnResources.modBC, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMS = this.LerDouble(this.TipoCampo42, TpcnResources.pICMS, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Obrigatorio, 15);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.pFCP = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCP, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCP, ObOp.Obrigatorio, 15);
                    }
                    #endregion
                    break;

                case "N02A":
                    #region ICMS02

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.qBCMono = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCMono, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.adRemICMS = this.LerDouble(this.TipoCampo42, TpcnResources.adRemICMS, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMono, ObOp.Obrigatorio, 13);


                    #endregion
                    break;

                case "N03":
                    #region ICMS10

                    NFe.det[nProd].Imposto.ICMS.ICMSPart10 = 0;
                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.modBC = (TpcnDeterminacaoBaseIcms)this.LerInt32(TpcnResources.modBC, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMS = this.LerDouble(this.TipoCampo42, TpcnResources.pICMS, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vBCFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCP, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pFCP = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCP, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCP, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pMVAST = this.LerDouble(this.TipoCampo42, TpcnResources.pMVAST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pRedBCST = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBCST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMSST = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSST, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vBCFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pFCPST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Obrigatorio, 15);

                    if (lenPipesRegistro >= 21)
                    {
                        NFe.det[nProd].Imposto.ICMS.vICMSSTDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSTDeson, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.motDesICMSST = this.LerInt32(TpcnResources.motDesICMSST, ObOp.Opcional, 1, 2);
                    }
                    #endregion
                    break;

                case "N03A":
                    #region ICMS15

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.qBCMono = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCMono, ObOp.Opcional, 11);
                    NFe.det[nProd].Imposto.ICMS.adRemICMS = this.LerDouble(this.TipoCampo42, TpcnResources.adRemICMS, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMono, ObOp.Obrigatorio, 15);

                    NFe.det[nProd].Imposto.ICMS.qBCMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCMonoReten, ObOp.Opcional, 11);
                    NFe.det[nProd].Imposto.ICMS.adRemICMSReten = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.adRemICMSReten, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMonoReten, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pRedAdRem = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.pRedAdRem, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.motRedAdRem = this.LerInt32(TpcnResources.motRedAdRem, ObOp.Obrigatorio, 1, 1);


                    #endregion
                    break;

                case "N04":
                    //layout = "§N04|orig|CST|modBC|pRedBC|vBC|pICMS|vICMS|vICMSDeson|motDesICMS|";  //no manual
                    #region ICMS20

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.modBC = (TpcnDeterminacaoBaseIcms)this.LerInt32(TpcnResources.modBC, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pRedBC = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBC, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMS = this.LerDouble(this.TipoCampo42, TpcnResources.pICMS, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Obrigatorio, 15);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.vBCFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCP, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.pFCP = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCP, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCP, ObOp.Obrigatorio, 15);
                    }
                    NFe.det[nProd].Imposto.ICMS.vICMSDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSDeson, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.motDesICMS = this.LerInt32(TpcnResources.motDesICMS, ObOp.Opcional, 1, 1);
                    if (lenPipesRegistro >= 14)
                    {
                        NFe.det[nProd].Imposto.ICMS.indDeduzDeson = this.LerString(TpcnResources.indDeduzDeson, ObOp.Opcional, 1, 1);
                    }
                    #endregion
                    break;

                case "N05":
                    #region ICMS30

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pMVAST = this.LerDouble(this.TipoCampo42, TpcnResources.pMVAST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pRedBCST = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBCST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMSST = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSST, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSST, ObOp.Obrigatorio, 15);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.vBCFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.pFCPST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Obrigatorio, 15);
                    }
                    NFe.det[nProd].Imposto.ICMS.vICMSDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSDeson, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.motDesICMS = this.LerInt32(TpcnResources.motDesICMS, ObOp.Opcional, 1, 1);
                    if (lenPipesRegistro >= 15)
                    {
                        NFe.det[nProd].Imposto.ICMS.indDeduzDeson = this.LerString(TpcnResources.indDeduzDeson, ObOp.Opcional, 1, 1);
                    }
                    #endregion
                    break;

                case "N06":
                    //layout = (NFe.infNFe.Versao >= 3 ?
                    //            "§N06|orig|CST|vICMSDeson|motDesICMS" :
                    //            "§N06|orig|CST|vICMS|motDesICMS");

                    #region ICMS40, ICMS41 ICMS50

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);

                    if (lenPipesRegistro >= 5)
                    {
                        if (NFe.infNFe.Versao >= 3)
                        {
                            NFe.det[nProd].Imposto.ICMS.vICMSDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSDeson, ObOp.Opcional, 15);
                        }
                        else
                        {
                            NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Opcional, 15);
                        }

                        NFe.det[nProd].Imposto.ICMS.motDesICMS = this.LerInt32(TpcnResources.motDesICMS, ObOp.Opcional, 1, 1);
                    }

                    if (lenPipesRegistro >= 6)
                    {
                        NFe.det[nProd].Imposto.ICMS.indDeduzDeson = this.LerString(TpcnResources.indDeduzDeson, ObOp.Opcional, 1, 1);
                    }

                    #endregion
                    break;

                case "N07":
                    #region ICMS51

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.modBC = (TpcnDeterminacaoBaseIcms)this.LerInt32(TpcnResources.modBC, ObOp.Opcional, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pRedBC = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBC, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMS = this.LerDouble(this.TipoCampo42, TpcnResources.pICMS, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSOp = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSOp, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.pDif = this.LerDouble(this.TipoCampo42, TpcnResources.pDif, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSDif, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vBCFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCP, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pFCP = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCP, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCP, ObOp.Obrigatorio, 15);

                    if (lenPipesRegistro == 10) //a quem ainda nao alterou para a tag <vICMS>
                    {

                        ///
                        /// como nao tinha a tag vICMS definida no layout do TXT, vamos calcular o valor do ICMS
                        ///
                        if (NFe.det[nProd].Imposto.ICMS.vICMS == 0)
                        {
                            NFe.det[nProd].Imposto.ICMS.vICMS = NFe.det[nProd].Imposto.ICMS.vICMSOp - NFe.det[nProd].Imposto.ICMS.vICMSDif;

                            if (NFe.det[nProd].Imposto.ICMS.vICMS < 0) NFe.det[nProd].Imposto.ICMS.vICMS = 0;
                        }
                    }

                    if (lenPipesRegistro >= 17)
                    {
                        NFe.det[nProd].Imposto.ICMS.pFCPDif = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPDif, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPDif, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPEfet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPEfet, ObOp.Opcional, 15);
                        if (lenPipesRegistro >= 18)
                        {
                            NFe.det[nProd].Imposto.ICMS.cBenefRBC = this.LerString(TpcnResources.cBenefRBC, ObOp.Opcional, 8, 10);
                        }
                    }

                    #endregion
                    break;

                case "N07A":
                    #region ICMS53

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.qBCMono = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCMono, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.adRemICMS = this.LerDouble(this.TipoCampo42, TpcnResources.adRemICMS, ObOp.Obrigatorio, 15);

                    NFe.det[nProd].Imposto.ICMS.vICMSMonoOp = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMonoOp, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.pDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.pDif, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSMonoDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMonoDif, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMono, ObOp.Opcional, 15);

                    #endregion
                    break;

                case "N08":
                    #region ICMS60

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.vBCSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCSTRet, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.pST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pST, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.vICMSSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSTRet, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vBCFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPSTRet, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.pFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPSTRet, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPSTRet, ObOp.Opcional, 15);

                        if (lenPipesRegistro >= 13)
                        {
                            NFe.det[nProd].Imposto.ICMS.pRedBCEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pRedBCEfet, ObOp.Opcional, 15);
                            NFe.det[nProd].Imposto.ICMS.vBCEfet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCEfet, ObOp.Opcional, 15);
                            NFe.det[nProd].Imposto.ICMS.pICMSEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pICMSEfet, ObOp.Opcional, 15);
                            NFe.det[nProd].Imposto.ICMS.vICMSEfet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSEfet, ObOp.Opcional, 15);
                        }

                        NFe.det[nProd].Imposto.ICMS.vICMSSubstituto = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSubstituto, ObOp.Opcional, 15);
                    }
                    #endregion
                    break;

                case "N08A":
                    #region ICMS61

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.qBCMonoRet = this.LerDecimal(TpcnTipoCampo.tcDec4, TpcnResources.qBCMonoRet, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.adRemICMSRet = this.LerDouble(this.TipoCampo42, TpcnResources.adRemICMSRet, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.vICMSMonoRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMonoRet, ObOp.Obrigatorio, 15);

                    }
                    #endregion
                    break;

                case "N09":
                    #region ICMS70

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.modBC = (TpcnDeterminacaoBaseIcms)this.LerInt32(TpcnResources.modBC, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pRedBC = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBC, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMS = this.LerDouble(this.TipoCampo42, TpcnResources.pICMS, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pMVAST = this.LerDouble(this.TipoCampo42, TpcnResources.pMVAST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pRedBCST = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBCST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMSST = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSST, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vBCFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCP, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.pFCP = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCP, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCP, ObOp.Opcional, 15);

                    NFe.det[nProd].Imposto.ICMS.vBCFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPST, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.pFCPST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPST, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSDeson, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.motDesICMS = this.LerInt32(TpcnResources.motDesICMS, ObOp.Opcional, 1, 1);

                    if (lenPipesRegistro >= 24)
                    {
                        NFe.det[nProd].Imposto.ICMS.vICMSSTDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSTDeson, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.motDesICMSST = this.LerInt32(TpcnResources.motDesICMSST, ObOp.Opcional, 1, 2);
                        if (lenPipesRegistro >= 25)
                        {
                            NFe.det[nProd].Imposto.ICMS.indDeduzDeson = this.LerString(TpcnResources.indDeduzDeson, ObOp.Opcional, 1, 1);
                        }
                    }

                    #endregion
                    break;

                case "N10":
                    #region ICMS90

                    NFe.det[nProd].Imposto.ICMS.ICMSPart90 = 0;
                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.modBC = (TpcnDeterminacaoBaseIcms)this.LerInt32(TpcnResources.modBC, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pRedBC = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBC, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMS = this.LerDouble(this.TipoCampo42, TpcnResources.pICMS, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pMVAST = this.LerDouble(this.TipoCampo42, TpcnResources.pMVAST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pRedBCST = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBCST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMSST = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSST, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSDeson, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.motDesICMS = this.LerInt32(TpcnResources.motDesICMS, ObOp.Opcional, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.vBCFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCP, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.pFCP = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCP, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCP, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vBCFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPST, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.pFCPST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPST, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Opcional, 15);

                    if (lenPipesRegistro >= 24)
                    {
                        NFe.det[nProd].Imposto.ICMS.vICMSSTDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSTDeson, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ICMS.motDesICMSST = this.LerInt32(TpcnResources.motDesICMSST, ObOp.Opcional, 1, 2);
                        if (lenPipesRegistro >= 25)
                        {
                            NFe.det[nProd].Imposto.ICMS.indDeduzDeson = this.LerString(TpcnResources.indDeduzDeson, ObOp.Opcional, 1, 1);
                        }
                    }

                    #endregion
                    break;

                case "N10A":
                    //layout = (NFe.infNFe.Versao >= 3 ?
                    //            "§N10a|orig|CST|modBC|vBC|pRedBC|pICMS|vICMS|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|pBCOp|UFST|" :
                    //            "§N10a|orig|CST|modBC|vBC|pRedBC|pICMS|vICMS|modBCST|pMVAST|pRedBCST|vBCST|pICMSST|vICMSST|vBCFCPST|pFCPST|vFCPST|pBCOp|UFST|");

                    #region ICMSPart-10, ICMSPart-90

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.modBC = (TpcnDeterminacaoBaseIcms)this.LerInt32(TpcnResources.modBC, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pRedBC = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBC, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMS = this.LerDouble(this.TipoCampo42, TpcnResources.pICMS, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pMVAST = this.LerDouble(this.TipoCampo42, TpcnResources.pMVAST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pRedBCST = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBCST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMSST = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSST, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pBCOp = this.LerDouble(this.TipoCampo42, TpcnResources.pBCOp, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.UFST = this.LerString(TpcnResources.UFST, ObOp.Obrigatorio, 2, 2);
                    if (NFe.det[nProd].Imposto.ICMS.CST == "10")
                        NFe.det[nProd].Imposto.ICMS.ICMSPart10 = 1;
                    else
                        NFe.det[nProd].Imposto.ICMS.ICMSPart90 = 1;

                    if (lenPipesRegistro >= 19)
                    {
                        NFe.det[nProd].Imposto.ICMS.vBCFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.pFCPST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Obrigatorio, 15);
                    }

                    #endregion
                    break;

                case "N10B":
                    //layout = "§N10b|Orig|CST|vBCSTRet|vICMSSTRet|vBCSTDest|vICMSSTDest";

                    #region ICMS-ST

                    NFe.det[nProd].Imposto.ICMS.ICMSst = 1;
                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.ICMS.vBCSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCSTRet, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSTRet, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vBCSTDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCSTDest, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSSTDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSTDest, ObOp.Obrigatorio, 15);

                    NFe.det[nProd].Imposto.ICMS.pST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pST, ObOp.None, 15, true);
                    NFe.det[nProd].Imposto.ICMS.vICMSSubstituto = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSubstituto, ObOp.None, 15, true);
                    NFe.det[nProd].Imposto.ICMS.vBCFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPSTRet, ObOp.None, 15, true);
                    NFe.det[nProd].Imposto.ICMS.pFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPSTRet, ObOp.None, 15, true);
                    NFe.det[nProd].Imposto.ICMS.vFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPSTRet, ObOp.None, 15, true);
                    NFe.det[nProd].Imposto.ICMS.pRedBCEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pRedBCEfet, ObOp.None, 15, true);
                    NFe.det[nProd].Imposto.ICMS.vBCEfet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCEfet, ObOp.None, 15, true);
                    NFe.det[nProd].Imposto.ICMS.pICMSEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pICMSEfet, ObOp.None, 15, true);
                    NFe.det[nProd].Imposto.ICMS.vICMSEfet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSEfet, ObOp.None, 15, true);

                    #endregion
                    break;

                case "N10C":
                    //layout = "§N10c|Orig|CSOSN|pCredSN|vCredICMSSN";

                    #region ICMSSN101

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CSOSN = this.LerInt32(TpcnResources.CSOSN, ObOp.Obrigatorio, 3, 3);
                    NFe.det[nProd].Imposto.ICMS.pCredSN = this.LerDouble(this.TipoCampo42, TpcnResources.pCredSN, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vCredICMSSN = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredICMSSN, ObOp.Obrigatorio, 15);

                    #endregion
                    break;

                case "N10D":
                    //layout = "§N10d|Orig|CSOSN";

                    #region ICMSSN102

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CSOSN = this.LerInt32(TpcnResources.CSOSN, ObOp.Obrigatorio, 3, 3);

                    #endregion
                    break;

                case "N10E":
                    #region ICMSSN201

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CSOSN = this.LerInt32(TpcnResources.CSOSN, ObOp.Obrigatorio, 3, 3);
                    NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pMVAST = this.LerDouble(this.TipoCampo42, TpcnResources.pMVAST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pRedBCST = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBCST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMSST = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSST, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSST, ObOp.Obrigatorio, 15);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.vBCFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.pFCPST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Obrigatorio, 15);
                    }
                    NFe.det[nProd].Imposto.ICMS.pCredSN = this.LerDouble(this.TipoCampo42, TpcnResources.pCredSN, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vCredICMSSN = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredICMSSN, ObOp.Obrigatorio, 15);

                    #endregion
                    break;

                case "N10F":
                    #region ICMSSN202

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CSOSN = this.LerInt32(TpcnResources.CSOSN, ObOp.Obrigatorio, 3, 3);
                    NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.pMVAST = this.LerDouble(this.TipoCampo42, TpcnResources.pMVAST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pRedBCST = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBCST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ICMS.pICMSST = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSST, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vICMSST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSST, ObOp.Obrigatorio, 15);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.vBCFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.pFCPST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Obrigatorio, 15);
                    }

                    #endregion
                    break;

                case "N10G":
                    #region ICMSSN500

                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CSOSN = this.LerInt32(TpcnResources.CSOSN, ObOp.Obrigatorio, 3, 3);
                    if (NFe.infNFe.Versao < 3)
                        NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Opcional, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.vBCSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCSTRet, ObOp.Opcional, 15);
                    NFe.det[nProd].Imposto.ICMS.vICMSSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSTRet, ObOp.Opcional, 15);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.pST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vBCFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPSTRet, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.pFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPSTRet, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPSTRet, ObOp.Obrigatorio, 15);

                        if (lenPipesRegistro >= 13)
                        {
                            NFe.det[nProd].Imposto.ICMS.pRedBCEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pRedBCEfet, ObOp.Opcional, 15);
                            NFe.det[nProd].Imposto.ICMS.vBCEfet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCEfet, ObOp.Opcional, 15);
                            NFe.det[nProd].Imposto.ICMS.pICMSEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pICMSEfet, ObOp.Opcional, 15);
                            NFe.det[nProd].Imposto.ICMS.vICMSEfet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSEfet, ObOp.Opcional, 15);
                        }

                        NFe.det[nProd].Imposto.ICMS.vICMSSubstituto = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSSubstituto, ObOp.Opcional, 15);
                    }

                    #endregion
                    break;

                case "N10H":
                    #region ICMSSN900
                    NFe.det[nProd].Imposto.ICMS.orig = (TpcnOrigemMercadoria)this.LerInt32(TpcnResources.orig, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.CSOSN = this.LerInt32(TpcnResources.CSOSN, ObOp.Obrigatorio, 3, 3);
                    NFe.det[nProd].Imposto.ICMS.modBC = (TpcnDeterminacaoBaseIcms)this.LerInt32(TpcnResources.modBC, ObOp.Opcional, 1, 1);
                    NFe.det[nProd].Imposto.ICMS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Opcional, 15, true);
                    NFe.det[nProd].Imposto.ICMS.pRedBC = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBC, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pICMS = this.LerDouble(this.TipoCampo42, TpcnResources.pICMS, ObOp.Opcional, this.CasasDecimais75, true);
                    NFe.det[nProd].Imposto.ICMS.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Opcional, 15, true);
                    NFe.det[nProd].Imposto.ICMS.modBCST = (TpcnDeterminacaoBaseIcmsST)this.LerInt32(TpcnResources.modBCST, ObOp.Opcional, 1, 1, true);
                    NFe.det[nProd].Imposto.ICMS.pMVAST = this.LerDouble(this.TipoCampo42, TpcnResources.pMVAST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.pRedBCST = this.LerDouble(this.TipoCampo42, TpcnResources.pRedBCST, ObOp.Opcional, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ICMS.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Opcional, 15, true);
                    NFe.det[nProd].Imposto.ICMS.pICMSST = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSST, ObOp.Opcional, this.CasasDecimais75, true);
                    NFe.det[nProd].Imposto.ICMS.vICMSST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSST, ObOp.Opcional, 15, true);
                    NFe.det[nProd].Imposto.ICMS.pCredSN = this.LerDouble(this.TipoCampo42, TpcnResources.pCredSN, ObOp.Opcional, this.CasasDecimais75, true);
                    NFe.det[nProd].Imposto.ICMS.vCredICMSSN = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredICMSSN, ObOp.Opcional, 15, true);
                    if (NFe.infNFe.Versao >= 4)
                    {
                        NFe.det[nProd].Imposto.ICMS.vBCFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.pFCPST = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pFCPST, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.ICMS.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Obrigatorio, 15);
                    }
                    #endregion
                    break;

                case "NA":
                    #region ICMSUFDest
                    NFe.det[nProd].Imposto.ICMS.ICMSUFDest.vBCUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCUFDest, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Imposto.ICMS.ICMSUFDest.pFCPUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.pFCPUFDest, ObOp.Opcional, 1, 8);
                    NFe.det[nProd].Imposto.ICMS.ICMSUFDest.pICMSUFDest = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pICMSUFDest, ObOp.Opcional, 1, 8);
                    NFe.det[nProd].Imposto.ICMS.ICMSUFDest.pICMSInter = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.pICMSInter, ObOp.Obrigatorio, 1, 8);
                    NFe.det[nProd].Imposto.ICMS.ICMSUFDest.pICMSInterPart = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pICMSInterPart, ObOp.Opcional, 1, 8);
                    NFe.det[nProd].Imposto.ICMS.ICMSUFDest.vFCPUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPUFDest, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.ICMS.ICMSUFDest.vICMSUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSUFDest, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Imposto.ICMS.ICMSUFDest.vICMSUFRemet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSUFRemet, ObOp.Opcional, 1, 15);
                    if (NFe.infNFe.Versao >= 4)
                        NFe.det[nProd].Imposto.ICMS.ICMSUFDest.vBCFCPUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCFCPUFDest, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "O":
                    //layout = "§O|clEnq|CNPJProd|cSelo|qSelo|cEnq"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><IPI>
                    /// 
                    #region <det><imposto><IPI>
                    if (NFe.infNFe.Versao < 4)
                        NFe.det[nProd].Imposto.IPI.clEnq = this.LerString(TpcnResources.clEnq, ObOp.Opcional, 5, 5);
                    NFe.det[nProd].Imposto.IPI.CNPJProd = this.LerString(TpcnResources.CNPJProd, ObOp.Opcional, 14, 14);
                    NFe.det[nProd].Imposto.IPI.cSelo = this.LerString(TpcnResources.cSelo, ObOp.Opcional, 1, 60);
                    NFe.det[nProd].Imposto.IPI.qSelo = this.LerInt32(TpcnResources.qSelo, ObOp.Opcional, 1, 12);
                    NFe.det[nProd].Imposto.IPI.cEnq = this.LerString(TpcnResources.cEnq, ObOp.Obrigatorio, 3, 3);
                    #endregion
                    break;

                case "O07":
                    //layout = "§O07|CST|vIPI"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><IPITrib>
                    /// 
                    #region <det><imposto><IPITrib>
                    NFe.det[nProd].Imposto.IPI.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.IPI.vIPI = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIPI, ObOp.Opcional, 15);


                    #endregion
                    break;

                case "O08":
                    //layout = "§O08|CST"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><IPINT>
                    /// 
                    NFe.det[nProd].Imposto.IPI.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    break;

                case "O10":
                    //layout = "§O10|vBC|pIPI"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><IPI>
                    /// 
                    #region <det><imposto><IPI>
                    NFe.det[nProd].Imposto.IPI.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.IPI.pIPI = this.LerDouble(NFe.infNFe.Versao > 3 ? TpcnTipoCampo.tcDouble4 : TpcnTipoCampo.tcDouble2, TpcnResources.pIPI, ObOp.Obrigatorio, NFe.infNFe.Versao > 3 ? 7 : 5);
                    #endregion
                    break;

                case "O11":
                    //layout = (NFe.infNFe.Versao >= 3 ? "§O11|qUnid|vUnid|vIPI" : "§O11|qUnid|vUnid"); //ok
                    ///
                    /// Grupo da TAG <det><imposto><IPI>
                    /// 
                    #region <det><imposto><IPI>
                    NFe.det[nProd].Imposto.IPI.qUnid = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qUnid, ObOp.Obrigatorio, 16);
                    NFe.det[nProd].Imposto.IPI.vUnid = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vUnid, ObOp.Obrigatorio, 15);
                    if (NFe.infNFe.Versao >= 3)
                        NFe.det[nProd].Imposto.IPI.vIPI = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vIPI, ObOp.Opcional, 15);
                    #endregion
                    break;

                case "P":
                    //layout = "§P|vBC|vDespAdu|vII|vIOF"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><II>
                    /// 
                    #region <det><imposto><IPI>
                    NFe.det[nProd].Imposto.II.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.II.vDespAdu = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDespAdu, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.II.vII = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vII, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.II.vIOF = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIOF, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "Q02":
                    //layout = "§Q02|CST|VBC|PPIS|VPIS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><pis><pisaliq>
                    /// 
                    #region <det><imposto><pis><pisaliq>
                    NFe.det[nProd].Imposto.PIS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.PIS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.PIS.pPIS = this.LerDouble(NFe.infNFe.Versao > 3 ? TpcnTipoCampo.tcDouble4 : TpcnTipoCampo.tcDouble2, TpcnResources.pPIS, ObOp.Obrigatorio, NFe.infNFe.Versao > 3 ? 7 : 5);
                    NFe.det[nProd].Imposto.PIS.vPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPIS, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "Q03":
                    //layout = "§Q03|CST|QBCProd|VAliqProd|VPIS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><pis><pisqtde>
                    /// 
                    #region <det><imposto><pis><pisqtde>
                    NFe.det[nProd].Imposto.PIS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.PIS.qBCProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCProd, ObOp.Obrigatorio, 16);
                    NFe.det[nProd].Imposto.PIS.vAliqProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vAliqProd, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.PIS.vPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPIS, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "Q04":
                    //layout = "§Q04|CST"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><pis><pisNT>
                    /// 
                    #region <det><imposto><pis><pisNT>
                    NFe.det[nProd].Imposto.PIS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    #endregion
                    break;

                case "Q05":
                    //layout = "§Q05|CST|vPIS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><pis><pisOutr>
                    /// 
                    #region <det><imposto><pis><pisOutr>
                    NFe.det[nProd].Imposto.PIS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.PIS.vPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPIS, NFe.infNFe.Versao >= 3 ? ObOp.Opcional : ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "Q07":
                    //layout = (NFe.infNFe.Versao >= 3 ? "§Q07|vBC|pPIS|vPIS" : "§Q07|vBC|pPIS"); //ok
                    ///
                    /// Grupo da TAG <det><imposto><pis><pisqtde>
                    /// 
                    #region <det><imposto><pis><pisqtde>
                    if (NFe.infNFe.Versao >= 3)
                    {
                        NFe.det[nProd].Imposto.PIS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.PIS.pPIS = this.LerDouble(NFe.infNFe.Versao > 3 ? TpcnTipoCampo.tcDouble4 : TpcnTipoCampo.tcDouble2, TpcnResources.pPIS, ObOp.Obrigatorio, NFe.infNFe.Versao > 3 ? 7 : 5);
                        NFe.det[nProd].Imposto.PIS.vPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPIS, ObOp.Obrigatorio, 15);
                    }
                    else
                    {
                        NFe.det[nProd].Imposto.PIS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                        NFe.det[nProd].Imposto.PIS.pPIS = this.LerDouble(NFe.infNFe.Versao > 3 ? TpcnTipoCampo.tcDouble4 : TpcnTipoCampo.tcDouble2, TpcnResources.pPIS, ObOp.Obrigatorio, NFe.infNFe.Versao > 3 ? 7 : 5);
                    }
                    #endregion
                    break;

                case "Q10":
                    //layout = "§Q10|qBCProd|vAliqProd"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><pis><pisqtde>
                    /// 
                    #region <det><imposto><pis><pisqtde>
                    NFe.det[nProd].Imposto.PIS.qBCProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCProd, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.PIS.vAliqProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vAliqProd, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "R":
                    //layout = "§R|vPIS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><pisST>
                    /// 
                    #region <det><imposto><pisST>
                    NFe.det[nProd].Imposto.PISST.vPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPIS, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "R02":
                    //layout = "§R02|vBC|pPIS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><pisST>
                    /// 
                    #region <det><imposto><pisST>
                    NFe.det[nProd].Imposto.PISST.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.PISST.pPis = this.LerDouble(this.TipoCampo42, TpcnResources.pPIS, ObOp.Obrigatorio, this.CasasDecimais75);
                    #endregion
                    break;

                case "R04":
                    ///
                    /// Grupo da TAG <det><imposto><pisST>
                    /// 
                    #region <det><imposto><pisST>

                    NFe.det[nProd].Imposto.PISST.qBCProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCProd, ObOp.Obrigatorio, 16);
                    NFe.det[nProd].Imposto.PISST.vAliqProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vAliqProd, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.PISST.vPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPIS, ObOp.Obrigatorio, 15);

                    if (lenPipesRegistro >= 5)
                    {
                        NFe.det[nProd].Imposto.PISST.indSomaPISST = this.LerString(TpcnResources.indSomaPISST, ObOp.Opcional, 0, 1);
                    }

                    #endregion
                    break;

                case "S02":
                    //layout = "§S02|CST|vBC|pCOFINS|vCOFINS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><COFINS>
                    /// 
                    #region <det><imposto><COFINS>
                    NFe.det[nProd].Imposto.COFINS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.COFINS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.COFINS.pCOFINS = this.LerDouble(this.TipoCampo42, TpcnResources.pCOFINS, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.COFINS.vCOFINS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCOFINS, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "S03":
                    //layout = "§S03|CST|QBCProd|VAliqProd|VCOFINS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><COFINSQtde>
                    /// 
                    #region <det><imposto><COFINSQtde>
                    NFe.det[nProd].Imposto.COFINS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.COFINS.qBCProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCProd, ObOp.Obrigatorio, 16);
                    NFe.det[nProd].Imposto.COFINS.vAliqProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vAliqProd, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.COFINS.vCOFINS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCOFINS, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "S04":
                    //layout = "§S04|CST"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><COFINSNT>
                    /// 
                    #region <det><imposto><COFINSNT>
                    NFe.det[nProd].Imposto.COFINS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    #endregion
                    break;

                case "S05":
                    //layout = "§S05|CST|VCOFINS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><COFINSOutr>
                    /// 
                    #region <det><imposto><COFINSOutr>
                    NFe.det[nProd].Imposto.COFINS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 2, 2);
                    NFe.det[nProd].Imposto.COFINS.vCOFINS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCOFINS, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "S07":
                    //layout = "§S07|VBC|PCOFINS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><COFINSOutr>
                    /// 
                    #region <det><imposto><COFINSOutr>
                    NFe.det[nProd].Imposto.COFINS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.COFINS.pCOFINS = this.LerDouble(this.TipoCampo42, TpcnResources.pCOFINS, ObOp.Obrigatorio, this.CasasDecimais75);
                    #endregion
                    break;

                case "S09":
                    //layout = "§S09|QBCProd|VAliqProd"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><COFINSST>
                    /// 
                    #region <det><imposto><COFINSST>
                    NFe.det[nProd].Imposto.COFINS.qBCProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCProd, ObOp.Obrigatorio, 16);
                    NFe.det[nProd].Imposto.COFINS.vAliqProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vAliqProd, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "T":
                    //layout = "§T|VCOFINS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><COFINSST>
                    /// 
                    #region <det><imposto><COFINSST>
                    NFe.det[nProd].Imposto.COFINSST.vCOFINS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCOFINS, ObOp.Obrigatorio, 15);
                    #endregion
                    break;

                case "T02":
                    //layout = "§T02|VBC|PCOFINS"; //ok
                    ///
                    /// Grupo da TAG <det><imposto><COFINSST>
                    /// 
                    #region <det><imposto><COFINSST>
                    NFe.det[nProd].Imposto.COFINSST.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.COFINSST.pCOFINS = this.LerDouble(this.TipoCampo42, TpcnResources.pCOFINS, ObOp.Obrigatorio, this.CasasDecimais75);
                    #endregion
                    break;

                case "T04":
                    ///
                    /// Grupo da TAG <det><imposto><COFINSST>
                    /// 
                    #region <det><imposto><COFINSST>

                    NFe.det[nProd].Imposto.COFINSST.qBCProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCProd, ObOp.Obrigatorio, 16);
                    NFe.det[nProd].Imposto.COFINSST.vAliqProd = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vAliqProd, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.COFINSST.vCOFINS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCOFINS, ObOp.Obrigatorio, 15);

                    if (lenPipesRegistro >= 5)
                    {
                        NFe.det[nProd].Imposto.COFINSST.indSomaCOFINSST = this.LerString(TpcnResources.indSomaCOFINSST, ObOp.Opcional, 0, 1);
                    }


                    #endregion
                    break;

                case "U":
                    //layout = (NFe.infNFe.Versao >= 3 ?
                    //            "§U|VBC|VAliq|VISSQN|CMunFG|CListServ|vDeducao|vOutro|vDescIncond|vDescCond|vISSRet|indISS|cServico|cMun|cPais|nProcesso|indIncentivo|" :
                    //            "§U|VBC|VAliq|VISSQN|CMunFG|CListServ|cSitTrib"); //ok
                    ///
                    /// Grupo da TAG <det><imposto><ISSQN>
                    /// 
                    #region <det><imposto><ISSQN>
                    NFe.det[nProd].Imposto.ISSQN.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ISSQN.vAliq = this.LerDouble(this.TipoCampo42, TpcnResources.vAliq, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.det[nProd].Imposto.ISSQN.vISSQN = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vISSQN, ObOp.Obrigatorio, 15);
                    NFe.det[nProd].Imposto.ISSQN.cMunFG = this.LerInt32(TpcnResources.cMunFG, ObOp.Obrigatorio, 7, 7);
                    if (NFe.infNFe.Versao >= 3)
                    {
                        NFe.det[nProd].Imposto.ISSQN.cListServ = this.LerString(TpcnResources.cListServ, ObOp.Obrigatorio, 5, 5);
                        NFe.det[nProd].Imposto.ISSQN.vDeducao = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDeducao, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ISSQN.vOutro = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vOutro, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ISSQN.vDescIncond = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDescIncond, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ISSQN.vDescCond = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDescCond, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ISSQN.vISSRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vISSRet, ObOp.Opcional, 15);
                        NFe.det[nProd].Imposto.ISSQN.indISS = (TpcnindISS)this.LerInt32(TpcnResources.indISS, ObOp.Obrigatorio, 1, 1);
                        NFe.det[nProd].Imposto.ISSQN.cServico = this.LerString(TpcnResources.cServico, ObOp.Opcional, 1, 20);
                        NFe.det[nProd].Imposto.ISSQN.cMun = this.LerInt32(TpcnResources.cMun, ObOp.Opcional, 7, 7);
                        NFe.det[nProd].Imposto.ISSQN.cPais = this.LerInt32(TpcnResources.cPais, ObOp.Opcional, 4, 4);
                        NFe.det[nProd].Imposto.ISSQN.nProcesso = this.LerString(TpcnResources.nProcesso, ObOp.Opcional, 1, 30);
                        NFe.det[nProd].Imposto.ISSQN.indIncentivo = this.LerInt32(TpcnResources.indIncentivo, ObOp.Opcional, 1, 1) == 1;
                    }
                    else
                    {
                        NFe.det[nProd].Imposto.ISSQN.cListServ = this.LerString(TpcnResources.cListServ, ObOp.Obrigatorio, 3, 4);
                        NFe.det[nProd].Imposto.ISSQN.cSitTrib = this.LerString(TpcnResources.cSitTrib, ObOp.Obrigatorio, 1, 1);
                    }
                    #endregion
                    break;

                case "UA":
                    //layout = "§UA|pDevol|vIPIDevol";
                    NFe.det[nProd].impostoDevol.pDevol = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.pDevol, ObOp.Opcional, 5);
                    NFe.det[nProd].impostoDevol.vIPIDevol = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIPIDevol, ObOp.Opcional, 5);
                    break;

                case "UB01":
                    //layout = "UB01|CSTIS|cClassTribIS|vBCIS|pIS|pISEspec|uTrib|qTrib|vIS|";

                    NFe.det[nProd].Imposto.IS.CSTIS = this.LerString(TpcnResources.CSTIS, ObOp.Obrigatorio, 1, 3);
                    NFe.det[nProd].Imposto.IS.cClassTribIS = this.LerString(TpcnResources.cClassTribIS, ObOp.Obrigatorio, 1, 6);
                    NFe.det[nProd].Imposto.IS.vBCIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCIS, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IS.pIS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pIS, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IS.pISEspec = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pISEspec, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IS.uTrib = this.LerString(TpcnResources.uTrib, ObOp.Opcional, 1, 6);
                    NFe.det[nProd].Imposto.IS.qTrib = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qTrib, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IS.vIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIS, ObOp.Obrigatorio, 1, 15);
                    break;

                case "UB12":
                    //layout = "UB12|CST|cClassTrib|"
                    Auxiliar.WriteLog("ConverterTXT (" + ArqTXT + ") - ConversaoTXT: Achou o UB12 no TXT.", false);


                    NFe.det[nProd].Imposto.IBSCBS.CST = this.LerString(TpcnResources.CST, ObOp.Obrigatorio, 1, 3);
                    NFe.det[nProd].Imposto.IBSCBS.cClassTrib = this.LerString(TpcnResources.cClassTrib, ObOp.Obrigatorio, 1, 6);
                    break;

                case "UB15":
                    //layout = "UB15|vBC|vIBS|"
                    Auxiliar.WriteLog("ConverterTXT (" + ArqTXT + ") - ConversaoTXT: Achou o UB15 no TXT.", false);

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.vIBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBS, ObOp.Obrigatorio, 15, true);
                    break;

                case "UB17":
                    //layout = "UB17|pIBSUF|pDif|vDif|vDevTrib|pRedAliq|pAliqEfet|vIBSUF|"

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSUF.pIBSUF = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pIBSUF, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSUF.gDif.pDif = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pDif, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSUF.gDif.vDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDif, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSUF.gDevTrib.vDevTrib = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDevTrib, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSUF.gRed.pRedAliq = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pRedAliq, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSUF.gRed.pAliqEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqEfet, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSUF.vIBSUF = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSUF, ObOp.Obrigatorio, 1, 15);
                    break;

                case "UB36":
                    //layout = "UB36|pIBSMun|pDif|vDif|vDevTrib|pRedAliq|pAliqEfet|vIBSMun|"

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSMun.pIBSMun = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pIBSMun, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSMun.gDif.pDif = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pDif, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSMun.gDif.vDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDif, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSMun.gDevTrib.vDevTrib = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDevTrib, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSMun.gRed.pRedAliq = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pRedAliq, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSMun.gRed.pAliqEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqEfet, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gIBSMun.vIBSMun = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vIBSMun, ObOp.Obrigatorio, 1, 7);
                    break;

                case "UB55":
                    //layout = "UB55|pCBS|pDif|vDif|vDevTrib|pRedAliq|pAliqEfet|vCBS|"

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gCBS.pCBS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pCBS, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gCBS.gDif.pDif = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pDif, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gCBS.gDif.vDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDif, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gCBS.gDevTrib.vDevTrib = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDevTrib, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gCBS.gRed.pRedAliq = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pRedAliq, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gCBS.gRed.pAliqEfet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqEfet, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gCBS.vCBS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.vCBS, ObOp.Obrigatorio, 1, 7);
                    break;

                case "UB68":
                    //layout = "UB68|CSTReg|cClassTribReg|pAliqEfetRegIBSUF|vTribRegIBSUF|pAliqEfetRegIBSMun|vTribRegIBSMun|pAliqEfetRegCBS|vTribRegCBS|"

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribRegular.CSTReg = this.LerString(TpcnResources.CSTReg, ObOp.Obrigatorio, 1, 3);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribRegular.cClassTribReg = this.LerString(TpcnResources.cClassTribReg, ObOp.Obrigatorio, 1, 6);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribRegular.pAliqEfetRegIBSUF = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqEfetRegIBSUF, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribRegular.vTribRegIBSUF = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTribRegIBSUF, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribRegular.pAliqEfetRegIBSMun = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqEfetRegIBSMun, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribRegular.vTribRegIBSMun = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTribRegIBSMun, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribRegular.pAliqEfetRegCBS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqEfetRegCBS, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribRegular.vTribRegCBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTribRegCBS, ObOp.Obrigatorio, 1, 15);
                    break;

                case "UB82":
                    //layout = UB82|pAliqIBSUF|vTribIBSUF|pAliqIBSMun|vTribIBSMun|pAliqCBS|vTribCBS|

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribCompraGov.pAliqIBSUF = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqIBSUF, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribCompraGov.vTribIBSUF = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTribIBSUF, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribCompraGov.pAliqIBSMun = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqIBSMun, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribCompraGov.vTribIBSMun = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTribIBSMun, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribCompraGov.pAliqCBS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pAliqCBS, ObOp.Obrigatorio, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBS.gTribCompraGov.vTribCBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTribCBS, ObOp.Obrigatorio, 1, 15);
                    break;

                case "UB84":
                    //layout = "UB84|vTotIBSMonoItem|vTotCBSMonoItem|"
                    
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.vTotIBSMonoItem = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTotIBSMonoItem, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.vTotCBSMonoItem = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTotCBSMonoItem, ObOp.Opcional, 1, 15);
                    break;

                case "UB85":
                    //layout = "UB85|qBCMono|adRemIBS|adRemCBS|vIBSMono|vCBSMono|"

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoPadrao.qBCMono = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCMono, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoPadrao.adRemIBS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.adRemIBS, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoPadrao.adRemCBS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.adRemCBS, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoPadrao.vIBSMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSMono, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoPadrao.vCBSMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSMono, ObOp.Opcional, 1, 15);

                    break;

                case "UB91":
                    //layout = "UB91|qBCMonoReten|adRemIBSReten|vIBSMonoReten|adRemCBSReten|vCBSMonoReten|"

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoReten.qBCMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCMonoReten, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoReten.adRemIBSReten = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.adRemIBSReten, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoReten.vIBSMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSMonoReten, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoReten.adRemCBSReten = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.adRemCBSReten, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoReten.vCBSMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSMonoReten, ObOp.Opcional, 1, 15);

                    break;

                case "UB95":
                    //layout = "UB95|qBCMonoRet|adRemIBSRet|vIBSMonoRet|adRemCBSRet|vCBSMonoRet|"

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoRet.qBCMonoRet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.qBCMonoRet, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoRet.adRemIBSRet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.adRemIBSRet, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoRet.vIBSMonoRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSMonoRet, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoRet.adRemCBSRet = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.adRemCBSRet, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoRet.vCBSMonoRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSMonoRet, ObOp.Opcional, 1, 15);

                    break;

                case "UB100":
                    //layout = "UB100|pDifIBS|vIBSMonoDif|pDifCBS|vCBSMonoDif|"

                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoDif.pDifIBS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pDifIBS, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoDif.vIBSMonoDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSMonoDif, ObOp.Opcional, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoDif.pDifCBS = this.LerDouble(TpcnTipoCampo.tcDouble4, TpcnResources.pDifCBS, ObOp.Opcional, 1, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gIBSCBSMono.gMonoDif.vCBSMonoDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSMonoDif, ObOp.Opcional, 1, 15);

                    break;

                case "UB106":
                    //layout = UB106|vIBS|vCBS|

                    NFe.det[nProd].Imposto.IBSCBS.gTransfCred.vIBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBS, ObOp.Obrigatorio, 1, 15);
                    NFe.det[nProd].Imposto.IBSCBS.gTransfCred.vCBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBS, ObOp.Obrigatorio, 1, 15);
                    break;

                case "UB112":
                    NFe.det[nProd].Imposto.IBSCBS.gAjusteCompet.competApur = this.LerString(TpcnResources.competApur, ObOp.Obrigatorio, 7, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gAjusteCompet.vIBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBS, ObOp.Obrigatorio, 1, 13);
                    NFe.det[nProd].Imposto.IBSCBS.gAjusteCompet.vCBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBS, ObOp.Obrigatorio, 1, 13);
                    break;

                case "UB14A":
                    //layout = "UB14a|indDoacao|;
                    NFe.det[nProd].Imposto.IBSCBS.indDoacao = this.LerString(TpcnResources.indDoacao, ObOp.Opcional, 1, 1);
                    break;

                case "UB116":
                    NFe.det[nProd].Imposto.IBSCBS.gEstornoCred.vCBSEstCred = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSEstCred, ObOp.Obrigatorio, 1, 13); 
                    NFe.det[nProd].Imposto.IBSCBS.gEstornoCred.vIBSEstCred = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSEstCred, ObOp.Obrigatorio, 1, 13);
                    break;

                case "UB120":
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresOper.vBCCredPres = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCCredPres, ObOp.Obrigatorio, 1, 13);
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresOper.cCredPres = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.cCredPres, ObOp.Obrigatorio, 1, 2);
                    break;

                case "UB123":
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresOper.gIBSCredPres.pCredPres = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.pCredPres, ObOp.Opcional, 1, 4);
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresOper.gIBSCredPres.vCredPres = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPres, ObOp.Opcional, 1, 13);
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresOper.gIBSCredPres.vCredPresCondSus = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPresCondSus, ObOp.Opcional, 1, 13);
                    break;

                case "UB127":
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresOper.gCBSCredPres.pCredPres = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.pCredPres, ObOp.Opcional, 1, 4);
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresOper.gCBSCredPres.vCredPres = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPres, ObOp.Opcional, 1, 13);
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresOper.gCBSCredPres.vCredPresCondSus = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPresCondSus, ObOp.Opcional, 1, 13);
                    break;

                case "UB131":
                    //layout = UB109|tpCredPresIBSZFM|vCredPresIBSZFM|
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresIBSZFM.competApur = this.LerString(TpcnResources.competApur, ObOp.Obrigatorio, 7, 7);
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresIBSZFM.tpCredPresIBSZFM = this.LerString(TpcnResources.tpCredPresIBSZFM, ObOp.Obrigatorio, 1, 1);
                    NFe.det[nProd].Imposto.IBSCBS.gCredPresIBSZFM.vCredPresIBSZFM = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPresIBSZFM, ObOp.Opcional, 1, 15);
                    break;

                case "VA02":
                    //layout = "§V02|XCampo|XTexto"; //ok
                    ///
                    /// Grupo da TAG <obsItem><obsCont>
                    /// 
                    #region <obsItem><obsCont>

                    NFe.det[nProd].ObsItem.ObsCont.xCampo = this.LerString(TpcnResources.xCampo, ObOp.Obrigatorio, 1, 20);
                    NFe.det[nProd].ObsItem.ObsCont.xTexto = this.LerString(TpcnResources.xTexto, ObOp.Obrigatorio, 1, 60);

                    #endregion
                    break;

                case "VA05":
                    //layout = "§V05|XCampo|XTexto"; //ok - ?
                    ///
                    /// Grupo da TAG <obsItem><obsFisco>
                    /// 
                    #region <obsItem><obsFisco>

                    NFe.det[nProd].ObsItem.ObsFisco.xCampo = this.LerString(TpcnResources.xCampo, ObOp.Obrigatorio, 1, 20);
                    NFe.det[nProd].ObsItem.ObsFisco.xTexto = this.LerString(TpcnResources.xTexto, ObOp.Obrigatorio, 1, 60);

                    #endregion
                    break;

                case "VB01":
                    //layout = VB01|vItem|

                    NFe.det[nProd].vItem = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vItem, ObOp.Opcional, 15);
                    break;

                case "VC01":
                    //layout = VC01|chaveAcesso|nItem|

                    NFe.det[nProd].DfeReferenciado.chaveAcesso = this.LerString(TpcnResources.chaveAcesso, ObOp.Obrigatorio, 1, 44);
                    NFe.det[nProd].DfeReferenciado.nItem = this.LerInt32(TpcnResources.NItem, ObOp.Opcional, 1, 3);
                    break;

                case "W02":
                    ///
                    /// Grupo da TAG <total><ICMSTot>
                    /// 
                    #region <total><ICMSTot>
                    NFe.Total.ICMSTot.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vICMS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMS, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vBCST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCST, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vST, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vProd = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vProd, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vFrete = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFrete, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vSeg = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vSeg, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vDesc = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDesc, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vII = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vII, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vIPI = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIPI, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPIS, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vCOFINS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCOFINS, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vOutro = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vOutro, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vNF = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vNF, ObOp.Obrigatorio, 15);
                    NFe.Total.ICMSTot.vTotTrib = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTotTrib, ObOp.Opcional, 15);



                    NFe.Total.ICMSTot.vICMSDeson = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSDeson, ObOp.Opcional, 15);

                    if (NFe.infNFe.Versao >= 3 && lenPipesRegistro > 17)
                    {
                        NFe.Total.ICMSTot.vICMSUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSUFDest, ObOp.Opcional, 15);
                        NFe.Total.ICMSTot.vFCPUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPUFDest, ObOp.Opcional, 15);
                        NFe.Total.ICMSTot.vICMSUFRemet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSUFRemet, ObOp.Opcional, 15);

                        if (NFe.infNFe.Versao >= 4)
                        {
                            NFe.Total.ICMSTot.vFCP = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCP, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.vFCPST = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPST, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.vFCPSTRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPSTRet, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.vIPIDevol = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIPIDevol, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.qBCMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.qBCMono, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.vICMSMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMono, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.qBCMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.qBCMonoReten, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.vICMSMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMonoReten, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.qBCMonoRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.qBCMonoRet, ObOp.Opcional, 15);
                            NFe.Total.ICMSTot.vICMSMonoRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSMonoRet, ObOp.Opcional, 15);
                        }
                    }
                    #endregion
                    break;

                case "W04":
                    //layout = prefix + this.FSegmento + "|vICMSUFDest|vICMSUFRemet|vFCPUFDest";
                    NFe.Total.ICMSTot.vICMSUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSUFDest, ObOp.Opcional, 15);
                    NFe.Total.ICMSTot.vICMSUFRemet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSUFRemet, ObOp.Opcional, 15);
                    NFe.Total.ICMSTot.vFCPUFDest = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFCPUFDest, ObOp.Opcional, 15);
                    break;

                case "W17":
                    //layout = (NFe.infNFe.Versao >= 3 ?
                    //            "§W17|VServ|VBC|VISS|VPIS|VCOFINS|dCompet|vDeducao|vOutro|vDescIncond|vDescCond|vISSRet|cRegTrib" :
                    //            "§W17|VServ|VBC|VISS|VPIS|VCOFINS");
                    ///
                    /// Grupo da TAG <total><ISSQNtot>
                    /// 
                    #region <total><ISSQNtot>
                    NFe.Total.ISSQNtot.vServ = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vServ, ObOp.Opcional, 15);
                    NFe.Total.ISSQNtot.vBC = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBC, ObOp.Opcional, 15);
                    NFe.Total.ISSQNtot.vISS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vISS, ObOp.Opcional, 15);
                    NFe.Total.ISSQNtot.vPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPIS, ObOp.Opcional, 15);
                    NFe.Total.ISSQNtot.vCOFINS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCOFINS, ObOp.Opcional, 15);

                    if ((double)NFe.infNFe.Versao >= 3.10)
                    {
                        NFe.Total.ISSQNtot.dCompet = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dCompet, ObOp.Opcional, 10, 10, true, false);
                        NFe.Total.ISSQNtot.vDeducao = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDeducao, ObOp.Opcional, 15);
                        NFe.Total.ISSQNtot.vOutro = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vOutro, ObOp.Opcional, 15);
                        NFe.Total.ISSQNtot.vDescIncond = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDescIncond, ObOp.Opcional, 15);
                        NFe.Total.ISSQNtot.vDescCond = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDescCond, ObOp.Opcional, 15);
                        NFe.Total.ISSQNtot.vISSRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vISSRet, ObOp.Opcional, 15);
                        NFe.Total.ISSQNtot.cRegTrib = (TpcnRegimeTributario)this.LerInt32(TpcnResources.cRegTrib, ObOp.Opcional, 1, 1);
                    }
                    #endregion
                    break;

                case "W23":
                    //layout = "§W23|VRetPIS|VRetCOFINS|VRetCSLL|VBCIRRF|VIRRF|VBCRetPrev|VRetPrev"; //ok
                    ///
                    /// Grupo da TAG <total><retTrib>
                    /// 
                    #region <total><retTrib>
                    NFe.Total.retTrib.vRetPIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vRetPIS, ObOp.Opcional, 15);
                    NFe.Total.retTrib.vRetCOFINS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vRetCOFINS, ObOp.Opcional, 15);
                    NFe.Total.retTrib.vRetCSLL = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vRetCSLL, ObOp.Opcional, 15);
                    NFe.Total.retTrib.vBCIRRF = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCIRRF, ObOp.Opcional, 15);
                    NFe.Total.retTrib.vIRRF = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIRRF, ObOp.Opcional, 15);
                    NFe.Total.retTrib.vBCRetPrev = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCRetPrev, ObOp.Opcional, 15);
                    NFe.Total.retTrib.vRetPrev = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vRetPrev, ObOp.Opcional, 15);
                    #endregion
                    break;

                case "W31":
                    //layout = "W31|vIS|"

                    NFe.Total.ISTot.vIS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIS, ObOp.Obrigatorio, 15);
                    break;

                case "W34":
                    //layout = "W34|vBCIBSCBS|"

                    NFe.Total.IBSCBSTot.vBCIBSCBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCIBSCBS, ObOp.Obrigatorio, 15);
                    break;

                case "W36":
                    //layout = "W36|vIBS|vCredPres|vCredPresCondSus|"

                    NFe.Total.IBSCBSTot.gIBS.vIBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBS, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gIBS.vCredPres = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPres, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gIBS.vCredPresCondSus = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPresCondSus, ObOp.Obrigatorio, 15);
                    break;

                case "W37":
                    //layout = "W37|vDif|vDevTrib|vIBSUF|"

                    NFe.Total.IBSCBSTot.gIBS.gIBSUF.vDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDif, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gIBS.gIBSUF.vDevTrib = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDevTrib, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gIBS.gIBSUF.vIBSUF = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSUF, ObOp.Obrigatorio, 15);
                    break;

                case "W42":
                    //layout = "W42|vDif|vDevTrib|vIBSMun|"

                    NFe.Total.IBSCBSTot.gIBS.gIBSMun.vDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDif, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gIBS.gIBSMun.vDevTrib = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDevTrib, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gIBS.gIBSMun.vIBSMun = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSMun, ObOp.Obrigatorio, 15);
                    break;

                case "W50":
                    //layout = "W50|vDif|vDevTrib|vCBS|vCredPres|vCredPresCondSus|"

                    NFe.Total.IBSCBSTot.gCBS.vDif = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDif, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gCBS.vDevTrib = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDevTrib, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gCBS.vCBS = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBS, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gCBS.vCredPres = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPres, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gCBS.vCredPresCondSus = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCredPresCondSus, ObOp.Obrigatorio, 15);
                    break;

                case "W57":
                    //layout = "W57|vIBSMono|vCBSMono|vIBSMonoReten|vCBSMonoReten|vIBSMonoRet|vCBSMonoRet|"

                    NFe.Total.IBSCBSTot.gMono.vIBSMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSMono, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gMono.vCBSMono = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSMono, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gMono.vIBSMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSMonoReten, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gMono.vCBSMonoReten = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSMonoReten, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gMono.vIBSMonoRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSMonoRet, ObOp.Obrigatorio, 15);
                    NFe.Total.IBSCBSTot.gMono.vCBSMonoRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSMonoRet, ObOp.Obrigatorio, 15);
                    break;

                case "W59E":
                    NFe.Total.IBSCBSTot.gEstornoCred.vIBSEstCred = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vIBSEstCred, ObOp.Opcional, 1, 13);
                    NFe.Total.IBSCBSTot.gEstornoCred.vCBSEstCred = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vCBSEstCred, ObOp.Opcional, 1, 13);
                    break;

                case "W60":
                    //layout = "W60|vNFTot|"

                    NFe.Total.vNFTot = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vNFTot, ObOp.Opcional, 15);
                    break;

                case "X":
                    //layout = "§X|modFrete"; //ok
                    ///
                    /// Grupo da TAG <transp>
                    /// 
                    NFe.Transp.modFrete = (TpcnModalidadeFrete)this.LerInt32(TpcnResources.modFrete, ObOp.Obrigatorio, 1, 1);
                    break;

                case "X03":
                    //layout = "§X03|xNome|IE|xEnder|UF|xMun"; //ok - alterado em 18/3/15
                    //layout = "§X03|xNome|IE|xEnder|xMun|UF"; //ok
                    ///
                    /// Grupo da TAG <transp><TRansportadora>
                    /// 
                    #region <transp><TRansportadora>
                    NFe.Transp.Transporta.xNome = this.LerString(TpcnResources.xNome, ObOp.Opcional, 1, 60);
                    NFe.Transp.Transporta.IE = this.LerString(TpcnResources.IE, ObOp.Opcional, 0, 14);
                    NFe.Transp.Transporta.xEnder = this.LerString(TpcnResources.xEnder, ObOp.Opcional, 1, 60);
                    NFe.Transp.Transporta.xMun = this.LerString(TpcnResources.xMun, ObOp.Opcional, 1, 60);
                    NFe.Transp.Transporta.UF = this.LerString(TpcnResources.UF, ObOp.Opcional, 2, 2);
                    #endregion
                    break;

                case "X04":
                    //layout = "§X04|CNPJ"; //ok

                    NFe.Transp.Transporta.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Opcional, 14, 14);
                    break;

                case "X05":
                    //layout = "§X05|CPF"; //ok

                    NFe.Transp.Transporta.CPF = this.LerString(TpcnResources.CPF, ObOp.Opcional, 11, 11);
                    break;

                case "X11":
                    //layout = "§X11|VServ|VBCRet|PICMSRet|VICMSRet|CFOP|CMunFG"; //ok
                    ///
                    /// Grupo da TAG <transp><retTransp>
                    /// 
                    #region <transp><retTransp>
                    NFe.Transp.retTransp.vServ = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vServ, ObOp.Obrigatorio, 15);
                    NFe.Transp.retTransp.vBCRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vBCRet, ObOp.Obrigatorio, 15);
                    NFe.Transp.retTransp.pICMSRet = this.LerDouble(this.TipoCampo42, TpcnResources.pICMSRet, ObOp.Obrigatorio, this.CasasDecimais75);
                    NFe.Transp.retTransp.vICMSRet = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vICMSRet, ObOp.Obrigatorio, 15);
                    NFe.Transp.retTransp.CFOP = this.LerString(TpcnResources.CFOP, ObOp.Obrigatorio, 4, 4);
                    NFe.Transp.retTransp.cMunFG = this.LerInt32(TpcnResources.cMunFG, ObOp.Obrigatorio, 7, 7);
                    #endregion
                    break;

                case "X18":
                    //layout = "§X18|Placa|UF|RNTC"; //ok
                    ///
                    /// Grupo da TAG <transp><veicTransp>
                    /// 
                    #region <transp><veicTransp>
                    NFe.Transp.veicTransp.placa = this.LerString(TpcnResources.placa, ObOp.Obrigatorio, 1, 8);
                    NFe.Transp.veicTransp.UF = this.LerString(TpcnResources.UF, ObOp.Obrigatorio, 2, 2);
                    NFe.Transp.veicTransp.RNTC = this.LerString(TpcnResources.RNTC, ObOp.Opcional, 1, 20);
                    #endregion
                    break;

                case "X22":
                    //layout = "§X22|Placa|UF|RNTC" + (NFe.infNFe.Versao >= 3 ? "|vagao|balsa" : "");
                    ///
                    /// Grupo da TAG <transp><reboque>
                    /// 
                    #region <transp><reboque>
                    NFe.Transp.Reboque.Add(new Reboque());
                    NFe.Transp.Reboque[NFe.Transp.Reboque.Count - 1].placa = this.LerString(TpcnResources.placa, ObOp.Obrigatorio, 1, 8);
                    NFe.Transp.Reboque[NFe.Transp.Reboque.Count - 1].UF = this.LerString(TpcnResources.UF, ObOp.Obrigatorio, 2, 2);
                    NFe.Transp.Reboque[NFe.Transp.Reboque.Count - 1].RNTC = this.LerString(TpcnResources.RNTC, ObOp.Opcional, 1, 20);
                    if (NFe.infNFe.Versao >= 3)
                    {
                        NFe.Transp.vagao = this.LerString(TpcnResources.vagao, ObOp.Opcional, 1, 20);
                        NFe.Transp.balsa = this.LerString(TpcnResources.balsa, ObOp.Opcional, 1, 20);
                    }
                    #endregion
                    break;

                case "X26":
                    //layout = "§X26|QVol|Esp|Marca|NVol|PesoL|PesoB"; //ok
                    ///
                    /// Grupo da TAG <transp><vol>
                    /// 
                    #region <transp><vol>
                    NFe.Transp.Vol.Add(new Vol());
                    NFe.Transp.Vol[NFe.Transp.Vol.Count - 1].qVol = this.LerInt32(TpcnResources.qVol, ObOp.Obrigatorio, 1, 15);
                    NFe.Transp.Vol[NFe.Transp.Vol.Count - 1].esp = this.LerString(TpcnResources.esp, ObOp.Opcional, 1, 60);
                    NFe.Transp.Vol[NFe.Transp.Vol.Count - 1].marca = this.LerString(TpcnResources.marca, ObOp.Opcional, 1, 60);
                    NFe.Transp.Vol[NFe.Transp.Vol.Count - 1].nVol = this.LerString(TpcnResources.nVol, ObOp.Opcional, 1, 60);
                    NFe.Transp.Vol[NFe.Transp.Vol.Count - 1].pesoL = this.LerDouble(TpcnTipoCampo.tcDouble3, TpcnResources.pesoL, ObOp.Opcional, 15);
                    NFe.Transp.Vol[NFe.Transp.Vol.Count - 1].pesoB = this.LerDouble(TpcnTipoCampo.tcDouble3, TpcnResources.pesoB, ObOp.Opcional, 15);
                    #endregion
                    break;

                case "X33":
                    //layout = "§X33|NLacre"; //ok
                    ///
                    /// Grupo da TAG <transp><vol><lacres>
                    /// 
                    #region <transp><vol><lacres>
                    Lacres lacreItem = new Lacres();
                    lacreItem.nLacre = this.LerString(TpcnResources.nLacre, ObOp.Obrigatorio, 1, 60);

                    NFe.Transp.Vol[NFe.Transp.Vol.Count - 1].Lacres.Add(lacreItem);
                    #endregion
                    break;

                case "Y02":
                    //layout = "§Y02|NFat|VOrig|VDesc|VLiq"; //ok
                    ///
                    /// Grupo da TAG <cobr>
                    /// 
                    #region <cobr>
                    NFe.Cobr.Fat.nFat = this.LerString(TpcnResources.nFat, ObOp.Opcional, 1, 60);
                    NFe.Cobr.Fat.vOrig = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vOrig, ObOp.Opcional, 15);
                    NFe.Cobr.Fat.vDesc = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDesc, ObOp.None, 15, true);
                    NFe.Cobr.Fat.vLiq = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vLiq, ObOp.Opcional, 15);
                    #endregion
                    break;

                case "Y07":
                    //layout = "§Y07|NDup|DVenc|VDup"; //ok
                    ///
                    /// Grupo da TAG <cobr><dup>
                    /// 
                    #region <cobr><dup>
                    NFe.Cobr.Dup.Add(new Dup());
                    if (DateTime.Today >= new DateTime(2018, 9, 3))
                        NFe.Cobr.Dup[NFe.Cobr.Dup.Count - 1].nDup = this.LerString(TpcnResources.nDup, ObOp.Opcional, 1, NFe.infNFe.Versao >= 4 ? 3 : 60);
                    else
                        NFe.Cobr.Dup[NFe.Cobr.Dup.Count - 1].nDup = this.LerString(TpcnResources.nDup, ObOp.Opcional, 1, 60);
                    NFe.Cobr.Dup[NFe.Cobr.Dup.Count - 1].dVenc = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dVenc, ObOp.Opcional, 10, 10, true, false);
                    NFe.Cobr.Dup[NFe.Cobr.Dup.Count - 1].vDup = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDup, ObOp.Opcional, 15);
                    #endregion
                    break;

                ///
                /// NFC-e e NF-e
                /// 
                case "YA":
                    #region YA

                    //_LayoutTXT.Add("YA_9", prefix +  "YA|indPag|tPag|xPag|vPag|CNPJ|tBand|cAut|tpIntegra|");
                    //_LayoutTXT.Add("YA_14", prefix + "YA|indPag|tPag|xPag|vPag|dPag|CNPJPag|UFPag|CNPJ|tBand|cAut|tpIntegra|CNPJReceb|idTermPag|");


                    NFe.pag.Add(new pag());
                    NFe.pag[NFe.pag.Count - 1].indPag = (TpcnIndicadorPagamento)this.LerInt32(TpcnResources.indPag, ObOp.Opcional, 1, 1, true);
                    NFe.pag[NFe.pag.Count - 1].tPag = (TpcnFormaPagamento)this.LerInt32(TpcnResources.tPag, ObOp.Obrigatorio, 2, 2);
                    NFe.pag[NFe.pag.Count - 1].xPag = this.LerString(TpcnResources.xPag, ObOp.Opcional, 0, 60);
                    NFe.pag[NFe.pag.Count - 1].vPag = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vPag, ObOp.Obrigatorio, 15);

                    if (lenPipesRegistro >= 14)
                    {
                        NFe.pag[NFe.pag.Count - 1].dPag = (DateTime)this.LerCampo(TpcnTipoCampo.tcDatYYYY_MM_DD, TpcnResources.dPag, ObOp.Opcional, 10, 10, true, false);
                        NFe.pag[NFe.pag.Count - 1].CNPJPag = this.LerString(TpcnResources.CNPJPag, ObOp.Opcional, 14, 14);
                        NFe.pag[NFe.pag.Count - 1].UFPag = this.LerString(TpcnResources.UFPag, ObOp.Opcional, 1, 2);
                    }

                    NFe.pag[NFe.pag.Count - 1].CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Opcional, 14, 14);
                    NFe.pag[NFe.pag.Count - 1].tBand = (TpcnBandeiraCartao)this.LerInt32(TpcnResources.tBand, ObOp.Opcional, 2, 2);
                    NFe.pag[NFe.pag.Count - 1].cAut = this.LerString(TpcnResources.cAut, ObOp.Opcional, 1, 128);
                    NFe.pag[NFe.pag.Count - 1].tpIntegra = this.LerInt32(TpcnResources.tpIntegra, ObOp.Opcional, 1, 1);
                    
                    if (lenPipesRegistro >= 14)
                    {                    
                        NFe.pag[NFe.pag.Count - 1].CNPJReceb = this.LerString(TpcnResources.CNPJReceb, ObOp.Opcional, 14, 14);
                        NFe.pag[NFe.pag.Count - 1].idTermPag = this.LerString(TpcnResources.idTermPag, ObOp.Opcional, 0, 40);
                    }

                    #endregion
                    break;

                case "YA09":
                    NFe.vTroco = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTroco, ObOp.Opcional, 15);
                    break;

                case "YA04":
                case "YA04A":
                    //layout = prefix + this.FSegmento + "|tpIntegra";
                    NFe.pag[NFe.pag.Count - 1].tpIntegra = this.LerInt32(TpcnResources.tpIntegra, ObOp.Obrigatorio, 1, 1);
                    break;

                case "YB":
                    NFe.InfIntermed.CNPJ = LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 1, 14, true);
                    NFe.InfIntermed.idCadIntTran = LerString(TpcnResources.idCadIntTran, ObOp.Obrigatorio, 1, 60, true);
                    break;

                case "Z":
                    //layout = "§Z|InfAdFisco|InfCpl"; //ok
                    ///
                    /// Grupo da TAG <InfAdic>
                    /// 
                    #region <InfAdic>
                    NFe.InfAdic.infAdFisco += this.LerString(TpcnResources.infAdFisco, ObOp.Opcional, 1, 2000, false);
                    NFe.InfAdic.infCpl += this.LerString(TpcnResources.infCpl, ObOp.Opcional, 1, 5000, false);
                    #endregion
                    break;

                case "Z04":
                    //layout = "§Z04|XCampo|XTexto"; //ok
                    ///
                    /// Grupo da TAG <infAdic><obsCont>
                    /// 
                    #region <infAdic><obsCont>
                    NFe.InfAdic.obsCont.Add(new obsCont());
                    NFe.InfAdic.obsCont[NFe.InfAdic.obsCont.Count - 1].xCampo = this.LerString(TpcnResources.xCampo, ObOp.Obrigatorio, 1, 20);
                    NFe.InfAdic.obsCont[NFe.InfAdic.obsCont.Count - 1].xTexto = this.LerString(TpcnResources.xTexto, ObOp.Obrigatorio, 1, 60);
                    #endregion
                    break;

                case "Z07":
                    //layout = "§Z07|XCampo|XTexto"; //ok - ?
                    ///
                    /// Grupo da TAG <infAdic><obsFisco>
                    /// 
                    #region <infAdic><obsFisco>
                    NFe.InfAdic.obsFisco.Add(new obsFisco());
                    NFe.InfAdic.obsFisco[NFe.InfAdic.obsFisco.Count - 1].xCampo = this.LerString(TpcnResources.xCampo, ObOp.Obrigatorio, 1, 20);
                    NFe.InfAdic.obsFisco[NFe.InfAdic.obsFisco.Count - 1].xTexto = this.LerString(TpcnResources.xTexto, ObOp.Obrigatorio, 1, 60);
                    #endregion
                    break;

                case "Z10":
                    //layout = "§Z10|NProc|IndProc"; //ok
                    ///
                    /// Grupo da TAG <infAdic><procRef>
                    /// 
                    #region <infAdic><procRef>
                    NFe.InfAdic.procRef.Add(new procRef());
                    NFe.InfAdic.procRef[NFe.InfAdic.procRef.Count - 1].nProc = this.LerString(TpcnResources.nProc, ObOp.Obrigatorio, 1, 60);
                    NFe.InfAdic.procRef[NFe.InfAdic.procRef.Count - 1].indProc = this.LerString(TpcnResources.indProc, ObOp.Obrigatorio, 1, 1);
                    if (lenPipesRegistro >= 4)
                    {
                        NFe.InfAdic.procRef[NFe.InfAdic.procRef.Count - 1].tpAto = this.LerString(TpcnResources.tpAto, ObOp.Opcional, 2, 2);
                    }
                    #endregion
                    break;

                case "ZA":
                case "ZA01":    //Só UniNFe
                    if (NFe.infNFe.Versao >= 3)
                    {
                        //layout = prefix + this.FSegmento + "|UFSaidaPais|xLocExporta|xLocDespacho"; //ok
                        ///
                        /// Grupo da TAG <exporta>
                        /// 
                        NFe.exporta.UFSaidaPais = this.LerString(TpcnResources.UFSaidaPais, ObOp.Obrigatorio, 2, 2);
                        NFe.exporta.xLocExporta = this.LerString(TpcnResources.xLocExporta, ObOp.Obrigatorio, 1, 60);
                        NFe.exporta.xLocDespacho = this.LerString(TpcnResources.xLocDespacho, ObOp.Opcional, 1, 60);
                    }
                    else
                    {
                        //layout = "§ZA|UFEmbarq|XLocEmbarq"; //ok
                        ///
                        /// Grupo da TAG <exporta>
                        /// 
                        NFe.exporta.UFEmbarq = this.LerString(TpcnResources.UFEmbarq, ObOp.Obrigatorio, 2, 2);
                        NFe.exporta.xLocEmbarq = this.LerString(TpcnResources.xLocEmbarq, ObOp.Obrigatorio, 1, 60);
                    }
                    break;

                case "ZB":
                    //layout = "§ZB|XNEmp|XPed|XCont"; //ok
                    ///
                    /// Grupo da TAG <compra>
                    /// 
                    NFe.compra.xNEmp = this.LerString(TpcnResources.xNEmp, ObOp.Opcional, 1, 17);
                    NFe.compra.xPed = this.LerString(TpcnResources.xPed, ObOp.Opcional, 1, 60);
                    NFe.compra.xCont = this.LerString(TpcnResources.xCont, ObOp.Opcional, 1, 60);
                    break;

                case "ZC":
                case "ZC01":
                    //layout = prefix + this.FSegmento + "|safra|ref|qTotMes|qTotAnt|qTotGer|vFor|vTotDed|vLiqFor";
                    NFe.cana.safra = this.LerString(TpcnResources.safra, ObOp.Obrigatorio, 4, 9);
                    NFe.cana.Ref = this.LerString(TpcnResources.Ref, ObOp.Obrigatorio, 7, 7);
                    NFe.cana.qTotMes = this.LerDouble(TpcnTipoCampo.tcDouble10, TpcnResources.qTotMes, ObOp.Obrigatorio, 11);
                    NFe.cana.qTotAnt = this.LerDouble(TpcnTipoCampo.tcDouble10, TpcnResources.qTotAnt, ObOp.Obrigatorio, 11);
                    NFe.cana.qTotGer = this.LerDouble(TpcnTipoCampo.tcDouble10, TpcnResources.qTotGer, ObOp.Obrigatorio, 11);
                    NFe.cana.vFor = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vFor, ObOp.Obrigatorio, 15);
                    NFe.cana.vTotDed = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vTotDed, ObOp.Obrigatorio, 15);
                    NFe.cana.vLiqFor = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vLiqFor, ObOp.Obrigatorio, 15);
                    break;

                case "ZC04":
                    //layout = "§ZC04|dia|qtde";
                    NFe.cana.fordia.Add(new fordia());
                    NFe.cana.fordia[NFe.cana.fordia.Count - 1].dia = this.LerInt32(TpcnResources.dia, ObOp.Obrigatorio, 1, 2);
                    NFe.cana.fordia[NFe.cana.fordia.Count - 1].qtde = this.LerDouble(TpcnTipoCampo.tcDouble10, TpcnResources.qtde, ObOp.Obrigatorio, 11);
                    break;

                case "ZC10":
                    //layout = "§ZC10|xDed|vDed";
                    NFe.cana.deduc.Add(new deduc());
                    NFe.cana.deduc[NFe.cana.deduc.Count - 1].xDed = this.LerString(TpcnResources.xDed, ObOp.Obrigatorio, 1, 60);
                    NFe.cana.deduc[NFe.cana.deduc.Count - 1].vDed = this.LerDouble(TpcnTipoCampo.tcDouble2, TpcnResources.vDed, ObOp.Obrigatorio, 15);
                    break;

                case "ZD":
                    //layout = "ZD|CNPJ|xContato|email|fone|idCSRT|hashCSRT|"
                    NFe.resptecnico.CNPJ = this.LerString(TpcnResources.CNPJ, ObOp.Obrigatorio, 14, 14);
                    NFe.resptecnico.xContato = this.LerString(nameof(RespTecnico.xContato), ObOp.Obrigatorio, 2, 60);
                    NFe.resptecnico.email = this.LerString(TpcnResources.email, ObOp.Obrigatorio, 2, 60);
                    NFe.resptecnico.fone = this.LerString(TpcnResources.fone, ObOp.Obrigatorio, 6, 14);
                    NFe.resptecnico.idCSRT = this.LerInt32(nameof(RespTecnico.idCSRT), ObOp.Opcional, 2, 2);
                    NFe.resptecnico.hashCSRT = this.LerString(nameof(RespTecnico.hashCSRT), ObOp.Opcional, 16, 80);
                    break;

                case "ZF02":
                    //layout = "ZF02|nReceituario|CPFRespTec|"

                    var defensivo = new Defensivo();
                    defensivo.nReceituario = this.LerString(TpcnResources.nReceituario, ObOp.Obrigatorio, 1, 30);
                    defensivo.CPFResptec = this.LerString(TpcnResources.CPFRespTec, ObOp.Obrigatorio, 11, 11);

                    NFe.agropecuario.defensivo.Add(defensivo);

                    break;
                
                case "ZF04":
                    //layout = "ZF04|tpGuia|UFGuia|serieGuia|nGuia|"
                    NFe.agropecuario.guiaTransito.tpGuia = (TpcnTipoGuia)this.LerInt32(TpcnResources.tpGuia, ObOp.Obrigatorio, 1, 1);
                    NFe.agropecuario.guiaTransito.UFGuia = this.LerString(TpcnResources.UFGuia, ObOp.Obrigatorio, 2, 2);
                    NFe.agropecuario.guiaTransito.serieGuia = this.LerString(TpcnResources.serieGuia, ObOp.Opcional, 1, 9);
                    NFe.agropecuario.guiaTransito.nGuia = this.LerString(TpcnResources.nGuia, ObOp.Obrigatorio, 1, 9);
                    break;

            }
        }
    }

    public class txtTOxmlClassRetorno
    {
        public Int32 NotaFiscal { get; set; }
        public Int32 Serie { get; set; }
        public string XMLFileName { get; set; }
        public string ChaveNFe { get; set; }

        public txtTOxmlClassRetorno(string xmlFileName, string chaveNFe, Int32 notaFiscal, Int32 serie)
        {
            this.XMLFileName = xmlFileName;
            this.ChaveNFe = chaveNFe;
            this.NotaFiscal = notaFiscal;
            this.Serie = serie;
        }
    }
}