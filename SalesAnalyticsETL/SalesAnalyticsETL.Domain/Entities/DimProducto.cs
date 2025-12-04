using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Domain.Entities
{
    [Table("Dim_Producto")]
    public class DimProducto : BaseEntity
    {
        [Key]
        public int ProductoID { get; set; }

        [Required]
        [MaxLength(200)]
        public string NombreProducto { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioBase { get; set; }

        public int Stock { get; set; }

        public virtual ICollection<FactVentas> Ventas { get; set; } = new List<FactVentas>();
    }
}
