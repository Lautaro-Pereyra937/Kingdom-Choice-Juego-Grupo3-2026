using Microsoft.Xna.Framework;

namespace Project10.Modelos
{
    // Representa cualquier edificio del mapa (torre, hospital o centro militar)
    internal class Torre
    {
        public Vector2 Base;   // punto donde apoya el edificio (centro de abajo del sprite)
        public int Nivel;
        public TipoEdificio Tipo;


        public Torre(Vector2 posicionBase, int nivel) : this(posicionBase, nivel, TipoEdificio.Torre)
        {
        }


        public Torre(Vector2 posicionBase, int nivel, TipoEdificio tipo)
        {
            Base = posicionBase;
            Nivel = nivel;
            Tipo = tipo;
        }
    }
}
