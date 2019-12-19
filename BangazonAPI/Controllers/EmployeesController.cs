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
    public class EmployeesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeesController(IConfiguration config)
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
        /// Get all Employees
        /// </summary>
        /// <returns> A list of employees </returns>
        //[HttpGet]
        //public async Task<IActionResult> GetAllEmployees()
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"SELECT Id, FirstName, LastName, DepartmentId, Email, 
        //                                ComputerId, IsSupervisor FROM Employee";
        //            SqlDataReader reader = await cmd.ExecuteReaderAsync();
        //            List<Employee> employees = new List<Employee>();

        //            while (reader.Read())
        //            {
        //                Employee employee = new Employee
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
        //                    DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
        //                    Email = reader.GetString(reader.GetOrdinal("Email")),
        //                    ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
        //                    IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
        //                };

        //                employees.Add(employee);
        //            }
        //            reader.Close();

        //            return Ok(employees);
        //        }
        //    }
        //}

        /// <summary>
        /// Get Employee By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns> A single Employee </returns>
        [HttpGet("{id}", Name = "GetEmployee")]

        public async Task<IActionResult> GetById([FromRoute] int id)
            {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, FirstName, LastName, DepartmentId, Email, 
                                        ComputerId, IsSupervisor FROM Employee
                                        WHERE Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Employee employee = null;

                    if (reader.Read())
                    {
                        employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                        };
                    }

                    reader.Close();

                    if (employee == null)
                    {
                        return NotFound($"No employee found with the Id of {id}");
                    }

                    return Ok(employee);

                }
            }
        }

        /// <summary>
        /// Post new Employee to database
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee 
                                       (FirstName, LastName, DepartmentId, Email, 
                                        ComputerId, IsSupervisor)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @DepartmentId, @Email, 
                                        @ComputerId, @IsSupervisor)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));
                    cmd.Parameters.Add(new SqlParameter("@Email", employee.Email));
                    cmd.Parameters.Add(new SqlParameter("@ComputerId", employee.ComputerId));
                    cmd.Parameters.Add(new SqlParameter("@IsSupervisor", employee.IsSupervisor));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    employee.Id = newId;
                    return CreatedAtRoute("GetEmployee", new { id = newId }, employee);
                }
            }
        }

        /// <summary>
        /// Edit/Update Employee in database
        ///</summary>
        /// <param name="id"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee
                                            SET FirstName = @FirstName,
                                                LastName = @LastName,
                                                DepartmentId = @DepartmentId,
                                                Email = @Email,
                                                ComputerId = @ComputerId,
                                                IsSupervisor = @IsSupervisor
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@Email", employee.Email));
                        cmd.Parameters.Add(new SqlParameter("@ComputerId", employee.ComputerId));
                        cmd.Parameters.Add(new SqlParameter("@IsSupervisor", employee.IsSupervisor));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest($"No employee with the Id {id}");
                    }
                }
            }
            catch (Exception)
            {
                bool exists = await EmployeeExists(id);
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

        /// <summary>
        /// Query database to find employees by firstname and lastname
        /// </summary>
        /// <returns> A list of Students </returns>
        [HttpGet]
        public async Task<IActionResult> GetBySearch([FromQuery] string searchName)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, FirstName, LastName, 
                                       DepartmentId, Email, ComputerId, IsSupervisor
                                       FROM Employee WHERE 1=1";

                    if (!string.IsNullOrWhiteSpace(searchName))
                    {
                        cmd.CommandText += " AND FirstName Like @searchName OR LastName LIKE @searchName";
                        cmd.Parameters.Add(new SqlParameter(searchName, "%" + searchName + "%"));
                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Employee> employees = new List<Employee>();

                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                        };

                        employees.Add(employee);
                    }
                    reader.Close();

                    return Ok(employees);
                }
            }
        }


        /// <summary>
        /// Private method to see if an employee exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<bool> EmployeeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, DepartmentId, Email, ComputerId, IsSupervisor
                        FROM Employee
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    return reader.Read();
                }
            }
        }
    }
}