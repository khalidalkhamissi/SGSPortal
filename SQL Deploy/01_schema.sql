-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: sgs_forms_db
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `approvals`
--

DROP TABLE IF EXISTS `approvals`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `approvals` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ReportKind` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StationId` int NOT NULL,
  `ArrivalReportId` int DEFAULT NULL,
  `DepartureReportId` int DEFAULT NULL,
  `Satisfaction` int NOT NULL,
  `AirlineRemarks` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `AirlineRepName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `AirlineCompany` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `AirlineRepPosition` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `SignatureImage` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Decision` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ReturnReason` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ApprovedByUserId` int NOT NULL,
  `ApprovedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Approvals_ArrivalReportId` (`ArrivalReportId`),
  KEY `IX_Approvals_DepartureReportId` (`DepartureReportId`),
  KEY `IX_Approvals_ReportKind_ArrivalReportId_DepartureReportId` (`ReportKind`,`ArrivalReportId`,`DepartureReportId`),
  CONSTRAINT `FK_Approvals_ArrivalReports_ArrivalReportId` FOREIGN KEY (`ArrivalReportId`) REFERENCES `arrivalreports` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Approvals_DepartureReports_DepartureReportId` FOREIGN KEY (`DepartureReportId`) REFERENCES `departurereports` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `arrivaldelays`
--

DROP TABLE IF EXISTS `arrivaldelays`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `arrivaldelays` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ArrivalReportId` int NOT NULL,
  `Code` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Reason` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DurationMinutes` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ArrivalDelays_ArrivalReportId` (`ArrivalReportId`),
  CONSTRAINT `FK_ArrivalDelays_ArrivalReports_ArrivalReportId` FOREIGN KEY (`ArrivalReportId`) REFERENCES `arrivalreports` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `arrivalreports`
--

DROP TABLE IF EXISTS `arrivalreports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `arrivalreports` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `StationId` int NOT NULL,
  `Status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FlightNo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Route` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `AircraftReg` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `FlightDate` date NOT NULL,
  `Sta` time(6) DEFAULT NULL,
  `Ata` time(6) DEFAULT NULL,
  `SupervisorName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ReportTime` datetime(6) DEFAULT NULL,
  `GateNo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `GateOpen` time(6) DEFAULT NULL,
  `GateClose` time(6) DEFAULT NULL,
  `FirstPaxTime` time(6) DEFAULT NULL,
  `LastPaxTime` time(6) DEFAULT NULL,
  `TtlPaxArr` int NOT NULL,
  `TtlWchr` int NOT NULL,
  `ActualPax` int NOT NULL,
  `NoShow` int NOT NULL,
  `Offloaded` int NOT NULL,
  `Remarks` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedByUserId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  `SubmittedAt` datetime(6) DEFAULT NULL,
  `DraftExpiresAt` datetime(6) DEFAULT NULL,
  `BagTotal` int NOT NULL DEFAULT '0',
  `PaxVip` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_ArrivalReports_FlightDate` (`FlightDate`),
  KEY `IX_ArrivalReports_StationId_Status` (`StationId`,`Status`),
  KEY `IX_ArrivalReports_Status_FlightDate` (`Status`,`FlightDate`),
  CONSTRAINT `FK_ArrivalReports_Stations_StationId` FOREIGN KEY (`StationId`) REFERENCES `stations` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `arrivalservices`
--

DROP TABLE IF EXISTS `arrivalservices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `arrivalservices` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ArrivalReportId` int NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Count` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ArrivalServices_ArrivalReportId` (`ArrivalReportId`),
  CONSTRAINT `FK_ArrivalServices_ArrivalReports_ArrivalReportId` FOREIGN KEY (`ArrivalReportId`) REFERENCES `arrivalreports` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `auditlogs`
--

DROP TABLE IF EXISTS `auditlogs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auditlogs` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `At` datetime(6) NOT NULL,
  `UserId` int DEFAULT NULL,
  `UserName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserRole` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StationId` int DEFAULT NULL,
  `StationCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Action` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ReportKind` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ReportId` int DEFAULT NULL,
  `FlightNo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Details` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_AuditLogs_At` (`At`)
) ENGINE=InnoDB AUTO_INCREMENT=857 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `coordinationactivities`
--

DROP TABLE IF EXISTS `coordinationactivities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `coordinationactivities` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CoordinationSheetId` int NOT NULL,
  `ActivityKey` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ActualStart` time(6) DEFAULT NULL,
  `ActualFinish` time(6) DEFAULT NULL,
  `Remarks` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_CoordinationActivities_CoordinationSheetId` (`CoordinationSheetId`),
  CONSTRAINT `FK_CoordinationActivities_CoordinationSheets_CoordinationSheetId` FOREIGN KEY (`CoordinationSheetId`) REFERENCES `coordinationsheets` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=243 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `coordinationbuses`
--

DROP TABLE IF EXISTS `coordinationbuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `coordinationbuses` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `CoordinationSheetId` int NOT NULL,
  `Phase` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `BusNo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Time` time(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_CoordinationBuses_CoordinationSheetId` (`CoordinationSheetId`),
  CONSTRAINT `FK_CoordinationBuses_CoordinationSheets_CoordinationSheetId` FOREIGN KEY (`CoordinationSheetId`) REFERENCES `coordinationsheets` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=234 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `coordinationsheets`
--

