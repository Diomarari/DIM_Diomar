using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Infrastructure.context
{
    public class DataWarehouseContextFactory : IDesignTimeDbContextFactory<DataWarehouseContext>
    {
        public DataWarehouseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataWarehouseContext>();

           
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=SalesAnalyticsDW;Trusted_Connection=True;TrustServerCertificate=True;",
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                )
            );

            return new DataWarehouseContext(optionsBuilder.Options);
        }
    }
}
