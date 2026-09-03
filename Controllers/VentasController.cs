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

        // GET: /Ventas (Vista unificada: Registro + Historial)
        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var nombreUsuario = HttpContext.Session.GetString("UsuarioNombre");

            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Filtrar historial según el rol de manera segura
            var query = _context.Ventas.AsQueryable();
            if (rol.Equals("Vendedor", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(nombreUsuario))
            {
                query = query.Where(v => v.Vendedor == nombreUsuario);
            }

            var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();

            await CargarProductosYUsuarios();

            // Pasamos el modelo del formulario inicializado
            ViewBag.FormModel = new VentaFormViewModel { Unidades = 1 };
            ViewBag.RolUsuario = rol;

            return View(ventas);
        }

        // POST: /Ventas/Create (Procesa el formulario de la vista unificada)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaFormViewModel model)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            var nombreUsuario = HttpContext.Session.GetString("UsuarioNombre");

            if (string.IsNullOrEmpty(rol))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Si es vendedor, forzamos que el vendedor sea él mismo por seguridad
            if (rol.Equals("Vendedor", StringComparison.OrdinalIgnoreCase))
            {
                model.Vendedor = nombreUsuario ?? string.Empty;
                ModelState.Remove(nameof(model.Vendedor));
            }
            else if (rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(model.Vendedor))
            {
                ModelState.AddModelError("Vendedor", "Debe seleccionar un vendedor.");
            }

            var producto = await _context.Inventario.FindAsync(model.ProductoId);

            if (producto == null)
            {
                ModelState.AddModelError("", "El producto seleccionado no existe.");
            }
            else if (model.Unidades > producto.Unidades)
            {
                ModelState.AddModelError("", $"Stock insuficiente. Disponible: {producto.Unidades}.");
            }

            if (!ModelState.IsValid)
            {
                // Recargar datos y la vista principal en caso de error
                var queryError = _context.Ventas.AsQueryable();
                if (rol.Equals("Vendedor", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(nombreUsuario))
                {
                    queryError = queryError.Where(v => v.Vendedor == nombreUsuario);
                }
                var ventasError = await queryError.OrderByDescending(v => v.Fecha).ToListAsync();

                await CargarProductosYUsuarios();
                ViewBag.FormModel = model;
                ViewBag.RolUsuario = rol;
                return View("Index", ventasError);
            }

            var subtotal = producto!.PrecioUnitario * model.Unidades;

            var venta = new Venta
            {
                Producto = producto.ProductoNombre,
                PrecioUnitario = producto.PrecioUnitario,
                Unidades = model.Unidades,
                SubTotal = subtotal,
                TotalAPagar = subtotal,
                Fecha = DateTime.UtcNow,
                Vendedor = model.Vendedor
            };

            producto.Unidades -= model.Unidades;

            _context.Ventas.Add(venta);
            _context.Inventario.Update(producto);
            await _context.SaveChangesAsync();

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