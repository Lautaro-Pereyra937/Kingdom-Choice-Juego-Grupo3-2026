using System;

namespace Project10.Modelos
{
    internal class Partida
    {
        public int Turno { get; set; }
        public string Nombre { get; set; }
        public Jugador datosJugador { get; set; }
        public DateTime Fecha { get; set; }


        public Partida(string Nombre, int Turno, Jugador datosJugador)
        {
            this.Nombre = Nombre;
            this.Turno = Turno;
            this.datosJugador = datosJugador;
            Fecha = DateTime.Now;
        }
    }
}
