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

        // GET: /Inventario
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Inventario
                .Include(p => p.Categoria)
                .OrderBy(p => p.ProductoNombre)
                .ToListAsync();

            return View(productos);
        }

        // GET: /Inventario/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var producto = await _context.Inventario
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();
            return View(producto);
        }

        // GET: /Inventario/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            ViewBag.ProductosExistentes = await _context.Inventario
                .Select(p => p.ProductoNombre)
                .ToListAsync();

            return View();
        }

        // POST: /Inventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();
                return View(producto);
            }

            _context.Inventario.Add(producto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Producto agregado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Inventario/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.Inventario.FindAsync(id);
            if (producto == null) return NotFound();

            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();
            return View(producto);
        }

        // POST: /Inventario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();
                return View(producto);
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
            return RedirectToAction(nameof(Index));
        }

        // GET: /Inventario/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Inventario
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();
            return View(producto);
        }

        // POST: /Inventario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Inventario.FindAsync(id);
            if (producto != null)
            {
                _context.Inventario.Remove(producto);
                await _context.SaveChangesAsync();
            }

            TempData["Mensaje"] = "Producto eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}