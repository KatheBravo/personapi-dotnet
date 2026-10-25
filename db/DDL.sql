/* =====================================================================
   Laboratorio 1 - personapi-dotnet
   Script DDL (Data Definition Language) para Microsoft SQL Server
   Crea la base de datos persona_db, le asigna como propietario al
   usuario sa y crea las tablas profesion, persona, estudios y telefono.

   Equivalencias desde el modelo original (MySQL Workbench):
     INT(6) / INT(15)      -> INT        (SQL Server no maneja ancho en INT)
     TEXT                  -> VARCHAR(MAX) (TEXT esta obsoleto en SQL Server)
     ENUM('M','F')         -> CHAR(1) + restriccion CHECK
   El script es idempotente: se puede ejecutar varias veces sin error.
   ===================================================================== */

IF DB_ID(N'persona_db') IS NULL
BEGIN
    CREATE DATABASE persona_db;
END
GO

-- La base de datos queda a nombre del usuario sa
ALTER AUTHORIZATION ON DATABASE::persona_db TO sa;
GO

USE persona_db;
GO

/* ---------------------------------------------------------------------
   Tabla profesion
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.profesion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.profesion (
        id   INT          NOT NULL,
        nom  VARCHAR(90)  NOT NULL,
        des  VARCHAR(MAX) NULL,
        CONSTRAINT PK_profesion PRIMARY KEY (id)
    );
END
GO

/* ---------------------------------------------------------------------
   Tabla persona
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.persona', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.persona (
        cc        INT          NOT NULL,
        nombre    VARCHAR(45)  NOT NULL,
        apellido  VARCHAR(45)  NOT NULL,
        genero    CHAR(1)      NOT NULL,
        edad      INT          NULL,
        CONSTRAINT PK_persona PRIMARY KEY (cc),
        CONSTRAINT CK_persona_genero CHECK (genero IN ('M', 'F')),
        CONSTRAINT CK_persona_edad   CHECK (edad IS NULL OR edad BETWEEN 0 AND 150)
    );
END
GO

/* ---------------------------------------------------------------------
   Tabla estudios (relacion muchos a muchos persona <-> profesion)
   Llave primaria compuesta (id_prof, cc_per)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.estudios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.estudios (
        id_prof  INT          NOT NULL,
        cc_per   INT          NOT NULL,
        fecha    DATE         NULL,
        univer   VARCHAR(50)  NULL,
        CONSTRAINT PK_estudios PRIMARY KEY (id_prof, cc_per),
        CONSTRAINT estudio_persona_fk FOREIGN KEY (cc_per)
            REFERENCES dbo.persona (cc)
            ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT estudio_profesion_fk FOREIGN KEY (id_prof)
            REFERENCES dbo.profesion (id)
            ON DELETE CASCADE ON UPDATE NO ACTION
    );

    CREATE INDEX estudio_persona_fk ON dbo.estudios (cc_per);
END
GO

/* ---------------------------------------------------------------------
   Tabla telefono (una persona tiene 0..n telefonos)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.telefono', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.telefono (
        num     VARCHAR(15)  NOT NULL,
        oper    VARCHAR(45)  NOT NULL,
        duenio  INT          NOT NULL,
        CONSTRAINT PK_telefono PRIMARY KEY (num),
        CONSTRAINT telefono_persona_fk FOREIGN KEY (duenio)
            REFERENCES dbo.persona (cc)
            ON DELETE CASCADE ON UPDATE NO ACTION
    );

    CREATE INDEX telefono_persona_fk ON dbo.telefono (duenio);
END
GO
