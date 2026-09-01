using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- Asegúrate de incluir este using
using Sistema_de_inventario_y_ventas.Data; // <-- Y este
using Sistema_de_inventario_y_ventas.Models;

namespace Sistema_de_inventario_y_ventas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // 1. Declaramos la variable del contexto

        // 2. Lo recibimos por el constructor (Inyección de dependencias)
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 3. Hacemos una prueba trayendo las categorías de Neon de forma asíncrona
            var listaCategorias = await _context.Categorias.ToListAsync();

            // Pasamos la lista a la vista para verla en pantalla
            return View(listaCategorias);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}