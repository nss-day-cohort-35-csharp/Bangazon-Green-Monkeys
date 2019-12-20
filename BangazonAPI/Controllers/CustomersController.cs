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
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomersController(IConfiguration config)
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

        [HttpGet] // Get All Customers
        public async Task<IActionResult> Get([FromQuery] string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Customer WHERE 1=1";

                    if (q != null)
                    {
                        cmd.CommandText += " AND LastName LIKE @q";
                        cmd.Parameters.Add(new SqlParameter("@q", "%" + q + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        Customer customer = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            City = reader.GetString(reader.GetOrdinal("City")),
                            State = reader.GetString(reader.GetOrdinal("State")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone"))
                        };

                        customers.Add(customer);
                    }
                    reader.Close();

                    return Ok(customers);
                }
            }
        }

        [HttpGet("{id}", Name = "GetCustomer")] //Code for getting a single exercise
        public async Task<IActionResult> Get([FromRoute] int id, string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    if (include == "products")
                    {
                        cmd.CommandText = @"SELECT
                                          c.Id AS CustomerPrimaryId,
                                          c.Active,
                                          c.CreatedDate,
                                          c.FirstName,
                                          c.LastName,
                                          c.Email,
                                          c.[Address],
                                          c.Phone,
                                          c.City,
                                          c.State,
                                          p.Id AS ProductPrimaryId,
                                          p.CustomerId,
                                          p.DateAdded,
                                          p.[Description],
                                          p.Price,
                                          p.ProductTypeId,
                                          p.Title
                                          FROM Customer c
                                          LEFT JOIN Product p ON c.Id = p.CustomerId
                                          WHERE 1=1";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();


                        List<Product> products = new List<Product>();
                        Customer customer = null;

                        if (reader.Read())
                        {
                            Product product = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductPrimaryId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                                Description = reader.GetString(reader.GetOrdinal("Address")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                Title = reader.GetString(reader.GetOrdinal("Title"))
                            };

                            products.Add(product);

                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CustomerPrimaryId")),
                                Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                State = reader.GetString(reader.GetOrdinal("State")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Products = products
                            };
                        }
                        reader.Close();
                        return Ok(customer);
                    }
                    else
                    {
                        cmd.CommandText = "SELECT * FROM Customer WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();

                        Customer customer = null;

                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                State = reader.GetString(reader.GetOrdinal("State")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            };
                        }
                        reader.Close();
                        return Ok(customer);

                    };
                }
            }
        }

        [HttpPost] //Adds Customer
        public async Task<IActionResult> post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT into Customer (Active, CreatedDate, FirstName, LastName, Address, City, State, Email, Phone)
                                                OUTPUT INSERTED.id
                                                values (@Active, @CreateDate, @FirstName, @LastName, @Address, @City, @State, @Email, @Phone)";
                    cmd.Parameters.Add(new SqlParameter("@Active", customer.Active));
                    cmd.Parameters.Add(new SqlParameter("@CreateDate", customer.CreatedDate));
                    cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));
                    cmd.Parameters.Add(new SqlParameter("@Address", customer.Address));
                    cmd.Parameters.Add(new SqlParameter("@City", customer.City));
                    cmd.Parameters.Add(new SqlParameter("@State", customer.State));
                    cmd.Parameters.Add(new SqlParameter("@Email", customer.Email));
                    cmd.Parameters.Add(new SqlParameter("@Phone", customer.Phone));

                    int newid = (int)cmd.ExecuteScalar();
                    customer.Id = newid;
                    return CreatedAtRoute("GetCustomer", new { id = newid }, customer);
                }
            }
        }
        [HttpPut("{id}")] //Code for editing an exercise
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody]Customer customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer
                                        SET Active = @Active,
                                        CreatedDate = @CreatedDate,
                                        FirstName = @FirstName,
                                        LastName = @LastName,
                                        Address = @Address,
                                        City = @City,
                                        State = @State,
                                        Email = @Email
                                        WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Active", customer.Active));
                        cmd.Parameters.Add(new SqlParameter("@CreatedDate", customer.CreatedDate));
                        cmd.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));
                        cmd.Parameters.Add(new SqlParameter("@Address", customer.Address));
                        cmd.Parameters.Add(new SqlParameter("@City", customer.City));
                        cmd.Parameters.Add(new SqlParameter("@State", customer.State));
                        cmd.Parameters.Add(new SqlParameter("@Email", customer.Email));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest($"No Customer with id: {id}");
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
        public async Task<IActionResult> Delete([FromRoute] int id, [FromBody]Customer customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer SET Active = @Active WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Active", customer.Active));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
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













