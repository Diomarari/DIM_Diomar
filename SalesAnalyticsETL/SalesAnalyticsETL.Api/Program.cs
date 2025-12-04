using Microsoft.EntityFrameworkCore;
using SalesAnalyticsETL.Infrastructure.context;
using SalesAnalyticsETL.Domain.Interfaces;
using SalesAnalyticsETL.Infrastructure.Loaders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataWarehouseContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DataWarehouse"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);

builder.Services.AddScoped<IDimClienteLoader, DimClienteLoader>();
builder.Services.AddScoped<IDimProductoLoader, DimProductoLoader>();
builder.Services.AddScoped<IDimTiempoLoader, DimTiempoLoader>();
builder.Services.AddScoped<IDimEstadoLoader, DimEstadoLoader>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
