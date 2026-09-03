using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_inventario_y_ventas.Models
{
    [Table("inventario")]
    public class Producto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("producto")]
        public string ProductoNombre { get; set; } = string.Empty;
        
        [Required]
        [Column("nombre_categoria")]
        public string NombreCategoria { get; set; } = string.Empty;
        
        [Column("descripcion")]
        public string? Descripcion { get; set; }
        
        [Required]
        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }
        
        [Required]
        [Column("unidades")]
        public int Unidades { get; set; }

        [ForeignKey(nameof(NombreCategoria))]
        public Categoria? Categoria { get; set; }
    }
}