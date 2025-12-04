using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Domain.Interfaces
{
    public interface ITransformer<TInput, TOutput>
       where TInput : class
       where TOutput : class
    {
        Task<IEnumerable<TOutput>> TransformAsync(IEnumerable<TInput> data);
        Task<bool> ValidateAsync(TInput data);
    }
}
