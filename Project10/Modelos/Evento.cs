namespace Project10.Modelos
{
    internal class Evento
    {
        public TipoEdificio Tipo;      // qué edificio (y qué personaje) trae la decisión
        public string Titulo;
        public string Texto;
        public Opcion[] Opciones;


        public Evento(TipoEdificio tipo, string titulo, string texto, Opcion[] opciones)
        {
            Tipo = tipo; Titulo = titulo; Texto = texto; Opciones = opciones;
        }
    }
}
