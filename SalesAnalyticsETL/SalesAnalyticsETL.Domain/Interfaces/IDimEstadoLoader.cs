namespace SalesAnalyticsETL.Domain.Interfaces
{
    public interface IDimEstadoLoader
    {
        Task<int> GetOrCreateEstadoByNameAsync(string nombreEstado);
        Task<int?> GetEstadoIDByNameAsync(string nombreEstado);
    }
}
