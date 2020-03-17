IF DB_ID('NPGeek') IS NULL CREATE DATABASE NPGeek
GO

USE NPGeek
GO

IF OBJECT_ID('AddNationalPark')					IS NOT NULL DROP PROCEDURE	AddNationalPark
IF OBJECT_ID('AddEntranceFees')					IS NOT NULL DROP PROCEDURE	AddEntranceFees
IF OBJECT_ID('AddOperatingHours')				IS NOT NULL DROP PROCEDURE	AddOperatingHours
IF OBJECT_ID('AddImage')						IS NOT NULL DROP PROCEDURE	AddImage

IF OBJECT_ID('NationalParkExists')				IS NOT NULL DROP FUNCTION	NationalParkExists

IF OBJECT_ID('ActualNationalParks')				IS NOT NULL	DROP VIEW		ActualNationalParks

IF OBJECT_ID('national_park_images')			IS NOT NULL DROP TABLE		national_park_images
IF OBJECT_ID('national_park_operating_hours')	IS NOT NULL DROP TABLE		national_park_operating_hours
IF OBJECT_ID('national_park_entrance_fees')		IS NOT NULL DROP TABLE		national_park_entrance_fees
IF OBJECT_ID('national_park')					IS NOT NULL DROP TABLE		national_park
GO

CREATE TABLE national_park
(
	national_park_id		INT				IDENTITY(1,1),
	fullname				VARCHAR(1000)	NOT NULL,
	park_code				VARCHAR(10)		NOT NULL UNIQUE,
	designation				VARCHAR(50)		NOT NULL,
	name					VARCHAR(1000)	NOT NULL,
	state					VARCHAR(10)		NOT NULL,
	longitude				VARCHAR(16)		NOT NULL,
	latitude				VARCHAR(16)		NOT NULL,
	description				VARCHAR(1000)	NOT NULL,
	directions				VARCHAR(1000)	NOT NULL,
	directions_url			VARCHAR(1000)	NOT NULL,
	url						VARCHAR(1000)	NOT NULL,
	CONSTRAINT national_park_pk PRIMARY KEY (national_park_id)
)

CREATE TABLE national_park_entrance_fees
(
	national_park_id		INT				NOT NULL,
	entrance_fee_id			INT				IDENTITY(1,1),
	cost					DEC				NOT NULL,
	title					VARCHAR(1000)	NOT NULL,
	description				VARCHAR(1000)	NOT NULL,
	CONSTRAINT national_park_entrance_fees_pk PRIMARY KEY (national_park_id, entrance_fee_id)
)

CREATE TABLE national_park_operating_hours
(
	national_park_id		INT				NOT NULL,
	operating_hours_id		INT				IDENTITY(1,1),
	description				VARCHAR(1000)		NULL,
	sunday					VARCHAR(1000)		NULL,
	monday					VARCHAR(1000)		NULL,
	tuesday					VARCHAR(1000)		NULL,
	wednesday				VARCHAR(1000)		NULL,
	thursday				VARCHAR(1000)		NULL,
	friday					VARCHAR(1000)		NULL,
	saturday				VARCHAR(1000)		NULL,
	name					VARCHAR(1000)		NULL,
	is_exception			BIT					NULL,
	start_date				DATE				NULL,
	end_date				DATE				NULL,
	CONSTRAINT national_park_operating_hours_pk PRIMARY KEY (national_park_id, operating_hours_id)
)

CREATE TABLE national_park_images
(
	national_park_id		INT					NOT NULL,
	image_id				INT					IDENTITY(1,1),
	url						VARCHAR(1000)		NOT NULL,
	title					VARCHAR(50)			NOT NULL,
	caption					VARCHAR(1000)		NOT NULL,
	alt_text				VARCHAR(1000)		NOT NULL,
	credit					VARCHAR(1000)		NOT NULL,
	CONSTRAINT national_park_images_pk PRIMARY KEY (national_park_id, image_id)
)

ALTER TABLE national_park_entrance_fees ADD CONSTRAINT national_park_entrance_fees_fk
FOREIGN KEY (national_park_id) REFERENCES national_park (national_park_id)

ALTER TABLE national_park_operating_hours ADD CONSTRAINT national_park_operating_hours_fk
FOREIGN KEY (national_park_id) REFERENCES national_park (national_park_id)

ALTER TABLE national_park_images ADD CONSTRAINT national_park_images_fk
FOREIGN KEY (national_park_id) REFERENCES national_park (national_park_id)
GO

