namespace Sistema_de_inventario_y_ventas.Models
{
    public class ItemCarrito
    {
        public int ProductoId { get; set; }
        public int Unidades { get; set; }
        public decimal PorcentajeImpuesto { get; set; } = 15;
    }

    public class VentaFormViewModel
    {
        public string Vendedor { get; set; } = string.Empty;
        public List<ItemCarrito> Items { get; set; } = new();
    }
}