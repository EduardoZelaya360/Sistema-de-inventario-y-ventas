using System.ComponentModel.DataAnnotations;

namespace Sistema_de_inventario_y_ventas.Models
{
    public class VentaFormViewModel
    {
        [Required(ErrorMessage = "Selecciona un producto")]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Unidades { get; set; }

        [Required(ErrorMessage = "Selecciona un vendedor")]
        public string Vendedor { get; set; } = string.Empty;
    }
}