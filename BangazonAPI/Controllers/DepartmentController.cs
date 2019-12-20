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
        public class DepartmentController : ControllerBase
        {
            private readonly IConfiguration _config;

            public DepartmentController(IConfiguration config)
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
            /// get all departments
            /// </summary>
            [HttpGet]
            public async Task<IActionResult> GetAllDepartments([FromQuery] string dept)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                    cmd.CommandText = "SELECT Id, Name, Budget FROM Department";


                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        List<Department> departments = new List<Department>();

                        while (reader.Read())
                        {
                            Department department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            };

                            departments.Add(department);
                        }
                        reader.Close();

                        return Ok(departments);
                    }
                }
            }

            /// <summary>
            /// Get Department By Id
            /// </summary>
            /// 
            [HttpGet("{id}", Name = "GetDepartments")]

            public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery]string include)
            {
                using (SqlConnection conn = Connection)
                {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                { 
                    if (include == "employees")
                    {
                        cmd.CommandText = @"SELECT Id, Name, Budget FROM Department WHERE id = @id
                                           LEFT JOIN 
                                           ON Employee e";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                    }

                    else

                    cmd.CommandText = "SELECT Id, Name, Budget FROM Department WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Department department = null;

                    if (reader.Read())
                    {
                        department = new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                        };
                    }

                    reader.Close();

                    if (department == null)
                    {
                        return NotFound($"No Department found with the Id of {id}");
                    }

                    return Ok(department);
                }
            }
        }
        /// <summary>
        /// Post new Department to database
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department (Name, Budget) OUTPUT INSERTED.Id
                                        VALUES (@Name, @Budget)";

                    cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                    cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    department.Id = newId;
                    return CreatedAtRoute("GetDepartment", new { id = newId }, department);
                }
            }
        }

        /// <summary>
        /// Edit/Update Department in database
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department 
                                            SET Name = @Name,
                                                Budget = @Budget
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Id", id));
                        cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest($"No department with the id of {id}");
                    }
                }
            }

            catch (Exception)
            {
                bool exists = await DepartmentExists(id);
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

        ///<summary>
        /// Private method to see if a department exists
        /// </summary>
        private async Task<bool> DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Budget FROM Department WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    return reader.Read();
                }
            }
        }
    }
}

