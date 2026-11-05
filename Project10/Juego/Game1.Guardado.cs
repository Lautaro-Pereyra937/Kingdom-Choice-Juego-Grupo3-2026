using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project10.Datos;
using Project10.Graficos;
using Project10.Modelos;

namespace Project10
{
    // Guardar, cargar y borrar la partida.
    public partial class Game1
    {
        // ------------------------------------------------------------------ GUARDADO


        // Manda a la base los valores actuales del reino (oro, ejército, reputación y vida del imperio)
        private void SincronizarBD()
        {
            Jugador j = partidaActual.datosJugador;
            BaseDatos.GuardarEstado(partidaActual.Turno, j.Oro, j.Ejercito, j.Reputacion, vidaImperio);
        }


        private static readonly JsonSerializerOptions OpcionesJson = new JsonSerializerOptions { WriteIndented = true };


        // Lee configuracion.json. Si no existe (o está roto) usa los valores por defecto y lo vuelve a crear.
        private void CargarConfiguracion()
        {
            ConfiguracionJuego c = new ConfiguracionJuego();
            try
            {
                if (File.Exists(rutaConfig))
                {
                    ConfiguracionJuego leida = JsonSerializer.Deserialize<ConfiguracionJuego>(File.ReadAllText(rutaConfig));
                    if (leida != null) c = leida;
                }
            }
            catch (Exception)
            {
                c = new ConfiguracionJuego();
            }

            sonidoActivo = c.Sonido;
            indiceResolucion = MathHelper.Clamp(c.Resolucion, 0, resoluciones.Length - 1);
            TiempoOleada = MathHelper.Clamp(c.TiempoOleada, 5f, 120f);
            TurnoFinalHistoria = MathHelper.Clamp(c.TurnoFinalHistoria, 20, 1000);
            IntervaloOleada = MathHelper.Clamp(c.IntervaloOleada, 2, 20);
            GuardarConfiguracion();
        }


        private void GuardarConfiguracion()
        {
            try
            {
                ConfiguracionJuego c = new ConfiguracionJuego
                {
                    Sonido = sonidoActivo,
                    Resolucion = indiceResolucion,
                    TiempoOleada = TiempoOleada,
                    TurnoFinalHistoria = TurnoFinalHistoria,
                    IntervaloOleada = IntervaloOleada
                };
                File.WriteAllText(rutaConfig, JsonSerializer.Serialize(c, OpcionesJson));
            }
            catch (Exception)
            {
                // si no se puede escribir, el juego sigue funcionando igual
            }
        }


        // Guarda la partida en partida.json
        private void Guardar()
        {
            try
            {
                Jugador j = partidaActual.datosJugador;
                DatosGuardado d = new DatosGuardado
                {
                    Nombre = j.Nombre,
                    Oro = j.Oro,
                    Reputacion = j.Reputacion,
                    Ejercito = j.Ejercito,
                    Turno = partidaActual.Turno,
                    Mapa = mapaActual,
                    Vida = vidaImperio,
                    Espadas = espadas,
                    Escudos = escudos,
                    Arcos = arcos,
                    Oleada = oleadaNum,
                    Modo = modoHistoria ? 0 : 1,
                    Torres = torres.Select(t => new TorreGuardada { X = (int)t.Base.X, Y = (int)t.Base.Y, Nivel = t.Nivel, Tipo = (int)t.Tipo }).ToList(),
                    Mochila = mochila.Reverse().Select(x => (int)x).ToList(),
                    Historial = HistorialAJson()
                };
                File.WriteAllText(rutaJson, JsonSerializer.Serialize(d, OpcionesJson));
                SincronizarBD();
                Mensaje("Partida guardada.");
            }
            catch (Exception)
            {
                Mensaje("No se pudo guardar la partida.");
            }
        }


        private void Borrar()
        {
            try
            {
                bool habia = File.Exists(rutaJson) || File.Exists(rutaGuardado);
                if (File.Exists(rutaJson)) File.Delete(rutaJson);
                if (File.Exists(rutaGuardado)) File.Delete(rutaGuardado);
                Mensaje(habia ? "Partida borrada." : "No hay ninguna partida guardada.");
            }
            catch (Exception)
            {
                Mensaje("No se pudo borrar la partida.");
            }
        }


        // Carga la partida: primero busca partida.json; si no está, intenta con el archivo viejo partida.sav
        private bool Cargar()
        {
            try
            {
                if (File.Exists(rutaJson)) return CargarJson();
            }
            catch (Exception)
            {
                // el .json está roto: se prueba con el viejo
            }
            return CargarTexto();
        }


