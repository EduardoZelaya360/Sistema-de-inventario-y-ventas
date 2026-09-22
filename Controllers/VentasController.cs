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
                    var porcentaje = items[i].PorcentajeImpuesto;
                    var impuesto = Math.Round(subtotal * (porcentaje / 100m), 2);
                    var totalLinea = subtotal + impuesto;

                    totalGeneral += totalLinea;

                    _context.DetalleVentas.Add(new DetalleVenta
                    {
                        VentaId = venta.Id,
                        Producto = producto.ProductoNombre,
                        PrecioUnitario = producto.PrecioUnitario,
                        Unidades = items[i].Unidades,
                        Subtotal = subtotal,
                        PorcentajeImpuesto = porcentaje,
                        Impuesto = impuesto
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

        // GET: /Ventas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (string.IsNullOrEmpty(rol) || !rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "No tienes permisos para editar ventas.";
                return RedirectToAction(nameof(Index));
            }

            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                TempData["Error"] = "La venta no fue encontrada.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Usuarios = await _context.Usuarios.ToListAsync();

            return View(venta);
        }

        // POST: /Ventas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venta ventaForm)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (string.IsNullOrEmpty(rol) || !rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "No tienes permisos para editar ventas.";
                return RedirectToAction(nameof(Index));
            }

            var ventaDb = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (ventaDb == null)
            {
                TempData["Error"] = "La venta no fue encontrada.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal nuevoTotalGeneral = 0;

                for (int i = 0; i < ventaDb.Detalles.Count; i++)
                {
                    var detalleDb = ventaDb.Detalles[i];
                    var detalleForm = ventaForm.Detalles[i];

                    int unidadesViejas = detalleDb.Unidades;
                    int unidadesNuevas = detalleForm.Unidades;

                    if (unidadesNuevas < 1)
                    {
                        TempData["Error"] = "Las unidades no pueden ser menores a 1.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (unidadesNuevas != unidadesViejas)
                    {
                        var producto = await _context.Inventario
                            .FirstOrDefaultAsync(p => p.ProductoNombre == detalleDb.Producto);

                        if (producto != null)
                        {
                            int diferencia = unidadesNuevas - unidadesViejas;

                            if (diferencia > 0)
                            {
                                if (producto.Unidades < diferencia)
                                {
                                    TempData["Error"] = $"Stock insuficiente para {producto.ProductoNombre}. Disponible: {producto.Unidades}.";
                                    return RedirectToAction(nameof(Index));
                                }
                                producto.Unidades -= diferencia;
                            }
                            else
                            {
                                producto.Unidades += Math.Abs(diferencia);
                            }

                            _context.Inventario.Update(producto);
                        }
                    }

                    detalleDb.Unidades = unidadesNuevas;
                    detalleDb.Subtotal = detalleDb.PrecioUnitario * unidadesNuevas;
                    detalleDb.Impuesto = Math.Round(detalleDb.Subtotal * (detalleDb.PorcentajeImpuesto / 100m), 2);
                    nuevoTotalGeneral += detalleDb.Subtotal + detalleDb.Impuesto;
                }

                ventaDb.Vendedor = ventaForm.Vendedor;
                ventaDb.TotalAPagar = nuevoTotalGeneral;
                ventaDb.Fecha = DateTime.UtcNow;

                _context.Ventas.Update(ventaDb);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Mensaje"] = "Venta y stock actualizados correctamente.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                string mensajeReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Error al actualizar la venta: " + mensajeReal;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Ventas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (string.IsNullOrEmpty(rol) || !rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "No tienes permisos para eliminar ventas.";
                return RedirectToAction(nameof(Index));
            }

            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                TempData["Error"] = "La venta no existe.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var detalle in venta.Detalles)
                {
                    var producto = await _context.Inventario
                        .FirstOrDefaultAsync(p => p.ProductoNombre == detalle.Producto);

                    if (producto != null)
                    {
                        producto.Unidades += detalle.Unidades;
                        _context.Inventario.Update(producto);
                    }
                }

                _context.DetalleVentas.RemoveRange(venta.Detalles);
                _context.Ventas.Remove(venta);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Mensaje"] = "Venta eliminada y stock restaurado correctamente.";
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Ocurrió un error al eliminar la venta.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}