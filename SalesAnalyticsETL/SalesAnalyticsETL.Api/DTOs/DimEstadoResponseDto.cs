namespace SalesAnalyticsETL.Api.DTOs
{
    public class DimEstadoResponseDto
    {
        public int EstadoID { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