        private bool CargarJson()
        {
            DatosGuardado d = JsonSerializer.Deserialize<DatosGuardado>(File.ReadAllText(rutaJson));
            if (d == null) return false;

            string nombre = new string((d.Nombre ?? "Rey").Where(c => FuenteJuego.Characters.Contains(c)).ToArray());
            if (nombre.Length == 0) nombre = "Rey";

            mapaActual = MathHelper.Clamp(d.Mapa, 0, texturasMapas.Length - 1);
            modoHistoria = d.Modo == 0;

            torresGuardadas = new List<Torre>();
            if (d.Torres != null)
            {
                foreach (TorreGuardada tg in d.Torres)
                {
                    int tt = MathHelper.Clamp(tg.Tipo, 0, 3);
                    torresGuardadas.Add(new Torre(new Vector2(tg.X, tg.Y), MathHelper.Clamp(tg.Nivel, 1, NivelMaxDe[tt]), (TipoEdificio)tt));
                }
            }

            ReiniciarMundo();
            vidaImperio = MathHelper.Clamp(d.Vida, 1, VidaMax);
            espadas = d.Espadas;
            escudos = d.Escudos;
            arcos = d.Arcos;
            oleadaNum = d.Oleada;
            if (d.Mochila != null)
            {
                foreach (int v in d.Mochila)
                    if (v >= 0 && v < 4) mochila.Push((TipoItem)v);
            }
            JsonAHistorial(d.Historial);

            NombreRey = nombre;
            partidaActual = new Partida("Partida cargada", Math.Max(1, d.Turno), new Jugador(nombre, d.Oro, d.Reputacion, d.Ejercito));
            return true;
        }


        // Lee el formato viejo (partida.sav) para no perder las partidas que ya tenías guardadas
        private bool CargarTexto()
        {
            try
            {
                if (!File.Exists(rutaGuardado)) return false;


                Dictionary<string, string> datos = new Dictionary<string, string>();
                foreach (string linea in File.ReadAllLines(rutaGuardado))
                {
                    int igual = linea.IndexOf('=');
                    if (igual > 0) datos[linea.Substring(0, igual)] = linea.Substring(igual + 1);
                }


                string nombre = datos.ContainsKey("Nombre") ? datos["Nombre"] : "Rey";
                nombre = new string(nombre.Where(c => FuenteJuego.Characters.Contains(c)).ToArray());
                if (nombre.Length == 0) nombre = "Rey";


                int oro = Leer(datos, "Oro", 100);
                int reputacion = Leer(datos, "Reputacion", 50);
                int ejercito = Leer(datos, "Ejercito", 200);
                int turno = Leer(datos, "Turno", 1);
                mapaActual = MathHelper.Clamp(Leer(datos, "Mapa", 2), 0, texturasMapas.Length - 1);
                modoHistoria = Leer(datos, "Modo", 0) == 0;   // las partidas viejas (sin Modo) son de historia


                torresGuardadas = new List<Torre>();
                if (datos.ContainsKey("Torres"))
                {
                    foreach (string item in datos["Torres"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] p = item.Split(',');
                        int tx, ty, tn, tt = 0;
                        if (p.Length >= 3 && int.TryParse(p[0], out tx) && int.TryParse(p[1], out ty) && int.TryParse(p[2], out tn))
                        {
                            if (p.Length >= 4) int.TryParse(p[3], out tt);
                            tt = MathHelper.Clamp(tt, 0, 3);
                            torresGuardadas.Add(new Torre(new Vector2(tx, ty), MathHelper.Clamp(tn, 1, NivelMaxDe[tt]), (TipoEdificio)tt));
                        }
                    }
                }


                ReiniciarMundo();
                vidaImperio = MathHelper.Clamp(Leer(datos, "Vida", VidaMax), 1, VidaMax);
                espadas = Leer(datos, "Espadas", 0);
                escudos = Leer(datos, "Escudos", 0);
                arcos = Leer(datos, "Arcos", 0);
                oleadaNum = Leer(datos, "Oleada", 0);
                if (datos.ContainsKey("Mochila"))
                {
                    foreach (string sItem in datos["Mochila"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int v;
                        if (int.TryParse(sItem, out v) && v >= 0 && v < 4) mochila.Push((TipoItem)v);
                    }
                }


                NombreRey = nombre;
                partidaActual = new Partida("Partida cargada", turno, new Jugador(nombre, oro, reputacion, ejercito));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static int Leer(Dictionary<string, string> datos, string clave, int porDefecto)
        {
            int valor;
            if (datos.ContainsKey(clave) && int.TryParse(datos[clave], out valor)) return valor;
            return porDefecto;
        }
    }
}
