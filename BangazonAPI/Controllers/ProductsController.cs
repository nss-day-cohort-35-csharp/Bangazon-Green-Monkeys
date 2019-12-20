using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _config;


        public ProductsController(IConfiguration config)
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
        /// <summary>
        /// Get list of product from database.
        /// </summary>
        /// <returns>list of product from database.</returns>
        //[HttpGet]
        //public async Task<IActionResult> GetAllProduct()
        //{

        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();

        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {

        //            cmd.CommandText = @"SELECT Id, DateAdded, ProductTypeId, CustomerId, Price, Title, [Description] from Product";

        //            SqlDataReader reader = await cmd.ExecuteReaderAsync();

        //            List<Product> products = new List<Product>();

        //            while (reader.Read())
        //            {
        //                Product product = new Product()
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
        //                    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
        //                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
        //                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
        //                    Title = reader.GetString(reader.GetOrdinal("Title")),
        //                    Description = reader.GetString(reader.GetOrdinal("Description"))

        //                };

        //                products.Add(product);
        //            }

        //            reader.Close();
        //            return Ok(products);
        //        }
        //    }
        //}

        /// <summary>
        /// Get a product from database by id.
        /// </summary>
        /// <returns>Get a product from database by id.</returns>

        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> GetProductById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, DateAdded, ProductTypeId, CustomerId, Price, Title, [Description] from Product WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Product product = null;

                    if (reader.Read())
                    {
                        product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description"))

                        };
                    }
                    reader.Close();
                    if (product == null)
                    {
                        return NotFound($"No product found with the id {id}");
                    }
                    return Ok(product);

                }
            }
        }

        /// <summary>
        /// Get list of product from database.
        /// </summary>
        /// <returns>list of product from database.</returns>
        [HttpGet]
        public async Task<List<Product>> GetAllProduct(string q, string sortBy, string asc)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, DateAdded, ProductTypeId, CustomerId, Price, Title, [Description] from Product";

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        cmd.CommandText += @" WHERE Title LIKE @q OR Description LIKE @q";
                    }
                    cmd.Parameters.Add(new SqlParameter("@q", "%" + q + "%"));
                    if (sortBy == "recent")
                    {
                        cmd.CommandText += " ORDER BY DateAdded";
                    }
                    else if (sortBy == "price")
                    {
                        cmd.CommandText += " ORDER BY Price";
                    }
                    if (!string.IsNullOrWhiteSpace(sortBy) && asc == "false")
                    {
                        cmd.CommandText += " desc";
                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Product> products = new List<Product>();
                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                        };
                        products.Add(product);
                    }
                    reader.Close();
                    return products;

                }
            }

        }

        /// <summary>
        /// Add product to the database.
        /// </summary>
        /// <returns> Add product to the database.</returns>
        [HttpPost]
        public void AddProduct(Product product)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Product(DateAdded, ProductTypeId, CustomerId, Price, Title, Description) " +
                                      "OUTPUT INSERTED.Id Values(@DateAdded, @ProductTypeId, @CustomerId, @Price, @Title, @Description)";
                    cmd.Parameters.Add(new SqlParameter("@DateAdded", DateTime.Now));
                    cmd.Parameters.Add(new SqlParameter("@ProductTypeId", product.ProductTypeId));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", product.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@Price", product.Price));
                    cmd.Parameters.Add(new SqlParameter("@Title", product.Title));
                    cmd.Parameters.Add(new SqlParameter("@Description", product.Description));

                    int id = (int)cmd.ExecuteScalar();
                    product.Id = id;

                }

            }
        }

        /// <summary>
        /// Update product.
        /// </summary>
        /// <returns> Update product.</returns>

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute]int id, [FromBody]Product product)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Product
                                            SET DateAdded = @DateAdded, ProductTypeId = @ProductTypeId,
                                                CustomerId = @CustomerId, Price = @Price, Title = @Title,
                                                Description = @Description
                                            WHERE Id  = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@DateAdded", DateTime.Now));
                        cmd.Parameters.Add(new SqlParameter("@ProductTypeId", product.ProductTypeId));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", product.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@Price", product.Price));
                        cmd.Parameters.Add(new SqlParameter("@Title", product.Title));
                        cmd.Parameters.Add(new SqlParameter("@Description", product.Description));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// Delete product.
        /// </summary>
        /// <returns> Delete product.</returns>

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Product WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, DateAdded, ProductTypeId, CustomerId, Price, Title, 
                        [Description] from Product WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
