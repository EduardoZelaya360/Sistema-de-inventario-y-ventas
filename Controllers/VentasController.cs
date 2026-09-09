using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_de_inventario_y_ventas.Data;
using Sistema_de_inventario_y_ventas.Models;

namespace Sistema_de_inventario_y_ventas.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Ventas
        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var nombreUsuario = HttpContext.Session.GetString("UsuarioNombre");

            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.Ventas.Include(v => v.Detalles).AsQueryable();
            if (rol.Equals("Vendedor", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(nombreUsuario))
            {
                query = query.Where(v => v.Vendedor == nombreUsuario);
            }

            var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();

            await CargarProductosYUsuarios();
            ViewBag.RolUsuario = rol;

            return View(ventas);
        }

        // POST: /Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] string vendedor, [FromForm] string itemsJson)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var nombreUsuario = HttpContext.Session.GetString("UsuarioNombre");

            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (rol.Equals("Vendedor", StringComparison.OrdinalIgnoreCase))
            {
                vendedor = nombreUsuario ?? string.Empty;
            }

            List<ItemCarrito> items;
            try
            {
                items = System.Text.Json.JsonSerializer.Deserialize<List<ItemCarrito>>(itemsJson) ?? new();
            }
            catch
            {
                TempData["Error"] = "Error al leer el carrito.";
                return RedirectToAction(nameof(Index));
            }

            if (!items.Any())
            {
                TempData["Error"] = "Agrega al menos un producto al carrito.";
                return RedirectToAction(nameof(Index));
            }

            var productosDb = new List<Producto>();
            foreach (var item in items)
            {
                var producto = await _context.Inventario.FindAsync(item.ProductoId);
                if (producto == null)
                {
                    TempData["Error"] = "Uno de los productos ya no existe.";
                    return RedirectToAction(nameof(Index));
                }
                if (item.Unidades > producto.Unidades)
                {
                    TempData["Error"] = $"Stock insuficiente para {producto.ProductoNombre}. Disponible: {producto.Unidades}.";
                    return RedirectToAction(nameof(Index));
                }
                productosDb.Add(producto);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = new Venta
                {
                    Vendedor = vendedor,
                    Fecha = DateTime.UtcNow,
                    TotalAPagar = 0
                };
                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                decimal totalGeneral = 0;

                for (int i = 0; i < items.Count; i++)
                {
                    var producto = productosDb[i];
                    var subtotal = producto.PrecioUnitario * items[i].Unidades;
                    totalGeneral += subtotal;

                    _context.DetalleVentas.Add(new DetalleVenta
                    {
                        VentaId = venta.Id,
                        Producto = producto.ProductoNombre,
                        PrecioUnitario = producto.PrecioUnitario,
                        Unidades = items[i].Unidades,
                        Subtotal = subtotal
                    });

                    producto.Unidades -= items[i].Unidades;
                    _context.Inventario.Update(producto);
                }

                venta.TotalAPagar = totalGeneral;
                _context.Ventas.Update(venta);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Ocurrió un error al registrar la venta.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = "Venta registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarProductosYUsuarios()
        {
            ViewBag.Productos = await _context.Inventario
                .Where(p => p.Unidades > 0)
                .OrderBy(p => p.ProductoNombre)
                .ToListAsync();

            ViewBag.Usuarios = await _context.Usuarios
                .OrderBy(u => u.Nombre)
                .ToListAsync();
        }
    }
}