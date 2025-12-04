namespace SalesAnalyticsETL.Api.DTOs
{
    public class DimProductoResponseDto
    {
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public decimal PrecioBase { get; set; }
        public int Stock { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
