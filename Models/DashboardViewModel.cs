namespace Sistema_de_inventario_y_ventas.Models
{
    public class DashboardViewModel
    {
        public int TotalProductos { get; set; }
        public int TotalCategorias { get; set; }
        public int VentasHoy { get; set; }
        public decimal IngresosHoy { get; set; }
        public decimal TicketPromedio { get; set; }

        public List<Producto> ProductosStockBajo { get; set; } = new();
        public List<Venta> UltimasVentas { get; set; } = new();

        // Gráfico: ingresos últimos 7 días
        public List<string> DiasLabels { get; set; } = new();
        public List<decimal> IngresosPorDia { get; set; } = new();

        // Gráfico: top productos más vendidos
        public List<string> ProductosTopLabels { get; set; } = new();
        public List<int> ProductosTopUnidades { get; set; } = new();

        // Gráfico: ventas por categoría (dona)
        public List<string> VentasCategoriaLabels { get; set; } = new();
        public List<decimal> VentasCategoriaMontos { get; set; } = new();

        // Gráfico: valor de inventario por categoría (capital invertido)
        public List<string> ValorInventarioLabels { get; set; } = new();
        public List<decimal> ValorInventarioMontos { get; set; } = new();

        // Comparación semana actual vs anterior (métrica positiva/negativa)
        public decimal IngresosSemanaActual { get; set; }
        public decimal IngresosSemanaAnterior { get; set; }
        public decimal PorcentajeCambioSemanal { get; set; }

        // Gráfico: productos con stock crítico (negativo)
        public List<string> StockCriticoLabels { get; set; } = new();
        public List<int> StockCriticoUnidades { get; set; } = new();
    }
}