using System;
using System.Linq;

namespace Project10.Datos
{
    // Puente entre el juego y la base "reino" (XAMPP).
    internal static class BaseDatos
    {
        private static int sesionId = 0;   // 0 = no hay partida registrada

        // Resultado de la última operación (el juego lo muestra en pantalla)
        public static string Ultimo = "La base no fue llamada todavía";


        // Crea (o reutiliza) al jugador y abre una partida.
        // continuar = true  -> retoma la última partida "en_curso" de ese jugador (botón Continuar)
        // continuar = false -> abre una partida nueva (y marca la anterior como "abandonada")
        public static void IniciarSesion(string nombreRey, bool continuar = false)
        {
            Ultimo = "Llamando a la base...";

            // Si había otra partida abierta en memoria y empezamos una nueva, se cierra como abandonada
            if (!continuar && sesionId != 0) CerrarComoAbandonada(sesionId);

            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    players jugador = db.Players.FirstOrDefault(p => p.nombre == nombreRey);
                    if (jugador == null)
                    {
                        jugador = new players { nombre = nombreRey, fecha_registro = DateTime.Now };
                        db.Players.Add(jugador);
                        db.SaveChanges();
                    }

                    sessions sesion = null;
                    if (continuar)
                    {
                        sesion = db.Sessions
                            .Where(s => s.player_id == jugador.id && s.resultado == "en_curso")
                            .OrderByDescending(s => s.id)
                            .FirstOrDefault();
                    }

                    if (sesion == null)
                    {
                        sesion = new sessions
                        {
                            player_id = jugador.id,
                            inicio = DateTime.Now,
                            turnos_jugados = 0,
                            resultado = "en_curso"
                        };
                        db.Sessions.Add(sesion);
                        db.SaveChanges();
                    }
                    sesionId = sesion.id;
                }
                Ultimo = "BD OK: partida " + sesionId + (continuar ? " retomada" : " guardada");
            }
            catch (Exception ex) { Error(ex); }
        }


        private static void CerrarComoAbandonada(int id)
        {
            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    sessions anterior = db.Sessions.Find(id);
                    if (anterior != null && anterior.resultado == "en_curso")
                    {
                        anterior.fin = DateTime.Now;
                        anterior.resultado = "abandonada";
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex) { Error(ex); }
        }


        // Foto del reino al terminar un turno. También actualiza turnos_jugados y el puntaje parcial.
        public static void GuardarEstado(int turno, int oro, int ejercito, int moral, int estabilidad)
        {
            if (sesionId == 0) { Aviso("BD: no hay partida abierta (no se guardó el turno)"); return; }
            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    int t = Math.Max(1, turno);
                    int est = Math.Min(100, Math.Max(0, estabilidad));

                    kingdomstate estado = db.KingdomState.FirstOrDefault(k => k.session_id == sesionId && k.turno == t);
                    if (estado == null)
                    {
                        estado = new kingdomstate { session_id = sesionId, turno = t };
                        db.KingdomState.Add(estado);
                    }
                    estado.oro = Math.Max(0, oro);
                    estado.poblacion = 0;                       // el juego todavía no maneja población
                    estado.ejercito = Math.Max(0, ejercito);
                    estado.moral = Math.Max(0, moral);
                    estado.estabilidad = est;

                    // La sesión y el ranking se mantienen al día turno a turno
                    sessions sesion = db.Sessions.Find(sesionId);
                    if (sesion != null) sesion.turnos_jugados = Math.Max(0, turno);

                    sessionrank rank = db.SessionRank.FirstOrDefault(r => r.session_id == sesionId);
                    if (rank == null)
                    {
                        rank = new sessionrank { session_id = sesionId };
                        db.SessionRank.Add(rank);
                    }
                    rank.puntaje = Puntaje(turno, oro, ejercito, moral, est);
                    string logros = Logros(turno, oro);
                    rank.logros = logros.Length > 0 ? logros : null;

                    db.SaveChanges();
                }
                Ultimo = "BD OK: turno " + turno + " guardado";
            }
            catch (Exception ex) { Error(ex); }
        }


        // Opción elegida en una decisión
        public static void RegistrarDecision(int turno, string tituloDecision, string textoOpcion)
        {
            if (sesionId == 0) { Aviso("BD: no hay partida abierta (decisión no registrada)"); return; }
            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    decisions decision = db.Decisions.FirstOrDefault(d => d.nombre == tituloDecision);
                    if (decision == null)
                    {
                        Aviso("BD: falta la decisión '" + tituloDecision + "' en la tabla decisions");
                        return;
                    }
                    decisionoutcomes opcion = db.DecisionOutcomes
                        .FirstOrDefault(o => o.decision_id == decision.id && o.texto_opcion == textoOpcion);
                    if (opcion == null)
                    {
                        Aviso("BD: falta la opción '" + textoOpcion + "' en decisionoutcomes");
                        return;
                    }

                    db.ChoiceLogs.Add(new choicelogs
                    {
                        session_id = sesionId,
                        turno = Math.Max(1, turno),
                        decision_id = decision.id,
                        outcome_id = opcion.id
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex) { Error(ex); }
        }


        // Evento aleatorio ocurrido (por ejemplo "Oleada enemiga")
        public static void RegistrarEvento(int turno, string nombreEvento)
        {
            if (sesionId == 0) { Aviso("BD: no hay partida abierta (evento no registrado)"); return; }
            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    randomevents ev = db.RandomEvents.FirstOrDefault(e => e.nombre == nombreEvento);
                    if (ev == null)
                    {
                        Aviso("BD: falta el evento '" + nombreEvento + "' en la tabla randomevents");
                        return;
                    }

                    db.EventLogs.Add(new eventlogs { session_id = sesionId, turno = Math.Max(1, turno), event_id = ev.id });
                    db.SaveChanges();
                }
            }
            catch (Exception ex) { Error(ex); }
        }


        // Cierra la partida y guarda el puntaje. resultado: "victoria", "derrota" o "abandonada"
        public static void FinalizarSesion(string resultado, int turnos, int oro, int ejercito, int moral, int estabilidad)
        {
            if (sesionId == 0) return;
            try
            {
                int puntaje = Puntaje(turnos, oro, ejercito, moral, estabilidad);
                string logros = Logros(turnos, oro);

                using (AppDbContext db = new AppDbContext())
                {
                    sessions sesion = db.Sessions.Find(sesionId);
                    if (sesion == null) return;
                    sesion.fin = DateTime.Now;
                    sesion.turnos_jugados = Math.Max(0, turnos);
                    sesion.resultado = resultado;

                    sessionrank rank = db.SessionRank.FirstOrDefault(r => r.session_id == sesionId);
                    if (rank == null)
                    {
                        rank = new sessionrank { session_id = sesionId };
                        db.SessionRank.Add(rank);
                    }
                    rank.puntaje = Math.Max(0, puntaje);
                    rank.logros = logros.Length > 0 ? logros : null;

                    db.SaveChanges();
                }
                Ultimo = "BD OK: partida " + sesionId + " cerrada (" + resultado + ")";
                sesionId = 0;
            }
            catch (Exception ex) { Error(ex); }
        }


        // Fórmula de puntaje de ejemplo: cambiala como quieras
        private static int Puntaje(int turnos, int oro, int ejercito, int moral, int estabilidad)
        {
            return Math.Max(0, turnos * 10 + oro + ejercito + moral * 2 + estabilidad);
        }

        private static string Logros(int turnos, int oro)
        {
            string logros = "";
            if (turnos >= 10) logros += "Sobreviviente (10 turnos)";
            if (turnos >= 30) logros += (logros.Length > 0 ? ", " : "") + "Veterano (30 turnos)";
            if (oro >= 500) logros += (logros.Length > 0 ? ", " : "") + "Tesoro real (500 de oro)";
            return logros;
        }


        // Avisos que no son excepciones (faltan datos en la base): se muestran y se anotan en el log
        private static void Aviso(string texto)
        {
            Ultimo = texto;
            Anotar(texto);
        }

        // Guarda el error para mostrarlo en pantalla y lo anota en el Escritorio
        private static void Error(Exception ex)
        {
            Ultimo = "BD ERROR: " + ex.GetBaseException().Message;
            Anotar(ex.ToString());
        }

        private static void Anotar(string texto)
        {
            try
            {
                string ruta = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "errores_bd.txt");
                System.IO.File.AppendAllText(ruta,
                    DateTime.Now + "  " + texto + Environment.NewLine + Environment.NewLine);
            }
            catch { }
        }
    }
}
