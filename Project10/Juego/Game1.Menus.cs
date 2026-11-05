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
    // Pantallas de menu: nombre del rey, menu principal, opciones y creditos.
    public partial class Game1
    {
        private void ActualizarNombre(bool esc)
        {
            if (esc) { Exit(); return; }


            Boton ok = Colocar("confirmar", VW / 2f, 470, 260, NombreRey.Trim().Length > 0);
            if (Pulsado(ok)) IrAlMenu();
        }


        private void ActualizarMenu(bool esc)
        {
            // Después de "Partida nueva": elegir el modo de juego
            if (eligiendoModo)
            {
                if (esc) { eligiendoModo = false; return; }
                if (Rb(new Rectangle(340, 260, 600, 60), "Modo Historia")) { modoHistoria = true; eligiendoModo = false; NuevaPartida(); return; }
                if (Rb(new Rectangle(340, 370, 600, 60), "Modo Infinito")) { modoHistoria = false; eligiendoModo = false; NuevaPartida(); return; }
                if (Rb(new Rectangle(490, 500, 300, 46), "Volver")) eligiendoModo = false;
                return;
            }

            if (esc) { Exit(); return; }


            string[] ids = { "partida_nueva", "continuar", "opciones", "creditos", "salir" };
            for (int i = 0; i < ids.Length; i++)
            {
                Boton b = Colocar(ids[i], VW / 2f, 250 + i * 76, 340);
                if (!Pulsado(b)) continue;


                switch (ids[i])
                {
                    case "partida_nueva":
                        eligiendoModo = true;   // primero se elige Historia o Infinito
                        break;
                    case "continuar":
                        if (Cargar())
                        {
                            BaseDatos.IniciarSesion(NombreRey, true);   // <-- BASE DE DATOS: retoma la partida en curso
                            SincronizarBD();
                            EmpezarMapa();
                            Mensaje(BaseDatos.Ultimo);
                        }
                        else Mensaje("No hay ninguna partida guardada.");
                        break;
                    case "opciones": CambiarEstado(EstadoJuego.Opciones); break;
                    case "creditos": CambiarEstado(EstadoJuego.Creditos); break;
                    case "salir": Exit(); break;
                }
                return;
            }
        }


        private void NuevaPartida()
        {
            Jugador nuevoJugador = new Jugador(NombreRey, 100, 50, 200);
            partidaActual = new Partida("Nueva Partida", 1, nuevoJugador);
            BaseDatos.IniciarSesion(NombreRey);   // <-- BASE DE DATOS
            mapaActual = azar.Next(texturasMapas.Length); // mapa al azar
            torresGuardadas = null;
            ReiniciarMundo();
            EmpezarMapa();
            Mensaje(modoHistoria ? "Modo Historia: sobreviví hasta el turno " + TurnoFinalHistoria + "." : "Modo Infinito: sin final, ¡aguantá todo lo que puedas!");
        }


        private void ActualizarOpciones(bool esc)
        {
            if (esc) { CambiarEstado(EstadoJuego.MenuPrincipal); return; }


            string[] ids = { "sonido", "resolucion", "idioma", "adquirir_musica", "creditos", "volver" };
            for (int i = 0; i < ids.Length; i++)
            {
                Boton b = Colocar(ids[i], 400, 200 + i * 72, 300);
                if (!Pulsado(b)) continue;


                switch (ids[i])
                {
                    case "sonido":
                        sonidoActivo = !sonidoActivo;
                        GuardarConfiguracion();
                        break;
                    case "resolucion":
                        indiceResolucion = (indiceResolucion + 1) % resoluciones.Length;
                        _graphics.PreferredBackBufferWidth = resoluciones[indiceResolucion][0];
                        _graphics.PreferredBackBufferHeight = resoluciones[indiceResolucion][1];
                        _graphics.ApplyChanges();
                        GuardarConfiguracion();
                        break;
                    case "idioma":
                        Mensaje("Por ahora el juego solo está en español.");
                        break;
                    case "adquirir_musica":
                        Mensaje("Próximamente.");
                        break;
                    case "creditos":
                        CambiarEstado(EstadoJuego.Creditos);
                        return;
                    case "volver":
                        CambiarEstado(EstadoJuego.MenuPrincipal);
                        return;
                }
                return;
            }
        }


        private void ActualizarCreditos(bool esc)
        {
            if (esc) { CambiarEstado(EstadoJuego.MenuPrincipal); return; }


            Boton v = Colocar("volver", VW / 2f, 590, 240);
            if (Pulsado(v)) CambiarEstado(EstadoJuego.MenuPrincipal);
        }


        private void EmpezarMapa()
        {
            CambiarEstado(EstadoJuego.Jugando);
            spriteMapa.texture = texturasMapas[mapaActual];
            float mapaW = spriteMapa.texture.Width * spriteMapa.Escala;
            float mapaH = spriteMapa.texture.Height * spriteMapa.Escala;
            camara = new Vector2(Math.Max(0, (mapaW - VW) / 2f), Math.Max(0, (mapaH - VH) / 2f));


            // El mapa arranca vacío (o con las torres de la partida guardada)
            torres.Clear();
            if (torresGuardadas != null) torres.AddRange(torresGuardadas);
            torresGuardadas = null;
        }
    }
}