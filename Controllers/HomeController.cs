using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_de_inventario_y_ventas.Data;
using Sistema_de_inventario_y_ventas.Models;

namespace Sistema_de_inventario_y_ventas.Controllers
{
    public class HomeController : Controller
    {
        private const int UmbralStockBajo = 5;

        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.UtcNow.Date;
            var manana = hoy.AddDays(1);
            var inicioSemanaActual = hoy.AddDays(-6);
            var inicioSemanaAnterior = hoy.AddDays(-13);

            var todasLasVentas = await _context.Ventas.ToListAsync();
            var todoElInventario = await _context.Inventario.ToListAsync();

            var ventasHoy = todasLasVentas.Where(v => v.Fecha >= hoy && v.Fecha < manana).ToList();

            // --- Ingresos últimos 7 días ---
            var diasLabels = new List<string>();
            var ingresosPorDia = new List<decimal>();
            for (var dia = inicioSemanaActual; dia <= hoy; dia = dia.AddDays(1))
            {
                diasLabels.Add(dia.ToString("dd/MM"));
                ingresosPorDia.Add(todasLasVentas.Where(v => v.Fecha.Date == dia).Sum(v => v.TotalAPagar));
            }

            // --- Top 5 productos más vendidos ---
            var productosTop = todasLasVentas
                .GroupBy(v => v.Producto)
                .Select(g => new { Producto = g.Key, Unidades = g.Sum(v => v.Unidades) })
                .OrderByDescending(g => g.Unidades)
                .Take(5)
                .ToList();

            // --- Ventas por categoría (join en memoria: venta.Producto = inventario.ProductoNombre) ---
            var ventasPorCategoria = todasLasVentas
                .Join(todoElInventario, v => v.Producto, p => p.ProductoNombre, (v, p) => new { v.TotalAPagar, p.NombreCategoria })
                .GroupBy(x => x.NombreCategoria)
                .Select(g => new { Categoria = g.Key, Total = g.Sum(x => x.TotalAPagar) })
                .OrderByDescending(g => g.Total)
                .ToList();

            // --- Valor de inventario por categoría (capital invertido) ---
            var valorInventario = todoElInventario
                .GroupBy(p => p.NombreCategoria)
                .Select(g => new { Categoria = g.Key, Valor = g.Sum(p => p.PrecioUnitario * p.Unidades) })
                .OrderByDescending(g => g.Valor)
                .ToList();

            // --- Comparación semana actual vs anterior ---
            var ingresosSemanaActual = todasLasVentas
                .Where(v => v.Fecha >= inicioSemanaActual && v.Fecha < manana)
                .Sum(v => v.TotalAPagar);

            var ingresosSemanaAnterior = todasLasVentas
                .Where(v => v.Fecha >= inicioSemanaAnterior && v.Fecha < inicioSemanaActual)
                .Sum(v => v.TotalAPagar);

            var porcentajeCambio = ingresosSemanaAnterior == 0
                ? (ingresosSemanaActual > 0 ? 100 : 0)
                : Math.Round(((ingresosSemanaActual - ingresosSemanaAnterior) / ingresosSemanaAnterior) * 100, 1);

            // --- Stock crítico ---
            var stockCritico = todoElInventario
                .Where(p => p.Unidades <= UmbralStockBajo)
                .OrderBy(p => p.Unidades)
                .ToList();

            var dashboard = new DashboardViewModel
            {
                TotalProductos = todoElInventario.Count,
                TotalCategorias = await _context.Categorias.CountAsync(),
                VentasHoy = ventasHoy.Count,
                IngresosHoy = ventasHoy.Sum(v => v.TotalAPagar),
                TicketPromedio = todasLasVentas.Any() ? todasLasVentas.Average(v => v.TotalAPagar) : 0,

                ProductosStockBajo = stockCritico,
                UltimasVentas = todasLasVentas.OrderByDescending(v => v.Fecha).Take(5).ToList(),

                DiasLabels = diasLabels,
                IngresosPorDia = ingresosPorDia,

                ProductosTopLabels = productosTop.Select(p => p.Producto).ToList(),
                ProductosTopUnidades = productosTop.Select(p => p.Unidades).ToList(),

                VentasCategoriaLabels = ventasPorCategoria.Select(v => v.Categoria).ToList(),
                VentasCategoriaMontos = ventasPorCategoria.Select(v => v.Total).ToList(),

                ValorInventarioLabels = valorInventario.Select(v => v.Categoria).ToList(),
                ValorInventarioMontos = valorInventario.Select(v => v.Valor).ToList(),

                IngresosSemanaActual = ingresosSemanaActual,
                IngresosSemanaAnterior = ingresosSemanaAnterior,
                PorcentajeCambioSemanal = porcentajeCambio,

                StockCriticoLabels = stockCritico.Select(p => p.ProductoNombre).ToList(),
                StockCriticoUnidades = stockCritico.Select(p => p.Unidades).ToList()
            };

            return View(dashboard);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}