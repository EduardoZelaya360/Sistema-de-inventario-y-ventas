using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_de_inventario_y_ventas.Data;
using Sistema_de_inventario_y_ventas.Models;
using Microsoft.AspNetCore.Identity;

namespace Sistema_de_inventario_y_ventas.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Login
        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Login

        [HttpPost]
        [Route("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string contrasena)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(contrasena))
            {
                ModelState.AddModelError(string.Empty, "Por favor completa todos los campos.");
                return View();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario != null)
            {
                var passwordHasher = new PasswordHasher<Usuario>();
                bool esValida = false;

                try
                {
                    // Intentar verificar como hash seguro de ASP.NET Core
                    var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.Contrasena, contrasena);
                    if (resultado == PasswordVerificationResult.Success || resultado == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        esValida = true;
                    }
                }
                catch (FormatException)
                {
                    // Si lanza el error de Base64, significa que la contraseña está en texto plano antiguo
                    if (usuario.Contrasena == contrasena)
                    {
                        esValida = true;
                        // Actualizarla automáticamente a hash seguro para la próxima vez
                        usuario.Contrasena = passwordHasher.HashPassword(usuario, contrasena);
                        _context.Usuarios.Update(usuario);
                        await _context.SaveChangesAsync();
                    }
                }

                if (esValida)
                {
                    HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                    HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                    HttpContext.Session.SetString("UsuarioRol", usuario.Rol);

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View();
        }

        // GET: /Logout
        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Limpiar la sesión al cerrar
            return RedirectToAction("Login", "Auth");
        }
    }
}