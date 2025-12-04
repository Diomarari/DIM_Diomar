namespace SalesAnalyticsETL.Domain.Interfaces
{
    public interface IDimProductoLoader
    {
        Task<int> LoadDimensionAsync<TProductoDTO>(IEnumerable<TProductoDTO> productos) where TProductoDTO : class;
        Task<int?> GetProductoIDByNameAsync(string nombreProducto);
        Task<int> GetOrCreateUnknownProductoAsync();
    }
}
