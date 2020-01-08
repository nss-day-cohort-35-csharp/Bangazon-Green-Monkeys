using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentsController(IConfiguration config)
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
        public async Task<IActionResult> GetAllDepartments()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, [Name] AS DepartmentName, Budget AS DepartmentBudget FROM Department";
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Department> departments = new List<Department>();
                    while (reader.Read())
                    {
                        Department department = new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
                            Budget = reader.GetInt32(reader.GetOrdinal("DepartmentBudget"))
                        };
                        departments.Add(department);
                    }
                    reader.Close();
                    return Ok(departments);
                }
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery]string include)
        {
            if (include != null && include == "employees")
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT d.Id AS DepartmentId, d.[Name] AS DepartmentName, d.Budget AS DepartmentBudget, e.DepartmentId AS EmployeeDepartmentId, e.FirstName AS EmployeeFirstName,
                                            e.LastName AS EmployeeLastName, e.Id AS EmployeeId, e.ComputerId AS EmployeeComputerId, e.Email AS EmployeeEmail, e.IsSupervisor AS EmployeeSupervisor FROM Department d
                                            LEFT JOIN Employee e ON d.Id = e.DepartmentId
                                            WHERE d.Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        List<Department> departments = new List<Department>();
                        while (reader.Read())
                        {
                            var departmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"));
                            var departmentAlreadyAdded = departments.FirstOrDefault(d => d.Id == departmentId);
                            var hasEmployee = !reader.IsDBNull(reader.GetOrdinal("EmployeeId"));
                            if (departmentAlreadyAdded == null)
                            {
                                Department department = new Department
                                {
                                    Id = departmentId,
                                    Name = reader.GetString(reader.GetOrdinal("DepartmentName")),
                                    Budget = reader.GetInt32(reader.GetOrdinal("DepartmentBudget")),
                                    Employees = new List<Employee>()
                                };
                                departments.Add(department);
                                {
                                    if (hasEmployee)
                                    {
                                        Employee employee = new Employee()
                                        {
                                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                            FirstName = reader.GetString(reader.GetOrdinal("EmployeeFirstName")),
                                            LastName = reader.GetString(reader.GetOrdinal("EmployeeLastName")),
                                            DepartmentId = reader.GetInt32(reader.GetOrdinal("EmployeeDepartmentId")),
                                            ComputerId = reader.GetInt32(reader.GetOrdinal("EmployeeComputerId")),
                                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("EmployeeSupervisor")),
                                            Email = reader.GetString(reader.GetOrdinal("EmployeeEmail"))
                                        };
                                        department.Employees.Add(employee);
                                    }
                                }
                            }
                            else
                            {
                                Employee employee = new Employee()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("EmployeeFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("EmployeeLastName")),
                                    DepartmentId = reader.GetInt32(reader.GetOrdinal("EmployeeDepartmentId")),
                                    ComputerId = reader.GetInt32(reader.GetOrdinal("EmployeeComputerId")),
                                    IsSupervisor = reader.GetBoolean(reader.GetOrdinal("EmployeeSupervisor")),
                                    Email = reader.GetString(reader.GetOrdinal("EmployeeEmail"))
                                };
                                departmentAlreadyAdded.Employees.Add(employee);
                            }
                        }
                        reader.Close();
                        return Ok(departments);
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
                        cmd.CommandText = @"SELECT d.Id AS DepartmentId, d.[Name] AS DepartmentName, d.Budget AS DepartmentBudget FROM Department d
                                            WHERE d.Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        Department department = null;
                        if (reader.Read())
                        {
                            department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Budget = reader.GetInt32(reader.GetOrdinal("DepartmentBudget")),
                                Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                            };
                        }
                        return Ok(department);
                    }
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



