using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;


namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPaymentTypesController : ControllerBase

    {
        private readonly IConfiguration _config;

        public UserPaymentTypesController(IConfiguration config)
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

        [HttpGet(Name = "GetUserPayment")]
        public async Task<IActionResult> Get([FromQuery]int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT 
Id, AcctNumber, Active, CustomerId, PaymentTypeId
FROM UserPaymentType
WHERE CustomerId = @CustomerId";

                    //dylan recording minute 2:45 1/7/20
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", customerId));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<UserPaymentType> userPaymentTypes = new List<UserPaymentType>();


                    while (reader.Read())
                    {
                        UserPaymentType userPaymentType = new UserPaymentType()
                        {



                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            AcctNumber = reader.GetString(reader.GetOrdinal("AcctNumber")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active"))
                        };
                        userPaymentTypes.Add(userPaymentType);
                    }
                    reader.Close();

                    return Ok(userPaymentTypes);

           
                }
            }
        }



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserPaymentType userPaymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"INSERT INTO UserPaymentType (AcctNumber, Active, CustomerId, PaymentTypeId)
OUTPUT INSERTED.Id
VALUES (@acctNumber, @active, @customerId, @paymentTypeId)";
                    cmd.Parameters.Add(new SqlParameter("@acctNumber", userPaymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@active", userPaymentType.Active));
                    cmd.Parameters.Add(new SqlParameter("@customerId", userPaymentType.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@paymentTypeId", userPaymentType.PaymentTypeId));

                    int newId = (int)cmd.ExecuteScalar();
                    userPaymentType.Id = newId;
                    return CreatedAtRoute("GetUserPayment", new { id = newId }, userPaymentType);
                }
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] UserPaymentType userPaymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE UserPaymentType
                                            SET AcctNumber = @acctNumber,
                                                Active = @active 
                                                WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@acctNumber", userPaymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@active", userPaymentType.Active));
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
                if (!UserPaymentExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE UserPaymentType
                                            SET Active = 0
                                            WHERE Id = @id";
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
                if (!UserPaymentExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool UserPaymentExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, AcctNumber, Active
                        FROM UserPaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

