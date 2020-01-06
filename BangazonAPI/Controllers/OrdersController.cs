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

        [HttpGet(Name = "GetCustomerOrders")] //Code for getting a order by orderid
        public async Task<IActionResult> Get([FromQuery] int? customerId, string cart)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (customerId != null && cart == "true")
                    {
                        cmd.CommandText = @"SELECT 
                                          o.Id AS OrderId,
                                          o.CustomerId AS OrderCustomerId,
                                          o.UserPaymentTypeId,
                                          op.Id,
                                          op.ProductId,
                                          p.ProductTypeId,
                                          p.DateAdded,
                                          p.[Description],
                                          p.Title,
                                          p.CustomerId,
                                          p.Price,
                                          p.Id
                                          FROM  [Order] o
                                          LEFT JOIN OrderProduct op ON o.Id = op.OrderId
                                          LEFT JOIN Product p ON op.ProductId = p.Id
                                          WHERE o.CustomerId = @CustomerId";

                        //if there is a customer id then do the where
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Product> products = new List<Product>();
                        List<Order> orders = new List<Order>();
                        Order order = null;

                        while (reader.Read())
                        {
                            Product product = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                Title = reader.GetString(reader.GetOrdinal("Title"))
                            };

                            products.Add(product);
                            var hasUserPaymentId = reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId"));

                            // Userpayment is NUll if it is not null do what I Am doing ISDBNUll
                            if (hasUserPaymentId)
                            {
                                order = new Order
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("OrderCustomerId")),
                                    UserPaymentId = null,
                                    //UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")),
                                    Products = products
                                };
                                if (!orders.Exists(o => o.Id == order.Id))
                                {
                                    orders.Add(order);
                                };

                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                        reader.Close();
                        return Ok(orders);
                    }
                   else if (customerId != null)
                    {
                        cmd.CommandText = @"SELECT 
                                          o.Id AS OrderId,
                                          o.CustomerId AS OrderCustomerId,
                                          o.UserPaymentTypeId,
                                          op.Id,
                                          op.ProductId,
                                          p.ProductTypeId,
                                          p.DateAdded,
                                          p.[Description],
                                          p.Title,
                                          p.CustomerId,
                                          p.Price,
                                          p.Id
                                          FROM [Order] o
                                          LEFT JOIN OrderProduct op ON o.Id = op.OrderId
                                          LEFT JOIN Product p ON op.ProductId = p.Id
                                          WHERE o.CustomerId = @CustomerId";

                        //if there is a customer id then do the where
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Product> products = new List<Product>();
                        List<Order> orders = new List<Order>();
                        Order order = null;

                        while (reader.Read())
                        {
                            Product product = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                Title = reader.GetString(reader.GetOrdinal("Title"))
                            };

                            products.Add(product);

                            //If order is already in the list dont place it in there
                            var hasUserPaymentId = !reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId"));

                            // Userpayment is NUll if it is not null do what I Am doing ISDBNUll
                            if (hasUserPaymentId)
                            {
                                order = new Order
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("OrderCustomerId")),
                                    UserPaymentId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")),
                                    Products = products
                                };
                                if(!orders.Exists( o => o.Id == order.Id))
                                {
                                    orders.Add(order);
                                }                                                        
                            }
                            else
                            {
                                order = new Order
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("OrderCustomerId")),
                                    UserPaymentId = null,
                                    //UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")),
                                    Products = products
                                };
                                if (!orders.Exists(o => o.Id == order.Id))
                                {
                                    orders.Add(order);
                                }
                            }
                        }
                        reader.Close();
                        return Ok(orders);
                    }
                    
                    else {
                        //Return bad request
                        return NotFound();
                    }

                }
            }
        }


        [HttpGet("{id}", Name = "GetOrders")] //Code for getting a order by orderid
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    {
                        cmd.CommandText = @"SELECT 
                                          o.Id AS OrderId,
                                          o.CustomerId AS OrderCustomerId,
                                          o.UserPaymentTypeId,
                                          op.Id,
                                          op.ProductId,
                                          p.ProductTypeId,
                                          p.DateAdded,
                                          p.[Description],
                                          p.Title,
                                          p.CustomerId,
                                          p.Price,
                                          p.Id

                                          FROM [Order] o
                                          LEFT JOIN OrderProduct op ON o.Id = op.OrderId
                                          LEFT JOIN Product p ON op.ProductId = p.Id
                                          WHERE OrderId=@id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();


                        List<Product> products = new List<Product>();
                        List<Order> orders = new List<Order>();
                        Order order = null;

                        while (reader.Read())
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
                                    UserPaymentId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")),
                                    Products = products
                                };                       
                            }
                            else
                            {
                                order = new Order
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    UserPaymentId = null,
                                    //UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId")),
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


        // Create a customerId/ProductId chart
        // Find out if that customer has a cart (order)
        // If No insert new 
        // If Yes update cart with  a new reocrd into the orderProduct table
        //If I do not have a shopping cart 
        //Only need a product id and customer id for the shopping cart
        [HttpPost] //Adds Customer
        public async Task<IActionResult> post([FromBody] Order order)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSER T into Order (CustomerId, UserPaymentId)
                                                OUTPUT INSERTED.id
                                                values (@CustomerId, @UserPaymentId)";
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", order.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@UserPaymentId", order.UserPaymentId));
                  
                    int newid = (int)cmd.ExecuteScalar();
                    order.Id = newid;
                    return CreatedAtRoute("GetOrders", new { id = newid }, order);
                }
            }
        }


        private bool CustomerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT *
                    FROM Customer
                    WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}




