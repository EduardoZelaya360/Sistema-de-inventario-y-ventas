using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_inventario_y_ventas.Models
{
    [Table("categorias")]
    public class Categoria
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("nombre_categoria")]
        public string NombreCategoria { get; set; } = string.Empty;

        public List<Producto> Productos { get; set; } = new();
    }
}