CREATE VIEW ActualNationalParks
AS

SELECT
	*
FROM
	national_park
WHERE
	designation LIKE 'National%'
AND designation LIKE '%Park'

GO

CREATE PROCEDURE AddNationalPark
	@fullname		VARCHAR(1000),
	@park_code		VARCHAR(10),
	@designation	VARCHAR(50),
	@name			VARCHAR(1000),
	@state			VARCHAR(10),
	@longitude		VARCHAR(16),
	@latitude		VARCHAR(16),
	@description	VARCHAR(1000),
	@directions		VARCHAR(1000),
	@directions_url	VARCHAR(1000),
	@url			VARCHAR(1000)
AS
BEGIN TRANSACTION
	DECLARE @newId INT = -1
	IF((SELECT COUNT(*) FROM national_park WHERE park_code = @park_code)=0)
	BEGIN
	
		INSERT INTO national_park (fullname, park_code, designation, name, state, longitude, latitude, description, directions, directions_url, url)
		VALUES (@fullname, @park_code, @designation, @name, @state, @longitude, @latitude, @description, @directions, @directions_url, @url)

		SET @newId = SCOPE_IDENTITY()
	END
	SELECT @newId	 
	 
COMMIT TRANSACTION
GO

CREATE FUNCTION NationalParkExists(@national_park_id INT)
RETURNS BIT
AS
BEGIN
	DECLARE @exists BIT  
	
	SET @exists =
	(
		SELECT 
			CASE 
				WHEN(COUNT(*) > 0) 
				THEN 1 
				ELSE 0 
			END 
		FROM
			national_park 
		WHERE 
			national_park_id = @national_park_id
	)

	RETURN @exists
END
GO

CREATE PROCEDURE AddEntranceFees
	@national_park_id	INT,
	@cost				DECIMAL,
	@title				VARCHAR(1000),
	@description		VARCHAR(1000)
AS 
BEGIN TRANSACTION
DECLARE @new_id INT = -1 -- -1 as the new id will denote failure to insert
	
	-- Insert only if valid id
	IF(dbo.NationalParkExists(@national_park_id) = 1)
	BEGIN
		INSERT INTO national_park_entrance_fees
		VALUES (@national_park_id, @cost, @title, @description)

		SET @new_id = SCOPE_IDENTITY()
	END

	SELECT @new_id -- return new id
COMMIT TRANSACTION
GO

CREATE PROCEDURE AddOperatingHours
	@national_park_id	INT,
	@description		VARCHAR(1000) = NULL,
	@sunday				VARCHAR(1000) = NULL,
	@monday				VARCHAR(1000) = NULL,
	@tuesday			VARCHAR(1000) = NULL,
	@wednesday			VARCHAR(1000) = NULL,
	@thursday			VARCHAR(1000) = NULL,
	@friday				VARCHAR(1000) = NULL,
	@saturday			VARCHAR(1000) = NULL,
	@name				VARCHAR(1000) = NULL,
	@is_exception		BIT,
	@start_date			DATE = NULL,
	@end_date			DATE = NUll
AS

BEGIN TRANSACTION

	DECLARE @new_id INT = -1 -- -1 as the new id will denote failure to insert
	
	-- Insert only if valid id
	IF(dbo.NationalParkExists(@national_park_id) = 1)
	BEGIN
		INSERT INTO national_park_operating_hours 
		VALUES (@national_park_id, @description, @sunday, @monday, @tuesday, @wednesday, @thursday, @friday, @saturday, @name, @is_exception, @start_date, @end_date)

		SET @new_id = SCOPE_IDENTITY()
	END

	SELECT @new_id -- return new id

COMMIT TRANSACTION
GO

CREATE PROCEDURE AddImage
	@national_park_id	INT,
	@url				VARCHAR(1000),
	@title				VARCHAR(50),
	@caption			VARCHAR(1000),
	@alt_text			VARCHAR(1000),
	@credit				VARCHAR(1000)
AS
BEGIN TRANSACTION
	
	DECLARE @new_id INT = -1

	IF(dbo.NationalParkExists(@national_park_id) = 1)
	BEGIN
		INSERT INTO national_park_images
		VALUES (@national_park_id, @url, @title, @caption, @alt_text, @credit)

		SET @new_id = SCOPE_IDENTITY()
	END

	SELECT @new_id

COMMIT TRANSACTION
