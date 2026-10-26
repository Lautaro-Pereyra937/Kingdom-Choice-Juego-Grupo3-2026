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
    // Eventos / decisiones que traen los personajes de cada edificio.
    public partial class Game1
    {
        // ------------------------------------------------------------------ EVENTOS (DECISIONES)

        // Puede que aparezca un evento de alguno de los edificios que ya construiste
        private void GenerarEvento()
        {
            if (azar.Next(100) >= 45) return;


            List<TipoEdificio> disponibles = new List<TipoEdificio>();
            foreach (TipoEdificio tp in Enum.GetValues(typeof(TipoEdificio)))
                if (ContarEdificios(tp) > 0) disponibles.Add(tp);
            if (disponibles.Count == 0) return;


            TipoEdificio elegido = disponibles[azar.Next(disponibles.Count)];
            List<Evento> candidatos = eventos.Where(ev => ev.Tipo == elegido).ToList();
            colaEventos.Enqueue(candidatos[azar.Next(candidatos.Count)]);   // se suma al final de la fila
        }


        private void MostrarSiguienteEvento()
        {
            eventoActual = colaEventos.Dequeue();                          // sale el primero de la fila
            opcionElegida = null;
            evFase = 1;
            panelActivo = "Evento";
            torreSeleccionada = -1;
            construirTipo = -1;
        }


        private void AplicarOpcion(Opcion o)
        {
            Jugador j = partidaActual.datosJugador;
            j.Oro = Math.Max(0, j.Oro + o.Oro);
            vidaImperio = MathHelper.Clamp(vidaImperio + o.Vida, 1, VidaMax);
            j.Ejercito = Math.Max(0, j.Ejercito + o.Ejercito);
            j.Reputacion = Math.Max(0, j.Reputacion + o.Reputacion);
            BaseDatos.RegistrarDecision(partidaActual.Turno, eventoActual.Titulo, o.Texto);   // <-- BASE DE DATOS
            SincronizarBD();   // los valores ya cambiaron: se actualiza la foto del turno
            opcionElegida = o;
            evFase = 2;
        }


        private static string ResumenEfectos(Opcion o)
        {
            List<string> partes = new List<string>();
            if (o.Oro != 0) partes.Add("Oro " + (o.Oro > 0 ? "+" : "") + o.Oro);
            if (o.Vida != 0) partes.Add("Vida " + (o.Vida > 0 ? "+" : "") + o.Vida);
            if (o.Ejercito != 0) partes.Add("Ejército " + (o.Ejercito > 0 ? "+" : "") + o.Ejercito);
            if (o.Reputacion != 0) partes.Add("Reputación " + (o.Reputacion > 0 ? "+" : "") + o.Reputacion);
            return partes.Count == 0 ? "Sin cambios" : string.Join("     ", partes);
        }


        private static List<string> Envolver(string texto, int max)
        {
            List<string> lineas = new List<string>();
            string actual = "";
            foreach (string palabra in texto.Split(' '))
            {
                if (actual.Length > 0 && actual.Length + palabra.Length + 1 > max)
                {
                    lineas.Add(actual);
                    actual = palabra;
                }
                else
                {
                    actual = actual.Length == 0 ? palabra : actual + " " + palabra;
                }
            }
            if (actual.Length > 0) lineas.Add(actual);
            return lineas;
        }


        private void ActualizarEvento()
        {
            Jugador j = partidaActual.datosJugador;
            if (eventoActual == null) { panelActivo = null; return; }


            if (evFase == 1)
            {
                for (int i = 0; i < eventoActual.Opciones.Length; i++)
                {
                    Opcion o = eventoActual.Opciones[i];
                    Rectangle r = new Rectangle(380, 330 + i * 62, 740, 50);
                    bool puede = j.Oro + o.Oro >= 0;
                    if (Rb(r, o.Texto, puede))
                    {
                        AplicarOpcion(o);
                        return;
                    }
                    if (clic && r.Contains(mouseVirtual) && !puede)
                    {
                        Mensaje("No tenés suficiente oro para eso.");
                        return;
                    }
                }
            }
            else if (evFase == 2)
            {
                if (Rb(new Rectangle(520, 600, 240, 50), "Continuar"))
                {
                    panelActivo = null;
                    evFase = 0;
                    if (colaEventos.Count > 0) MostrarSiguienteEvento();
                }
            }
        }
    }
}