using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Domain.Entities
{
    [Table("Dim_Estado")]
    public class DimEstado : BaseEntity
    {
        [Key]
        public int EstadoID { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreEstado { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public virtual ICollection<FactVentas> Ventas { get; set; } = new List<FactVentas>();
    }
}
