using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
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
                        Product product = null;

                        while (reader.Read())
                        {
                            product = new Product
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
                                if (!orders.Exists(o => o.Id == order.Id))
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

                    else
                    {
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

        // Gets active order for customer or creates one. Returns an order id.
        private int getOrCreateOrder(int customerid)
        {
            int oid = -1;
            using (SqlConnection conn = Connection)
            {
                SqlCommand cmd;
                SqlDataReader reader;
                conn.Open();
                while (oid < 0) {
                    using (cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Id FROM [Order] WHERE CustomerId = @cid AND UserPaymentTypeId IS NULL";
                        cmd.Parameters.Add(new SqlParameter("@cid", customerid));
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            oid = reader.GetInt32(reader.GetOrdinal("Id"));
                            break;
                        }
                    }
                    reader.Close();

                    using (cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO [Order] (CustomerId, UserPaymentTypeId) VALUES (@cid, NULL)";
                        cmd.Parameters.Add(new SqlParameter("@cid", customerid));
                        reader = cmd.ExecuteReader();
                        
                    }
                    reader.Close();
                }

                conn.Close();
            }

            return oid;
        }


        [HttpPost] //Adds OrderProduct
        public async Task<IActionResult> Post([FromBody] CustomerProduct cp)
        {
            int customerid = cp.CustomerId;
            int productid = cp.ProductId;

            SqlDataReader reader;
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    int orderid = getOrCreateOrder(customerid);
                    cmd.CommandText = @"INSERT INTO OrderProduct (OrderId, ProductId) VALUES (@oid, @pid)";
                    cmd.Parameters.Add(new SqlParameter("@oid", orderid));
                    cmd.Parameters.Add(new SqlParameter("@pid", productid));
                    reader = cmd.ExecuteReader();
                }
                reader.Close();
                conn.Close();
            }
            return Ok();
        }
                    //Order order = new Order();

                    //    int newid = (int)cmd.ExecuteScalar();
                    //    order.Id = newid;
                    //    return CreatedAtRoute("GetOrders", new { id = newid }, order);
                    //}
                    //else
                    //{
                    //    cmd.CommandText = @"SELECT Id FROM [Order]";
                    //    SqlDataReader reader = cmd.ExecuteReader();

                    //    while (reader.Read())
                    //    {
                    //        Order order = new Order()
                    //        {
                    //            Id = reader.GetInt32(reader.GetOrdinal("Id"))
                    //        };

                    //        cmd.CommandText = @"INSERT INTO OrderProduct (OrderId, ProductId)
                    //                                OUTPUT INSERTED.id
                    //                                values (@OrderId, @ProductId)";

                    //        cmd.Parameters.Add(new SqlParameter("@OrderId", order.Id));
                    //        cmd.Parameters.Add(new SqlParameter("@ProductId", productid));
                    //    }
                    //    reader.Close();

                    //    OrderProduct orderproduct = new OrderProduct();
                    //    int newid = (int)cmd.ExecuteScalar();
                    //    orderproduct.Id = newid;

                    //    return CreatedAtRoute("GetOrders", new { id = newid }, orderproduct);
                    //}


        [HttpPut("{id}")] //Code for editing an exercise
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order order)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer
                                        SET customerId = @CustomerId,
                                        UserPaymentId = @UserPaymentId                                        
                                        WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", order.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@UserPaymentId", order.UserPaymentId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest($"No Customer with that id: {id}");
                    }
                }
            }
            catch (Exception)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        [HttpDelete("{id}")] //Code for deleting an exercise
        public async Task<IActionResult> Delete([FromRoute] int orderId, int productId)
        {
            //Delete order product

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM OrderProduct WHERE orderId= @orderId AND ProductId= @productId";
                    cmd.Parameters.Add(new SqlParameter("@orderId", orderId));
                    cmd.Parameters.Add(new SqlParameter("@productId", productId));

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
        }

        // Three helper methods
        // Private because nothing outside of the controller needs to use it

        //If  cart update Order Product 
        // IF doesnt method create cart takes in customer Id Returns Order

        //Get Cart - Cart Return cart or NULL
        //private Order cartExists(int customerid)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //            SELECT *
        //            FROM [Order]
        //            WHERE CustomerId = @CustomerId AND UserPaymentTypeId is NULL";
        //            cmd.Parameters.Add(new SqlParameter("@CustomerId", customerid));

        //            SqlDataReader reader = cmd.ExecuteReader();

        //            if (reader.Read())
        //            {
        //                Order order = new Order
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
        //                    UserPaymentId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"))
        //                };
        //                reader.Close();
        //                return (order);

        //            }
        //            else
        //            {
        //                reader.Close();

        //            }

        //        }
        //    }
        //}

        //If  cart update Order Product 
        //private OrderProduct CreateOrderProduct(int customerid)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //            SELECT *
        //            FROM [Order]
        //            WHERE CustomerId = @CustomerId AND UserPaymentTypeId is NULL";
        //            cmd.Parameters.Add(new SqlParameter("@CustomerId", customerid));

        //            SqlDataReader reader = cmd.ExecuteReader();

        //            if (reader.Read())
        //            {
        //                Order order = new Order
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
        //                    UserPaymentId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"))
        //                };
        //                reader.Close();
        //                return (order);

        //            }
        //            else
        //            {
        //                reader.Close();

        //            }

        //        }
        //    }
        //}


        // IF doesnt method create cart takes in customer Id Returns Order
        //private OrderProduct CreateCart(int customerid)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //            SELECT *
        //            FROM [Order]
        //            WHERE CustomerId = @CustomerId AND UserPaymentTypeId is NULL";
        //            cmd.Parameters.Add(new SqlParameter("@CustomerId", customerid));

        //            SqlDataReader reader = cmd.ExecuteReader();

        //            if (reader.Read())
        //            {
        //                Order order = new Order
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
        //                    UserPaymentId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"))
        //                };
        //                reader.Close();
        //                return (order);

        //            }
        //            else
        //            {
        //                reader.Close();

        //            }

        //        }
        //    }
        //}






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





