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
    // Torres, hospitales y centros militares: construir, mejorar y el bucle principal del mapa.
    public partial class Game1
    {
        // ------------------------------------------------------------------ TORRES


        // Rectángulo de la torre en coordenadas del mapa (para detectar el clic)
        private Rectangle AreaTorre(Torre t)
        {
            if (t.Tipo == TipoEdificio.Torre)
            {
                int w = (int)(150 * EscalaTorre);
                int h = (int)((256 - TopeSprite[t.Nivel - 1]) * EscalaTorre);
                return new Rectangle((int)t.Base.X - w / 2, (int)t.Base.Y - h, w, h);
            }
            int idx = IndiceSpriteEdif(t.Tipo, t.Nivel);
            int ww = (int)(EdifAncho[idx] * EscalaEdif);
            int hh = (int)((EdifBaseY - EdifTope[idx]) * EscalaEdif);
            return new Rectangle((int)t.Base.X - ww / 2, (int)t.Base.Y - hh, ww, hh);
        }


        private int IndiceSpriteEdif(TipoEdificio tipo, int nivel)
        {
            return (tipo == TipoEdificio.Hospital ? 0 : 3) + nivel - 1;
        }


        private float AlturaEdificio(Torre t)
        {
            if (t.Tipo == TipoEdificio.Torre) return (256 - TopeSprite[t.Nivel - 1]) * EscalaTorre;
            return (EdifBaseY - EdifTope[IndiceSpriteEdif(t.Tipo, t.Nivel)]) * EscalaEdif;
        }


        private void DibujarSpriteEdificio(TipoEdificio tipo, int nivel, Vector2 baseP, Color color)
        {
            if (tipo == TipoEdificio.Torre)
            {
                Vector2 ancla = new Vector2(texturasTorre[0].Width / 2f, texturasTorre[0].Height);
                _spriteBatch.Draw(texturasTorre[nivel - 1], baseP, null, color, 0f, ancla, EscalaTorre, SpriteEffects.None, 0f);
            }
            else
            {
                int idx = IndiceSpriteEdif(tipo, nivel);
                _spriteBatch.Draw(texturasEdif[idx], baseP, null, color, 0f, new Vector2(EdifLado / 2f, EdifBaseY), EscalaEdif, SpriteEffects.None, 0f);
            }
        }


        private int ContarEdificios(TipoEdificio tipo)
        {
            int n = 0;
            foreach (Torre t in torres) if (t.Tipo == tipo) n++;
            return n;
        }


        private int NivelTotal(TipoEdificio tipo)
        {
            int n = 0;
            foreach (Torre t in torres) if (t.Tipo == tipo) n += t.Nivel;
            return n;
        }


        private int MaxRef() { return BaseRefuerzos + 5 * NivelTotal(TipoEdificio.Cuartel); }


        private string DescEdificio(Torre t)
        {
            switch (t.Tipo)
            {
                case TipoEdificio.Hospital: return "+" + (2 * t.Nivel) + " de vida por turno y salva soldados";
                case TipoEdificio.Cuartel: return "+" + (3 * t.Nivel) + " soldados por turno, +" + (5 * t.Nivel) + " refuerzos máx.";
                default: return "Dispara " + t.Nivel + " de daño a cada enemigo";
            }
        }


        private int CostoSiguienteNivel(Torre t)
        {
            if (t.Nivel >= NivelMaxDe[(int)t.Tipo]) return 0;
            switch (t.Tipo)
            {
                case TipoEdificio.Hospital: return CostoMejoraHospital[t.Nivel - 1];
                case TipoEdificio.Cuartel: return CostoMejoraCuartel[t.Nivel - 1];
                default: return CostoMejora[t.Nivel - 1];
            }
        }


        // Rectángulo del cartel de mejora (en pantalla)
        private Rectangle AreaCartelTorre
        {
            get { return new Rectangle(VW / 2 - 175, VH - 270, 350, 210); }
        }


        // True si el mouse está sobre algún panel de la interfaz (ahí no se construye)
        private bool MouseSobreInterfaz()
        {
            Point m = mouseVirtual;
            if (new Rectangle(VW - 250, 100, 240, 390).Contains(m)) return true;
            if (new Rectangle(15, 10, 270, 440).Contains(m)) return true;
            if (new Rectangle(15, 455, 270, 205).Contains(m)) return true;
            if (new Rectangle(20, VH - 52, 900, 38).Contains(m)) return true;
            if (torreSeleccionada >= 0 && AreaCartelTorre.Contains(m)) return true;
            return false;
        }


        // No se puede construir muy pegado a otra torre
        private bool LugarLibre(Vector2 p)
        {
            foreach (Torre t in torres)
                if (Math.Abs(t.Base.X - p.X) < 100 && Math.Abs(t.Base.Y - p.Y) < 45) return false;
            return true;
        }


        private void MejorarTorre()
        {
            if (torreSeleccionada < 0) return;
            Torre t = torres[torreSeleccionada];
            Jugador j = partidaActual.datosJugador;


            if (t.Nivel >= NivelMaxDe[(int)t.Tipo]) { Mensaje("Este edificio ya está al nivel máximo."); return; }


            int costo = CostoSiguienteNivel(t);
            if (j.Oro < costo) { Mensaje("No tenés suficiente oro (necesitás " + costo + ")."); return; }


            j.Oro -= costo;
            t.Nivel++;
            Mensaje("¡" + NombreEdificio[(int)t.Tipo] + " mejorado al nivel " + t.Nivel + "!");
        }


        private void ActualizarTorres()
        {
            Jugador j = partidaActual.datosJugador;
            float mapaW = spriteMapa.texture.Width * spriteMapa.Escala;
            float mapaH = spriteMapa.texture.Height * spriteMapa.Escala;
            Vector2 mouseMapa = new Vector2(mouseVirtual.X + camara.X, mouseVirtual.Y + camara.Y);
            bool sobreUI = MouseSobreInterfaz();


            // Menú de construcción (izquierda): primero elegís QUÉ construir
            for (int i = 0; i < 3; i++)
            {
                Rectangle r = new Rectangle(25, 495 + i * 56, 250, 48);
                TipoEdificio tipoI = (TipoEdificio)i;
                bool lleno = ContarEdificios(tipoI) >= MaxPorTipo[i];
                bool puedeC = !lleno && j.Oro >= CostoConstruirDe[i];


                bool pulsado = Rb(r, NombreCorto[i] + " " + CostoConstruirDe[i], puedeC);
                BotonRect ult = botonesRect[botonesRect.Count - 1];
                if (i == 0)
                {
                    ult.Icono = texturasTorre[0];
                    ult.IconoSrc = new Rectangle(0, 0, 256, 256);
                    ult.IconoEscala = 40f / 256f;
                }
                else
                {
                    ult.Icono = texturasEdif[i == 1 ? 0 : 3];
                    ult.IconoSrc = new Rectangle(0, 0, EdifLado, EdifLado);
                    ult.IconoEscala = 40f / EdifLado;
                }
                ult.Extra = ContarEdificios(tipoI) + "/" + MaxPorTipo[i];
                ult.Resaltado = construirTipo == i;


                if (pulsado)
                {
                    construirTipo = (construirTipo == i) ? -1 : i;
                    torreSeleccionada = -1;
                    return;
                }
                if (clic && r.Contains(mouseVirtual))
                {
                    Mensaje(lleno ? "Ya tenés el máximo de " + NombrePlural[i] + "." : "No tenés suficiente oro (necesitás " + CostoConstruirDe[i] + ").");
                    return;
                }
            }


            // Edificio bajo el mouse
            torreEncima = -1;
            if (!sobreUI)
            {
                for (int i = torres.Count - 1; i >= 0; i--)
                    if (AreaTorre(torres[i]).Contains((int)mouseMapa.X, (int)mouseMapa.Y)) { torreEncima = i; break; }
            }


            // Cartel de mejora del edificio seleccionado
            if (torreSeleccionada >= 0)
            {
                Torre sel = torres[torreSeleccionada];
                Rectangle cartel = AreaCartelTorre;
                bool puede = sel.Nivel < NivelMaxDe[(int)sel.Tipo] && j.Oro >= CostoSiguienteNivel(sel);
                Boton mejorar = Colocar("mejorar", cartel.Center.X, cartel.Bottom - 32, 218, puede);


                if (Pulsado(mejorar)) { MejorarTorre(); return; }
                // clic en el botón apagado: explicar por qué no se puede
                if (clic && mejorar.Area.Contains(mouseVirtual) && !puede) { MejorarTorre(); return; }
            }


            // Clic derecho: cancelar construcción / soltar la selección
            if (mouseActual.RightButton == ButtonState.Pressed && mouseAnterior.RightButton == ButtonState.Released)
            {
                torreSeleccionada = -1;
                construirTipo = -1;
            }


            // Vista previa del edificio elegido
            mostrarPrevia = construirTipo >= 0 && !sobreUI && torreEncima < 0;
            posPrevia = new Vector2(MathHelper.Clamp(mouseMapa.X, 0, mapaW), MathHelper.Clamp(mouseMapa.Y, 0, mapaH));
            previaValida = construirTipo >= 0 && LugarLibre(posPrevia) && j.Oro >= CostoConstruirDe[construirTipo];


            if (!clic || sobreUI) return;


            if (torreEncima >= 0)
            {
                torreSeleccionada = torreEncima;          // seleccionar un edificio existente
                construirTipo = -1;
            }
            else if (construirTipo >= 0)
            {
                if (!LugarLibre(posPrevia))
                {
                    Mensaje("Hay otro edificio demasiado cerca.");
                }
                else if (j.Oro < CostoConstruirDe[construirTipo])
                {
                    Mensaje("No tenés suficiente oro (necesitás " + CostoConstruirDe[construirTipo] + ").");
                }
                else if (ContarEdificios((TipoEdificio)construirTipo) >= MaxPorTipo[construirTipo])
                {
                    Mensaje("Ya tenés el máximo de " + NombrePlural[construirTipo] + ".");
                }
                else
                {
                    j.Oro -= CostoConstruirDe[construirTipo];
                    torres.Add(new Torre(posPrevia, 1, (TipoEdificio)construirTipo));
                    Mensaje("¡" + NombreEdificio[construirTipo] + " construido!");
                    construirTipo = -1;
                }
            }
            else if (torreSeleccionada >= 0)
            {
                torreSeleccionada = -1;                   // clic en el suelo: solo deselecciona
            }
        }


        // Anillo dorado (elipse) que se dibuja bajo la torre seleccionada
        private Texture2D CrearAnillo(int w, int h)
        {
            Texture2D t = new Texture2D(GraphicsDevice, w, h);
            Color[] datos = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + 0.5f - w / 2f) / (w / 2f);
                    float ny = (y + 0.5f - h / 2f) / (h / 2f);
                    float r = (float)Math.Sqrt(nx * nx + ny * ny);
                    float a = 0f;
                    if (r <= 1f) a = r > 0.86f ? 0.95f : 0.18f;
                    datos[y * w + x] = Color.Gold * a;   // alfa premultiplicado
                }
            }
            t.SetData(datos);
            return t;
        }


        private void ActualizarJuego(float dt, bool esc)
        {
            if (esc)
            {
                if (panelActivo == "Oleada" || panelActivo == "Evento") return;   // no se puede escapar
                if (panelActivo != null) { panelActivo = null; pistaSeleccionada = -1; }
                else if (torreSeleccionada >= 0) torreSeleccionada = -1;
                else if (construirTipo >= 0) construirTipo = -1;
                else CambiarEstado(EstadoJuego.MenuPrincipal);
                return;
            }


            mostrarPrevia = false;
            torreEncima = -1;


            // Oleada de enemigos en curso
            if (panelActivo == "Oleada")
            {
                ActualizarOleada(dt);
                return;
            }


            // Decisión (evento) en curso
            if (panelActivo == "Evento")
            {
                ActualizarEvento();
                return;
            }


            // Ventana abierta (Inventario, Personajes, Diario, Controles)
            if (panelActivo != null)
            {
                Boton volver = Colocar("volver", VW / 2f, 570, 220);
                if (panelActivo == "Inventario") ActualizarInventario();
                if (panelActivo == "Diario")
                {
                    string[] ids = { "pista1", "pista2", "pista3", "pista4" };
                    for (int i = 0; i < ids.Length; i++)
                    {
                        Boton p = Colocar(ids[i], 340 + i * 200, 480, 170);
                        if (Pulsado(p)) pistaSeleccionada = i;
                    }
                }
                if (Pulsado(volver)) { panelActivo = null; pistaSeleccionada = -1; }
                return;
            }


            // Cámara (WASD o flechas)
            Vector2 dir = Vector2.Zero;
            if (tecladoActual.IsKeyDown(Keys.W) || tecladoActual.IsKeyDown(Keys.Up)) dir.Y -= 1;
            if (tecladoActual.IsKeyDown(Keys.S) || tecladoActual.IsKeyDown(Keys.Down)) dir.Y += 1;
            if (tecladoActual.IsKeyDown(Keys.A) || tecladoActual.IsKeyDown(Keys.Left)) dir.X -= 1;
            if (tecladoActual.IsKeyDown(Keys.D) || tecladoActual.IsKeyDown(Keys.Right)) dir.X += 1;
            if (dir != Vector2.Zero)
            {
                dir.Normalize();
                camara += dir * VelocidadCamara * dt;
            }
            float maxX = Math.Max(0, spriteMapa.texture.Width * spriteMapa.Escala - VW);
            float maxY = Math.Max(0, spriteMapa.texture.Height * spriteMapa.Escala - VH);
            camara.X = MathHelper.Clamp(camara.X, 0, maxX);
            camara.Y = MathHelper.Clamp(camara.Y, 0, maxY);


            // Pasar de turno
            if (Presionada(Keys.Space))
            {
                partidaActual.Turno++;
                partidaActual.datosJugador.Oro += 10;
                // Efectos de los edificios en cada turno
                partidaActual.datosJugador.Ejercito += 2 + 3 * NivelTotal(TipoEdificio.Cuartel);
                vidaImperio = Math.Min(VidaMax, vidaImperio + 2 * NivelTotal(TipoEdificio.Hospital));


                GenerarEvento();
                SincronizarBD();   // <-- BASE DE DATOS
                if (partidaActual.Turno % IntervaloOleada == 0)
                {
                    IniciarOleada();   // las decisiones pendientes se muestran después de la batalla
                    return;
                }
                if (colaEventos.Count > 0)
                {
                    MostrarSiguienteEvento();
                    return;
                }
            }


            // Botones laterales
            string[] lateral = { "inventario", "personajes", "diario", "controles", "guardar_partida", "borrar_partida", "salir" };
            for (int i = 0; i < lateral.Length; i++)
            {
                Boton b = Colocar(lateral[i], VW - 130, 140 + i * 52, 210);
                if (!Pulsado(b)) continue;


                switch (lateral[i])
                {
                    case "inventario": panelActivo = "Inventario"; break;
                    case "personajes": panelActivo = "Personajes"; break;
                    case "diario": panelActivo = "Diario"; break;
                    case "controles": panelActivo = "Controles"; break;
                    case "guardar_partida": Guardar(); break;
                    case "borrar_partida": Borrar(); break;
                    case "salir": CambiarEstado(EstadoJuego.MenuPrincipal); return;
                }
                if (panelActivo != null) visibles.Clear();
                return;
            }


            // Construir / seleccionar / mejorar torres (clic sobre el mapa)
            ActualizarTorres();
        }
    }
}