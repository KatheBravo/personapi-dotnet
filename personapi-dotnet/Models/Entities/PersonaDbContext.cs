using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// El DbContext es la "puerta" de Entity Framework Core hacia la base persona_db:
/// representa una sesión de trabajo con la base de datos. Cada DbSet equivale a una tabla.
///
/// Lo generó Scaffold-DbContext. El único cambio que le hicimos fue quitar el método
/// OnConfiguring, que traía la cadena de conexión escrita en el código; ahora la cadena
/// vive en appsettings.json y se entrega desde Program.cs (más seguro y más fácil de cambiar).
/// </summary>
public partial class PersonaDbContext : DbContext
{
    public PersonaDbContext()
    {
    }

    // Este es el constructor que usa la aplicación: recibe las opciones (proveedor
    // SQL Server + cadena de conexión) que se configuraron en Program.cs.
    public PersonaDbContext(DbContextOptions<PersonaDbContext> options)
        : base(options)
    {
    }

    // Una propiedad por tabla. Con ellas se consulta (LINQ) y se agregan o borran filas.
    public virtual DbSet<Estudio> Estudios { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Profesion> Profesions { get; set; }

    public virtual DbSet<Telefono> Telefonos { get; set; }

    /// <summary>
    /// Aquí se describe, en código, cómo es cada tabla: nombre real, columnas, tamaños,
    /// llaves primarias y relaciones. EF Core usa esta información para escribir el SQL.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ----- Tabla estudios -----
        modelBuilder.Entity<Estudio>(entity =>
        {
            // Llave primaria compuesta: la combinación profesión + persona no se puede repetir.
            entity.HasKey(e => new { e.IdProf, e.CcPer }).HasName("PK_estudios");

            entity.ToTable("estudios");

            entity.HasIndex(e => e.CcPer, "estudio_persona_fk");

            // HasColumnName conecta el nombre de la propiedad en C# con el de la columna en SQL.
            entity.Property(e => e.IdProf).HasColumnName("id_prof");
            entity.Property(e => e.CcPer).HasColumnName("cc_per");
            entity.Property(e => e.Fecha)
                .HasColumnType("date")
                .HasColumnName("fecha");
            entity.Property(e => e.Univer)
                .HasMaxLength(50)
                .IsUnicode(false)          // VARCHAR y no NVARCHAR, igual que en el script DDL
                .HasColumnName("univer");

            // Relación: muchos estudios pertenecen a una persona.
            entity.HasOne(d => d.CcPerNavigation).WithMany(p => p.Estudios)
                .HasForeignKey(d => d.CcPer)
                .HasConstraintName("estudio_persona_fk");

            // Relación: muchos estudios apuntan a una profesión.
            entity.HasOne(d => d.IdProfNavigation).WithMany(p => p.Estudios)
                .HasForeignKey(d => d.IdProf)
                .HasConstraintName("estudio_profesion_fk");
        });

        // ----- Tabla persona -----
        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.Cc).HasName("PK_persona");

            entity.ToTable("persona");

            entity.Property(e => e.Cc)
                .ValueGeneratedNever()     // la cédula la escribe el usuario; no es autoincremental
                .HasColumnName("cc");
            entity.Property(e => e.Apellido)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Edad).HasColumnName("edad");
            entity.Property(e => e.Genero)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()           // CHAR(1): siempre ocupa exactamente un carácter
                .HasColumnName("genero");
            entity.Property(e => e.Nombre)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        // ----- Tabla profesion -----
        modelBuilder.Entity<Profesion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_profesion");

            entity.ToTable("profesion");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Des)
                .IsUnicode(false)
                .HasColumnName("des");
            entity.Property(e => e.Nom)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("nom");
        });

        // ----- Tabla telefono -----
        modelBuilder.Entity<Telefono>(entity =>
        {
            entity.HasKey(e => e.Num).HasName("PK_telefono");

            entity.ToTable("telefono");

            entity.HasIndex(e => e.Duenio, "telefono_persona_fk");

            entity.Property(e => e.Num)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("num");
            entity.Property(e => e.Duenio).HasColumnName("duenio");
            entity.Property(e => e.Oper)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("oper");

            // Relación: muchos teléfonos pertenecen a una persona (su dueño).
            entity.HasOne(d => d.DuenioNavigation).WithMany(p => p.Telefonos)
                .HasForeignKey(d => d.Duenio)
                .HasConstraintName("telefono_persona_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    // Gancho que deja el scaffold para agregar configuración en otro archivo parcial
    // sin modificar este. En este proyecto no se usa.
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
