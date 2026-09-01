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
    }
}