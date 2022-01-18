-- MySQL Script generated by MySQL Workbench
-- Wed Oct 28 21:07:09 2020
-- Model: New Model    Version: 1.0
-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema apollo
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema apollo
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `apollo` DEFAULT CHARACTER SET utf8 ;
USE `apollo` ;


-- -----------------------------------------------------
-- Table `apollo`.`cinema`
-- -----------------------------------------------------
CREATE TABLE `apollo`.`cinema` ( 
	`id` INT NOT NULL AUTO_INCREMENT , 
	`name` VARCHAR(128) NOT NULL , 
	PRIMARY KEY (`id`), 
	UNIQUE `idx_cinema_uniqueCinemaName` (`name`)
) ENGINE = InnoDB; 

-- -----------------------------------------------------
-- Table `apollo`.`cinemaHall`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`cinemaHall` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `cinemaId` INT NOT NULL,
  `name` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`id`),
  CONSTRAINT `fk_cinemaHall_cinemaId`
    FOREIGN KEY (`cinemaId`)
    REFERENCES `apollo`.`cinema` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION
) ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`category`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`category` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `cinemaId` INT NOT NULL,
  `name` VARCHAR(45) NULL,
  `price` DOUBLE(5,2) NULL,
  PRIMARY KEY (`id`),
  CONSTRAINT `fk_category_cinemaId`
	FOREIGN KEY (`cinemaId`)
	REFERENCES `apollo`.`cinema`(`id`)
	ON DELETE CASCADE
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`hallSeat`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`hallSeat` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `seatNr` INT NOT NULL,
  `cinemaHallId` INT NOT NULL,
  `categoryId` INT NOT NULL,
  `row` INT NOT NULL,
  `col` INT NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `idx_seatPos_hallSeat` (`cinemaHallId` ASC, `row` ASC, `col` ASC) VISIBLE,
  INDEX `fk_hallSeat_categoryId_idx` (`categoryId` ASC) VISIBLE,
  CONSTRAINT `fk_hallSeat_cinemaHallId`
    FOREIGN KEY (`cinemaHallId`)
    REFERENCES `apollo`.`cinemaHall` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_hallSeat_categoryId`
    FOREIGN KEY (`categoryId`)
    REFERENCES `apollo`.`category` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`genre`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`genre` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE `idx_unique_genre_name` (`name`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`movie`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`movie` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `title` VARCHAR(100) NULL,
  `description` LONGTEXT NULL,
  `durationMinutes` INT NULL,
  `image` VARCHAR(256) NULL,
  `trailer` VARCHAR(2048) NULL,
  `releasedate` DATE NULL,
  `tmdbid` INT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`person`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`person` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(255) NULL,
  `tmdbid` INT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`movieRole`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`movieRole` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE `idx_unique_movieRole_name` (`name`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;

-- -----------------------------------------------------
-- Table `apollo`.`movieGenre`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`movieGenre` (
  `movieId` INT NOT NULL,
  `genreId` INT NOT NULL,
  PRIMARY KEY (`movieId`, `genreId`),
  INDEX `fk_personMovie_movieId_idx` (`movieId` ASC) VISIBLE,
  INDEX `fk_personMovie_gereId_idx` (`genreId` ASC) VISIBLE,
  CONSTRAINT `fk_movieGenre_genreId`
    FOREIGN KEY (`genreId`)
    REFERENCES `apollo`.`genre` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_movieGenre_movieId`
    FOREIGN KEY (`movieId`)
    REFERENCES `apollo`.`movie` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;

