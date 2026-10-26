using System.Collections.Generic;
using Project10.Modelos;

namespace Project10.Datos
{
    // Todos los eventos (decisiones) posibles del juego. Para agregar uno nuevo, copia un l.Add(...) y cambia los textos.
    internal static class BibliotecaEventos
    {
        public static List<Evento> Crear()
        {
            List<Evento> l = new List<Evento>();


            // Hospital - Médica Elena
            l.Add(new Evento(TipoEdificio.Hospital, "Brote de fiebre",
                "Varios soldados cayeron enfermos. Necesitamos medicinas urgentes, mi rey.",
                new[]
                {
                    new Opcion("Comprar medicinas (-30 de oro)", -30, 10, 0, 0, "La fiebre se controla y el imperio se siente mejor."),
                    new Opcion("Dejar que se curen solos", 0, 0, -8, 0, "Varios soldados no lo lograron.")
                }));
            l.Add(new Evento(TipoEdificio.Hospital, "Heridos en el camino",
                "Llegaron campesinos heridos pidiendo ayuda. Atenderlos nos costará recursos.",
                new[]
                {
                    new Opcion("Atenderlos (-20 de oro)", -20, 0, 5, 8, "La gente te lo agradece y algunos se unen al ejército."),
                    new Opcion("Echarlos", 0, 0, 0, -8, "El pueblo se entera y tu fama se resiente.")
                }));
            l.Add(new Evento(TipoEdificio.Hospital, "Hierbas curativas",
                "Un viajero ofrece hierbas raras que curan casi todo, pero las vende caras.",
                new[]
                {
                    new Opcion("Comprarlas (-40 de oro)", -40, 20, 0, 0, "Las hierbas funcionan de maravilla."),
                    new Opcion("Rechazar la oferta", 0, 0, 0, 0, "El viajero se marcha con sus hierbas.")
                }));


            // Centro militar - General Marcos
            l.Add(new Evento(TipoEdificio.Cuartel, "Soldados descontentos",
                "Un grupo de soldados quiere abandonar el ejército porque no cobró a tiempo.",
                new[]
                {
                    new Opcion("Pagar el sueldo (-40 de oro)", -40, 0, 0, 5, "Los soldados quedan conformes y te respetan más."),
                    new Opcion("Castigarlos", 0, 0, -10, -5, "Se van de todos modos y el ejército se debilita.")
                }));
            l.Add(new Evento(TipoEdificio.Cuartel, "Entrenamiento intensivo",
                "Propongo una semana de entrenamiento duro. Los reclutas saldrán mucho mejor preparados.",
                new[]
                {
                    new Opcion("Autorizarlo (-30 de oro)", -30, 0, 15, 0, "Se suman 15 soldados bien entrenados."),
                    new Opcion("Posponerlo", 0, 0, 0, 0, "Quizás para otra ocasión.")
                }));
            l.Add(new Evento(TipoEdificio.Cuartel, "Mercenarios",
                "Una compañía de mercenarios ofrece unirse a tu ejército por un buen precio.",
                new[]
                {
                    new Opcion("Contratarlos (-60 de oro)", -60, 0, 25, 0, "Los mercenarios se suman a tus filas."),
                    new Opcion("Rechazarlos", 0, 0, 0, 0, "Los mercenarios se van a ofrecer sus servicios a otro.")
                }));


            // Torre - Vigía Tomás
            l.Add(new Evento(TipoEdificio.Torre, "Humo en el horizonte",
                "Desde la torre veo humo a lo lejos. Puede ser un campamento enemigo.",
                new[]
                {
                    new Opcion("Enviar exploradores (-15 de oro)", -15, 0, 0, 10, "Los exploradores vuelven con información valiosa."),
                    new Opcion("Ignorarlo", 0, 0, 0, 0, "No pasa nada... por ahora.")
                }));
            l.Add(new Evento(TipoEdificio.Torre, "Mercader en la puerta",
                "Un mercader pide permiso para entrar. Trae provisiones, pero también podría cobrarle peaje.",
                new[]
                {
                    new Opcion("Comprarle provisiones (-30 de oro)", -30, 8, 5, 0, "Las provisiones alimentan a todo el imperio."),
                    new Opcion("Cobrarle peaje", 25, 0, 0, -3, "Ganás oro, pero el mercader se va enojado.")
                }));
            l.Add(new Evento(TipoEdificio.Torre, "Murallas dañadas",
                "La última tormenta dañó las murallas. Conviene repararlas antes de que lleguen enemigos.",
                new[]
                {
                    new Opcion("Repararlas (-40 de oro)", -40, 15, 0, 0, "Las murallas quedan más fuertes que antes."),
                    new Opcion("Aguantar así", 0, -5, 0, 0, "Las murallas se debilitan un poco más.")
                }));


            return l;
        }
    }
}
