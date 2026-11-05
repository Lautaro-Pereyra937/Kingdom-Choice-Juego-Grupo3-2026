using Microsoft.Xna.Framework;

namespace Project10.Modelos
{
    // Animacion de flechas que aparece sobre un edificio cuando sube de nivel
    internal class EfectoSubida
    {
        public const int Frames = 52;
        public const int Columnas = 13;
        public const int AnchoFrame = 174;
        public const int AltoFrame = 455;
        public const float Segundos = 0.03f;      // duracion de cada frame

        public Vector2 Base;       // base del edificio (en coordenadas del mapa)
        public float Altura;       // alto del edificio ya mejorado
        public float Tiempo;

        public EfectoSubida(Vector2 baseEdificio, float altura)
        {
            Base = baseEdificio;
            Altura = altura;
        }

        public int Frame { get { return (int)(Tiempo / Segundos); } }
        public bool Terminado { get { return Frame >= Frames; } }
    }
}
