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
            unidades.Clear();
            disparos.Clear();
            logBatalla.Clear();
            LimpiarHistorial();
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
            foreach (Torre t in torres) total += (int)Math.Round(t.Nivel * 2.2f);
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


        // ------------------------------------------------------------------ OLEADA EN EL MAPA (TIEMPO REAL)
        // Fases: 0 = sin oleada, 1 = cuenta regresiva (elegís refuerzos), 2 = batalla corriendo en el mapa, 3 = resultado.

        private const int GeneralVida = 40;
        private const float VelSoldado = 190f, VelGeneral = 165f, VelEnemigo = 70f;
        private const float AlcanceGolpe = 38f, EsperaGolpe = 0.8f, VisionEnemigo = 240f, RadioTorres = 430f;


        // RECURSIVIDAD: cantidad de enemigos de la oleada n (la base es 3 y cada oleada suma 2 enemigos más)
        private int EnemigosOleada(int n)
        {
            if (n <= 0) return 3;                      // caso base
            return EnemigosOleada(n - 1) + 2;          // caso recursivo: la oleada anterior + 2
        }


        // ------------------------------------------------------------------ MATRIZ DE ESTADÍSTICAS
        // Cada fila es una oleada y cada columna un dato: 0 = número de oleada, 1 = derrotados, 2 = bajas, 3 = daño al castillo, 4 = oro.
        // Guarda las últimas 10 oleadas.
        private const int MaxFilasHistorial = 10, ColsHistorial = 5;
        private int[,] historialOleadas = new int[MaxFilasHistorial, ColsHistorial];
        private int filasHistorial;
        private static readonly string[] TitulosHistorial = { "Oleada", "Derrotados", "Bajas", "Daño", "Oro" };


        private void LimpiarHistorial()
        {
            historialOleadas = new int[MaxFilasHistorial, ColsHistorial];
            filasHistorial = 0;
        }


        // Agrega una fila a la matriz con los resultados de la oleada que acaba de terminar
        private void RegistrarOleada()
        {
            if (filasHistorial == MaxFilasHistorial)
            {
                // La matriz está llena: se sube cada fila un lugar y se descarta la más vieja
                for (int f = 1; f < MaxFilasHistorial; f++)
                    for (int c = 0; c < ColsHistorial; c++)
                        historialOleadas[f - 1, c] = historialOleadas[f, c];
                filasHistorial--;
            }
            int fila = filasHistorial++;
            historialOleadas[fila, 0] = oleadaNum;
            historialOleadas[fila, 1] = enemigosDerrotados;
            historialOleadas[fila, 2] = bajas;
            historialOleadas[fila, 3] = danoCastillo;
            historialOleadas[fila, 4] = botinOro;
        }


        // RECURSIVIDAD: suma una columna de la matriz, desde la fila 'fila' hasta la última cargada
        private int SumarColumna(int col, int fila)
        {
            if (fila >= filasHistorial) return 0;                                  // caso base: no quedan filas
            return historialOleadas[fila, col] + SumarColumna(col, fila + 1);      // caso recursivo: esta fila + las que siguen
        }


        // Para guardar en JSON se pasa la matriz a un arreglo de arreglos (solo las filas usadas)
        private int[][] HistorialAJson()
        {
            int[][] r = new int[filasHistorial][];
            for (int f = 0; f < filasHistorial; f++)
            {
                r[f] = new int[ColsHistorial];
                for (int c = 0; c < ColsHistorial; c++) r[f][c] = historialOleadas[f, c];
            }
            return r;
        }


        // Al cargar el JSON se vuelve a armar la matriz
        private void JsonAHistorial(int[][] datos)
        {
            LimpiarHistorial();
            if (datos == null) return;
            for (int f = 0; f < datos.Length && filasHistorial < MaxFilasHistorial; f++)
            {
                if (datos[f] == null || datos[f].Length < ColsHistorial) continue;
                for (int c = 0; c < ColsHistorial; c++) historialOleadas[filasHistorial, c] = datos[f][c];
                filasHistorial++;
            }
        }


        // Centro del pueblo: el promedio de todos tus edificios (o el centro del mapa si todavía no construiste nada)
        private Vector2 CentroPueblo()
        {
            if (torres.Count == 0)
                return new Vector2(spriteMapa.texture.Width * spriteMapa.Escala / 2f, spriteMapa.texture.Height * spriteMapa.Escala / 2f);
            float sx = 0f, sy = 0f;
            foreach (Torre t in torres) { sx += t.Base.X; sy += t.Base.Y; }
            return new Vector2(sx / torres.Count, sy / torres.Count);
        }


        // Por dónde salen los soldados: de un centro militar (se van turnando) o, si no hay, del centro del pueblo
        private Vector2 PuntoSalida()
        {
            List<Torre> cuarteles = torres.Where(t => t.Tipo == TipoEdificio.Cuartel).ToList();
            Vector2 b = cuarteles.Count > 0 ? cuarteles[cuartelTurno++ % cuarteles.Count].Base : CentroPueblo();
            return b + new Vector2(azar.Next(-25, 26), azar.Next(10, 30));
        }


        private void IniciarOleada()
        {
            BaseDatos.RegistrarEvento(partidaActual.Turno, "Oleada enemiga");   // <-- BASE DE DATOS
            oleadaNum++;
            colaEnemigos.Clear();
            colaRefuerzos.Clear();
            unidades.Clear();
            disparos.Clear();
            int cantidad = EnemigosOleada(oleadaNum);
            for (int i = 0; i < cantidad; i++) colaEnemigos.Enqueue(new Enemigo(oleadaNum));   // Enqueue: se suman al final de la fila

            // Los enemigos entran por el lado del mapa más lejano al pueblo
            float mapaW = spriteMapa.texture.Width * spriteMapa.Escala;
            float mapaH = spriteMapa.texture.Height * spriteMapa.Escala;
            Vector2 pueblo = CentroPueblo();
            float ex = pueblo.X < mapaW / 2f ? mapaW - 60f : 60f;
            entradaEnemigos = new Vector2(ex, MathHelper.Clamp(pueblo.Y, 260f, mapaH - 260f));

            refuerzosElegidos = Math.Min(10, Math.Min(MaxRef(), partidaActual.datosJugador.Ejercito));
            contadorOleada = TiempoOleada;
            oleadaFase = 1;                 // cuenta regresiva sobre el mapa (ya no hay pantalla aparte)
            pistaSeleccionada = -1;
            logBatalla.Clear();
        }


        private void EmpezarBatalla()
        {
            Jugador j = partidaActual.datosJugador;
            int n = Math.Min(refuerzosElegidos, Math.Min(MaxRef(), j.Ejercito));
            j.Ejercito -= n;
            for (int i = 0; i < n; i++) colaRefuerzos.Enqueue(new Soldado(AtaqueSoldado()));
            refuerzosEnviados = n;
            enemigosDerrotados = 0; bajas = 0; danoCastillo = 0; botinOro = 0; botinItems = 0; salvadosHospital = 0;
            logBatalla.Clear();
            spawnSoldadoTimer = 0f;
            spawnEnemigoTimer = 0f;
            cuartelTurno = 0;
            generalCayo = false;

            // El General Espadachín sale siempre (si tenés un centro militar) y se suma a los soldados que elegiste
            generalEnBatalla = ContarEdificios(TipoEdificio.Cuartel) > 0;
            if (generalEnBatalla)
            {
                Soldado g = new Soldado(AtaqueSoldado() + 3);
                g.Vida = GeneralVida;
                unidades.Add(new UnidadBatalla { Pos = PuntoSalida(), Soldado = g, EsGeneral = true, VidaMax = GeneralVida });
            }

            camaraLibreTimer = 99f;         // la cámara empieza a seguir la batalla
            oleadaFase = 2;
        }


        // El enemigo (o soldado) más cercano al que puede atacar la unidad u. Los enemigos solo ven hasta cierta distancia.
        private UnidadBatalla BuscarObjetivo(UnidadBatalla u)
        {
            UnidadBatalla mejor = null;
            float mejorDist = u.EsSoldado ? float.MaxValue : VisionEnemigo;
            for (int i = 0; i < unidades.Count; i++)
            {
                UnidadBatalla o = unidades[i];
                if (!o.Viva || o.EsSoldado == u.EsSoldado) continue;
                float d = Vector2.Distance(u.Pos, o.Pos);
                if (d < mejorDist) { mejorDist = d; mejor = o; }
            }
            return mejor;
        }


        private void Golpear(UnidadBatalla atacante, UnidadBatalla victima)
        {
            if (atacante.EsSoldado)
            {
                victima.Enemigo.Vida -= atacante.Soldado.Ataque;
            }
            else
            {
                victima.Soldado.Vida -= Math.Max(1, atacante.Enemigo.Ataque - escudos);   // los escudos reducen el daño
            }
            victima.Flash = 0.15f;
        }


        // Avanza la batalla dt segundos: los soldados corren hacia los enemigos y los enemigos hacia el pueblo
        private void ActualizarBatalla(float dt)
        {
            Vector2 pueblo = CentroPueblo();

            // Salen de a uno de cada fila (Dequeue): soldados desde el centro militar, enemigos desde el borde del mapa
            spawnSoldadoTimer -= dt;
            if (spawnSoldadoTimer <= 0f && colaRefuerzos.Count > 0)
            {
                spawnSoldadoTimer = 0.14f;
                Soldado s = colaRefuerzos.Dequeue();
                unidades.Add(new UnidadBatalla { Pos = PuntoSalida(), Soldado = s, VidaMax = s.Vida, Fase = (float)azar.NextDouble() * 6f });
            }
            spawnEnemigoTimer -= dt;
            if (spawnEnemigoTimer <= 0f && colaEnemigos.Count > 0)
            {
                spawnEnemigoTimer = 0.45f;
                Enemigo e = colaEnemigos.Dequeue();
                unidades.Add(new UnidadBatalla
                {
                    Pos = entradaEnemigos + new Vector2(azar.Next(-40, 41), azar.Next(-170, 171)),
                    Enemigo = e, VidaMax = e.VidaMax, Fase = (float)azar.NextDouble() * 6f, Izquierda = entradaEnemigos.X > pueblo.X
                });
            }

            // Si no hay enemigos a la vista, los soldados esperan en el camino por donde van a llegar
            Vector2 haciaEntrada = entradaEnemigos - pueblo;
            if (haciaEntrada.LengthSquared() > 1f) haciaEntrada.Normalize();
            Vector2 puestoEspera = pueblo + haciaEntrada * 220f;

            bool castilloGolpeado = false;
            for (int i = 0; i < unidades.Count; i++)
            {
                UnidadBatalla u = unidades[i];
                if (!u.Viva) continue;
                u.Cooldown -= dt;
                u.Flash -= dt;
                u.Golpeando -= dt;

                // Las torres disparan una sola vez a cada enemigo cuando se acerca a ellas
                if (!u.EsSoldado && !u.Enemigo.Atacado)
                {
                    List<Torre> cerca = torres.FindAll(t => t.Tipo == TipoEdificio.Torre && Vector2.Distance(t.Base, u.Pos) < RadioTorres);
                    if (cerca.Count > 0)
                    {
                        u.Enemigo.Atacado = true;
                        int d = DanoTorres();
                        if (d > 0)
                        {
                            u.Enemigo.Vida -= d;
                            u.Flash = 0.25f;
                            foreach (Torre t in cerca)
                                disparos.Add(new Disparo { Desde = t.Base + new Vector2(0f, -AlturaEdificio(t) * 0.85f), Hasta = u.Pos + new Vector2(0f, -25f) });
                        }
                        if (!u.Viva) continue;
                    }
                }

                UnidadBatalla objetivo = BuscarObjetivo(u);
                Vector2 meta = objetivo != null ? objetivo.Pos : (u.EsSoldado ? puestoEspera : pueblo);
                float dist = Vector2.Distance(u.Pos, meta);
                u.Moviendo = false;

                if (objetivo != null && dist <= AlcanceGolpe)
                {
                    u.Izquierda = meta.X < u.Pos.X;
                    if (u.Cooldown <= 0f)
                    {
                        u.Cooldown = EsperaGolpe;
                        u.Golpeando = 0.18f;
                        Golpear(u, objetivo);
                    }
                }
                else if (objetivo == null && !u.EsSoldado && dist <= 55f)
                {
                    // El enemigo llegó al pueblo: golpea el castillo y desaparece
                    int dano = u.Enemigo.Ataque * 2;
                    vidaImperio -= dano;
                    danoCastillo += dano;
                    u.Enemigo.Vida = 0;
                    u.Llego = true;
                    castilloGolpeado = true;
                }
                else if (dist > 4f)
                {
                    Vector2 dir = (meta - u.Pos) / dist;
                    float vel = u.EsGeneral ? VelGeneral : (u.EsSoldado ? VelSoldado : VelEnemigo);
                    u.Pos += dir * Math.Min(vel * dt, dist);
                    u.Izquierda = dir.X < 0f;
                    u.Moviendo = true;
                    u.Fase += dt * (u.EsSoldado ? 14f : 8f);
                }
            }

            if (castilloGolpeado) golpeTimer = 0.4f;

            // Que no se amontonen todos en el mismo punto
            for (int i = 0; i < unidades.Count; i++)
            {
                for (int k = i + 1; k < unidades.Count; k++)
                {
                    Vector2 dif = unidades[i].Pos - unidades[k].Pos;
                    float d = dif.Length();
                    if (d < 0.01f) { unidades[k].Pos += new Vector2(1f, 0.5f); continue; }
                    if (d < 18f)
                    {
                        Vector2 empuje = dif / d * ((18f - d) * 0.5f);
                        unidades[i].Pos += empuje;
                        unidades[k].Pos -= empuje;
                    }
                }
            }

            // Los que cayeron: bajas, enemigos derrotados y botín
            for (int i = unidades.Count - 1; i >= 0; i--)
            {
                UnidadBatalla u = unidades[i];
                if (u.Viva) continue;
                if (u.EsSoldado)
                {
                    if (u.EsGeneral) generalCayo = true; else bajas++;
                }
                else if (!u.Llego)
                {
                    enemigosDerrotados++;
                    botinOro += 10;
                    if (azar.Next(100) < 20 && mochila.Count < MaxMochila)
                    {
                        TipoItem it = (TipoItem)azar.Next(4);
                        mochila.Push(it);
                        botinItems++;
                    }
                }
                unidades.RemoveAt(i);
            }

            for (int i = disparos.Count - 1; i >= 0; i--)
            {
                disparos[i].Tiempo -= dt;
                if (disparos[i].Tiempo <= 0f) disparos.RemoveAt(i);
            }

            bool quedanEnemigos = colaEnemigos.Count > 0 || unidades.Exists(x => !x.EsSoldado);
            if (vidaImperio <= 0) { vidaImperio = 0; TerminarBatalla(); }
            else if (!quedanEnemigos) TerminarBatalla();
        }


        private void TerminarBatalla()
        {
            Jugador j = partidaActual.datosJugador;
            int vivos = colaRefuerzos.Count;       // los que ni llegaron a salir
            foreach (UnidadBatalla u in unidades)
                if (u.EsSoldado && !u.EsGeneral && u.Viva) vivos++;
            j.Ejercito += vivos;                   // los soldados que sobreviven vuelven al ejército
            colaRefuerzos.Clear();
            colaEnemigos.Clear();
            unidades.Clear();
            disparos.Clear();
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
            RegistrarOleada();              // se anota la oleada en la matriz de estadísticas
            oleadaFase = 3;
            panelActivo = null;             // si había una ventana abierta, se cierra para mostrar el resultado
            pistaSeleccionada = -1;
            torreSeleccionada = -1;
            construirTipo = -1;
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
                if (VerificarVictoria()) return;   // Modo Historia: si ya era el turno final, ganaste
                if (colaEventos.Count > 0) MostrarSiguienteEvento();
            }
        }
        // Modo Historia: al llegar al turno 200 ganás. Devuelve true si la partida se ganó.
        private bool VerificarVictoria()
        {
            if (!modoHistoria || partidaActual.Turno < TurnoFinalHistoria) return false;
            Jugador j = partidaActual.datosJugador;
            BaseDatos.FinalizarSesion("victoria", partidaActual.Turno, j.Oro, j.Ejercito, j.Reputacion, 0);   // <-- BASE DE DATOS
            victoriaHistoria = true;
            colaEventos.Clear();
            panelActivo = null;
            torreSeleccionada = -1;
            construirTipo = -1;
            return true;
        }


        // La cámara se acerca sola a donde está la acción (si no la estás moviendo vos con WASD)
        private void SeguirBatalla(float dt)
        {
            if (unidades.Count == 0) return;
            Vector2 suma = Vector2.Zero;
            foreach (UnidadBatalla u in unidades) suma += u.Pos;
            Vector2 centro = suma / unidades.Count;
            Vector2 objetivo = centro - new Vector2(VW / 2f, VH / 2f);
            camara += (objetivo - camara) * Math.Min(1f, 2.5f * dt);
        }


        private void ActualizarOleada(float dt)
        {
            Jugador j = partidaActual.datosJugador;
            if (golpeTimer > 0) golpeTimer -= dt;
            bool libre = panelActivo == null;   // con una ventana abierta la oleada sigue corriendo, pero sin botones


            if (oleadaFase == 1)
            {
                contadorOleada -= dt;
                int max = Math.Min(MaxRef(), j.Ejercito);
                if (libre)
                {
                    Rectangle p = PanelRefuerzos;
                    int bx = p.X + 75;
                    int by = p.Bottom - 46;
                    if (Rb(new Rectangle(bx, by, 70, 36), "-5")) refuerzosElegidos -= 5;
                    if (Rb(new Rectangle(bx + 80, by, 70, 36), "-1")) refuerzosElegidos -= 1;
                    if (Rb(new Rectangle(bx + 160, by, 70, 36), "+1")) refuerzosElegidos += 1;
                    if (Rb(new Rectangle(bx + 240, by, 70, 36), "+5")) refuerzosElegidos += 5;
                    if (Rb(new Rectangle(bx + 320, by, 70, 36), "Máx")) refuerzosElegidos = max;
                    if (Rb(new Rectangle(bx + 410, by, 140, 36), "¡Ya!")) contadorOleada = 0f;
                }
                refuerzosElegidos = MathHelper.Clamp(refuerzosElegidos, 0, max);

                if (contadorOleada <= 0f) EmpezarBatalla();
            }
            else if (oleadaFase == 2)
            {
                if (libre && Rb(new Rectangle(VW / 2 - 90, 124, 180, 34), "Saltar batalla"))
                {
                    int seguridad = 0;
                    while (oleadaFase == 2 && seguridad++ < 20000) ActualizarBatalla(0.05f);
                    return;
                }
                ActualizarBatalla(dt);
            }
            else if (oleadaFase == 3)
            {
                if (Rb(new Rectangle(VW / 2 - 120, 610, 240, 44), "Aceptar")) CerrarOleada();
            }
        }
    }
}
