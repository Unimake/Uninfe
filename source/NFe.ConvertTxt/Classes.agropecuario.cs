using System.Collections.Generic;

namespace NFe.ConvertTxt
{
    public class Agropecuario
    {
        public List<Defensivo> defensivo;
        public GuiaTransito guiaTransito;

        public Agropecuario() 
        {
            defensivo = new List<Defensivo>();
            guiaTransito = new GuiaTransito();
        }
    }

    public struct Defensivo
    {
        public string nReceituario { get; set; }
        public string CPFResptec { get; set; }
    }

    public struct GuiaTransito
    {
        public TpcnTipoGuia tpGuia { get; set; }
        public string UFGuia { get; set; }
        public string serieGuia { get; set; }
        public string nGuia { get; set; }
    }
}