DROP TABLE IF EXISTS `coordinationsheets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `coordinationsheets` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `StationId` int NOT NULL,
  `Status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FlightDate` date NOT NULL,
  `AcType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `AcReg` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ArrFlightNo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ArrFrom` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ArrPaxF` int NOT NULL,
  `ArrPaxJ` int NOT NULL,
  `ArrPaxY` int NOT NULL,
  `DepFlightNo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DepTo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DepPaxF` int NOT NULL,
  `DepPaxJ` int NOT NULL,
  `DepPaxY` int NOT NULL,
  `Turnaround` bit(1) NOT NULL,
  `Transit` bit(1) NOT NULL,
  `Terminating` bit(1) NOT NULL,
  `Originating` bit(1) NOT NULL,
  `Sta` time(6) DEFAULT NULL,
  `Ata` time(6) DEFAULT NULL,
  `Std` time(6) DEFAULT NULL,
  `Atd` time(6) DEFAULT NULL,
  `GainTime` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DlyAmount` int NOT NULL,
  `DelayCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `SupervisorName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ReportTime` datetime(6) DEFAULT NULL,
  `Remarks` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedByUserId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  `SubmittedAt` datetime(6) DEFAULT NULL,
  `DraftExpiresAt` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_CoordinationSheets_FlightDate` (`FlightDate`),
  KEY `IX_CoordinationSheets_StationId_Status` (`StationId`,`Status`),
  KEY `IX_CoordinationSheets_Status_FlightDate` (`Status`,`FlightDate`),
  CONSTRAINT `FK_CoordinationSheets_Stations_StationId` FOREIGN KEY (`StationId`) REFERENCES `stations` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=421 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `departuredelays`
--

DROP TABLE IF EXISTS `departuredelays`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `departuredelays` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `DepartureReportId` int NOT NULL,
  `Code` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Reason` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DurationMinutes` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_DepartureDelays_DepartureReportId` (`DepartureReportId`),
  CONSTRAINT `FK_DepartureDelays_DepartureReports_DepartureReportId` FOREIGN KEY (`DepartureReportId`) REFERENCES `departurereports` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `departurereports`
--

DROP TABLE IF EXISTS `departurereports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `departurereports` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `StationId` int NOT NULL,
  `Status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FlightNo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Route` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `AircraftReg` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `FlightDate` date NOT NULL,
  `Std` time(6) DEFAULT NULL,
  `Atd` time(6) DEFAULT NULL,
  `CounterNo` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CountersStartedAt` time(6) DEFAULT NULL,
  `SupervisorName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ReportTime` datetime(6) DEFAULT NULL,
  `PaxExec` int NOT NULL,
  `PaxF` int NOT NULL,
  `PaxJ` int NOT NULL,
  `PaxW` int NOT NULL,
  `PaxY` int NOT NULL,
  `PaxInf` int NOT NULL,
  `PaxHajj` int NOT NULL,
  `PaxTotal` int NOT NULL,
  `BagNormal` int NOT NULL,
  `BagWchr` int NOT NULL,
  `BagCbbg` int NOT NULL,
  `BagStcr` int NOT NULL,
  `BagAvih` int NOT NULL,
  `BagVip` int NOT NULL,
  `BagZamzam` int NOT NULL,
  `BagHajj` int NOT NULL,
  `BagTotal` int NOT NULL,
  `ExcessTickets` int NOT NULL,
  `ExcessSales` decimal(12,2) NOT NULL,
  `BoardingGate` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `BoardingMode` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `BoardingStarted` time(6) DEFAULT NULL,
  `BoardingCompleted` time(6) DEFAULT NULL,
  `GateOpened` time(6) DEFAULT NULL,
  `GateClosed` time(6) DEFAULT NULL,
  `TotalBuses` int NOT NULL,
  `SpecialHandling` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ActualPax` int NOT NULL,
  `NoShow` int NOT NULL,
  `Offloaded` int NOT NULL,
  `Remarks` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `CreatedByUserId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  `SubmittedAt` datetime(6) DEFAULT NULL,
  `DraftExpiresAt` datetime(6) DEFAULT NULL,
  `PaxVip` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_DepartureReports_FlightDate` (`FlightDate`),
  KEY `IX_DepartureReports_StationId_Status` (`StationId`,`Status`),
  KEY `IX_DepartureReports_Status_FlightDate` (`Status`,`FlightDate`),
  CONSTRAINT `FK_DepartureReports_Stations_StationId` FOREIGN KEY (`StationId`) REFERENCES `stations` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `rolepermissions`
--

DROP TABLE IF EXISTS `rolepermissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rolepermissions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` int NOT NULL,
  `Permission` varchar(60) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_RolePermissions_RoleId_Permission` (`RoleId`,`Permission`),
  CONSTRAINT `FK_RolePermissions_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=115 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(40) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `NameAr` varchar(80) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `NameEn` varchar(80) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Color` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsSystem` bit(1) NOT NULL,
  `IsLocked` bit(1) NOT NULL,
  `IsActive` bit(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Roles_Key` (`Key`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `staffproductivity`
--

DROP TABLE IF EXISTS `staffproductivity`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `staffproductivity` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `DepartureReportId` int NOT NULL,
  `StaffName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PassengersServed` int NOT NULL,
  `Comments` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_StaffProductivity_DepartureReportId` (`DepartureReportId`),
  CONSTRAINT `FK_StaffProductivity_DepartureReports_DepartureReportId` FOREIGN KEY (`DepartureReportId`) REFERENCES `departurereports` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `stations`
--

DROP TABLE IF EXISTS `stations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stations` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `NameAr` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `NameEn` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsActive` bit(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Stations_Code` (`Code`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PasswordHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `StationId` int DEFAULT NULL,
  `IsActive` bit(1) NOT NULL,
  `LastLogin` datetime(6) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  `RoleId` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Users_Email` (`Email`),
  KEY `IX_Users_StationId` (`StationId`),
  KEY `IX_Users_RoleId` (`RoleId`),
  CONSTRAINT `FK_Users_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Users_Stations_StationId` FOREIGN KEY (`StationId`) REFERENCES `stations` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=70 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-08-22 14:38:19
