namespace SalesAnalyticsETL.Domain.Interfaces
{
    public interface IDimClienteLoader
    {
        Task<int> LoadDimensionAsync<TClienteDTO>(IEnumerable<TClienteDTO> clientes) where TClienteDTO : class;
        Task<int?> GetClienteIDByEmailAsync(string email);
        Task<int> GetOrCreateUnknownClienteAsync();
    }
}
