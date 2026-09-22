using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_inventario_y_ventas.Models
{
    [Table("detalle_venta")]
    public class DetalleVenta
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("venta_id")]
        public int VentaId { get; set; }

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
        [Column("subtotal")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column("porcentaje_impuesto")]
        public decimal PorcentajeImpuesto { get; set; } = 15;

        [Required]
        [Column("impuesto")]
        public decimal Impuesto { get; set; }

        [ForeignKey(nameof(VentaId))]
        public Venta? Venta { get; set; }
    }
}