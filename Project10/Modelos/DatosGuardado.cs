using System.Collections.Generic;

namespace Project10.Modelos
{
    // Un edificio tal como se guarda en el archivo .json
    internal class TorreGuardada
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Nivel { get; set; }
        public int Tipo { get; set; }       // 0 = Torre, 1 = Hospital, 2 = Centro militar, 3 = Mina
    }

    // Todo lo que se guarda de una partida (archivo partida.json)
    internal class DatosGuardado
    {
        public string Nombre { get; set; } = "Rey";
        public int Oro { get; set; } = 100;
        public int Reputacion { get; set; } = 50;
        public int Ejercito { get; set; } = 200;
        public int Turno { get; set; } = 1;
        public int Mapa { get; set; } = 2;
        public int Vida { get; set; } = 100;
        public int Espadas { get; set; }
        public int Escudos { get; set; }
        public int Arcos { get; set; }
        public int Oleada { get; set; }
        public int Modo { get; set; }                                   // 0 = Historia, 1 = Infinito
        public List<TorreGuardada> Torres { get; set; } = new List<TorreGuardada>();
        public List<int> Mochila { get; set; } = new List<int>();       // del fondo al tope de la pila
        public int[][] Historial { get; set; } = new int[0][];          // la matriz de estadísticas de las oleadas
    }

    // Opciones del juego (archivo configuracion.json). Se puede editar a mano con el Bloc de notas.
    internal class ConfiguracionJuego
    {
        public bool Sonido { get; set; } = true;
        public int Resolucion { get; set; } = 1;              // 0 = 960x540, 1 = 1280x720, 2 = 1600x900
        public float TiempoOleada { get; set; } = 30f;        // segundos de cuenta regresiva de cada oleada
        public int TurnoFinalHistoria { get; set; } = 200;    // turno en el que se gana el Modo Historia
        public int IntervaloOleada { get; set; } = 5;         // cada cuántos turnos llega una oleada
    }
}
