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
    public class ProductTypesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypesController(IConfiguration config)
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
        /// Gets a list of all product types from database
        /// </summary>
        /// <returns>A list of product types</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllProductTypes()
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"SELECT Id, Name AS ProductName FROM ProductType";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<ProductType> productTypes = new List<ProductType>();

                    while (reader.Read())
                    {
                        ProductType productType = new ProductType()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("ProductName")),

                        };

                        productTypes.Add(productType);
                    }

                    reader.Close();
                    return Ok(productTypes);
                }
            }
        }

        /// <summary>
        /// Get product type from database by id.
        /// </summary>
        /// <returns>Product type by id.</returns>

        // [HttpGet("{id}", Name = "GetProductType")]
        //public async Task<IActionResult> GetProductTypeById([FromRoute]int id, string include)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "SELECT Id AS ProductId, Name AS ProductName FROM ProductType WHERE Id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));
        //            SqlDataReader reader = await cmd.ExecuteReaderAsync();

        //            ProductType productType = null;

        //            if (reader.Read())
        //            {
        //                productType = new ProductType
        //                {
        //                    Id = id,
        //                    Name = reader.GetString(reader.GetOrdinal("ProductName"))

        //                };
        //            }
        //            reader.Close();
        //            if (productType == null)
        //            {
        //                return NotFound($"No product type found with the id {id}");
        //            }
        //            return Ok(productType);

        //        }
        //    }
        //}

        /// <summary>
        /// Get product type from database by id and includes products.
        /// </summary>
        /// <returns>Product type by id and includes products.</returns>

        [HttpGet("{id}")]
        
        public async Task<IActionResult> GetProductTypeById(int id, string include)
        {
            if (include != null && include == "products")
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT pt.Id AS ProductTypeId, pt.[Name] AS ProductTypeName, p.Id AS ProductId, 
                                        p.DateAdded AS ProductDateAdded, p.ProductTypeId, p.CustomerId, p.Price, p.Title, p.[Description]
                                        FROM ProductType pt LEFT JOIN Product p ON
                                        pt.Id = p.ProductTypeId
                                        WHERE pt.Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        List<ProductType> productTypes = new List<ProductType>();
                        
                        while (reader.Read())
                        {
                            var productTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId"));
                            var productTypeAlreadyAdded = productTypes.FirstOrDefault(e => e.Id == productTypeId);
                            var hasProduct = !reader.IsDBNull(reader.GetOrdinal("ProductId"));

                            if (productTypeAlreadyAdded == null)
                            {
                                ProductType productType = new ProductType
                                {
                                    Id = productTypeId,
                                    Name = reader.GetString(reader.GetOrdinal("ProductTypeName")),
                                    Products = new List<Product>()

                                };
                                productTypes.Add(productType);
                               
                                {
                                    if (hasProduct)
                                    {
                                        Product product = new Product()

                                        {
                                            Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                            DateAdded = reader.GetDateTime(reader.GetOrdinal("ProductDateAdded")),
                                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                            Title = reader.GetString(reader.GetOrdinal("Title")),
                                            Description = reader.GetString(reader.GetOrdinal("Description")),

                                        };
                                        productType.Products.Add(product);
                                    }
                                }

                            }
                            else
                            {
                                if (hasProduct)
                                    {
                                        Product product = new Product()
                                        {
                                            Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                            DateAdded = reader.GetDateTime(reader.GetOrdinal("ProductDateAdded")),
                                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                            Title = reader.GetString(reader.GetOrdinal("Title")),
                                            Description = reader.GetString(reader.GetOrdinal("Description")),
                                        };
                                        productTypeAlreadyAdded.Products.Add(product);
                                    }
                                

                            }
                        }
                        reader.Close();
                        return Ok(productTypes);
                    }
                }


            }
            else
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT Id AS ProductId, Name AS ProductName FROM ProductType WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        ProductType productType = null;

                        if (reader.Read())
                        {
                            productType = new ProductType
                            {
                                Id = id,
                                Name = reader.GetString(reader.GetOrdinal("ProductName"))

                            };
                        }
                        reader.Close();
                        if (productType == null)
                        {
                            return NotFound($"No product type found with the id {id}");
                        }
                        return Ok(productType);

                    }
                }
            }

        }

        /// <summary>
        /// Add product type to the database.
        /// </summary>
        /// <returns> Add product type to the database.</returns>

        [HttpPost]
        public void AddProductType([FromBody] ProductType productType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO ProductType(Name) OUTPUT INSERTED.Id Values(@Name)";
                    cmd.Parameters.Add(new SqlParameter("@Name", productType.Name));

                    int id = (int)cmd.ExecuteScalar();
                    productType.Id = id;

                }


            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductType([FromRoute]int id, [FromBody]ProductType productType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE ProductType
                                            SET Name = @Name
                                            WHERE Id  = @id;";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@Name", productType.Name));

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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
