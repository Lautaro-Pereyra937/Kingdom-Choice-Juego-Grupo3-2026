using Microsoft.Xna.Framework;

namespace Project10.Modelos
{
    // Una unidad que corre y pelea sobre el mapa durante la oleada.
    // Guarda el Soldado o el Enemigo que ya existía (con su vida y ataque) y le suma posición y animación.
    internal class UnidadBatalla
    {
        public Vector2 Pos;
        public Soldado Soldado;      // distinto de null si es de los nuestros
        public Enemigo Enemigo;      // distinto de null si es un enemigo
        public bool EsGeneral;       // el General Espadachín del centro militar
        public bool Llego;           // enemigo que ya llegó al pueblo (no cuenta como derrotado)
        public bool Izquierda;       // hacia dónde mira
        public bool Moviendo;
        public int VidaMax;
        public float Cooldown;       // espera entre golpes
        public float Golpeando;      // tiempo que dura la sacudida al pegar
        public float Flash;          // destello rojo al recibir un golpe
        public float Fase;           // animación del salto al correr

        public bool EsSoldado { get { return Soldado != null; } }

        public int Vida
        {
            get { return Soldado != null ? Soldado.Vida : Enemigo.Vida; }
        }

        public bool Viva { get { return Vida > 0; } }
    }

    // Línea que dibuja el disparo de una torre contra un enemigo (dura una fracción de segundo).
    internal class Disparo
    {
        public Vector2 Desde, Hasta;
        public float Tiempo = 0.2f;
    }
}
