namespace SalesAnalyticsETL.Api.DTOs
{
    public class DimTiempoResponseDto
    {
        public int TiempoID { get; set; }
        public DateTime Fecha { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Dia { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public string NombreMes { get; set; } = string.Empty;
        public int Trimestre { get; set; }
        public bool EsFinDeSemana { get; set; }
        public bool EsFeriado { get; set; }
    }
}
