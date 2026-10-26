using System;

namespace Project10.Datos
{
    // Cada clase es una tabla. Las propiedades se llaman igual que las columnas.

    public class players
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string email { get; set; }
        public DateTime fecha_registro { get; set; }
    }

    public class sessions
    {
        public int id { get; set; }
        public int player_id { get; set; }
        public DateTime inicio { get; set; }
        public DateTime? fin { get; set; }
        public int turnos_jugados { get; set; }
        public string resultado { get; set; }
    }

    public class kingdomstate
    {
        public int id { get; set; }
        public int session_id { get; set; }
        public int turno { get; set; }
        public int oro { get; set; }
        public int poblacion { get; set; }
        public int ejercito { get; set; }
        public int moral { get; set; }
        public int estabilidad { get; set; }
    }

    public class decisions
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string requisitos { get; set; }
    }

    public class decisionoutcomes
    {
        public int id { get; set; }
        public int decision_id { get; set; }
        public string texto_opcion { get; set; }
        public string resultado { get; set; }
        public int impacto_oro { get; set; }
        public int impacto_poblacion { get; set; }
        public int impacto_ejercito { get; set; }
        public int impacto_moral { get; set; }
        public int impacto_estabilidad { get; set; }
    }

    public class choicelogs
    {
        public int id { get; set; }
        public int session_id { get; set; }
        public int turno { get; set; }
        public int decision_id { get; set; }
        public int outcome_id { get; set; }
    }

    public class randomevents
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public decimal probabilidad { get; set; }
        public string efectos { get; set; }
    }

    public class eventlogs
    {
        public int id { get; set; }
        public int session_id { get; set; }
        public int turno { get; set; }
        public int event_id { get; set; }
    }

    public class sessionrank
    {
        public int id { get; set; }
        public int session_id { get; set; }
        public int puntaje { get; set; }
        public string logros { get; set; }
    }
}