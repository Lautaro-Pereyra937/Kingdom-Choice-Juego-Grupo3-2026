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
    // Oleadas de enemigos, mochila (pila) y batalla por turnos (colas).
    public partial class Game1
    {
        // ------------------------------------------------------------------ OLEADAS, MOCHILA


        private void ReiniciarMundo()
        {
            vidaImperio = VidaMax;
            espadas = 0; escudos = 0; arcos = 0;
            oleadaNum = 0; oleadaFase = 0;
            mochila.Clear();
            colaEventos.Clear();
            construirTipo = -1;
            evFase = 0;
            colaEnemigos.Clear();
            colaRefuerzos.Clear();
            logBatalla.Clear();
        }


        // Botón rectangular simple (se dibuja en DibujarBotonRect). Devuelve true si le hicieron clic.
        private bool Rb(Rectangle r, string texto, bool activo = true)
        {
            BotonRect b = new BotonRect();
            b.R = r;
            b.Texto = texto;
            b.Activo = activo;
            b.Encima = r.Contains(mouseVirtual);
            botonesRect.Add(b);
            return clic && activo && b.Encima;
        }


        private void DibujarBotonRect(BotonRect b)
        {
            Color fondo = !b.Activo ? new Color(60, 60, 60) : (b.Encima ? new Color(160, 115, 30) : new Color(105, 72, 22));
            Color borde = b.Activo ? Color.Gold : Color.Gray;
            if (b.Resaltado) _spriteBatch.Draw(pixel, new Rectangle(b.R.X - 3, b.R.Y - 3, b.R.Width + 6, b.R.Height + 6), Color.LimeGreen);
            _spriteBatch.Draw(pixel, b.R, borde);
            _spriteBatch.Draw(pixel, new Rectangle(b.R.X + 2, b.R.Y + 2, b.R.Width - 4, b.R.Height - 4), fondo);
            if (b.Icono != null)
                _spriteBatch.Draw(b.Icono, new Vector2(b.R.X + 6, b.R.Y + 4), b.IconoSrc, b.Activo ? Color.White : Color.Gray,
                    0f, Vector2.Zero, b.IconoEscala, SpriteEffects.None, 0f);
            Vector2 m = FuenteJuego.MeasureString(b.Texto) * 0.7f;
            Texto(b.Texto, new Vector2(b.R.Center.X - m.X / 2f + (b.Icono != null ? 12 : 0), b.R.Center.Y - m.Y / 2f), b.Activo ? Color.White : Color.LightGray, 0.7f);
            if (b.Extra != null) Texto(b.Extra, new Vector2(b.R.Right - 44, b.R.Y + 4), Color.LightGray, 0.5f);
        }


        private int AtaqueSoldado() { return 3 + espadas * 2 + arcos; }


        private int DanoTorres()
        {
            int total = 0;
            foreach (Torre t in torres) total += t.Nivel;
            return total;
        }


        private void Log(string texto)
        {
            logBatalla.Add(texto);
            if (logBatalla.Count > 40) logBatalla.RemoveAt(0);
        }


        private void EquiparItem(TipoItem t)
        {
            switch (t)
            {
                case TipoItem.Espada: espadas++; Mensaje("Espada equipada: +2 de ataque a los soldados."); break;
                case TipoItem.Escudo: escudos++; Mensaje("Escudo equipado: los soldados reciben -1 de daño."); break;
                case TipoItem.Arco: arcos++; Mensaje("Arco equipado: +1 de ataque a los soldados."); break;
                case TipoItem.Pocion:
                    vidaImperio = Math.Min(VidaMax, vidaImperio + 30);
                    Mensaje("Poción usada: +30 de vida al imperio.");
                    break;
            }
        }


        private void ActualizarInventario()
        {
            Jugador j = partidaActual.datosJugador;


            // Tienda: comprar = apilar en la mochila (Push)
            for (int i = 0; i < 4; i++)
            {
                Rectangle r = new Rectangle(515, 215 + i * 78 + 14, 115, 40);
                bool puede = j.Oro >= PrecioItem[i] && mochila.Count < MaxMochila;
                if (Rb(r, "Comprar " + PrecioItem[i], puede))
                {
                    j.Oro -= PrecioItem[i];
                    mochila.Push((TipoItem)i);
                    Mensaje(NombreItem[i] + " guardada en la mochila.");
                }
                else if (clic && r.Contains(mouseVirtual) && !puede)
                {
                    Mensaje(mochila.Count >= MaxMochila ? "La mochila está llena." : "No tenés suficiente oro.");
                }
            }


            // Equipar el objeto de arriba de la pila (Pop)
            if (Rb(new Rectangle(690, 320, 190, 44), "Equipar tope", mochila.Count > 0))
            {
                TipoItem sacado = mochila.Pop();
                EquiparItem(sacado);
            }
        }


        private void IniciarOleada()
        {
            BaseDatos.RegistrarEvento(partidaActual.Turno, "Oleada enemiga");   // <-- BASE DE DATOS
            oleadaNum++;
            colaEnemigos.Clear();
            colaRefuerzos.Clear();
            int cantidad = 3 + oleadaNum * 2;
            for (int i = 0; i < cantidad; i++) colaEnemigos.Enqueue(new Enemigo(oleadaNum));   // Enqueue: se suman al final de la fila


            refuerzosElegidos = Math.Min(10, Math.Min(MaxRef(), partidaActual.datosJugador.Ejercito));
            oleadaFase = 1;
            panelActivo = "Oleada";
            construirTipo = -1;
            pistaSeleccionada = -1;
            torreSeleccionada = -1;
            logBatalla.Clear();
        }


        private void EmpezarBatalla()
        {
            Jugador j = partidaActual.datosJugador;
            int n = Math.Min(refuerzosElegidos, j.Ejercito);
            j.Ejercito -= n;
            for (int i = 0; i < n; i++) colaRefuerzos.Enqueue(new Soldado(AtaqueSoldado()));
            refuerzosEnviados = n;
            enemigosDerrotados = 0; bajas = 0; danoCastillo = 0; botinOro = 0; botinItems = 0; salvadosHospital = 0;
            logBatalla.Clear();
            pasoTimer = 0f;
            oleadaFase = 2;
            Log("¡Comienza la batalla!");
        }


        // Un paso de la batalla: siempre pelean el primero de cada fila (Peek) y el que cae sale de la fila (Dequeue)
        private void PasoBatalla()
        {
            if (colaEnemigos.Count == 0) { TerminarBatalla(); return; }
            Enemigo e = colaEnemigos.Peek();


            if (!e.Atacado)
            {
                e.Atacado = true;
                int d = DanoTorres();
                if (d > 0)
                {
                    e.Vida -= d;
                    golpeTimer = 0.3f;
                    Log("Las torres disparan: -" + d + " al enemigo.");
                }
            }
            else if (colaRefuerzos.Count > 0)
            {
                Soldado s = colaRefuerzos.Peek();
                int danoSoldado = Math.Max(1, e.Ataque - escudos);
                e.Vida -= s.Ataque;
                s.Vida -= danoSoldado;
                golpeTimer = 0.3f;
                Log("Golpe: enemigo -" + s.Ataque + ", soldado -" + danoSoldado + ".");
                if (s.Vida <= 0)
                {
                    colaRefuerzos.Dequeue();
                    bajas++;
                    Log("Cae un soldado.");
                }
            }
            else
            {
                colaEnemigos.Dequeue();
                int dano = e.Ataque * 2;
                vidaImperio -= dano;
                danoCastillo += dano;
                Log("¡Un enemigo llegó al castillo! -" + dano + " de vida.");
                if (vidaImperio <= 0) { vidaImperio = 0; TerminarBatalla(); return; }
                if (colaEnemigos.Count == 0) TerminarBatalla();
                return;
            }


            if (e.Vida <= 0 && colaEnemigos.Count > 0 && ReferenceEquals(colaEnemigos.Peek(), e))
            {
                colaEnemigos.Dequeue();
                enemigosDerrotados++;
                botinOro += 10;
                Log("Enemigo derrotado (+10 de oro).");
                if (azar.Next(100) < 20 && mochila.Count < MaxMochila)
                {
                    TipoItem it = (TipoItem)azar.Next(4);
                    mochila.Push(it);
                    botinItems++;
                    Log("¡Botín: " + NombreItem[(int)it] + "!");
                }
            }


            if (colaEnemigos.Count == 0) TerminarBatalla();
        }


        private void TerminarBatalla()
        {
            Jugador j = partidaActual.datosJugador;
            j.Ejercito += colaRefuerzos.Count;      // los soldados que sobreviven vuelven al ejército
            colaRefuerzos.Clear();
            colaEnemigos.Clear();
            j.Oro += botinOro;
            if (vidaImperio > 0) j.Reputacion += 5;


            // El hospital salva a una parte de los soldados caídos
            int nivelHosp = NivelTotal(TipoEdificio.Hospital);
            if (nivelHosp > 0 && bajas > 0)
            {
                double tasa = Math.Min(0.6, 0.15 * nivelHosp);
                salvadosHospital = (int)(bajas * tasa);
                j.Ejercito += salvadosHospital;
            }
            oleadaFase = 3;
        }


        private void CerrarOleada()
        {
            oleadaFase = 0;
            panelActivo = null;
            if (vidaImperio <= 0)
            {
                BaseDatos.FinalizarSesion("derrota", partidaActual.Turno, partidaActual.datosJugador.Oro,
                    partidaActual.datosJugador.Ejercito, partidaActual.datosJugador.Reputacion, 0);   // <-- BASE DE DATOS
                CambiarEstado(EstadoJuego.MenuPrincipal);
                Mensaje("Tu imperio cayó... Empezá una partida nueva.");
            }
            else
            {
                SincronizarBD();   // <-- BASE DE DATOS: botín, reputación y bajas de la batalla
                if (colaEventos.Count > 0) MostrarSiguienteEvento();
            }
        }


        private void ActualizarOleada(float dt)
        {
            Jugador j = partidaActual.datosJugador;
            if (golpeTimer > 0) golpeTimer -= dt;


            if (oleadaFase == 1)
            {
                int max = Math.Min(MaxRef(), j.Ejercito);
                if (Rb(new Rectangle(445, 515, 70, 44), "-5")) refuerzosElegidos -= 5;
                if (Rb(new Rectangle(525, 515, 70, 44), "-1")) refuerzosElegidos -= 1;
                if (Rb(new Rectangle(605, 515, 70, 44), "+1")) refuerzosElegidos += 1;
                if (Rb(new Rectangle(685, 515, 70, 44), "+5")) refuerzosElegidos += 5;
                if (Rb(new Rectangle(765, 515, 70, 44), "Máx")) refuerzosElegidos = max;
                refuerzosElegidos = MathHelper.Clamp(refuerzosElegidos, 0, max);


                if (Rb(new Rectangle(520, 590, 240, 52), "¡Defender!")) EmpezarBatalla();
            }
            else if (oleadaFase == 2)
            {
                if (Rb(new Rectangle(540, 610, 200, 40), "Saltar batalla"))
                {
                    int seguridad = 0;
                    while (oleadaFase == 2 && seguridad++ < 5000) PasoBatalla();
                    return;
                }
                pasoTimer += dt;
                if (pasoTimer >= 0.45f)
                {
                    pasoTimer = 0f;
                    PasoBatalla();
                }
            }
            else if (oleadaFase == 3)
            {
                if (Rb(new Rectangle(520, 610, 240, 44), "Aceptar")) CerrarOleada();
            }
        }
    }
}