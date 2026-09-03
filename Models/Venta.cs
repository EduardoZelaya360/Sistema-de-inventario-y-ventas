using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_inventario_y_ventas.Models
{
    // Venta.cs
[Table("ventas")]
    public class Venta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("producto")]
        public string Producto { get; set; } = string.Empty;

        [Required]
        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column("unidades")]
        public int Unidades { get; set; }

        [Required]
        [Column("sub_total")]
        public decimal SubTotal { get; set; }

        [Required]
        [Column("total_a_pagar")]
        public decimal TotalAPagar { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("vendedor")]
        public string Vendedor { get; set; } = string.Empty;
    }
}