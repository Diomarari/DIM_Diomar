namespace SalesAnalyticsETL.Domain.Interfaces
{

    public interface IDimTiempoLoader
    {
        Task<int> LoadDimensionForDateAsync(DateTime fecha);
        Task<int> LoadDimensionForDateRangeAsync(IEnumerable<DateTime> fechas);
        Task<int?> GetTiempoIDByDateAsync(DateTime fecha);
        Task<int> PreloadYearAsync(int year);
    }
}
