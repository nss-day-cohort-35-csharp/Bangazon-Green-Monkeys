using BangazonAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueReportController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RevenueReportController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> GetRevenueReport()
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"SELECT pt.Id AS ProductTypeId, pt.[Name] AS ProductType, IsNULL((SELECT COUNT(OrderProduct.Id)*Product.Price 
                                         FROM OrderProduct LEFT JOIN Product ON Orderproduct.ProductId = Product.Id
                                         WHERE Product.ProductTypeId = pt.Id GROUP BY Product.Price),0) AS TotalRevenue
                                         FROM ProductType pt";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<ProductTypeRevenue> productTypeRevenues = new List<ProductTypeRevenue>();

                    while (reader.Read())
                    {
                        ProductTypeRevenue productTypeRevenue = new ProductTypeRevenue()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            ProductTypeName = reader.GetString(reader.GetOrdinal("ProductType")),
                            TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue")),

                        };

                        productTypeRevenues.Add(productTypeRevenue);
                    }

                    reader.Close();
                    return Ok(productTypeRevenues);
                }
            }
        }

    }
}
