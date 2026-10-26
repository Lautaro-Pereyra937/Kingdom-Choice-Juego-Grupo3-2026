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
    // Clase principal: campos, carga de contenido y bucle Update.
    // El resto de la logica esta repartida en la carpeta Juego/ (Game1.*.cs).
    public partial class Game1 : Game
    {
        // Todo el juego se dibuja en un espacio virtual de 1280x720 y se escala a la ventana.
        // Así los menús se ven igual con cualquier resolución.
        private const int VW = 1280;
        private const int VH = 720;


        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;


        private SpriteFont FuenteJuego;
        private Texture2D pixel;


        // Sprites de torres (nivel 1 a 5) e iconos de mejora
        private Texture2D[] texturasTorre;
        private Texture2D iconoOro, iconoMejora, estrellaLlena, estrellaVacia;


        // Torres
        private Texture2D anillo;
        private readonly List<Torre> torres = new List<Torre>();
        private List<Torre> torresGuardadas = null;   // torres leídas de la partida guardada
        private int torreSeleccionada = -1;
        private int torreEncima = -1;
        private bool mostrarPrevia, previaValida;
        private Vector2 posPrevia;
        private const float EscalaTorre = 0.6f;       // sprite de 256 px -> ~154 px en el mapa
        private const int NivelMaximo = 5;
        // Edificios: 0 = Torre, 1 = Hospital, 2 = Centro militar
        private static readonly int[] CostoConstruirDe = { 30, 60, 80 };
        private static readonly int[] MaxPorTipo = { 8, 2, 2 };
        private static readonly int[] NivelMaxDe = { 5, 3, 3 };
        private static readonly int[] CostoMejoraHospital = { 60, 120 };
        private static readonly int[] CostoMejoraCuartel = { 80, 160 };
        private static readonly string[] NombreEdificio = { "Torre", "Hospital", "Centro militar" };
        private static readonly string[] NombreCorto = { "Torre", "Hospital", "C. militar" };
        private static readonly string[] NombrePlural = { "torres", "hospitales", "centros militares" };
        private static readonly string[] NombrePersonaje = { "Vigía Tomás", "Médica Elena", "General Marcos" };
        private int construirTipo = -1;               // edificio elegido en el menú de construcción (-1 = ninguno)
        private Texture2D[] texturasEdif;             // hospital nivel 1-3 (0-2) y centro militar nivel 1-3 (3-5)
        private Texture2D[] spritesPersonaje;         // un personaje por edificio
        private const float EscalaEdif = 0.85f;
        private const int EdifLado = 192;
        private const int EdifBaseY = 186;
        private static readonly int[] EdifTope = { 81, 67, 35, 75, 38, 30 };
        private static readonly int[] EdifAncho = { 130, 155, 180, 130, 131, 156 };


        // ---- Eventos (decisiones). Cola de eventos pendientes: se atienden en orden de llegada.
        private readonly Queue<Evento> colaEventos = new Queue<Evento>();
        private readonly List<Evento> eventos = BibliotecaEventos.Crear();
        private Evento eventoActual;
        private Opcion opcionElegida;
        private int evFase;                           // 1 = elegir, 2 = resultado
        private int salvadosHospital;
        // Oro que cuesta pasar del nivel 1->2, 2->3, 3->4 y 4->5
        private static readonly int[] CostoMejora = { 50, 100, 150, 250 };
        // Fila donde empiezan los pixeles en cada sprite (para el área de clic)
        private static readonly int[] TopeSprite = { 94, 47, 46, 27, 0 };


        // ---- Oleadas, mochila y ejército (estructuras de datos)
        // COLA (Queue): los enemigos llegan en fila y pelean en orden de llegada (FIFO).
        private readonly Queue<Enemigo> colaEnemigos = new Queue<Enemigo>();
        // COLA (Queue): los refuerzos también hacen fila; el primero en la fila es el primero en pelear.
        private readonly Queue<Soldado> colaRefuerzos = new Queue<Soldado>();
        // PILA (Stack): la mochila. Lo último que guardás queda arriba y es lo primero que sacás (LIFO).
        private readonly Stack<TipoItem> mochila = new Stack<TipoItem>();


        private int vidaImperio = 100;
        private int espadas, escudos, arcos;
        private int oleadaNum, oleadaFase, refuerzosElegidos, refuerzosEnviados;
        private int enemigosDerrotados, bajas, danoCastillo, botinOro, botinItems;
        private float pasoTimer, golpeTimer;
        private readonly List<string> logBatalla = new List<string>();
        private Texture2D[] spritesItem;              // uno por cada TipoItem
        private Texture2D spriteSoldado, spriteEnemigo;
        private readonly List<BotonRect> botonesRect = new List<BotonRect>();
        private const int IntervaloOleada = 5;        // cada cuántos turnos llega una oleada
        private const int MaxMochila = 20;
        private const int BaseRefuerzos = 30;
        private const int VidaMax = 100;
        private static readonly string[] NombreItem = { "Espada", "Escudo", "Arco", "Poción" };
        private static readonly int[] PrecioItem = { 40, 40, 30, 20 };
        private static readonly string[] DescItem = { "+2 de ataque a soldados", "-1 de daño recibido", "+1 de ataque a soldados", "Cura 30 de vida" };

        // Fondos
        private Texture2D texturaMenu;
        private Texture2D texturaIsla;
        private Sprite spriteMapa;
        private Texture2D[] texturasMapas;
        private readonly string[] nombresMapas = { "Aldea", "Rio", "Fortaleza" };
        private int mapaActual = 2;
        private readonly Random azar = new Random();


        // Botones (se cargan todos desde Content/Botones)
        private static readonly string[] NombresBotones =
        {
            "volver", "confirmar", "pista1", "pista2", "pista3", "pista4",
            "adquirir_musica", "creditos", "idioma",
            "personajes", "controles", "diario", "inventario",
            "sonido", "continuar", "diario_g", "resolucion", "partida_nueva",
            "opciones", "salir", "borrar_partida", "guardar_partida", "vacio", "mejorar"
        };
        private readonly Dictionary<string, Boton> botones = new Dictionary<string, Boton>();
        private readonly List<Boton> visibles = new List<Boton>();


        // Estado
        private Partida partidaActual;
        private EstadoJuego estadoActual = EstadoJuego.IngresandoNombre;
        private string NombreRey = "";


        // Entrada
        private KeyboardState tecladoActual, tecladoAnterior;
        private MouseState mouseActual, mouseAnterior;
        private Point mouseVirtual;
        private bool clic;
        private double tiempo;


        // Cámara del mapa
        private Vector2 camara = Vector2.Zero;
        private const float VelocidadCamara = 700f;
        private const float EscalaMapa = 2f;


        // Opciones
        private bool sonidoActivo = true;
        private readonly int[][] resoluciones = { new[] { 960, 540 }, new[] { 1280, 720 }, new[] { 1600, 900 } };
        private int indiceResolucion = 1;


        // Ventanas dentro del juego
        private string panelActivo = null;
        private int pistaSeleccionada = -1;
        private readonly string[] pistas =
        {
            "Pista 1: (escribí acá el texto de la pista 1)",
            "Pista 2: (escribí acá el texto de la pista 2)",
            "Pista 3: (escribí acá el texto de la pista 3)",
            "Pista 4: (escribí acá el texto de la pista 4)"
        };


        // Mensajes cortos
        private string mensaje;
        private float mensajeTiempo;


        private readonly string rutaGuardado = Path.Combine(AppContext.BaseDirectory, "partida.sav");


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = resoluciones[indiceResolucion][0];
            _graphics.PreferredBackBufferHeight = resoluciones[indiceResolucion][1];
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "Project10 - VERSION EDIFICIOS";
        }


        protected override void Initialize()
        {
            base.Initialize();


            // Escribir el nombre del rey (ya no hace falta tener el mouse apretado)
            Window.TextInput += (sender, e) =>
            {
                if (estadoActual != EstadoJuego.IngresandoNombre) return;


                if (e.Key == Keys.Back)
                {
                    if (NombreRey.Length > 0)
                        NombreRey = NombreRey.Substring(0, NombreRey.Length - 1);
                }
                else if (e.Key == Keys.Enter)
                {
                    if (NombreRey.Trim().Length > 0) IrAlMenu();
                }
                else if (!char.IsControl(e.Character)
                         && NombreRey.Length < 16
                         && FuenteJuego != null
                         && FuenteJuego.Characters.Contains(e.Character))
                {
                    NombreRey += e.Character;
                }
            };
        }


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);


            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });


            FuenteJuego = Content.Load<SpriteFont>("FuenteJuego");


            // Mapas posibles: 0 = Aldea, 1 = Rio, 2 = Fortaleza (Content/Fondos)
            texturasMapas = new[]
            {
                Content.Load<Texture2D>("Fondos/imperio_aldea"),
                Content.Load<Texture2D>("Fondos/imperio_rio"),
                Content.Load<Texture2D>("Fondos/imperio_montana")
            };
            texturaMenu = texturasMapas[0];   // fondo del menu / nombre
            texturaIsla = texturasMapas[1];   // fondo de opciones / creditos
            spriteMapa = new Sprite(texturasMapas[mapaActual], Vector2.Zero);
            spriteMapa.Escala = EscalaMapa;


            anillo = CrearAnillo(150, 54);


            texturasTorre = new Texture2D[5];
            for (int i = 0; i < 5; i++)
                texturasTorre[i] = Content.Load<Texture2D>("Torres/torre_nivel" + (i + 1));
            iconoOro = Content.Load<Texture2D>("Torres/icono_oro");
            iconoMejora = Content.Load<Texture2D>("Torres/icono_mejora");
            estrellaLlena = Content.Load<Texture2D>("Torres/estrella_llena");
            estrellaVacia = Content.Load<Texture2D>("Torres/estrella_vacia");


            // Items (Content/Items)
            spritesItem = new Texture2D[4];
            spritesItem[(int)TipoItem.Espada] = Content.Load<Texture2D>("Items/item_espada");
            spritesItem[(int)TipoItem.Escudo] = Content.Load<Texture2D>("Items/item_escudo");
            spritesItem[(int)TipoItem.Arco] = Content.Load<Texture2D>("Items/item_arco");
            spritesItem[(int)TipoItem.Pocion] = Content.Load<Texture2D>("Items/item_pocion");

            // Unidades de batalla (Content/Unidades)
            spriteSoldado = Content.Load<Texture2D>("Unidades/soldado");
            spriteEnemigo = Content.Load<Texture2D>("Unidades/enemigo");

            // Personajes de los eventos (Content/Personajes): vigia, medica, general
            spritesPersonaje = new Texture2D[3];
            spritesPersonaje[0] = Content.Load<Texture2D>("Personajes/personaje_vigia");
            spritesPersonaje[1] = Content.Load<Texture2D>("Personajes/personaje_medica");
            spritesPersonaje[2] = Content.Load<Texture2D>("Personajes/personaje_general");

            // Edificios (Content/Edificios): hospital 1-3 y centro militar 1-3
            texturasEdif = new Texture2D[6];
            for (int i = 0; i < 3; i++)
            {
                texturasEdif[i] = Content.Load<Texture2D>("Edificios/hospital_nivel" + (i + 1));
                texturasEdif[3 + i] = Content.Load<Texture2D>("Edificios/cuartel_nivel" + (i + 1));
            }


            foreach (string n in NombresBotones)
            {
                botones[n] = new Boton(
                    Content.Load<Texture2D>("Botones/btn_" + n),
                    Content.Load<Texture2D>("Botones/btn_" + n + "_hover"));
            }

            CargarMusica();
        }


        // ------------------------------------------------------------------ UPDATE


        protected override void Update(GameTime gameTime)
        {
            tecladoActual = Keyboard.GetState();
            mouseActual = Mouse.GetState();


            Viewport vp = GraphicsDevice.Viewport;
            mouseVirtual = new Point(
                (int)(mouseActual.X * VW / (float)vp.Width),
                (int)(mouseActual.Y * VH / (float)vp.Height));


            clic = mouseActual.LeftButton == ButtonState.Pressed && mouseAnterior.LeftButton == ButtonState.Released;
            tiempo = gameTime.TotalGameTime.TotalSeconds;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;


            if (mensajeTiempo > 0) mensajeTiempo -= dt;
            visibles.Clear();
            botonesRect.Clear();


            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();


            bool esc = Presionada(Keys.Escape);


            switch (estadoActual)
            {
                case EstadoJuego.IngresandoNombre: ActualizarNombre(esc); break;
                case EstadoJuego.MenuPrincipal: ActualizarMenu(esc); break;
                case EstadoJuego.Opciones: ActualizarOpciones(esc); break;
                case EstadoJuego.Creditos: ActualizarCreditos(esc); break;
                case EstadoJuego.Jugando: ActualizarJuego(dt, esc); break;
            }


            ActualizarMusica();   // menú = música del menú, partida = música de juego

            tecladoAnterior = tecladoActual;
            mouseAnterior = mouseActual;


            base.Update(gameTime);
        }


        private bool Presionada(Keys tecla)
        {
            return tecladoActual.IsKeyDown(tecla) && tecladoAnterior.IsKeyUp(tecla);
        }


        // Coloca un botón (por nombre) con un ancho en pixeles virtuales y lo deja listo para dibujar.
        private Boton Colocar(string nombre, float x, float y, float ancho, bool habilitado = true)
        {
            Boton b = botones[nombre];
            b.Centro = new Vector2(x, y);
            b.Escala = ancho / b.Normal.Width;
            b.Habilitado = habilitado;
            b.Actualizar(mouseVirtual);
            visibles.Add(b);
            return b;
        }


        private bool Pulsado(Boton b)
        {
            return clic && b.Encima;
        }


        private void CambiarEstado(EstadoJuego nuevo)
        {
            estadoActual = nuevo;
            panelActivo = null;
            pistaSeleccionada = -1;
            torreSeleccionada = -1;
            torreEncima = -1;
            mostrarPrevia = false;
            oleadaFase = 0;
            evFase = 0;
            construirTipo = -1;
            visibles.Clear();
        }


        private void IrAlMenu()
        {
            NombreRey = NombreRey.Trim();
            CambiarEstado(EstadoJuego.MenuPrincipal);
        }


        private void Mensaje(string texto)
        {
            mensaje = texto;
            mensajeTiempo = 2.5f;
        }
    }
}
