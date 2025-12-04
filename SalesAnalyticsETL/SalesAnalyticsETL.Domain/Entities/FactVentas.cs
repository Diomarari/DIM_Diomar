using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Domain.Entities
{
    [Table("Fact_Ventas")]
    public class FactVentas : BaseEntity
    {
        [Key]
        public int VentaID { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrdenID { get; set; } = string.Empty;

        [Required]
        public int ClienteID { get; set; }

        [Required]
        public int ProductoID { get; set; }

        [Required]
        public int TiempoID { get; set; }

        [Required]
        public int EstadoID { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalVenta { get; set; }

        public DateTime FechaCarga { get; set; } = DateTime.Now;

        [ForeignKey(nameof(ClienteID))]
        public virtual DimCliente Cliente { get; set; } = null!;

        [ForeignKey(nameof(ProductoID))]
        public virtual DimProducto Producto { get; set; } = null!;

        [ForeignKey(nameof(TiempoID))]
        public virtual DimTiempo Tiempo { get; set; } = null!;

        [ForeignKey(nameof(EstadoID))]
        public virtual DimEstado Estado { get; set; } = null!;
    }
}
