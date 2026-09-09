using Microsoft.EntityFrameworkCore;
using Sistema_de_inventario_y_ventas.Models;

namespace Sistema_de_inventario_y_ventas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Inventario { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.NombreCategoria)
                .HasPrincipalKey(c => c.NombreCategoria);

            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.NombreCategoria)
                .IsUnique();

            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.ProductoNombre)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Nombre)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.VentaId);
        }
    }
}