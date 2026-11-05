namespace Project10.Modelos
{
    internal class Enemigo
    {
        public int Vida;
        public int VidaMax;
        public int Ataque;
        public bool Atacado;   // true cuando las torres ya le dispararon


        public Enemigo(int oleada)
        {
            VidaMax = 10 + 2 * oleada;
            Vida = VidaMax;
            Ataque = 2 + (oleada + 1) / 2;
        }
    }
}
