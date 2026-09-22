using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_de_inventario_y_ventas.Data;
using Sistema_de_inventario_y_ventas.Models;

namespace Sistema_de_inventario_y_ventas.Controllers
{
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool EsAdministrador()
        {
            return HttpContext.Session.GetString("UsuarioRol") == "Administrador";
        }

        // GET: /Inventario
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Inventario
                .Include(p => p.Categoria)
                .OrderBy(p => p.ProductoNombre)
                .ToListAsync();

            var categorias = await _context.Categorias
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            ViewBag.Categorias = categorias;
            ViewBag.ProductosExistentes = productos.Select(p => p.ProductoNombre).ToList();
            ViewBag.CategoriasExistentes = categorias.Select(c => c.NombreCategoria).ToList();
            ViewBag.TabActivo = TempData["TabActivo"] as string ?? "lista";

            return View(productos);
        }

        // ---------- Partials para modales ----------

        // GET: /Inventario/DetailsModal/5
        public async Task<IActionResult> DetailsModal(int id)
        {
            var producto = await _context.Inventario
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();
            return PartialView("_DetailsModal", producto);
        }

        // GET: /Inventario/EditModal/5
        public async Task<IActionResult> EditModal(int id)
        {
            if (!EsAdministrador()) return Forbid();

            var producto = await _context.Inventario.FindAsync(id);
            if (producto == null) return NotFound();

            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();
            return PartialView("_EditModal", producto);
        }

        // GET: /Inventario/DeleteModal/5
        public async Task<IActionResult> DeleteModal(int id)
        {
            if (!EsAdministrador()) return Forbid();

            var producto = await _context.Inventario
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();
            return PartialView("_DeleteModal", producto);
        }

        // ---------- CRUD Producto ----------

        public async Task<IActionResult> Details(int id)
        {
            var producto = await _context.Inventario
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();
            return View(producto);
        }

        public async Task<IActionResult> Create()
        {
            if (!EsAdministrador()) return RedirectToAction(nameof(Index));

            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();
            ViewBag.ProductosExistentes = await _context.Inventario.Select(p => p.ProductoNombre).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!EsAdministrador()) return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();
                ViewBag.ProductosExistentes = await _context.Inventario.Select(p => p.ProductoNombre).ToListAsync();
                return View(producto);
            }

            _context.Inventario.Add(producto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Producto agregado correctamente.";
            TempData["TabActivo"] = "lista";
            return RedirectToAction(nameof(Index));
        }


       
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Producto producto)
{
    if (!EsAdministrador()) return RedirectToAction(nameof(Index));
    if (id != producto.Id) return NotFound();

    bool esAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    if (!ModelState.IsValid)
    {
        ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();
        return PartialView("_EditModal", producto);
    }

    try
    {
        _context.Update(producto);
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!await _context.Inventario.AnyAsync(p => p.Id == id))
            return NotFound();
        throw;
    }

    TempData["Mensaje"] = "Producto actualizado correctamente.";
    TempData["TabActivo"] = "lista";

    if (esAjax)
    {
        return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
    }

    return RedirectToAction(nameof(Index));
}


        public async Task<IActionResult> Delete(int id)
        {
            if (!EsAdministrador()) return RedirectToAction(nameof(Index));

            var producto = await _context.Inventario
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!EsAdministrador()) return RedirectToAction(nameof(Index));

            var producto = await _context.Inventario.FindAsync(id);
            if (producto != null)
            {
                _context.Inventario.Remove(producto);
                await _context.SaveChangesAsync();
            }

            TempData["Mensaje"] = "Producto eliminado.";
            TempData["TabActivo"] = "lista";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Categorías ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategoria(string nombreCategoria)
        {
            if (!EsAdministrador()) return RedirectToAction(nameof(Index));

            if (string.IsNullOrWhiteSpace(nombreCategoria))
            {
                TempData["ErrorCategoria"] = "El nombre de la categoría es obligatorio.";
                TempData["TabActivo"] = "categorias";
                return RedirectToAction(nameof(Index));
            }

            bool existe = await _context.Categorias
                .AnyAsync(c => c.NombreCategoria.ToLower() == nombreCategoria.Trim().ToLower());

            if (existe)
            {
                TempData["ErrorCategoria"] = $"La categoría \"{nombreCategoria}\" ya existe.";
                TempData["TabActivo"] = "categorias";
                return RedirectToAction(nameof(Index));
            }

            _context.Categorias.Add(new Categoria { NombreCategoria = nombreCategoria.Trim() });
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Categoría agregada correctamente.";
            TempData["TabActivo"] = "categorias";
            return RedirectToAction(nameof(Index));
        }
    }
}