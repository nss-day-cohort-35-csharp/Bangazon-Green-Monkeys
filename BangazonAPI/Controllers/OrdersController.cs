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
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrdersController(IConfiguration config)
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

        [HttpGet("{id}", Name = "GetOrders")] //Code for getting a order by orderid
        public async Task<IActionResult> Get([FromRoute] int id, int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                   // if (customerId == customerId)
                    //{
                        cmd.CommandText = @"SELECT 
                                          o.Id AS OrderId,
                                          o.CustomerId,
                                          o.UserPaymentTypeId,
                                          p.ProductTypeId,
                                          p.CustomerId,
                                          p.price,
                                          p.[Description],
                                          p.Title,
                                          p.DateAdded,
                                          p.Id AS ProductId
                                          FROM [Order] o
                                          LEFT JOIN Product p ON o.CustomerId = p.Id
                                          WHERE 1=1";
                    //}
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();


                    List<Product> products = new List<Product>();
                    Order order = null;

                    if (reader.Read())
                    {
                        Product product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            Title = reader.GetString(reader.GetOrdinal("Title"))
                        };

                        products.Add(product);

                        var hasUserPaymentId = !reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId"));

                        // Userpayment is NUll if it is not null do what I Am doing ISDBNUll
                        if (hasUserPaymentId)
                        {
                            order = new Order
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")),
                                Products = products
                            };

                        } else {
                           
                            
                            order = new Order
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                // UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")),
                                Products = products
                            };
                        }
                    }
                    reader.Close();
                    return Ok(order);
                }
            }
        }
    }
}
