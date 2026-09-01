using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_inventario_y_ventas.Models
{
    [Table("ventas")]
    public class Venta
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Producto { get; set; } = string.Empty;
        
        [Required]
        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }
        
        [Required]
        public int Unidades { get; set; }
        
        [Required]
        [Column("sub_total")]
        public decimal SubTotal { get; set; }
        
        [Required]
        [Column("total_a_pagar")]
        public decimal TotalAPagar { get; set; }
        
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        [Required]
        public string Vendedor { get; set; } = string.Empty;
    }
}