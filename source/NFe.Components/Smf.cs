namespace NFe.Components
{
    public class Smf
    {
        #region Objetos para semaforos
        /// <summary>
        /// Objeto para auxiliar no semaforo do fluxonfe
        /// </summary>
        public static SmfAuxiliar Fluxo = new SmfAuxiliar();
        /// <summary>
        /// Objeto para auxiliar no semaforo do carregamento das webproxy's
        /// </summary>
        public static SmfAuxiliar WebProxy = new SmfAuxiliar();
        /// <summary>
        /// Objeto para auxiliar no semaforo do controle do número de lote
        /// </summary>
        public static SmfAuxiliar NumLote = new SmfAuxiliar();
        /// <summary>
        /// Objeto para auxiliar no semaforo do controle do processamento de vários XMLs de NFSe ao mesmo tempo
        /// </summary>
        public static SmfAuxiliar RecepcionarLoteRps = new SmfAuxiliar();
        #endregion
    }

    public class SmfAuxiliar
    {
    }
}
