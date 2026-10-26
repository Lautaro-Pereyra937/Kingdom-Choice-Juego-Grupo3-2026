using System;
using System.Collections.Generic;
using System.IO;
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


        private void Guardar()
        {
            try
            {
                Jugador j = partidaActual.datosJugador;
                File.WriteAllLines(rutaGuardado, new[]
                {
                    "Nombre=" + j.Nombre,
                    "Oro=" + j.Oro,
                    "Reputacion=" + j.Reputacion,
                    "Ejercito=" + j.Ejercito,
                    "Turno=" + partidaActual.Turno,
                    "Mapa=" + mapaActual,
                    "Torres=" + string.Join(";", torres.Select(t => (int)t.Base.X + "," + (int)t.Base.Y + "," + t.Nivel + "," + (int)t.Tipo)),
                    "Vida=" + vidaImperio,
                    "Espadas=" + espadas,
                    "Escudos=" + escudos,
                    "Arcos=" + arcos,
                    "Oleada=" + oleadaNum,
                    "Mochila=" + string.Join(",", mochila.Reverse().Select(x => ((int)x).ToString()))
                });
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
                if (File.Exists(rutaGuardado))
                {
                    File.Delete(rutaGuardado);
                    Mensaje("Partida borrada.");
                }
                else
                {
                    Mensaje("No hay ninguna partida guardada.");
                }
            }
            catch (Exception)
            {
                Mensaje("No se pudo borrar la partida.");
            }
        }


        private bool Cargar()
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
                            tt = MathHelper.Clamp(tt, 0, 2);
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
