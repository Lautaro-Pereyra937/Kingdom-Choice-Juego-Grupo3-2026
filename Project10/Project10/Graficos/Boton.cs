using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project10.Graficos
{
    // Botón con sprite normal y sprite "hover" (el resplandor dorado).
    // Se dibuja centrado en "Centro", así el sprite hover crece parejo hacia todos lados.
    internal class Boton
    {
        public Texture2D Normal;
        public Texture2D Hover;
        public Vector2 Centro;
        public float Escala = 1f;
        public bool Encima;
        public bool Habilitado = true;

        public Boton(Texture2D normal, Texture2D hover)
        {
            Normal = normal;
            Hover = hover;
        }

        public Rectangle Area
        {
            get
            {
                int w = (int)(Normal.Width * Escala);
                int h = (int)(Normal.Height * Escala);
                return new Rectangle((int)(Centro.X - w / 2f), (int)(Centro.Y - h / 2f), w, h);
            }
        }

        public void Actualizar(Point mouse)
        {
            Encima = Habilitado && Area.Contains(mouse);
        }

        public void Dibujar(SpriteBatch sb)
        {
            Texture2D t = Encima ? Hover : Normal;
            Color color = Habilitado ? Color.White : Color.White * 0.45f;
            sb.Draw(t, Centro, null, color, 0f, new Vector2(t.Width / 2f, t.Height / 2f), Escala, SpriteEffects.None, 0f);
        }
    }
}
