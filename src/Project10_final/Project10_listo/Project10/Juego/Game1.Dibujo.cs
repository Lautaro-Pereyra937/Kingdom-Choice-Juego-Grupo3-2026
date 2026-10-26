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
        // ------------------------------------------------------------------ DRAW


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


            // Estado de la música (diagnóstico; se puede borrar cuando ya suene)
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


            // De atrás hacia adelante (los de más abajo tapan a los de más arriba)
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


                // Flechita que salta cuando alcanza el oro para mejorarlo
                if (t.Nivel < NivelMaxDe[(int)t.Tipo] && j.Oro >= CostoSiguienteNivel(t))
                {
                    float salto = (float)Math.Sin(tiempo * 4) * 4f;
                    float y = t.Base.Y - AlturaEdificio(t) - 26 + salto;
                    _spriteBatch.Draw(iconoMejora, new Vector2(t.Base.X, y), null, Color.White, 0f,
                        new Vector2(iconoMejora.Width / 2f, iconoMejora.Height / 2f), 0.6f, SpriteEffects.None, 0f);
                }
            }
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


        private void DibujarOleada()
        {
            Jugador j = partidaActual.datosJugador;
            Panel(new Rectangle(0, 0, VW, VH), 0.55f);
            Panel(new Rectangle(140, 50, 1000, 620), 0.94f);
            TextoCentrado("¡OLEADA " + oleadaNum + "!", VW / 2f, 62, Color.IndianRed, 1.3f);


            // Escena: refuerzos a la izquierda, enemigos a la derecha; el primero de cada fila está al centro
            int nSold = oleadaFase == 1 ? refuerzosElegidos : colaRefuerzos.Count;
            int nEne = colaEnemigos.Count;
            _spriteBatch.Draw(pixel, new Rectangle(150, 318, 980, 6), new Color(120, 90, 50));


            for (int i = 0; i < Math.Min(nSold, 6); i++)
            {
                float sacudida = (i == 0 && oleadaFase == 2 && golpeTimer > 0) ? (float)Math.Sin(tiempo * 60) * 4f : 0f;
                _spriteBatch.Draw(spriteSoldado, new Vector2(500 - i * 58 + sacudida, 254), Color.White);
            }
            int idx = 0;
            foreach (Enemigo en in colaEnemigos.Take(6))
            {
                float sacudida = (idx == 0 && oleadaFase == 2 && golpeTimer > 0) ? (float)Math.Sin(tiempo * 60) * 4f : 0f;
                _spriteBatch.Draw(spriteEnemigo, new Vector2(716 + idx * 58 + sacudida, 254), Color.White);
                idx++;
            }


            if (oleadaFase == 2)
            {
                if (colaRefuerzos.Count > 0)
                {
                    Soldado s = colaRefuerzos.Peek();
                    BarraVida(500, 244, 64, s.Vida, 12, Color.LimeGreen);
                }
                if (colaEnemigos.Count > 0)
                {
                    Enemigo e = colaEnemigos.Peek();
                    BarraVida(716, 244, 64, e.Vida, e.VidaMax, Color.Red);
                }
            }


            Texto("Refuerzos en fila: " + nSold, new Vector2(200, 332), Color.LightGreen, 0.65f);
            Texto("Enemigos en fila: " + nEne, new Vector2(780, 332), Color.IndianRed, 0.65f);


            if (oleadaFase == 1)
            {
                if (colaEnemigos.Count > 0)
                {
                    Enemigo e = colaEnemigos.Peek();
                    TextoCentrado("Llegan " + colaEnemigos.Count + " enemigos (vida " + e.Vida + ", ataque " + e.Ataque + " cada uno)", VW / 2f, 365, Color.White, 0.75f);
                }
                TextoCentrado("Tus torres disparan " + DanoTorres() + " de daño a cada enemigo que llega", VW / 2f, 395, Color.White, 0.7f);
                TextoCentrado("Cada soldado: vida 12, ataque " + AtaqueSoldado() + ", defensa " + escudos, VW / 2f, 422, Color.White, 0.7f);
                TextoCentrado("Ejército: " + j.Ejercito + " (máx. " + MaxRef() + " por oleada)   |   Vida del imperio: " + vidaImperio, VW / 2f, 449, Color.White, 0.7f);
                TextoCentrado("¿Cuántos refuerzos querés enviar?  " + refuerzosElegidos, VW / 2f, 480, Color.Gold, 0.9f);
            }
            else if (oleadaFase == 2)
            {
                TextoCentrado("Derrotados: " + enemigosDerrotados + "   Bajas: " + bajas + "   Vida del imperio: " + vidaImperio, VW / 2f, 365, Color.White, 0.75f);
                int desde = Math.Max(0, logBatalla.Count - 6);
                for (int i = desde; i < logBatalla.Count; i++)
                    TextoCentrado(logBatalla[i], VW / 2f, 410 + (i - desde) * 28, Color.LightGray, 0.65f);
            }
            else if (oleadaFase == 3)
            {
                bool cayo = vidaImperio <= 0;
                TextoCentrado(cayo ? "¡Tu imperio cayó!" : "¡Resististe la oleada!", VW / 2f, 380, cayo ? Color.IndianRed : Color.LightGreen, 1.1f);
                TextoCentrado("Enemigos derrotados: " + enemigosDerrotados + "   Bajas: " + bajas + " de " + refuerzosEnviados, VW / 2f, 430, Color.White, 0.75f);
                TextoCentrado("Daño al castillo: " + danoCastillo + "   Vida del imperio: " + vidaImperio + "/" + VidaMax, VW / 2f, 460, Color.White, 0.75f);
                TextoCentrado("Botín: +" + botinOro + " de oro   Objetos ganados: " + botinItems, VW / 2f, 490, Color.Gold, 0.75f);
                if (salvadosHospital > 0) TextoCentrado("El hospital salvó a " + salvadosHospital + " soldados", VW / 2f, 520, Color.LightGreen, 0.75f);
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


        private void DibujarPersonajes()
        {
            for (int i = 0; i < 3; i++)
            {
                float cx = 400 + i * 240;
                bool conocido = ContarEdificios((TipoEdificio)i) > 0;
                _spriteBatch.Draw(spritesPersonaje[i], new Vector2(cx - 64, 190), conocido ? Color.White : new Color(40, 40, 40));
                TextoCentrado(conocido ? NombrePersonaje[i] : "???", cx, 330, Color.White, 0.75f);
                TextoCentrado(conocido ? "Trabaja en tu " + NombreEdificio[i].ToLowerInvariant() : "Construí: " + NombreEdificio[i], cx, 362, Color.LightGray, 0.55f);
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
                "Turno: " + partidaActual.Turno,
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
                Panel(new Rectangle(15, 455, 270, 205), 0.55f);
                TextoCentrado("CONSTRUIR", 150, 461, Color.Gold, 0.7f);
                Texto(construirTipo >= 0
                        ? "Clic en el mapa para construir: " + NombreEdificio[construirTipo] + "   |   Clic derecho: cancelar"
                        : "Elegí qué construir a la izquierda   |   Clic en un edificio: mejorar   |   Espacio: turno   |   WASD: cámara",
                      new Vector2(32, VH - 47), Color.White, 0.6f);
                if (torreSeleccionada >= 0) DibujarCartelTorre();
            }
            else if (panelActivo == "Oleada")
            {
                DibujarOleada();
            }
            else if (panelActivo == "Evento")
            {
                DibujarEvento();
            }
            else
            {
                DibujarPanelJuego();
            }
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
                    TextoCentrado("Espacio: pasar de turno (+10 de oro)", VW / 2f, 270, Color.White, 0.85f);
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
