namespace Project10.Modelos
{
    internal class Opcion
    {
        public string Texto;
        public int Oro, Vida, Ejercito, Reputacion;
        public string Resultado;


        public Opcion(string texto, int oro, int vida, int ejercito, int reputacion, string resultado)
        {
            Texto = texto; Oro = oro; Vida = vida; Ejercito = ejercito; Reputacion = reputacion; Resultado = resultado;
        }
    }
}