-- -----------------------------------------------------
-- Table `apollo`.`personMovie`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`personMovie` (
  `personId` INT NOT NULL,
  `movieId` INT NOT NULL,
  `roleId` INT NOT NULL,
  PRIMARY KEY (`personId`, `movieId`, `roleId`),
  INDEX `fk_personMovie_movieId_idx` (`movieId` ASC) VISIBLE,
  INDEX `fk_personMovie_roleId_idx` (`roleId` ASC) VISIBLE,
  CONSTRAINT `fk_personMovie_personId`
    FOREIGN KEY (`personId`)
    REFERENCES `apollo`.`person` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_personMovie_movieId`
    FOREIGN KEY (`movieId`)
    REFERENCES `apollo`.`movie` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_personMovie_roleId`
    FOREIGN KEY (`roleId`)
    REFERENCES `apollo`.`movieRole` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`schedule`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`schedule` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `cinemaHallId` INT NULL,
  `movieId` INT NULL,
  `startTime` DATETIME NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_schedule_cinemaHallId_idx` (`cinemaHallId` ASC) VISIBLE,
  INDEX `fk_schedule_movieId_idx` (`movieId` ASC) VISIBLE,
  CONSTRAINT `fk_schedule_cinemaHallId`
    FOREIGN KEY (`cinemaHallId`)
    REFERENCES `apollo`.`cinemaHall` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_schedule_movieId`
    FOREIGN KEY (`movieId`)
    REFERENCES `apollo`.`movie` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;

--
-- Datenbank: `apollo`
--

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `user`
--

CREATE TABLE `user` (
  `id` int NOT NULL,
  `username` varchar(45) COLLATE utf8_bin NOT NULL,
  `password` varchar(256) COLLATE utf8_bin NULL,
  `email` varchar(45) COLLATE utf8_bin NOT NULL,
  `role` varchar(45) COLLATE utf8_bin NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `userRole`
--

CREATE TABLE `userRole` (
  `name` varchar(45) COLLATE utf8_bin NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

--
-- Indizes der exportierten Tabellen
--

--
-- Indizes für die Tabelle `user`
--
ALTER TABLE `user`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_user_userrole_role` (`role`);

--
-- Indizes für die Tabelle `userRole`
--
ALTER TABLE `userRole`
  ADD PRIMARY KEY (`name`);

--
-- AUTO_INCREMENT für exportierte Tabellen
--

--
-- AUTO_INCREMENT für Tabelle `user`
--
ALTER TABLE `user`
  MODIFY `id` int NOT NULL AUTO_INCREMENT;

--
-- Constraints der exportierten Tabellen
--

--
-- Constraints der Tabelle `user`
--
ALTER TABLE `user`
  ADD CONSTRAINT `fk_user_userrole_role` FOREIGN KEY (`role`) REFERENCES `userRole` (`name`) ON DELETE RESTRICT ON UPDATE RESTRICT;
COMMIT;



-- -----------------------------------------------------
-- Table `apollo`.`reservation`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`reservation` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `scheduleId` INT NULL,
  `userId` INT NULL,
  `isPayed` TINYINT(1) NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_reservation_scheduleId_idx` (`scheduleId` ASC) VISIBLE,
  INDEX `fk_reservation_userId_idx` (`userId` ASC) VISIBLE,
  CONSTRAINT `fk_reservation_scheduleId`
    FOREIGN KEY (`scheduleId`)
    REFERENCES `apollo`.`schedule` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_reservation_userId`
    FOREIGN KEY (`userId`)
    REFERENCES `apollo`.`user` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`reservationSeat`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`reservationSeat` (
  `reservationId` INT NOT NULL,
  `hallSeatId` INT NOT NULL,
  PRIMARY KEY (`reservationId`, `hallSeatId`),
  INDEX `fk_reservationSeat_idx` (`hallSeatId` ASC) VISIBLE,
  CONSTRAINT `fk_reservationSeat_hallSeatId`
    FOREIGN KEY (`hallSeatId`)
    REFERENCES `apollo`.`hallSeat` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_reservationSeat_reservationid`
    FOREIGN KEY (`reservationId`)
    REFERENCES `apollo`.`reservation` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


-- -----------------------------------------------------
-- Table `apollo`.`configuration`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `apollo`.`configuration` (
  `key` VARCHAR(45) NOT NULL,
  `value` JSON NULL,
  PRIMARY KEY (`key`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_bin;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;