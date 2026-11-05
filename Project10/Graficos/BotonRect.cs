using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project10.Graficos
{
    internal class BotonRect
    {
        public Rectangle R;
        public string Texto;
        public bool Activo;
        public bool Encima;
        public Texture2D Icono;
        public Rectangle IconoSrc;
        public float IconoEscala = 1f;
        public string Extra;
        public bool Resaltado;
    }
}
