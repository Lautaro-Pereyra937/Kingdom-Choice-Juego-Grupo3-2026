-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 20-10-2026 a las 22:37:43
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `reino`
--
CREATE DATABASE IF NOT EXISTS `reino` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `reino`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `choicelogs`
--

DROP TABLE IF EXISTS `choicelogs`;
CREATE TABLE `choicelogs` (
  `id` int(11) NOT NULL,
  `session_id` int(11) NOT NULL,
  `turno` int(11) NOT NULL,
  `decision_id` int(11) NOT NULL,
  `outcome_id` int(11) NOT NULL
) ;

--
-- Volcado de datos para la tabla `choicelogs`
--

INSERT INTO `choicelogs` (`id`, `session_id`, `turno`, `decision_id`, `outcome_id`) VALUES
(1, 1, 2, 9, 17),
(2, 1, 3, 7, 13),
(3, 1, 8, 9, 17),
(4, 1, 10, 7, 13),
(5, 2, 7, 7, 13),
(6, 2, 8, 8, 15),
(7, 2, 9, 9, 17),
(8, 3, 2, 2, 3),
(9, 3, 4, 8, 16),
(10, 3, 9, 2, 3);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `decisionoutcomes`
--

DROP TABLE IF EXISTS `decisionoutcomes`;
CREATE TABLE `decisionoutcomes` (
  `id` int(11) NOT NULL,
  `decision_id` int(11) NOT NULL,
  `texto_opcion` varchar(150) NOT NULL,
  `resultado` varchar(255) DEFAULT NULL,
  `impacto_oro` int(11) NOT NULL DEFAULT 0,
  `impacto_poblacion` int(11) NOT NULL DEFAULT 0,
  `impacto_ejercito` int(11) NOT NULL DEFAULT 0,
  `impacto_moral` int(11) NOT NULL DEFAULT 0,
  `impacto_estabilidad` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `decisionoutcomes`
--

INSERT INTO `decisionoutcomes` (`id`, `decision_id`, `texto_opcion`, `resultado`, `impacto_oro`, `impacto_poblacion`, `impacto_ejercito`, `impacto_moral`, `impacto_estabilidad`) VALUES
(1, 1, 'Comprar medicinas (-30 de oro)', 'La fiebre se controla y el imperio se siente mejor.', -30, 0, 0, 0, 10),
(2, 1, 'Dejar que se curen solos', 'Varios soldados no lo lograron.', 0, 0, -8, 0, 0),
(3, 2, 'Atenderlos (-20 de oro)', 'La gente te lo agradece y algunos se unen al ejército.', -20, 0, 5, 8, 0),
(4, 2, 'Echarlos', 'El pueblo se entera y tu fama se resiente.', 0, 0, 0, -8, 0),
(5, 3, 'Comprarlas (-40 de oro)', 'Las hierbas funcionan de maravilla.', -40, 0, 0, 0, 20),
(6, 3, 'Rechazar la oferta', 'El viajero se marcha con sus hierbas.', 0, 0, 0, 0, 0),
(7, 4, 'Pagar el sueldo (-40 de oro)', 'Los soldados quedan conformes y te respetan más.', -40, 0, 0, 5, 0),
(8, 4, 'Castigarlos', 'Se van de todos modos y el ejército se debilita.', 0, 0, -10, -5, 0),
(9, 5, 'Autorizarlo (-30 de oro)', 'Se suman 15 soldados bien entrenados.', -30, 0, 15, 0, 0),
(10, 5, 'Posponerlo', 'Quizás para otra ocasión.', 0, 0, 0, 0, 0),
(11, 6, 'Contratarlos (-60 de oro)', 'Los mercenarios se suman a tus filas.', -60, 0, 25, 0, 0),
(12, 6, 'Rechazarlos', 'Los mercenarios se van a ofrecer sus servicios a otro.', 0, 0, 0, 0, 0),
(13, 7, 'Enviar exploradores (-15 de oro)', 'Los exploradores vuelven con información valiosa.', -15, 0, 0, 10, 0),
(14, 7, 'Ignorarlo', 'No pasa nada... por ahora.', 0, 0, 0, 0, 0),
(15, 8, 'Comprarle provisiones (-30 de oro)', 'Las provisiones alimentan a todo el imperio.', -30, 0, 5, 0, 8),
(16, 8, 'Cobrarle peaje', 'Ganás oro, pero el mercader se va enojado.', 25, 0, 0, -3, 0),
(17, 9, 'Repararlas (-40 de oro)', 'Las murallas quedan más fuertes que antes.', -40, 0, 0, 0, 15),
(18, 9, 'Aguantar así', 'Las murallas se debilitan un poco más.', 0, 0, 0, 0, -5);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `decisions`
--

DROP TABLE IF EXISTS `decisions`;
CREATE TABLE `decisions` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `descripcion` text NOT NULL,
  `requisitos` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `decisions`
--

INSERT INTO `decisions` (`id`, `nombre`, `descripcion`, `requisitos`) VALUES
(1, 'Brote de fiebre', 'Varios soldados cayeron enfermos. Necesitamos medicinas urgentes, mi rey.', 'Edificio requerido: Hospital (Médica Elena)'),
(2, 'Heridos en el camino', 'Llegaron campesinos heridos pidiendo ayuda. Atenderlos nos costará recursos.', 'Edificio requerido: Hospital (Médica Elena)'),
(3, 'Hierbas curativas', 'Un viajero ofrece hierbas raras que curan casi todo, pero las vende caras.', 'Edificio requerido: Hospital (Médica Elena)'),
(4, 'Soldados descontentos', 'Un grupo de soldados quiere abandonar el ejército porque no cobró a tiempo.', 'Edificio requerido: Cuartel (General Marcos)'),
(5, 'Entrenamiento intensivo', 'Propongo una semana de entrenamiento duro. Los reclutas saldrán mucho mejor preparados.', 'Edificio requerido: Cuartel (General Marcos)'),
(6, 'Mercenarios', 'Una compañía de mercenarios ofrece unirse a tu ejército por un buen precio.', 'Edificio requerido: Cuartel (General Marcos)'),
(7, 'Humo en el horizonte', 'Desde la torre veo humo a lo lejos. Puede ser un campamento enemigo.', 'Edificio requerido: Torre (Vigía Tomás)'),
(8, 'Mercader en la puerta', 'Un mercader pide permiso para entrar. Trae provisiones, pero también podría cobrarle peaje.', 'Edificio requerido: Torre (Vigía Tomás)'),
(9, 'Murallas dañadas', 'La última tormenta dañó las murallas. Conviene repararlas antes de que lleguen enemigos.', 'Edificio requerido: Torre (Vigía Tomás)');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `eventlogs`
--

DROP TABLE IF EXISTS `eventlogs`;
CREATE TABLE `eventlogs` (
  `id` int(11) NOT NULL,
  `session_id` int(11) NOT NULL,
  `turno` int(11) NOT NULL,
  `event_id` int(11) NOT NULL
) ;

--
-- Volcado de datos para la tabla `eventlogs`
--

INSERT INTO `eventlogs` (`id`, `session_id`, `turno`, `event_id`) VALUES
(1, 1, 5, 1),
(2, 1, 10, 1),
(3, 2, 5, 1),
(4, 2, 10, 1),
(5, 2, 15, 1),
(6, 3, 5, 1),
(7, 3, 10, 1);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `kingdomstate`
--

DROP TABLE IF EXISTS `kingdomstate`;
CREATE TABLE `kingdomstate` (
  `id` int(11) NOT NULL,
  `session_id` int(11) NOT NULL,
  `turno` int(11) NOT NULL,
  `oro` int(11) NOT NULL DEFAULT 100,
  `poblacion` int(11) NOT NULL DEFAULT 0,
  `ejercito` int(11) NOT NULL DEFAULT 200,
  `moral` int(11) NOT NULL DEFAULT 50,
  `estabilidad` int(11) NOT NULL DEFAULT 100
) ;

--
-- Volcado de datos para la tabla `kingdomstate`
--

INSERT INTO `kingdomstate` (`id`, `session_id`, `turno`, `oro`, `poblacion`, `ejercito`, `moral`, `estabilidad`) VALUES
(1, 1, 2, 10, 0, 202, 50, 100),
(2, 1, 3, 5, 0, 204, 60, 100),
(3, 1, 4, 15, 0, 206, 60, 100),
(4, 1, 5, 75, 0, 203, 65, 100),
(5, 1, 6, 85, 0, 205, 65, 100),
(6, 1, 7, 95, 0, 207, 65, 100),
(7, 1, 8, 65, 0, 209, 65, 100),
(8, 1, 9, 75, 0, 211, 65, 100),
(9, 1, 10, 140, 0, 206, 80, 100),
(10, 2, 2, 50, 0, 202, 50, 100),
(11, 2, 3, 60, 0, 204, 50, 100),
(12, 2, 4, 70, 0, 206, 50, 100),
(13, 2, 5, 130, 0, 203, 55, 100),
(14, 2, 6, 90, 0, 205, 55, 100),
(15, 2, 7, 85, 0, 207, 65, 100),
(16, 2, 8, 65, 0, 214, 65, 100),
(17, 2, 9, 35, 0, 216, 65, 100),
(18, 2, 10, 115, 0, 211, 70, 100),
(19, 2, 11, 125, 0, 213, 70, 100),
(20, 2, 12, 135, 0, 215, 70, 100),
(21, 2, 13, 145, 0, 217, 70, 100),
(22, 2, 14, 155, 0, 219, 70, 100),
(23, 2, 15, 65, 0, 220, 75, 28),
(24, 3, 2, 0, 0, 207, 58, 100),
(25, 3, 3, 10, 0, 209, 58, 100),
(26, 3, 4, 45, 0, 211, 55, 100),
(27, 3, 5, 105, 0, 208, 60, 100),
(28, 3, 6, 115, 0, 210, 60, 100),
(29, 3, 7, 125, 0, 212, 60, 100),
(30, 3, 8, 135, 0, 214, 60, 100),
(31, 3, 9, 125, 0, 221, 68, 100),
(32, 3, 10, 205, 0, 217, 73, 100);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `players`
--

DROP TABLE IF EXISTS `players`;
CREATE TABLE `players` (
  `id` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `email` varchar(100) DEFAULT NULL,
  `fecha_registro` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `players`
--

INSERT INTO `players` (`id`, `nombre`, `email`, `fecha_registro`) VALUES
(1, 'popeye', NULL, '2026-10-19 18:47:41'),
(2, 'monalo', NULL, '2026-10-20 11:47:32'),
(3, 'lopeztasp', NULL, '2026-10-20 13:18:02');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `randomevents`
--

DROP TABLE IF EXISTS `randomevents`;
CREATE TABLE `randomevents` (
  `id` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `probabilidad` decimal(3,2) NOT NULL,
  `efectos` text NOT NULL
) ;

--
-- Volcado de datos para la tabla `randomevents`
--

INSERT INTO `randomevents` (`id`, `nombre`, `descripcion`, `probabilidad`, `efectos`) VALUES
(1, 'Oleada enemiga', 'Un grupo de enemigos ataca el castillo; cada oleada es más numerosa.', 1.00, '{\"oro\":10,\"estabilidad\":-5,\"nota\":\"+10 de oro por enemigo derrotado\"}'),
(2, 'Buena cosecha', 'Los campos rinden más de lo esperado.', 0.15, '{\"oro\":20,\"poblacion\":10,\"moral\":5}'),
(3, 'Epidemia', 'Una enfermedad se extiende por el reino.', 0.10, '{\"poblacion\":-15,\"estabilidad\":-10,\"moral\":-5}'),
(4, 'Festival del pueblo', 'El pueblo celebra y la moral sube.', 0.12, '{\"oro\":-10,\"moral\":10}'),
(5, 'Deserción nocturna', 'Parte de las tropas abandona el campamento.', 0.08, '{\"ejercito\":-10,\"moral\":-5}');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `sessionrank`
--

DROP TABLE IF EXISTS `sessionrank`;
CREATE TABLE `sessionrank` (
  `id` int(11) NOT NULL,
  `session_id` int(11) NOT NULL,
  `puntaje` int(11) NOT NULL DEFAULT 0,
  `logros` text DEFAULT NULL
) ;

--
-- Volcado de datos para la tabla `sessionrank`
--

INSERT INTO `sessionrank` (`id`, `session_id`, `puntaje`, `logros`) VALUES
(1, 1, 706, 'Sobreviviente (10 turnos)'),
(2, 2, 613, 'Sobreviviente (10 turnos)'),
(3, 3, 768, 'Sobreviviente (10 turnos)');

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `sessions`
--

DROP TABLE IF EXISTS `sessions`;
CREATE TABLE `sessions` (
  `id` int(11) NOT NULL,
  `player_id` int(11) NOT NULL,
  `inicio` datetime NOT NULL DEFAULT current_timestamp(),
  `fin` datetime DEFAULT NULL,
  `turnos_jugados` int(11) NOT NULL DEFAULT 0,
  `resultado` enum('en_curso','victoria','derrota','abandonada') NOT NULL DEFAULT 'en_curso'
) ;

--
-- Volcado de datos para la tabla `sessions`
--

INSERT INTO `sessions` (`id`, `player_id`, `inicio`, `fin`, `turnos_jugados`, `resultado`) VALUES
(1, 1, '2026-10-19 18:47:41', NULL, 10, 'en_curso'),
(2, 2, '2026-10-20 11:47:32', NULL, 15, 'en_curso'),
(3, 3, '2026-10-20 13:18:02', NULL, 10, 'en_curso');

-- --------------------------------------------------------

--
-- Estructura Stand-in para la vista `v_ranking`
-- (Véase abajo para la vista actual)
--
DROP VIEW IF EXISTS `v_ranking`;
CREATE TABLE `v_ranking` (
`puntaje` int(11)
,`jugador` varchar(50)
,`turnos_jugados` int(11)
,`resultado` enum('en_curso','victoria','derrota','abandonada')
,`logros` text
);

-- --------------------------------------------------------

--
-- Estructura para la vista `v_ranking`
--
DROP TABLE IF EXISTS `v_ranking`;

DROP VIEW IF EXISTS `v_ranking`;
CREATE ALGORITHM=UNDEFINED DEFINER=`root`@`localhost` SQL SECURITY DEFINER VIEW `v_ranking`  AS SELECT `r`.`puntaje` AS `puntaje`, `p`.`nombre` AS `jugador`, `s`.`turnos_jugados` AS `turnos_jugados`, `s`.`resultado` AS `resultado`, `r`.`logros` AS `logros` FROM ((`sessionrank` `r` join `sessions` `s` on(`s`.`id` = `r`.`session_id`)) join `players` `p` on(`p`.`id` = `s`.`player_id`)) ORDER BY `r`.`puntaje` DESC ;

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `choicelogs`
--
ALTER TABLE `choicelogs`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_choice_outcome` (`decision_id`,`outcome_id`),
  ADD KEY `idx_choicelogs_sess` (`session_id`,`turno`);

--
-- Indices de la tabla `decisionoutcomes`
--
ALTER TABLE `decisionoutcomes`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_outcome_decision` (`decision_id`,`id`);

--
-- Indices de la tabla `decisions`
--
ALTER TABLE `decisions`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indices de la tabla `eventlogs`
--
ALTER TABLE `eventlogs`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_evlog_event` (`event_id`),
  ADD KEY `idx_eventlogs_sess` (`session_id`,`turno`);

--
-- Indices de la tabla `kingdomstate`
--
ALTER TABLE `kingdomstate`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_state_turno` (`session_id`,`turno`);

--
-- Indices de la tabla `players`
--
ALTER TABLE `players`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`),
  ADD UNIQUE KEY `email` (`email`);

--
-- Indices de la tabla `randomevents`
--
ALTER TABLE `randomevents`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indices de la tabla `sessionrank`
--
ALTER TABLE `sessionrank`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `session_id` (`session_id`),
  ADD KEY `idx_rank_puntaje` (`puntaje`);

--
-- Indices de la tabla `sessions`
--
ALTER TABLE `sessions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_sessions_player` (`player_id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `choicelogs`
--
ALTER TABLE `choicelogs`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `decisionoutcomes`
--
ALTER TABLE `decisionoutcomes`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT de la tabla `decisions`
--
ALTER TABLE `decisions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT de la tabla `eventlogs`
--
ALTER TABLE `eventlogs`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `kingdomstate`
--
ALTER TABLE `kingdomstate`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `players`
--
ALTER TABLE `players`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT de la tabla `randomevents`
--
ALTER TABLE `randomevents`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `sessionrank`
--
ALTER TABLE `sessionrank`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `sessions`
--
ALTER TABLE `sessions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `choicelogs`
--
ALTER TABLE `choicelogs`
  ADD CONSTRAINT `fk_choice_decision` FOREIGN KEY (`decision_id`) REFERENCES `decisions` (`id`),
  ADD CONSTRAINT `fk_choice_outcome` FOREIGN KEY (`decision_id`,`outcome_id`) REFERENCES `decisionoutcomes` (`decision_id`, `id`),
  ADD CONSTRAINT `fk_choice_session` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `decisionoutcomes`
--
ALTER TABLE `decisionoutcomes`
  ADD CONSTRAINT `fk_outcome_decision` FOREIGN KEY (`decision_id`) REFERENCES `decisions` (`id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `eventlogs`
--
ALTER TABLE `eventlogs`
  ADD CONSTRAINT `fk_evlog_event` FOREIGN KEY (`event_id`) REFERENCES `randomevents` (`id`),
  ADD CONSTRAINT `fk_evlog_session` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `kingdomstate`
--
ALTER TABLE `kingdomstate`
  ADD CONSTRAINT `fk_state_session` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `sessionrank`
--
ALTER TABLE `sessionrank`
  ADD CONSTRAINT `fk_rank_session` FOREIGN KEY (`session_id`) REFERENCES `sessions` (`id`) ON DELETE CASCADE;

--
-- Filtros para la tabla `sessions`
--
ALTER TABLE `sessions`
  ADD CONSTRAINT `fk_sessions_player` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
