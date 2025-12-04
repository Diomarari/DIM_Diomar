using Microsoft.EntityFrameworkCore;
using SalesAnalyticsETL.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Infrastructure.context
{
    public class DataWarehouseContext : DbContext
    {
        public DataWarehouseContext(DbContextOptions<DataWarehouseContext> options)
            : base(options)
        {
            this.Database.SetCommandTimeout(300);
        }

        public DbSet<DimCliente> DimClientes { get; set; }
        public DbSet<DimProducto> DimProductos { get; set; }
        public DbSet<DimTiempo> DimTiempos { get; set; }
        public DbSet<DimEstado> DimEstados { get; set; }
        public DbSet<FactVentas> FactVentas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
                        
            modelBuilder.Entity<DimCliente>()
                .HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_Dim_Cliente_Email");

            modelBuilder.Entity<FactVentas>()
                .HasIndex(f => f.OrdenID)
                .IsUnique()
                .HasDatabaseName("IX_Fact_Ventas_OrdenID");

            modelBuilder.Entity<FactVentas>()
                .HasIndex(f => new { f.ClienteID, f.ProductoID, f.TiempoID })
                .HasDatabaseName("IX_Fact_Ventas_Dimensions");

            modelBuilder.Entity<FactVentas>()
                .HasIndex(f => f.TiempoID)
                .HasDatabaseName("IX_Fact_Ventas_TiempoID");

            modelBuilder.Entity<FactVentas>()
                .HasIndex(f => f.ClienteID)
                .HasDatabaseName("IX_Fact_Ventas_ClienteID");

            modelBuilder.Entity<FactVentas>()
                .HasIndex(f => f.ProductoID)
                .HasDatabaseName("IX_Fact_Ventas_ProductoID");

            modelBuilder.Entity<DimProducto>()
                .HasIndex(p => p.NombreProducto)
                .HasDatabaseName("IX_Dim_Producto_Nombre");

            modelBuilder.Entity<DimProducto>()
                .HasIndex(p => p.Categoria)
                .HasDatabaseName("IX_Dim_Producto_Categoria");

            modelBuilder.Entity<DimTiempo>()
                .HasIndex(t => t.Fecha)
                .IsUnique()
                .HasDatabaseName("IX_Dim_Tiempo_Fecha");

            modelBuilder.Entity<DimTiempo>()
                .HasIndex(t => new { t.Anio, t.Mes })
                .HasDatabaseName("IX_Dim_Tiempo_AnioMes");

            var fechaSeed = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            modelBuilder.Entity<DimEstado>().HasData(
                new DimEstado 
                { 
                    EstadoID = 1, 
                    NombreEstado = "PENDIENTE", 
                    Descripcion = "Orden pendiente de procesamiento", 
                    Activo = true,
                    FechaCreacion = fechaSeed,
                    FechaActualizacion = null
                },
                new DimEstado 
                { 
                    EstadoID = 2, 
                    NombreEstado = "PROCESANDO", 
                    Descripcion = "Orden en proceso", 
                    Activo = true,
                    FechaCreacion = fechaSeed,
                    FechaActualizacion = null
                },
                new DimEstado 
                { 
                    EstadoID = 3, 
                    NombreEstado = "COMPLETADO", 
                    Descripcion = "Orden completada exitosamente", 
                    Activo = true,
                    FechaCreacion = fechaSeed,
                    FechaActualizacion = null
                },
                new DimEstado 
                { 
                    EstadoID = 4, 
                    NombreEstado = "CANCELADO", 
                    Descripcion = "Orden cancelada", 
                    Activo = true,
                    FechaCreacion = fechaSeed,
                    FechaActualizacion = null
                },
                new DimEstado 
                { 
                    EstadoID = 5, 
                    NombreEstado = "DEVUELTO", 
                    Descripcion = "Orden devuelta", 
                    Activo = true,
                    FechaCreacion = fechaSeed,
                    FechaActualizacion = null
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder
                .EnableSensitiveDataLogging(false) 
                .EnableDetailedErrors(true)
                .ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.SensitiveDataLoggingEnabledWarning);
                });
        }
    }
}
