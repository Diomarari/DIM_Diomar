using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class ClienteCsvExtractor : IExtractor<ClienteDTO>
    {
        private readonly string _csvFilePath;
        private readonly ILogger<ClienteCsvExtractor> _logger;

        public ClienteCsvExtractor(string csvFilePath, ILogger<ClienteCsvExtractor> logger)
        {
            _csvFilePath = csvFilePath;
            _logger = logger;
        }

        public async Task<IEnumerable<ClienteDTO>> ExtractAsync()
        {
            try
            {
                var clientes = new List<ClienteDTO>();

                if (!File.Exists(_csvFilePath))
                {
                    _logger.LogWarning($"Archivo CSV de clientes no encontrado: {_csvFilePath}");
                    return clientes;
                }

                using var reader = new StreamReader(_csvFilePath);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null
                };

                using var csv = new CsvReader(reader, config);

                await Task.Run(() =>
                {
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        try
                        {
                            var cliente = new ClienteDTO
                            {
                                ClienteID = csv.GetField<int>("CustomerID"),
                                Nombre = csv.GetField<string>("FirstName") ?? string.Empty,
                                Apellido = csv.GetField<string>("LastName") ?? string.Empty,
                                Email = csv.GetField<string>("Email") ?? string.Empty,
                                Telefono = csv.GetField<string>("Phone") ?? string.Empty,
                                Ciudad = csv.GetField<string>("City") ?? string.Empty,
                                Pais = csv.GetField<string>("Country") ?? string.Empty
                            };
                            clientes.Add(cliente);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error al leer fila del CSV de clientes: {ex.Message}");
                        }
                    }
                });

                _logger.LogInformation($"CSV Clientes: {clientes.Count} registros extraídos exitosamente");
                return clientes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al extraer datos del CSV de clientes: {_csvFilePath}");
                throw;
            }
        }

        public string GetSourceName() => $"CSV Clientes ({Path.GetFileName(_csvFilePath)})";
    }

    public class ProductoCsvExtractor : IExtractor<ProductoDTO>
    {
        private readonly string _csvFilePath;
        private readonly ILogger<ProductoCsvExtractor> _logger;

        public ProductoCsvExtractor(string csvFilePath, ILogger<ProductoCsvExtractor> logger)
        {
            _csvFilePath = csvFilePath;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductoDTO>> ExtractAsync()
        {
            try
            {
                var productos = new List<ProductoDTO>();

                if (!File.Exists(_csvFilePath))
                {
                    _logger.LogWarning($"Archivo CSV de productos no encontrado: {_csvFilePath}");
                    return productos;
                }

                using var reader = new StreamReader(_csvFilePath);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null
                };

                using var csv = new CsvReader(reader, config);

                await Task.Run(() =>
                {
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        try
                        {
                            var producto = new ProductoDTO
                            {
                                ProductoID = csv.GetField<int>("ProductID"),
                                NombreProducto = csv.GetField<string>("ProductName") ?? string.Empty,
                                Categoria = csv.GetField<string>("Category") ?? string.Empty,
                                PrecioBase = csv.GetField<decimal>("Price"),
                                Stock = csv.GetField<int>("Stock")
                            };
                            productos.Add(producto);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error al leer fila del CSV de productos: {ex.Message}");
                        }
                    }
                });

                _logger.LogInformation($"CSV Productos: {productos.Count} registros extraídos exitosamente");
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al extraer datos del CSV de productos: {_csvFilePath}");
                throw;
            }
        }

        public string GetSourceName() => $"CSV Productos ({Path.GetFileName(_csvFilePath)})";
    }

    public class OrderDetailCsvExtractor : IExtractor<OrderDetailDTO>
    {
        private readonly string _csvFilePath;
        private readonly ILogger<OrderDetailCsvExtractor> _logger;

        public OrderDetailCsvExtractor(string csvFilePath, ILogger<OrderDetailCsvExtractor> logger)
        {
            _csvFilePath = csvFilePath;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDetailDTO>> ExtractAsync()
        {
            try
            {
                var orders = new List<OrderDetailDTO>();

                var ordersFilePath = Path.Combine(Path.GetDirectoryName(_csvFilePath) ?? "", "orders.csv");

                if (!File.Exists(_csvFilePath))
                {
                    _logger.LogWarning($"Archivo CSV de detalles no encontrado: {_csvFilePath}");
                    return orders;
                }

                if (!File.Exists(ordersFilePath))
                {
                    _logger.LogWarning($"Archivo CSV de órdenes no encontrado: {ordersFilePath}");
                    return orders;
                }

                var orderInfoDict = new Dictionary<string, (int CustomerID, DateTime OrderDate, string Status)>();
                
                using (var orderReader = new StreamReader(ordersFilePath))
                {
                    var orderConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        MissingFieldFound = null,
                        BadDataFound = null
                    };

                    using var orderCsv = new CsvReader(orderReader, orderConfig);
                    orderCsv.Read();
                    orderCsv.ReadHeader();

                    while (orderCsv.Read())
                    {
                        try
                        {
                            var orderId = orderCsv.GetField<string>("OrderID") ?? "";
                            var customerId = orderCsv.GetField<int>("CustomerID");
                            var orderDate = orderCsv.GetField<DateTime>("OrderDate");
                            var status = orderCsv.GetField<string>("Status") ?? "COMPLETADO";

                            if (!string.IsNullOrWhiteSpace(orderId))
                            {
                                orderInfoDict[orderId] = (customerId, orderDate, status);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error al leer fila de orders.csv: {ex.Message}");
                        }
                    }
                }

                using var detailReader = new StreamReader(_csvFilePath);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null
                };

                using var csv = new CsvReader(detailReader, config);

                await Task.Run(() =>
                {
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        try
                        {
                            var orderId = csv.GetField<string>("OrderID") ?? string.Empty;
                            var productoId = csv.GetField<int>("ProductID");
                            var quantity = csv.GetField<int>("Quantity");
                            var totalPrice = csv.GetField<decimal>("TotalPrice");

                            if (orderInfoDict.TryGetValue(orderId, out var orderInfo))
                            {
                                var precioUnitario = quantity > 0 ? totalPrice / quantity : totalPrice;

                                var order = new OrderDetailDTO
                                {
                                    OrdenID = orderId,
                                    ClienteID = orderInfo.CustomerID,
                                    ProductoID = productoId,
                                    Cantidad = quantity,
                                    Precio = precioUnitario,
                                    FechaVenta = orderInfo.OrderDate,
                                    Estado = orderInfo.Status.ToUpper()
                                };
                                orders.Add(order);
                            }
                            else
                            {
                                _logger.LogWarning($"No se encontró información de orden para OrderID: {orderId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error al leer fila del CSV de detalles: {ex.Message}");
                        }
                    }
                });

                _logger.LogInformation($"CSV Detalles: {orders.Count} registros extraídos exitosamente");
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al extraer datos del CSV de detalles: {_csvFilePath}");
                throw;
            }
        }

        public string GetSourceName() => $"CSV Detalles ({Path.GetFileName(_csvFilePath)})";
    }
}
