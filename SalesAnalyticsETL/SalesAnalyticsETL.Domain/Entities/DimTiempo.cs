using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Domain.Entities
{
    [Table("Dim_Tiempo")]
    public class DimTiempo : BaseEntity
    {
        [Key]
        public int TiempoID { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public int Dia { get; set; }
        public int Mes { get; set; }
        public int Trimestre { get; set; }
        public int Anio { get; set; }

        [MaxLength(20)]
        public string NombreMes { get; set; } = string.Empty;

        [MaxLength(20)]
        public string DiaSemana { get; set; } = string.Empty;

        public bool EsFinDeSemana { get; set; }
        public bool EsFeriado { get; set; }

        public virtual ICollection<FactVentas> Ventas { get; set; } = new List<FactVentas>();
    }
}
