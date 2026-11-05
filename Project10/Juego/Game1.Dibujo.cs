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
    // Todo lo que se dibuja en pantalla (Draw y ayudas de dibujo).
    public partial class Game1
    {


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);


            Viewport vp = GraphicsDevice.Viewport;
            Matrix pantalla = Matrix.CreateScale(vp.Width / (float)VW, vp.Height / (float)VH, 1f);


            // Mapa (con cámara)
            if (estadoActual == EstadoJuego.Jugando)
            {
                _spriteBatch.Begin(transformMatrix: Matrix.CreateTranslation(-camara.X, -camara.Y, 0f) * pantalla);
                spriteMapa.Draw(_spriteBatch);
                DibujarTorres();
                DibujarOleadaMapa();
                _spriteBatch.End();
            }


            // Todo lo demás (interfaz)
            _spriteBatch.Begin(transformMatrix: pantalla);


            switch (estadoActual)
            {
                case EstadoJuego.IngresandoNombre: DibujarNombre(); break;
                case EstadoJuego.MenuPrincipal: DibujarMenu(); break;
                case EstadoJuego.Opciones: DibujarOpciones(); break;
                case EstadoJuego.Creditos: DibujarCreditos(); break;
                case EstadoJuego.Jugando: DibujarJuego(); break;
            }


            foreach (Boton b in visibles) b.Dibujar(_spriteBatch);
            foreach (BotonRect br in botonesRect) DibujarBotonRect(br);


            if (mensajeTiempo > 0 && mensaje != null)
            {
                Vector2 m = FuenteJuego.MeasureString(mensaje) * 0.8f;
                Panel(new Rectangle((int)(VW / 2f - m.X / 2f - 18), 14, (int)m.X + 36, 46), 0.8f);
                TextoCentrado(mensaje, VW / 2f, 22, Color.White, 0.8f);
            }


            // Estado de la música 
            Panel(new Rectangle(0, 0, VW, 34), 0.9f);
            Texto("MUSICA: " + estadoMusica, new Vector2(10, 4), Color.Yellow, 0.7f);

            _spriteBatch.End();
            base.Draw(gameTime);
        }


        // Se dibuja con la cámara del mapa ya aplicada
        private void DibujarTorres()
        {
            Jugador j = partidaActual.datosJugador;


            // Vista previa del edificio que se está por construir
            if (mostrarPrevia && panelActivo == null && construirTipo >= 0)
            {
                Color c = previaValida ? Color.White * 0.5f : Color.Red * 0.4f;
                DibujarSpriteEdificio((TipoEdificio)construirTipo, 1, posPrevia, c);
            }


            // De atrás hacia adelante los de más abajo tapan a los de más arriba
            foreach (int i in Enumerable.Range(0, torres.Count).OrderBy(k => torres[k].Base.Y))
            {
                Torre t = torres[i];


                if (i == torreSeleccionada || i == torreEncima)
                {
                    Color cr = i == torreSeleccionada ? Color.White : Color.White * 0.5f;
                    _spriteBatch.Draw(anillo, t.Base + new Vector2(0, -6), null, cr, 0f,
                        new Vector2(anillo.Width / 2f, anillo.Height / 2f), 1f, SpriteEffects.None, 0f);
                }


                DibujarSpriteEdificio(t.Tipo, t.Nivel, t.Base, Color.White);
                if (t.Tipo == TipoEdificio.Mina) DibujarMineros(t);
                if (t.Tipo == TipoEdificio.Torre) { DibujarVigias(t); DibujarGeneralArquero(t); }
                if (t.Tipo == TipoEdificio.Cuartel) { DibujarGuardias(t); DibujarGeneralEspada(t); }


                // Flechita que salta cuando alcanza el oro para mejorarlo
                if (t.Nivel < NivelMaxDe[(int)t.Tipo] && j.Oro >= CostoSiguienteNivel(t))
                {
                    float salto = (float)Math.Sin(tiempo * 4) * 4f;
                    float y = t.Base.Y - AlturaEdificio(t) - 26 + salto;
                    _spriteBatch.Draw(iconoMejora, new Vector2(t.Base.X, y), null, Color.White, 0f,
                        new Vector2(iconoMejora.Width / 2f, iconoMejora.Height / 2f), 0.6f, SpriteEffects.None, 0f);
                }
            }

            // Animacion de subida de nivel: arriba de todos los edificios
            DibujarEfectos();
        }


        // General Arquero: UNO solo por torre (sin importar el nivel). Patrulla de un lado a otro delante de la torre.
        private void DibujarGeneralArquero(Torre t)
        {
            Vector2 origen = new Vector2(spriteGeneralArquero.Width / 2f, spriteGeneralArquero.Height - 1);
            float fase = (float)tiempo * 0.9f + t.Base.X * 0.013f;     // cada torre camina desfasada
            float x = t.Base.X + (float)Math.Sin(fase) * 62f;
            float y = t.Base.Y + 14f - Math.Abs((float)Math.Sin(fase * 6f)) * 3f;   // pequeños saltitos al caminar
            SpriteEffects giro = Math.Cos(fase) >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            _spriteBatch.Draw(spriteGeneralArquero, new Vector2(x, y), null, Color.White, 0f, origen, 0.8f, giro, 0f);
        }


        // Personajes normales de la torre: 2 por cada mejora (nivel 1 = ninguno, nivel 5 = 8)
        private void DibujarVigias(Torre t)
        {
            int cantidad = 2 * (t.Nivel - 1);
            Vector2 origen = new Vector2(spriteVigia.Width / 2f, spriteVigia.Height - 1);
            for (int k = 0; k < cantidad; k++)
            {
                float fase = (float)tiempo * (0.7f + 0.15f * (k % 3)) + k * 1.9f + t.Base.X * 0.011f;
                float x = t.Base.X + (float)Math.Sin(fase) * (40f + (k % 4) * 14f);
                float y = t.Base.Y + 6f + (k % 3) * 9f - Math.Abs((float)Math.Sin(fase * 5f)) * 3f;
                SpriteEffects giro = Math.Cos(fase) >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                _spriteBatch.Draw(spriteVigia, new Vector2(x, y), null, Color.White, 0f, origen, 0.8f, giro, 0f);
            }
        }


        // General Espadachín: UNO solo por centro militar. Patrulla delante del edificio.
        private void DibujarGeneralEspada(Torre t)
        {
            Vector2 origen = new Vector2(spriteGeneralEspada.Width / 2f, spriteGeneralEspada.Height - 1);
            float fase = (float)tiempo * 0.8f + t.Base.X * 0.012f;
            float x = t.Base.X + (float)Math.Sin(fase) * 70f;
            float y = t.Base.Y + 16f - Math.Abs((float)Math.Sin(fase * 6f)) * 3f;
            SpriteEffects giro = Math.Cos(fase) >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            _spriteBatch.Draw(spriteGeneralEspada, new Vector2(x, y), null, Color.White, 0f, origen, 0.8f, giro, 0f);
        }


        // Guardias normales del centro militar: 2 por cada mejora (nivel 1 = ninguno, nivel 3 = 4)
        private void DibujarGuardias(Torre t)
        {
            int cantidad = 2 * (t.Nivel - 1);
            Vector2 origen = new Vector2(spriteGuardia.Width / 2f, spriteGuardia.Height - 1);
            for (int k = 0; k < cantidad; k++)
            {
                float fase = (float)tiempo * (0.7f + 0.15f * (k % 3)) + k * 1.9f + t.Base.X * 0.011f;
                float x = t.Base.X + (float)Math.Sin(fase) * (45f + (k % 4) * 14f);
                float y = t.Base.Y + 6f + (k % 3) * 9f - Math.Abs((float)Math.Sin(fase * 5f)) * 3f;
                SpriteEffects giro = Math.Cos(fase) >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                _spriteBatch.Draw(spriteGuardia, new Vector2(x, y), null, Color.White, 0f, origen, 0.8f, giro, 0f);
            }
        }


        // General Minero: UNO solo por mina (el sprite grande del casco naranja) + 2 mineros normales por cada mejora.
        private void DibujarMineros(Torre t)
        {
            // Mineros normales (nivel 1 = ninguno, nivel 4 = 6)
            int cantidad = 2 * (t.Nivel - 1);
            Vector2 origenN = new Vector2(spriteMineroNormal.Width / 2f, spriteMineroNormal.Height - 1);
            for (int k = 0; k < cantidad; k++)
            {
                float fase = (float)tiempo * (1.6f + 0.2f * (k % 3)) + k * 2.1f + t.Base.X * 0.01f;
                float x = t.Base.X + (float)Math.Sin(fase) * (38f + (k % 4) * 14f);
                float y = t.Base.Y + 6f + (k % 3) * 9f - Math.Abs((float)Math.Sin(fase * 2.5f)) * 3f;
                SpriteEffects giroN = Math.Cos(fase) >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                _spriteBatch.Draw(spriteMineroNormal, new Vector2(x, y), null, Color.White, 0f, origenN, 0.8f, giroN, 0f);
            }

            // General Minero (siempre 1)
            Vector2 origen = new Vector2(spriteMinero.Width / 2f, spriteMinero.Height - 2);
            float faseG = (float)tiempo * 1.2f + t.Base.X * 0.01f;
            float xg = t.Base.X + (float)Math.Sin(faseG) * 60f;
            float yg = t.Base.Y + 16f - Math.Abs((float)Math.Sin(faseG * 2.5f)) * 3f;
            SpriteEffects giro = Math.Cos(faseG) >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            _spriteBatch.Draw(spriteMinero, new Vector2(xg, yg), null, Color.White, 0f, origen, 0.9f, giro, 0f);
        }


        private void DibujarCartelTorre()
        {
            Torre t = torres[torreSeleccionada];
            Rectangle c = AreaCartelTorre;
            float cx = c.Center.X;
            int nivMax = NivelMaxDe[(int)t.Tipo];


            Panel(c, 0.78f);
            TextoCentrado(NombreEdificio[(int)t.Tipo] + " - Nivel " + t.Nivel, cx, c.Y + 10, Color.Gold, 0.9f);
            TextoCentrado(DescEdificio(t), cx, c.Y + 42, Color.LightGray, 0.5f);


            for (int i = 0; i < nivMax; i++)
            {
                Texture2D e = i < t.Nivel ? estrellaLlena : estrellaVacia;
                _spriteBatch.Draw(e, new Vector2(cx + (i - (nivMax - 1) / 2f) * 36, c.Y + 88), null, Color.White, 0f,
                    new Vector2(e.Width / 2f, e.Height / 2f), 0.7f, SpriteEffects.None, 0f);
            }


            if (t.Nivel < nivMax)
            {
                TextoCentrado("Hasta el nivel máximo: " + CostoHastaMaximo(t.Tipo, t.Nivel) + " de oro en total", cx, c.Y + 58, Color.Gold, 0.5f);
                string txt = "Costo: " + CostoSiguienteNivel(t);
                Vector2 m = FuenteJuego.MeasureString(txt) * 0.8f;
                float ancho = 32 + 8 + m.X;
                float x0 = cx - ancho / 2f;
                bool alcanza = partidaActual.datosJugador.Oro >= CostoSiguienteNivel(t);
                _spriteBatch.Draw(iconoOro, new Vector2(x0 + 16, c.Y + 124), null, Color.White, 0f,
                    new Vector2(iconoOro.Width / 2f, iconoOro.Height / 2f), 0.5f, SpriteEffects.None, 0f);
                Texto(txt, new Vector2(x0 + 40, c.Y + 124 - m.Y / 2f), alcanza ? Color.White : Color.IndianRed, 0.8f);
            }
            else
            {
                TextoCentrado("Nivel máximo", cx, c.Y + 112, Color.LightGreen, 0.8f);
            }
        }


        private void DibujarInventario()
        {
            // Tienda (izquierda)
            TextoCentrado("TIENDA", 450, 172, Color.Gold, 0.85f);
            for (int i = 0; i < 4; i++)
            {
                int y = 215 + i * 78;
                Panel(new Rectangle(260, y, 380, 68), 0.45f);
                _spriteBatch.Draw(spritesItem[i], new Vector2(266, y + 2), Color.White);
                Texto(NombreItem[i], new Vector2(340, y + 8), Color.White, 0.75f);
                Texto(DescItem[i], new Vector2(340, y + 38), Color.LightGray, 0.5f);
            }


            // Mochila (derecha): es una pila
            Panel(new Rectangle(660, 170, 360, 370), 0.45f);
            TextoCentrado("MOCHILA (pila)", 840, 176, Color.Gold, 0.85f);
            Texto("Objetos: " + mochila.Count + "/" + MaxMochila, new Vector2(680, 212), Color.LightGray, 0.6f);


            if (mochila.Count > 0)
            {
                TipoItem tope = mochila.Peek();
                _spriteBatch.Draw(spritesItem[(int)tope], new Vector2(680, 240), Color.White);
                Texto("Tope: " + NombreItem[(int)tope], new Vector2(760, 250), Color.White, 0.75f);
                Texto(DescItem[(int)tope], new Vector2(760, 282), Color.LightGray, 0.5f);
            }
            else
            {
                Texto("La mochila está vacía.", new Vector2(690, 260), Color.LightGray, 0.7f);
            }


            Texto("Pila (del tope al fondo):", new Vector2(680, 385), Color.White, 0.55f);
            int k = 0;
            foreach (TipoItem it in mochila.Take(8))
            {
                _spriteBatch.Draw(spritesItem[(int)it], new Vector2(680 + k * 40, 410), null, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
                k++;
            }
            if (mochila.Count > 8) Texto("+" + (mochila.Count - 8), new Vector2(680 + 8 * 40, 420), Color.White, 0.6f);


            Texto("Espadas: " + espadas + "   Escudos: " + escudos + "   Arcos: " + arcos, new Vector2(680, 460), Color.White, 0.6f);
            Texto("Ataque del soldado: " + AtaqueSoldado() + "   Defensa: " + escudos, new Vector2(680, 490), Color.White, 0.6f);
        }


        private void BarraVida(int x, int y, int ancho, int vida, int vidaMax, Color color)
        {
            _spriteBatch.Draw(pixel, new Rectangle(x, y, ancho, 6), Color.Black * 0.8f);
            int w = (int)(ancho * MathHelper.Clamp(vida / (float)vidaMax, 0f, 1f));
            _spriteBatch.Draw(pixel, new Rectangle(x, y, w, 6), color);
        }


        // Se dibuja con la cámara del mapa: marca de entrada, soldados, general, enemigos y disparos de las torres
        private void DibujarOleadaMapa()
        {
            if (oleadaFase == 0) return;

            if (oleadaFase == 1)
            {
                // Marca roja que parpadea por donde van a entrar los enemigos
                float pulso = 0.55f + 0.35f * (float)Math.Sin(tiempo * 5);
                _spriteBatch.Draw(anillo, entradaEnemigos, null, Color.Red * pulso, 0f,
                    new Vector2(anillo.Width / 2f, anillo.Height / 2f), 2.2f, SpriteEffects.None, 0f);
                return;
            }

            // De atrás hacia adelante: los de más abajo tapan a los de más arriba
            foreach (UnidadBatalla u in unidades.OrderBy(k => k.Pos.Y))
            {
                Texture2D tex = u.Enemigo != null ? spriteEnemigo : (u.EsGeneral ? spriteGeneralEspada : spriteGuardia);
                Vector2 origen = new Vector2(tex.Width / 2f, tex.Height - 1);
                float escala = u.EsGeneral ? 0.9f : 0.8f;
                float salto = u.Moviendo ? Math.Abs((float)Math.Sin(u.Fase)) * 5f : 0f;                 // saltitos al correr
                float sacudida = u.Golpeando > 0f ? (float)Math.Sin(tiempo * 70) * 3f : 0f;            // se sacude al pegar
                Color color = u.Flash > 0f ? new Color(255, 120, 120) : Color.White;                    // rojo al recibir un golpe
                SpriteEffects giro = u.Izquierda ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                _spriteBatch.Draw(pixel, new Rectangle((int)u.Pos.X - 12, (int)u.Pos.Y - 2, 24, 5), Color.Black * 0.3f);   // sombra
                _spriteBatch.Draw(tex, new Vector2(u.Pos.X + sacudida, u.Pos.Y - salto), null, color, 0f, origen, escala, giro, 0f);

                int ancho = u.EsGeneral ? 44 : 32;
                BarraVida((int)u.Pos.X - ancho / 2, (int)(u.Pos.Y - tex.Height * escala - 8f - salto), ancho,
                    u.Vida, u.VidaMax, u.EsSoldado ? Color.LimeGreen : Color.Red);
            }

            // Rayitos de las torres contra los enemigos
            foreach (Disparo d in disparos)
            {
                Vector2 v = d.Hasta - d.Desde;
                float largo = v.Length();
                if (largo < 1f) continue;
                float ang = (float)Math.Atan2(v.Y, v.X);
                _spriteBatch.Draw(pixel, d.Desde, null, Color.Yellow * Math.Min(1f, d.Tiempo / 0.2f), ang,
                    Vector2.Zero, new Vector2(largo, 2f), SpriteEffects.None, 0f);
            }
        }


        // Contador, panel de refuerzos, marcador de la batalla y resultado (todo sobre el mapa, sin tapar la pantalla)
        private void DibujarOleada()
        {
            Jugador j = partidaActual.datosJugador;

            if (oleadaFase == 1)
            {
                Rectangle p = PanelRefuerzos;
                Panel(p, 0.82f);
                int seg = Math.Max(0, (int)Math.Ceiling(contadorOleada));
                TextoCentrado("¡OLEADA " + oleadaNum + " en " + seg + " s!", p.Center.X, p.Y + 6, seg <= 5 ? Color.IndianRed : Color.Gold, 1.1f);

                string lado = entradaEnemigos.X > spriteMapa.texture.Width * spriteMapa.Escala / 2f ? "derecho" : "izquierdo";
                if (colaEnemigos.Count > 0)
                {
                    Enemigo e = colaEnemigos.Peek();
                    TextoCentrado("Llegan " + colaEnemigos.Count + " enemigos (vida " + e.Vida + ", ataque " + e.Ataque + " cada uno) por el lado " + lado + " del mapa",
                        p.Center.X, p.Y + 44, Color.White, 0.6f);
                }
                TextoCentrado("Tus torres disparan " + DanoTorres() + " de daño a cada enemigo   |   Soldado: vida 12, ataque " + AtaqueSoldado() + ", defensa " + escudos,
                    p.Center.X, p.Y + 66, Color.White, 0.6f);
                TextoCentrado("Ejército: " + j.Ejercito + " (máx. " + MaxRef() + " por oleada)   |   Vida del imperio: " + vidaImperio,
                    p.Center.X, p.Y + 88, Color.White, 0.6f);
                TextoCentrado("Refuerzos a enviar: " + refuerzosElegidos + (ContarEdificios(TipoEdificio.Cuartel) > 0 ? "  (+ el General Espadachín)" : "  (construí un centro militar para tener General)"),
                    p.Center.X, p.Y + 108, Color.Gold, 0.75f);
            }
            else if (oleadaFase == 2)
            {
                if (golpeTimer > 0f)    // un enemigo golpeó el castillo: destello rojo en toda la pantalla
                    _spriteBatch.Draw(pixel, new Rectangle(0, 0, VW, VH), Color.Red * (0.28f * Math.Min(1f, golpeTimer / 0.4f)));

                Rectangle b = new Rectangle(VW / 2 - 270, 62, 540, 54);
                Panel(b, 0.8f);
                int ene = colaEnemigos.Count + unidades.Count(u => !u.EsSoldado);
                int sol = colaRefuerzos.Count + unidades.Count(u => u.EsSoldado && !u.EsGeneral);
                TextoCentrado("¡OLEADA " + oleadaNum + "!", b.Center.X, b.Y + 3, Color.IndianRed, 0.9f);
                TextoCentrado("Enemigos: " + ene + "   Soldados: " + sol + "   Derrotados: " + enemigosDerrotados + "   Vida del imperio: " + vidaImperio,
                    b.Center.X, b.Y + 31, Color.White, 0.6f);
            }
            else if (oleadaFase == 3)
            {
                Panel(new Rectangle(0, 0, VW, VH), 0.45f);
                Panel(new Rectangle(VW / 2 - 320, 50, 640, 620), 0.94f);
                bool cayo = vidaImperio <= 0;
                TextoCentrado(cayo ? "¡Tu imperio cayó!" : "¡Resististe la oleada!", VW / 2f, 64, cayo ? Color.IndianRed : Color.LightGreen, 1.2f);
                TextoCentrado("Enemigos derrotados: " + enemigosDerrotados + "   Bajas: " + bajas + " de " + refuerzosEnviados, VW / 2f, 126, Color.White, 0.75f);
                TextoCentrado("Daño al castillo: " + danoCastillo + "   Vida del imperio: " + vidaImperio + "/" + VidaMax, VW / 2f, 156, Color.White, 0.75f);
                TextoCentrado("Botín: +" + botinOro + " de oro   Objetos ganados: " + botinItems, VW / 2f, 186, Color.Gold, 0.75f);
                if (generalEnBatalla)
                    TextoCentrado(generalCayo ? "El General Espadachín cayó (vuelve en la próxima oleada)" : "El General Espadachín volvió sano y salvo", VW / 2f, 216, Color.LightGray, 0.7f);
                if (salvadosHospital > 0) TextoCentrado("El hospital salvó a " + salvadosHospital + " soldados", VW / 2f, 244, Color.LightGreen, 0.75f);

                // Tabla con la matriz de estadísticas (últimas oleadas)
                TextoCentrado("Historial de oleadas", VW / 2f, 276, Color.White, 0.65f);
                float[] colX = { VW / 2f - 240, VW / 2f - 120, VW / 2f, VW / 2f + 120, VW / 2f + 240 };
                for (int c = 0; c < ColsHistorial; c++)
                    TextoCentrado(TitulosHistorial[c], colX[c], 300, Color.Gold, 0.6f);
                _spriteBatch.Draw(pixel, new Rectangle(VW / 2 - 290, 322, 580, 1), Color.White * 0.3f);
                for (int f = 0; f < filasHistorial; f++)
                {
                    Color cf = f == filasHistorial - 1 ? Color.LightGreen : Color.LightGray;   // la última fila es la oleada que acaba de terminar
                    for (int c = 0; c < ColsHistorial; c++)
                        TextoCentrado(historialOleadas[f, c].ToString(), colX[c], 330 + f * 24, cf, 0.6f);
                }
                _spriteBatch.Draw(pixel, new Rectangle(VW / 2 - 290, 574, 580, 1), Color.White * 0.3f);
                TextoCentrado("Total", colX[0], 580, Color.Gold, 0.6f);
                for (int c = 1; c < ColsHistorial; c++)
                    TextoCentrado(SumarColumna(c, 0).ToString(), colX[c], 580, Color.Gold, 0.6f);   // suma recursiva de cada columna
            }
        }


        private void DibujarEvento()
        {
            if (eventoActual == null) return;
            Panel(new Rectangle(0, 0, VW, VH), 0.55f);
            Panel(new Rectangle(140, 50, 1000, 620), 0.94f);
            TextoCentrado(eventoActual.Titulo, VW / 2f, 62, Color.Gold, 1.3f);


            // Personaje del edificio
            int tipo = (int)eventoActual.Tipo;
            Panel(new Rectangle(180, 140, 170, 190), 0.55f);
            _spriteBatch.Draw(spritesPersonaje[tipo], new Vector2(202, 150), Color.White);
            TextoCentrado(NombrePersonaje[tipo], 265, 286, Color.White, 0.7f);
            TextoCentrado("(" + NombreEdificio[tipo] + ")", 265, 308, Color.LightGray, 0.55f);


            if (evFase == 1)
            {
                List<string> lineas = Envolver(eventoActual.Texto, 46);
                for (int i = 0; i < lineas.Count; i++)
                    Texto(lineas[i], new Vector2(380, 150 + i * 32), Color.White, 0.8f);
                Texto("¿Qué decidís?", new Vector2(380, 292), Color.Gold, 0.8f);
            }
            else if (evFase == 2 && opcionElegida != null)
            {
                Texto("Elegiste: " + opcionElegida.Texto, new Vector2(380, 150), Color.Gold, 0.7f);
                List<string> lineas = Envolver(opcionElegida.Resultado, 46);
                for (int i = 0; i < lineas.Count; i++)
                    Texto(lineas[i], new Vector2(380, 200 + i * 32), Color.White, 0.8f);
                Texto(ResumenEfectos(opcionElegida), new Vector2(380, 340), Color.LightGreen, 0.8f);
            }
        }


        // Fila de arriba: personajes NORMALES. Fila de abajo: GENERALES (el hospital no tiene general).
        private void DibujarPersonajes()
        {
            _spriteBatch.Draw(pixel, new Rectangle(270, 332, 740, 2), Color.White * 0.25f);   // línea que separa las filas

            for (int i = 0; i < 4; i++)
            {
                float cx = 340 + i * 200;
                bool conocido = ContarEdificios((TipoEdificio)i) > 0;
                Color tinte = conocido ? Color.White : new Color(40, 40, 40);

                // Normal (arriba)
                _spriteBatch.Draw(spritesNormal[i], new Vector2(cx - 64, 172), tinte);
                TextoCentrado(conocido ? NombreNormal[i] : "???", cx, 302, Color.White, 0.7f);

                // General (abajo)
                if (i == (int)TipoEdificio.Hospital)
                {
                    TextoCentrado("Sin general", cx, 395, Color.Gray, 0.65f);
                    continue;
                }
                _spriteBatch.Draw(spritesPersonaje[i], new Vector2(cx - 64, 338), tinte);
                TextoCentrado(conocido ? NombrePersonaje[i] : "???", cx, 468, Color.Gold, 0.7f);
                TextoCentrado(conocido ? "Trabaja en tu " + NombreEdificio[i].ToLowerInvariant() : "Construí: " + NombreEdificio[i], cx, 492, Color.LightGray, 0.5f);
            }
        }


        private void DibujarNombre()
        {
            FondoCubrir(texturaMenu);
            Panel(new Rectangle(290, 170, 700, 340), 0.75f);
            TextoCentrado("Nombre de tu Rey", VW / 2f, 200, Color.Gold, 1.3f);


            string cursor = ((int)(tiempo * 2)) % 2 == 0 ? "|" : "";
            Panel(new Rectangle(370, 290, 540, 60), 0.6f);
            TextoCentrado(NombreRey + cursor, VW / 2f, 302, Color.White, 1.1f);


            TextoCentrado("Escribí el nombre y apretá Enter o Confirmar", VW / 2f, 385, Color.LightGray, 0.7f);
        }


        private void DibujarMenu()
        {
            FondoCubrir(texturaMenu);
            Panel(new Rectangle(290, 90, 700, 80), 0.6f);
            TextoCentrado("Bienvenido, " + NombreRey, VW / 2f, 108, Color.Gold, 1.3f);

            if (eligiendoModo)
            {
                Panel(new Rectangle(240, 180, 800, 390), 0.88f);
                TextoCentrado("Elegí el modo de juego", VW / 2f, 200, Color.Gold, 1.1f);
                TextoCentrado("Sobreviví hasta el turno " + TurnoFinalHistoria + " para ganar la partida.", VW / 2f, 326, Color.LightGray, 0.65f);
                TextoCentrado("Sin final: jugá hasta que tu imperio caiga.", VW / 2f, 436, Color.LightGray, 0.65f);
            }
        }


        private void DibujarOpciones()
        {
            FondoCubrir(texturaIsla);
            Panel(new Rectangle(140, 100, 1000, 540), 0.7f);
            TextoCentrado("OPCIONES", VW / 2f, 118, Color.Gold, 1.3f);


            Texto("Sonido: " + (sonidoActivo ? "Activado" : "Desactivado"), new Vector2(580, 200 - 14), Color.White, 0.9f);
            Texto("Resolución: " + resoluciones[indiceResolucion][0] + " x " + resoluciones[indiceResolucion][1],
                  new Vector2(580, 272 - 14), Color.White, 0.9f);
            Texto("Idioma: Español", new Vector2(580, 344 - 14), Color.White, 0.9f);
        }


        private void DibujarCreditos()
        {
            FondoCubrir(texturaIsla);
            Panel(new Rectangle(240, 100, 800, 400), 0.75f);
            TextoCentrado("CRÉDITOS", VW / 2f, 130, Color.Gold, 1.3f);
            TextoCentrado("Project10", VW / 2f, 230, Color.White, 1.0f);
            TextoCentrado("Hecho con MonoGame", VW / 2f, 290, Color.White, 0.9f);
            TextoCentrado("Gracias por jugar", VW / 2f, 350, Color.LightGray, 0.9f);
        }


        private void DibujarJuego()
        {
            Jugador j = partidaActual.datosJugador;


            // Datos del jugador (arriba a la izquierda)
            string[] lineas =
            {
                "Rey: " + j.Nombre,
                "Oro: " + j.Oro,
                "Reputación: " + j.Reputacion,
                "Ejército: " + j.Ejercito,
                "Turno: " + partidaActual.Turno + (modoHistoria ? " / " + TurnoFinalHistoria : " (infinito)"),
                "Mapa: " + nombresMapas[mapaActual],
                "Vida: " + vidaImperio,
                "Oleada: turno " + ((partidaActual.Turno / IntervaloOleada + 1) * IntervaloOleada)
            };
            for (int i = 0; i < lineas.Length; i++)
                Placa(150, 45 + i * 52, lineas[i]);


            if (panelActivo == null)
            {
                Panel(new Rectangle(VW - 250, 100, 240, 390), 0.55f);
                Panel(new Rectangle(20, VH - 52, 900, 38), 0.55f);
                Panel(new Rectangle(15, 445, 270, 215), 0.55f);
                TextoCentrado("CONSTRUIR", 150, 450, Color.Gold, 0.7f);
                Texto(construirTipo >= 0
                        ? "Clic en el mapa para construir: " + NombreEdificio[construirTipo] + "   |   Clic derecho: cancelar"
                        : "Elegí qué construir a la izquierda   |   Clic en un edificio: mejorar   |   Espacio: turno   |   WASD: cámara",
                      new Vector2(32, VH - 47), Color.White, 0.6f);
                if (torreSeleccionada >= 0) DibujarCartelTorre();
                if (oleadaFase != 0) DibujarOleada();
            }
            else if (panelActivo == "Evento")
            {
                DibujarEvento();
            }
            else
            {
                DibujarPanelJuego();
            }

            if (victoriaHistoria) DibujarVictoria();
        }


        private void DibujarVictoria()
        {
            Jugador j = partidaActual.datosJugador;
            Panel(new Rectangle(0, 0, VW, VH), 0.6f);
            Panel(new Rectangle(VW / 2 - 330, 130, 660, 340), 0.94f);
            TextoCentrado("¡VICTORIA!", VW / 2f, 150, Color.Gold, 1.6f);
            TextoCentrado("Tu imperio sobrevivió hasta el turno " + TurnoFinalHistoria, VW / 2f, 235, Color.White, 0.85f);
            TextoCentrado("Modo Historia completado", VW / 2f, 275, Color.LightGreen, 0.85f);
            TextoCentrado("Oro: " + j.Oro + "   Ejército: " + j.Ejercito + "   Reputación: " + j.Reputacion + "   Vida: " + vidaImperio, VW / 2f, 330, Color.LightGray, 0.7f);
        }


        private void DibujarPanelJuego()
        {
            Panel(new Rectangle(0, 0, VW, VH), 0.5f);
            Panel(new Rectangle(240, 110, 800, 500), 0.92f);
            TextoCentrado(panelActivo.ToUpperInvariant(), VW / 2f, 130, Color.Gold, 1.2f);


            switch (panelActivo)
            {
                case "Inventario":
                    DibujarInventario();
                    break;
                case "Personajes":
                    DibujarPersonajes();
                    break;
                case "Diario":
                    TextoCentrado("Elegí una pista", VW / 2f, 250, Color.White, 0.9f);
                    if (pistaSeleccionada >= 0)
                        TextoCentrado(pistas[pistaSeleccionada], VW / 2f, 340, Color.LightGoldenrodYellow, 0.8f);
                    break;
                case "Controles":
                    TextoCentrado("W A S D o flechas: mover la cámara", VW / 2f, 220, Color.White, 0.85f);
                    TextoCentrado("Espacio: pasar de turno (+10 de oro y lo que den las minas)", VW / 2f, 270, Color.White, 0.85f);
                    TextoCentrado("Menú de la izquierda: elegí qué construir", VW / 2f, 320, Color.White, 0.85f);
                    TextoCentrado("Clic en un edificio: ver / mejorar", VW / 2f, 370, Color.White, 0.85f);
                    TextoCentrado("Clic derecho: cancelar / soltar", VW / 2f, 420, Color.White, 0.85f);
                    TextoCentrado("Esc: cerrar ventana / volver al menú", VW / 2f, 470, Color.White, 0.85f);
                    break;
            }
        }


        // ------------------------------------------------------------------ AYUDAS DE DIBUJO


        private void FondoCubrir(Texture2D t)
        {
            float esc = Math.Max(VW / (float)t.Width, VH / (float)t.Height);
            Vector2 pos = new Vector2((VW - t.Width * esc) / 2f, (VH - t.Height * esc) / 2f);
            _spriteBatch.Draw(t, pos, null, Color.White, 0f, Vector2.Zero, esc, SpriteEffects.None, 0f);
        }


        private void Panel(Rectangle r, float opacidad)
        {
            _spriteBatch.Draw(pixel, r, Color.Black * opacidad);
        }


        private void Placa(float cx, float cy, string texto)
        {
            Boton p = botones["vacio"];
            p.Centro = new Vector2(cx, cy);
            p.Escala = 270f / p.Normal.Width;
            p.Encima = false;
            p.Habilitado = true;
            p.Dibujar(_spriteBatch);


            Vector2 m = FuenteJuego.MeasureString(texto) * 0.8f;
            Texto(texto, new Vector2(cx - m.X / 2f, cy - m.Y / 2f), Color.Gold, 0.8f);
        }


        private void Texto(string t, Vector2 pos, Color color, float escala = 1f)
        {
            _spriteBatch.DrawString(FuenteJuego, t, pos + new Vector2(2, 2), Color.Black * 0.8f, 0f, Vector2.Zero, escala, SpriteEffects.None, 0f);
            _spriteBatch.DrawString(FuenteJuego, t, pos, color, 0f, Vector2.Zero, escala, SpriteEffects.None, 0f);
        }


        private void TextoCentrado(string t, float cx, float y, Color color, float escala = 1f)
        {
            Vector2 m = FuenteJuego.MeasureString(t) * escala;
            Texto(t, new Vector2(cx - m.X / 2f, y), color, escala);
        }
    }
}
