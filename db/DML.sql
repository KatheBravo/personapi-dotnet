/* =====================================================================
   Laboratorio 1 - personapi-dotnet
   Script DML (Data Manipulation Language): datos de prueba.
   Solo inserta si las tablas estan vacias, para poder re-ejecutarlo.
   ===================================================================== */

USE persona_db;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.profesion)
BEGIN
    INSERT INTO dbo.profesion (id, nom, des) VALUES
        (1, 'Ingenieria de Sistemas',   'Diseno, desarrollo y gestion de sistemas de informacion y software.'),
        (2, 'Ingenieria Electronica',   'Diseno de circuitos, sistemas embebidos y telecomunicaciones.'),
        (3, 'Medicina',                 'Prevencion, diagnostico y tratamiento de enfermedades.'),
        (4, 'Derecho',                  'Estudio y aplicacion del ordenamiento juridico.'),
        (5, 'Administracion de Empresas','Planeacion, organizacion y control de organizaciones.');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.persona)
BEGIN
    INSERT INTO dbo.persona (cc, nombre, apellido, genero, edad) VALUES
        (1010101010, 'Ana',     'Martinez', 'F', 28),
        (1020202020, 'Carlos',  'Rodriguez','M', 35),
        (1030303030, 'Laura',   'Gomez',    'F', 22),
        (1040404040, 'Andres',  'Lopez',    'M', 41),
        (1050505050, 'Valentina','Torres',  'F', NULL);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.estudios)
BEGIN
    INSERT INTO dbo.estudios (id_prof, cc_per, fecha, univer) VALUES
        (1, 1010101010, '2019-12-06', 'Pontificia Universidad Javeriana'),
        (2, 1020202020, '2012-06-15', 'Universidad Nacional de Colombia'),
        (5, 1020202020, '2016-11-30', 'Universidad de los Andes'),
        (3, 1030303030, NULL,         'Universidad del Rosario'),
        (4, 1040404040, '2008-07-20', 'Universidad Externado de Colombia'),
        (1, 1040404040, '2014-03-10', 'Universidad Distrital');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.telefono)
BEGIN
    INSERT INTO dbo.telefono (num, oper, duenio) VALUES
        ('3001234567', 'Claro',    1010101010),
        ('3109876543', 'Movistar', 1010101010),
        ('3205551234', 'Tigo',     1020202020),
        ('3154443322', 'Claro',    1030303030),
        ('6013334455', 'ETB',      1040404040);
END
GO

-- Verificacion rapida
SELECT 'profesion' AS tabla, COUNT(*) AS filas FROM dbo.profesion
UNION ALL SELECT 'persona',  COUNT(*) FROM dbo.persona
UNION ALL SELECT 'estudios', COUNT(*) FROM dbo.estudios
UNION ALL SELECT 'telefono', COUNT(*) FROM dbo.telefono;
GO
