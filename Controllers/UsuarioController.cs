using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_de_inventario_y_ventas.Data;
using Sistema_de_inventario_y_ventas.Models;
using Microsoft.AspNetCore.Identity;

namespace Sistema_de_inventario_y_ventas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool EsAdministrador()
        {
            return HttpContext.Session.GetString("UsuarioRol") == "Administrador";
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Home");
            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                var passwordHasher = new PasswordHasher<Usuario>();
                usuario.Contrasena = passwordHasher.HashPassword(usuario, usuario.Contrasena);

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Home");
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            // Limpiamos el campo para que el hash no se muestre en el formulario
            usuario.Contrasena = string.Empty;
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Home");
            if (id != usuario.Id) return NotFound();

            if (string.IsNullOrWhiteSpace(usuario.Contrasena))
            {
                ModelState.Remove(nameof(usuario.Contrasena));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioActual = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    if (usuarioActual == null) return NotFound();

                    if (string.IsNullOrWhiteSpace(usuario.Contrasena))
                    {
                        usuario.Contrasena = usuarioActual.Contrasena;
                    }
                    else
                    {
                        var passwordHasher = new PasswordHasher<Usuario>();
                        usuario.Contrasena = passwordHasher.HashPassword(usuario, usuario.Contrasena);
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Usuarios.Any(e => e.Id == usuario.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Home");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}