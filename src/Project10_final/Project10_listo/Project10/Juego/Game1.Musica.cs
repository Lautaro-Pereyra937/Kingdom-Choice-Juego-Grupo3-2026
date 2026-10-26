using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Project10.Modelos;

namespace Project10
{
    // Música: una canción para los menús y otra para cuando estás jugando.
    // En Windows se reproduce con el reproductor del sistema (winmm/MCI), que no depende de OpenAL.
    // En otros sistemas se usa el audio de MonoGame. Los .wav están en la carpeta Audio junto al .exe.
    public partial class Game1
    {
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern int mciSendString(string comando, StringBuilder resultado, int largo, IntPtr callback);

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern bool mciGetErrorString(int error, StringBuilder texto, int largo);

        private const string AliasMenu = "kcmenu";
        private const string AliasJuego = "kcjuego";
        private const float VolumenMusica = 0.5f;

        private bool usarMci;
        private string aliasActual;
        private int contadorMusica;
        private string estadoMusica = "sin cargar";
        private string tituloPuesto = "";

        // Solo para el modo OpenAL (no Windows)
        private SoundEffect sonidoMenu, sonidoJuego;
        private SoundEffectInstance musicaMenu, musicaJuego, musicaActual;

        private string Mci(string comando, out int error)
        {
            StringBuilder sb = new StringBuilder(256);
            error = mciSendString(comando, sb, sb.Capacity, IntPtr.Zero);
            if (error != 0)
            {
                StringBuilder e = new StringBuilder(256);
                mciGetErrorString(error, e, e.Capacity);
                return e.ToString();
            }
            return sb.ToString();
        }

        private SoundEffectInstance CargarCancion(string ruta, out SoundEffect sonido)
        {
            using (FileStream fs = File.OpenRead(ruta))
                sonido = SoundEffect.FromStream(fs);
            SoundEffectInstance inst = sonido.CreateInstance();
            inst.IsLooped = true;
            return inst;
        }

        private void CargarMusica()
        {
            try
            {
                string dir = Path.Combine(AppContext.BaseDirectory, "Audio");
                string rutaMenu = Path.Combine(dir, "musica_menu.wav");
                string rutaJuego = Path.Combine(dir, "musica_juego.wav");
                if (!File.Exists(rutaMenu) || !File.Exists(rutaJuego))
                    throw new FileNotFoundException("No encuentro los .wav en " + dir);

                if (OperatingSystem.IsWindows())
                {
                    string r1 = Mci("open \"" + rutaMenu + "\" type waveaudio alias " + AliasMenu, out int e1);
                    string r2 = Mci("open \"" + rutaJuego + "\" type waveaudio alias " + AliasJuego, out int e2);
                    if (e1 != 0 || e2 != 0)
                        throw new Exception("MCI: " + (e1 != 0 ? r1 : r2));
                    usarMci = true;
                }
                else
                {
                    musicaMenu = CargarCancion(rutaMenu, out sonidoMenu);
                    musicaJuego = CargarCancion(rutaJuego, out sonidoJuego);
                }
                estadoMusica = "cargada OK (" + (usarMci ? "Windows" : "OpenAL") + ")";
            }
            catch (Exception ex)
            {
                usarMci = false;
                musicaMenu = null;
                musicaJuego = null;
                estadoMusica = "ERROR al cargar: " + ex.GetType().Name + " - " + ex.Message;
                try { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "musica_error.txt"), ex.ToString()); } catch (Exception) { }
            }
        }

        // Se llama en cada Update: cambia de canción solo cuando cambia de pantalla.
        private void ActualizarMusica()
        {
            bool enPartida = estadoActual == EstadoJuego.Jugando;
            string nombre = enPartida ? "JUEGO" : "MENU";

            if (usarMci) ActualizarMusicaMci(enPartida, nombre);
            else if (musicaMenu != null) ActualizarMusicaOpenAL(enPartida, nombre);

            PonerTituloMusica();
        }

        private void ActualizarMusicaMci(bool enPartida, string nombre)
        {
            string deseado = enPartida ? AliasJuego : AliasMenu;
            bool cambio = aliasActual != deseado;
            if (cambio)
            {
                if (aliasActual != null) Mci("stop " + aliasActual, out int _);
                aliasActual = deseado;
            }

            if (!sonidoActivo)
            {
                Mci("stop " + aliasActual, out int _);
                estadoMusica = "silenciada (Sonido: Desactivado)";
                return;
            }

            // Revisa varias veces por segundo si hay que (re)iniciar la canción: así hace el loop.
            contadorMusica++;
            if (!cambio && contadorMusica % 6 != 0 && estadoMusica.StartsWith("sonando")) return;

            string modo = Mci("status " + aliasActual + " mode", out int es);
            if (es != 0) { estadoMusica = "ERROR MCI (status): " + modo; return; }

            if (modo != "playing")
            {
                string r = Mci("play " + aliasActual + " from 0", out int ep);
                if (ep != 0) { estadoMusica = "ERROR MCI (play): " + r; return; }
            }
            estadoMusica = "sonando (Windows) musica de " + nombre;
        }

        private void ActualizarMusicaOpenAL(bool enPartida, string nombre)
        {
            SoundEffectInstance deseada = enPartida ? musicaJuego : musicaMenu;
            deseada.Volume = sonidoActivo ? VolumenMusica : 0f;

            if (deseada != musicaActual)
            {
                if (musicaActual != null) musicaActual.Stop();
                musicaActual = deseada;
            }
            if (musicaActual.State != SoundState.Playing)
            {
                try { musicaActual.Play(); }
                catch (Exception ex) { estadoMusica = "ERROR al reproducir: " + ex.Message; return; }
            }
            estadoMusica = "sonando (OpenAL) musica de " + nombre + " (" + musicaActual.State + ")";
        }

        // El título de la ventana muestra el estado de la música.
        private void PonerTituloMusica()
        {
            string t = "Project10 - VERSION CON MUSICA - " + estadoMusica;
            if (t != tituloPuesto) { Window.Title = t; tituloPuesto = t; }
        }
    }
}
