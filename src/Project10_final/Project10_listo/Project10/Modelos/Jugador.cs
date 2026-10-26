namespace Project10.Modelos
{
    internal class Jugador
    {
        public string Nombre { get; set; }
        public int Oro { get; set; }
        public int Reputacion { get; set; }
        public int Ejercito { get; set; }


        public Jugador(string Nombre, int Oro, int Reputacion, int Ejercito)
        {
            this.Nombre = Nombre;
            this.Oro = Oro;
            this.Reputacion = Reputacion;   // antes no se asignaba y quedaba en 0
            this.Ejercito = Ejercito;
        }
    }
}
