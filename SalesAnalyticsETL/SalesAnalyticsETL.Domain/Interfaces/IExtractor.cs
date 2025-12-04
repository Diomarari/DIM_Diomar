using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Domain.Interfaces
{
    public interface IExtractor<T> where T : class
    {
        Task<IEnumerable<T>> ExtractAsync();
        string GetSourceName();
    }
}
