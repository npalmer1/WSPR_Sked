-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Apr 05, 2026 at 01:39 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `wspr`
--
CREATE DATABASE IF NOT EXISTS `wspr` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wspr`;

-- --------------------------------------------------------

--
-- Table structure for table `antennas`
--

CREATE TABLE `antennas` (
  `AntNo` int(11) NOT NULL,
  `Antenna` varchar(50) NOT NULL,
  `Description` varchar(250) NOT NULL,
  `Switch` int(11) NOT NULL,
  `Tuner` int(11) NOT NULL,
  `Rotator` tinyint(1) NOT NULL,
  `SwitchPort` int(11) NOT NULL,
  `TunerPort` int(11) NOT NULL,
  `Azimuth` int(11) NOT NULL,
  `Switch2` int(11) NOT NULL DEFAULT 0,
  `SwitchPort2` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `filters`
--

CREATE TABLE `filters` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Protocol` varchar(50) NOT NULL,
  `Port` varchar(50) NOT NULL,
  `IP` varchar(50) NOT NULL,
  `Baud` varchar(50) NOT NULL,
  `Serial` varchar(50) NOT NULL,
  `Channels` int(11) NOT NULL,
  `ChannelType` varchar(50) NOT NULL,
  `Type` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `frequencies`
--

CREATE TABLE `frequencies` (
  `Frequency` double NOT NULL,
  `AudioLevel` double NOT NULL,
  `RXAudioLevel` double NOT NULL,
  `Region` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `rigctl`
--

CREATE TABLE `rigctl` (
  `RigctlID` smallint(3) NOT NULL,
  `Radio` varchar(500) NOT NULL,
  `COMport` varchar(50) NOT NULL,
  `Baud` varchar(50) NOT NULL,
  `IPv4` varchar(50) NOT NULL,
  `Port` varchar(50) NOT NULL,
  `norigctld` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `rigs`
--

CREATE TABLE `rigs` (
  `id` int(11) NOT NULL,
  `rigname` varchar(500) NOT NULL,
  `type` int(11) NOT NULL,
  `selected` int(11) NOT NULL,
  `TXcommand` varchar(500) NOT NULL,
  `RXcommand` varchar(500) NOT NULL,
  `TXptt` varchar(500) DEFAULT NULL,
  `command1` varchar(500) DEFAULT NULL,
  `command2` varchar(500) DEFAULT NULL,
  `command3` varchar(500) DEFAULT NULL,
  `command4` varchar(500) DEFAULT NULL,
  `reply1` varchar(500) NOT NULL,
  `reply2` varchar(500) NOT NULL,
  `reply3` varchar(500) NOT NULL,
  `reply4` varchar(500) NOT NULL,
  `Protocol` varchar(50) DEFAULT NULL,
  `Port` varchar(50) DEFAULT NULL,
  `IP` varchar(50) DEFAULT NULL,
  `Baud` varchar(50) DEFAULT NULL,
  `Serial` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `switches`
--

CREATE TABLE `switches` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Protocol` varchar(50) NOT NULL,
  `Port` varchar(50) NOT NULL,
  `IP` varchar(50) NOT NULL,
  `Baud` varchar(50) NOT NULL,
  `Serial` varchar(50) NOT NULL,
  `Channels` int(11) NOT NULL,
  `ChannelType` varchar(50) NOT NULL,
  `Type` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `tuners`
--

CREATE TABLE `tuners` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Protocol` varchar(50) NOT NULL,
  `Port` varchar(50) NOT NULL,
  `IP` varchar(50) NOT NULL,
  `Baud` varchar(50) NOT NULL,
  `Serial` varchar(50) NOT NULL,
  `Channels` int(11) NOT NULL,
  `ChannelType` varchar(50) NOT NULL,
  `Type` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `antennas`
--
ALTER TABLE `antennas`
  ADD PRIMARY KEY (`AntNo`);

--
-- Indexes for table `filters`
--
ALTER TABLE `filters`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `frequencies`
--
ALTER TABLE `frequencies`
  ADD PRIMARY KEY (`Frequency`);

--
-- Indexes for table `rigctl`
--
ALTER TABLE `rigctl`
  ADD PRIMARY KEY (`RigctlID`);

--
-- Indexes for table `rigs`
--
ALTER TABLE `rigs`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `switches`
--
ALTER TABLE `switches`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `tuners`
--
ALTER TABLE `tuners`
  ADD PRIMARY KEY (`Id`);
--
-- Database: `wspr_configs`
--
CREATE DATABASE IF NOT EXISTS `wspr_configs` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wspr_configs`;

-- --------------------------------------------------------

--
-- Table structure for table `rxsettings`
--

CREATE TABLE `rxsettings` (
  `id` int(11) NOT NULL,
  `outputname` varchar(100) NOT NULL,
  `outputdevice` int(11) NOT NULL,
  `inputname` varchar(100) NOT NULL,
  `inputdevice` int(11) NOT NULL,
  `outlevel` int(11) NOT NULL,
  `inlevel` int(11) NOT NULL,
  `wsprdpath` varchar(500) NOT NULL,
  `samedev` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `settings`
--

CREATE TABLE `settings` (
  `ConfigID` int(3) NOT NULL,
  `Callsign` varchar(250) NOT NULL,
  `BaseCall` varchar(250) NOT NULL,
  `Offset` int(4) NOT NULL,
  `DefaultF` double NOT NULL,
  `Power` int(10) NOT NULL,
  `PowerW` int(5) NOT NULL,
  `FList` varchar(1024) NOT NULL,
  `Locator` varchar(50) NOT NULL,
  `LocatorLong` tinyint(1) NOT NULL,
  `DefaultAnt` varchar(250) NOT NULL,
  `Alpha` double NOT NULL,
  `DefaultAudio` int(11) NOT NULL,
  `HamlibPath` varchar(500) NOT NULL,
  `MsgType` int(11) NOT NULL,
  `TimeZone` varchar(250) NOT NULL,
  `AllowType2` tinyint(1) NOT NULL,
  `oneMsg` tinyint(1) NOT NULL,
  `WsprmsgPath` varchar(500) NOT NULL,
  `VolumeLevel` int(11) NOT NULL,
  `stopsolar` tinyint(1) NOT NULL,
  `stopRX` tinyint(1) NOT NULL,
  `Riseoff` int(11) NOT NULL,
  `Setoff` int(11) NOT NULL,
  `RXonly` tinyint(1) NOT NULL,
  `SlotDB` varchar(250) NOT NULL,
  `selectedFilter` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `rxsettings`
--
ALTER TABLE `rxsettings`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `settings`
--
ALTER TABLE `settings`
  ADD PRIMARY KEY (`ConfigID`);
--
-- Database: `wspr_grey`
--
CREATE DATABASE IF NOT EXISTS `wspr_grey` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wspr_grey`;

-- --------------------------------------------------------

--
-- Table structure for table `sunrise_sunset`
--

CREATE TABLE `sunrise_sunset` (
  `locator` varchar(8) NOT NULL,
  `date` varchar(10) NOT NULL,
  `sunrise` varchar(6) NOT NULL,
  `sunset` varchar(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `sunrise_sunset`
--
ALTER TABLE `sunrise_sunset`
  ADD PRIMARY KEY (`locator`,`date`);
--
-- Database: `wspr_rpt`
--
CREATE DATABASE IF NOT EXISTS `wspr_rpt` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wspr_rpt`;

-- --------------------------------------------------------

--
-- Table structure for table `received`
--

CREATE TABLE `received` (
  `datetime` datetime NOT NULL,
  `band` smallint(11) NOT NULL,
  `tx_sign` varchar(15) NOT NULL,
  `tx_loc` varchar(10) NOT NULL,
  `frequency` double NOT NULL,
  `power` smallint(11) NOT NULL,
  `snr` mediumint(11) NOT NULL,
  `drift` smallint(6) NOT NULL,
  `distance` int(11) NOT NULL,
  `azimuth` smallint(11) NOT NULL,
  `reporter` varchar(20) NOT NULL,
  `reporter_loc` varchar(10) NOT NULL,
  `dt` float NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `rxconfig`
--

CREATE TABLE `rxconfig` (
  `id` int(11) NOT NULL,
  `deep` tinyint(1) NOT NULL,
  `quick` tinyint(1) NOT NULL,
  `osd` int(11) NOT NULL,
  `upload` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `received`
--
ALTER TABLE `received`
  ADD PRIMARY KEY (`datetime`,`tx_sign`,`frequency`),
  ADD KEY `idx_received_datetime` (`datetime`),
  ADD KEY `idx_received_frequency` (`frequency`);

--
-- Indexes for table `rxconfig`
--
ALTER TABLE `rxconfig`
  ADD PRIMARY KEY (`id`);
--
-- Database: `wspr_rx`
--
CREATE DATABASE IF NOT EXISTS `wspr_rx` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wspr_rx`;

-- --------------------------------------------------------

--
-- Table structure for table `reported`
--

CREATE TABLE `reported` (
  `id` bigint(11) NOT NULL,
  `time` datetime NOT NULL,
  `band` smallint(9) NOT NULL,
  `rx_sign` varchar(25) NOT NULL,
  `rx_lat` float NOT NULL,
  `rx_lon` float NOT NULL,
  `rx_loc` varchar(8) NOT NULL,
  `tx_sign` varchar(15) NOT NULL,
  `tx_lat` float NOT NULL,
  `tx_lon` float NOT NULL,
  `tx_loc` varchar(8) NOT NULL,
  `distance` mediumint(9) NOT NULL,
  `azimuth` mediumint(9) NOT NULL,
  `rx_azimuth` mediumint(9) NOT NULL,
  `frequency` int(11) NOT NULL,
  `power` smallint(6) NOT NULL,
  `snr` smallint(6) NOT NULL,
  `drift` smallint(6) NOT NULL,
  `version` varchar(20) NOT NULL,
  `code` smallint(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `reported`
--
ALTER TABLE `reported`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `id` (`id`),
  ADD KEY `idx_reported_time` (`time`),
  ADD KEY `idx_reported_band` (`band`),
  ADD KEY `idx_reported_time_band` (`time`,`band`),
  ADD KEY `idx_reported_rx_sign` (`rx_sign`),
  ADD KEY `idx_reported_distance` (`distance`),
  ADD KEY `idx_reported_version` (`version`);
--
-- Database: `wspr_slots`
--
CREATE DATABASE IF NOT EXISTS `wspr_slots` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wspr_slots`;

-- --------------------------------------------------------

--
-- Table structure for table `slots`
--

CREATE TABLE `slots` (
  `Date` date NOT NULL,
  `Time` time NOT NULL,
  `Frequency` double NOT NULL,
  `Offset` int(11) NOT NULL,
  `Power` int(11) NOT NULL,
  `PowerW` double NOT NULL,
  `Antenna` varchar(250) NOT NULL,
  `Tuner` int(3) NOT NULL,
  `Switch` int(3) NOT NULL,
  `SwitchPort` int(11) NOT NULL,
  `Rotator` varchar(3) NOT NULL,
  `Azimuth` int(11) NOT NULL,
  `Elevation` int(11) NOT NULL,
  `End` date NOT NULL,
  `Active` tinyint(1) NOT NULL,
  `Repeating` tinyint(1) NOT NULL,
  `TimeEnd` time NOT NULL,
  `RptTime` tinyint(1) NOT NULL,
  `Parent` varchar(250) NOT NULL,
  `Audio` int(11) NOT NULL,
  `SlotNo` int(11) NOT NULL,
  `MsgType` int(11) NOT NULL,
  `RptType` tinyint(4) NOT NULL,
  `GreyOffset` int(4) NOT NULL,
  `Switch2` int(11) NOT NULL DEFAULT 0,
  `SwitchPort2` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `slots`
--
ALTER TABLE `slots`
  ADD PRIMARY KEY (`Date`,`Time`),
  ADD KEY `idx_slots_date` (`Date`),
  ADD KEY `idx_slots_datetime` (`Date`,`Time`),
  ADD KEY `idx_slots_parent` (`Parent`);
--
-- Database: `wspr_slots_test`
--
CREATE DATABASE IF NOT EXISTS `wspr_slots_test` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wspr_slots_test`;

-- --------------------------------------------------------

--
-- Table structure for table `slots`
--

CREATE TABLE `slots` (
  `date` date NOT NULL,
  `time` time NOT NULL,
  `frequency` double NOT NULL,
  `offset` int(11) NOT NULL,
  `power` int(11) NOT NULL,
  `powerw` double DEFAULT NULL,
  `antenna` varchar(250) NOT NULL,
  `tuner` int(3) NOT NULL,
  `switch` int(3) NOT NULL,
  `switchport` int(11) NOT NULL,
  `rotator` varchar(3) NOT NULL,
  `azimuth` int(11) NOT NULL,
  `elevation` int(11) NOT NULL,
  `end` date DEFAULT NULL,
  `active` tinyint(1) NOT NULL,
  `repeating` tinyint(1) NOT NULL,
  `timeend` time NOT NULL,
  `rpttime` tinyint(1) NOT NULL,
  `parent` varchar(250) NOT NULL,
  `audio` int(11) NOT NULL,
  `slotno` int(11) NOT NULL,
  `msgtype` int(11) NOT NULL,
  `rpttype` tinyint(4) NOT NULL,
  `greyoffset` int(4) NOT NULL,
  `switch2` int(11) NOT NULL DEFAULT 0,
  `switchport2` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `slots`
--
ALTER TABLE `slots`
  ADD PRIMARY KEY (`date`,`time`);
--
-- Database: `wspr_sol`
--
CREATE DATABASE IF NOT EXISTS `wspr_sol` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `wspr_sol`;

-- --------------------------------------------------------

--
-- Table structure for table `weather`
--

CREATE TABLE `weather` (
  `datetime` datetime NOT NULL,
  `Ap` int(11) DEFAULT NULL,
  `Kp00` float DEFAULT NULL,
  `Kp03` float DEFAULT NULL,
  `Kp06` float DEFAULT NULL,
  `Kp09` float DEFAULT NULL,
  `Kp12` float DEFAULT NULL,
  `Kp15` float DEFAULT NULL,
  `Kp18` float DEFAULT NULL,
  `Kp21` float DEFAULT NULL,
  `flux` float DEFAULT NULL,
  `SSN` int(11) DEFAULT NULL,
  `Xray` varchar(10) DEFAULT NULL,
  `pf00` varchar(25) DEFAULT NULL,
  `pf03` varchar(25) DEFAULT NULL,
  `pf06` varchar(25) DEFAULT NULL,
  `pf09` varchar(25) DEFAULT NULL,
  `pf12` varchar(25) DEFAULT NULL,
  `pf15` varchar(25) DEFAULT NULL,
  `pf18` varchar(25) DEFAULT NULL,
  `pf21` varchar(25) DEFAULT NULL,
  `fl00` varchar(300) DEFAULT NULL,
  `fl03` varchar(300) DEFAULT NULL,
  `fl06` varchar(300) DEFAULT NULL,
  `fl09` varchar(300) DEFAULT NULL,
  `fl12` varchar(300) DEFAULT NULL,
  `fl15` varchar(300) DEFAULT NULL,
  `fl18` varchar(180) DEFAULT NULL,
  `fl21` varchar(300) DEFAULT NULL,
  `s00` varchar(300) DEFAULT NULL,
  `s03` varchar(300) DEFAULT NULL,
  `s06` varchar(300) DEFAULT NULL,
  `s09` varchar(300) DEFAULT NULL,
  `s12` varchar(300) DEFAULT NULL,
  `s15` varchar(300) DEFAULT NULL,
  `s18` varchar(300) DEFAULT NULL,
  `s21` varchar(300) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `weather`
--
ALTER TABLE `weather`
  ADD PRIMARY KEY (`datetime`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
