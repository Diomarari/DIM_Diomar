using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Domain.Interfaces
{
    public interface ILoader<T> where T : class
    {
        Task<int> LoadAsync(IEnumerable<T> data);
        Task<bool> VerifyLoadAsync();
    }
}
