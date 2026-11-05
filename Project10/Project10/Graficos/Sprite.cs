using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project10.Graficos
{
    internal class Sprite
    {
        public Texture2D texture;
        public Vector2 position;
        public float Escala = 1f;


        public Sprite(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White, 0f, Vector2.Zero, Escala, SpriteEffects.None, 0f);
        }
    }
}
