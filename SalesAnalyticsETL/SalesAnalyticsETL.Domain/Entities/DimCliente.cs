using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Domain.Entities
{
   
        [Table("Dim_Cliente")]
        public class DimCliente : BaseEntity
        {
            [Key]
            public int ClienteID { get; set; }

            [Required]
            [MaxLength(100)]
            public string Nombre { get; set; } = string.Empty;

            [MaxLength(100)]
            public string Apellido { get; set; } = string.Empty;

            [MaxLength(150)]
            public string Email { get; set; } = string.Empty;

            [MaxLength(50)]
            public string Telefono { get; set; } = string.Empty;

            [MaxLength(100)]
            public string Ciudad { get; set; } = string.Empty;

            [MaxLength(100)]
            public string Pais { get; set; } = string.Empty;
            public virtual ICollection<FactVentas> Ventas { get; set; } = new List<FactVentas>();
        }
    }
