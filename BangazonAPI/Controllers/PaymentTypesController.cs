using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypesController(IConfiguration config)
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
        //Get all payment types (array) (GET_
        //get payment type by Id (object) (GET)
        //Add new payment type (object) (POST)
        //Remove payment type (DELETE) **this is a soft delete

        [HttpGet]
        public async Task<IActionResult> GetAllPaymentTypes()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, Active FROM PaymentType";

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<PaymentType> paymentTypes = new List<PaymentType>();

                    while (reader.Read())
                    {
                        //recording w/ dylan min 3:45
                        PaymentType paymentType = new PaymentType()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active")),

                        };

                        paymentTypes.Add(paymentType);
                    }

                    reader.Close();
                    return Ok(paymentTypes);
                }
            }
        }

        //get payment type from dB by Id

        [HttpGet("{id}", Name = "GetPayment")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            Id, Name, Active 
                            FROM PaymentType
                            WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    PaymentType paymentType = null;

                    if (reader.Read())
                    {
                        paymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active"))

                        };
                    }
                    reader.Close();

                    if (paymentType == null)
                    {
                        return NotFound($"No payment found with the following ID: {id}");
                    };
                    return Ok(paymentType);
                }
            }
        }

        [HttpPost]
    public async Task<IActionResult> Post([FromBody] PaymentType payment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO PaymentType (Name, Active)
OUTPUT INSERTED.Id
VALUES (@name, @active)";
                    cmd.Parameters.Add(new SqlParameter("@name", payment.Name));
                    cmd.Parameters.Add(new SqlParameter("@active", payment.Active));

                    int newId = (int)cmd.ExecuteScalar();
                    payment.Id = newId;
                    return CreatedAtRoute("GetPayment", new { id = newId }, payment);
                }
            }
        }


       [HttpDelete("{id}")]
       public async Task<IActionResult> DeletePaymentType([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
SET Active = @false
WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@false", false));

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
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool PaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Active, Name FROM PaymentType WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

