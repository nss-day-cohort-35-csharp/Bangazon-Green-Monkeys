using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using System;


namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputersController(IConfiguration config)
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
        /// Get computers available and unavailable
        /// </summary>
        /// <returns> A list of Computers </returns>
        [HttpGet]
        public async Task<IActionResult> Get(string available)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (available == "false")
                    {
                        cmd.CommandText = @"SELECT c.Id as ComputerId, PurchaseDate, DecomissionDate, Make, Model, e.Id as EmployeeId
                                            FROM Computer c 
                                            LEFT JOIN Employee e ON e.ComputerId = c.Id
                                           WHERE DecomissionDate IS NOT NULL OR e.Id IS NOT NULL";


                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        List<Computer> computers = new List<Computer>();
                        Computer computer = null;

                        while (reader.Read())
                        {

                            computer = new Computer()
                            {

                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Model = reader.GetString(reader.GetOrdinal("Model"))
                            };

                            var decomissionNotNull = !reader.IsDBNull(reader.GetOrdinal("DecomissionDate"));

                            if (decomissionNotNull)
                            {
                                computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                            }
                            computers.Add(computer);


                        }
                        reader.Close();

                        return Ok(computers);

                    }

                    else
                    {
                        cmd.CommandText = @"SELECT c.Id as ComputerId, PurchaseDate, DecomissionDate, Make, Model, e.Id as EmployeeId
                                           FROM Computer c 
                                           LEFT JOIN Employee e ON e.ComputerId = c.Id
                                           WHERE DecomissionDate IS NULL AND e.Id IS NULL";


                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        List<Computer> computers = new List<Computer>();
                        Computer computer = null;

                        while (reader.Read())
                        {


                            computer = new Computer()
                            {

                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Model = reader.GetString(reader.GetOrdinal("Model"))
                            };

                            computers.Add(computer);

                        }

                        reader.Close();

                        return Ok(computers);
                    }
                }
            }
        }


        ///// <summary>
        ///// Get Computer By Id
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns> A single computer </returns>
        [HttpGet("{id}", Name = "GetComputer")]

        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, PurchaseDate, DecomissionDate, Make, Model
                                        FROM Computer WHERE Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Computer computer = null;

                    if (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            //DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model"))
                        };
                    }


                    reader.Close();

                    return Ok(computer);

                }
            }
        }


        ///// <summary>
        ///// Post new Computer to database
        ///// </summary>
        ///// <param name="computer"></param>
        ///// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Computer 
                                       (Make, Model, PurchaseDate, DecomissionDate)
                                        OUTPUT INSERTED.id
                                        VALUES (@Make, @Model, @PurchaseDate, @DecomissionDate)";
                    cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                    cmd.Parameters.Add(new SqlParameter("@Model", computer.Model));
                    cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computer.PurchaseDate));
                    if (computer.DecomissionDate == null)
                    {
                        cmd.Parameters.Add(new SqlParameter("@DecomissionDate", DBNull.Value));

                    }
                    else
                    {
                        cmd.Parameters.Add(new SqlParameter("@DecomissionDate", computer.DecomissionDate));

                    }
                    int newId = (int)await cmd.ExecuteScalarAsync();
                    computer.Id = newId;
                    return CreatedAtRoute("GetComputer", new { id = newId }, computer);
                }
            }
        }

        ///// <summary>
        ///// Edit/Update Computer in database
        /////</summary>
        ///// <param name="id"></param>
        ///// <param name="computer"></param>
        ///// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Computer
                                            SET Make = @Make,
                                                Model = @Model,
                                                PurchaseDate = @PurchaseDate,
                                                DecomissionDate = @DecomissionDate
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@Make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@Model", computer.Model));
                        cmd.Parameters.Add(new SqlParameter("@PurchaseDate", computer.PurchaseDate));
                        if (computer.DecomissionDate == null)
                        {
                            cmd.Parameters.Add(new SqlParameter("@DecomissionDate", DBNull.Value));

                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@DecomissionDate", computer.DecomissionDate));

                        }

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest($"No computer with the Id {id}");
                    }
                }
            }
            catch (Exception)
            {
                bool exists = await ComputerExists(id);
                if (!exists)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")] //Code for deleting a computer
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Computer WHERE Id = @id";
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
                if (!await ComputerExists(id))
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
        /// Private method to see if an computer exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<bool> ComputerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                SELECT Id, Make, Model, PurchaseDate, DecomissionDate
                FROM Computer
                WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    return reader.Read();
                }
            }
        }
    }
}
