-- Datos que el juego busca POR NOMBRE en la base 'reino'.
-- Se puede ejecutar varias veces: no duplica filas.
SET NAMES utf8mb4;
USE reino;

INSERT INTO decisions (nombre, descripcion)
SELECT 'Brote de fiebre', 'Varios soldados cayeron enfermos. Necesitamos medicinas urgentes, mi rey.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Brote de fiebre');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Comprar medicinas (-30 de oro)', 'La fiebre se controla y el imperio se siente mejor.', -30, 0, 0, 0, 10 FROM decisions d
WHERE d.nombre = 'Brote de fiebre' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Comprar medicinas (-30 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Dejar que se curen solos', 'Varios soldados no lo lograron.', 0, 0, -8, 0, 0 FROM decisions d
WHERE d.nombre = 'Brote de fiebre' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Dejar que se curen solos');

INSERT INTO decisions (nombre, descripcion)
SELECT 'Heridos en el camino', 'Llegaron campesinos heridos pidiendo ayuda. Atenderlos nos costará recursos.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Heridos en el camino');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Atenderlos (-20 de oro)', 'La gente te lo agradece y algunos se unen al ejército.', -20, 0, 5, 8, 0 FROM decisions d
WHERE d.nombre = 'Heridos en el camino' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Atenderlos (-20 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Echarlos', 'El pueblo se entera y tu fama se resiente.', 0, 0, 0, -8, 0 FROM decisions d
WHERE d.nombre = 'Heridos en el camino' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Echarlos');

INSERT INTO decisions (nombre, descripcion)
SELECT 'Hierbas curativas', 'Un viajero ofrece hierbas raras que curan casi todo, pero las vende caras.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Hierbas curativas');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Comprarlas (-40 de oro)', 'Las hierbas funcionan de maravilla.', -40, 0, 0, 0, 20 FROM decisions d
WHERE d.nombre = 'Hierbas curativas' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Comprarlas (-40 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Rechazar la oferta', 'El viajero se marcha con sus hierbas.', 0, 0, 0, 0, 0 FROM decisions d
WHERE d.nombre = 'Hierbas curativas' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Rechazar la oferta');

INSERT INTO decisions (nombre, descripcion)
SELECT 'Soldados descontentos', 'Un grupo de soldados quiere abandonar el ejército porque no cobró a tiempo.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Soldados descontentos');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Pagar el sueldo (-40 de oro)', 'Los soldados quedan conformes y te respetan más.', -40, 0, 0, 5, 0 FROM decisions d
WHERE d.nombre = 'Soldados descontentos' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Pagar el sueldo (-40 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Castigarlos', 'Se van de todos modos y el ejército se debilita.', 0, 0, -10, -5, 0 FROM decisions d
WHERE d.nombre = 'Soldados descontentos' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Castigarlos');

INSERT INTO decisions (nombre, descripcion)
SELECT 'Entrenamiento intensivo', 'Propongo una semana de entrenamiento duro. Los reclutas saldrán mucho mejor preparados.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Entrenamiento intensivo');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Autorizarlo (-30 de oro)', 'Se suman 15 soldados bien entrenados.', -30, 0, 15, 0, 0 FROM decisions d
WHERE d.nombre = 'Entrenamiento intensivo' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Autorizarlo (-30 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Posponerlo', 'Quizás para otra ocasión.', 0, 0, 0, 0, 0 FROM decisions d
WHERE d.nombre = 'Entrenamiento intensivo' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Posponerlo');

INSERT INTO decisions (nombre, descripcion)
SELECT 'Mercenarios', 'Una compañía de mercenarios ofrece unirse a tu ejército por un buen precio.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Mercenarios');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Contratarlos (-60 de oro)', 'Los mercenarios se suman a tus filas.', -60, 0, 25, 0, 0 FROM decisions d
WHERE d.nombre = 'Mercenarios' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Contratarlos (-60 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Rechazarlos', 'Los mercenarios se van a ofrecer sus servicios a otro.', 0, 0, 0, 0, 0 FROM decisions d
WHERE d.nombre = 'Mercenarios' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Rechazarlos');

INSERT INTO decisions (nombre, descripcion)
SELECT 'Humo en el horizonte', 'Desde la torre veo humo a lo lejos. Puede ser un campamento enemigo.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Humo en el horizonte');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Enviar exploradores (-15 de oro)', 'Los exploradores vuelven con información valiosa.', -15, 0, 0, 10, 0 FROM decisions d
WHERE d.nombre = 'Humo en el horizonte' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Enviar exploradores (-15 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Ignorarlo', 'No pasa nada... por ahora.', 0, 0, 0, 0, 0 FROM decisions d
WHERE d.nombre = 'Humo en el horizonte' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Ignorarlo');

INSERT INTO decisions (nombre, descripcion)
SELECT 'Mercader en la puerta', 'Un mercader pide permiso para entrar. Trae provisiones, pero también podría cobrarle peaje.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Mercader en la puerta');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Comprarle provisiones (-30 de oro)', 'Las provisiones alimentan a todo el imperio.', -30, 0, 5, 0, 8 FROM decisions d
WHERE d.nombre = 'Mercader en la puerta' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Comprarle provisiones (-30 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Cobrarle peaje', 'Ganás oro, pero el mercader se va enojado.', 25, 0, 0, -3, 0 FROM decisions d
WHERE d.nombre = 'Mercader en la puerta' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Cobrarle peaje');

INSERT INTO decisions (nombre, descripcion)
SELECT 'Murallas dañadas', 'La última tormenta dañó las murallas. Conviene repararlas antes de que lleguen enemigos.' FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM decisions WHERE nombre = 'Murallas dañadas');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Repararlas (-40 de oro)', 'Las murallas quedan más fuertes que antes.', -40, 0, 0, 0, 15 FROM decisions d
WHERE d.nombre = 'Murallas dañadas' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Repararlas (-40 de oro)');
INSERT INTO decisionoutcomes (decision_id, texto_opcion, resultado, impacto_oro, impacto_poblacion, impacto_ejercito, impacto_moral, impacto_estabilidad)
SELECT d.id, 'Aguantar así', 'Las murallas se debilitan un poco más.', 0, 0, 0, 0, -5 FROM decisions d
WHERE d.nombre = 'Murallas dañadas' AND NOT EXISTS (SELECT 1 FROM decisionoutcomes o WHERE o.decision_id = d.id AND o.texto_opcion = 'Aguantar así');

INSERT INTO randomevents (nombre, descripcion, probabilidad)
SELECT 'Oleada enemiga', 'Un ejército enemigo ataca el imperio.', 0.25 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM randomevents WHERE nombre = 'Oleada enemiga');

-- Control: debería dar 9 decisiones, 18 opciones y 1 evento
SELECT (SELECT COUNT(*) FROM decisions) AS decisiones, (SELECT COUNT(*) FROM decisionoutcomes) AS opciones, (SELECT COUNT(*) FROM randomevents) AS eventos;
