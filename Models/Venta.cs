using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_inventario_y_ventas.Models
{
    [Table("ventas")]
    public class Venta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("total_a_pagar")]
        public decimal TotalAPagar { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("vendedor")]
        public string Vendedor { get; set; } = string.Empty;

        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}