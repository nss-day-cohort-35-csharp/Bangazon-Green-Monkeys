using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingProgramsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TrainingProgramsController(IConfiguration config)
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
        /// get all training programs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTrainingPrograms([FromQuery] string programs)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram";


                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        TrainingProgram trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        };

                        trainingPrograms.Add(trainingProgram);
                    }
                    reader.Close();

                    return Ok(trainingPrograms);
                }
            }
        }

        /// <summary>
        /// Get Training Program By Id
        /// </summary>
        /// 
        //[HttpGet("{id}", Name = "GetTrainingPrograms")]

        //public async Task<IActionResult> GetTrainingProgramById([FromRoute] int id, [FromQuery]string include)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            if (include == "")
        //            {
        //                cmd.CommandText = @"SELECT d.Id, d.[Name], d.Budget, e.DepartmentId, e.FirstName,
        //                                    e.LastName, e.Id, e.ComputerId, e.Email, e.IsSupervisor FROM Department d
        //                                    LEFT JOIN Employee e ON d.Id = e.DepartmentId";
        //                cmd.Parameters.Add(new SqlParameter("@id", id));
        //                SqlDataReader reader = await cmd.ExecuteReaderAsync();

        //                List<Employee> employees = new List<Employee>();

        //                Department department = null;

        //                Employee employee = null;

        //                if (reader.Read())
        //                {
        //                    employee = new Employee
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
        //                        Email = reader.GetString(reader.GetOrdinal("Email")),
        //                        ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
        //                        DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
        //                        IsSupervisor = reader.GetBoolean(reader.GetOrdinal("isSupervisor"))
        //                    };

        //                    employees.Add(employee);


        //                    department = new Department
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        Name = reader.GetString(reader.GetOrdinal("Name")),
        //                        Budget = reader.GetInt32(reader.GetOrdinal("Budget")),

        //                        Employees = employees
        //                    };
        //                }

        //                reader.Close();

        //                if (department == null)
        //                {
        //                    return NotFound($"No Department found with the Id of {id}");
        //                }

        //                return Ok(department);
        //            }

        //            else
        //            {
        //                cmd.CommandText = "SELECT Id, Name, Budget FROM Department WHERE id = @id";
        //                cmd.Parameters.Add(new SqlParameter("@id", id));


        //                SqlDataReader reader = await cmd.ExecuteReaderAsync();

        //                Department department = null;

        //                if (reader.Read())
        //                {
        //                    department = new Department
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        Name = reader.GetString(reader.GetOrdinal("Name")),
        //                        Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
        //                    };
        //                }

        //                reader.Close();

        //                if (department == null)
        //                {
        //                    return NotFound($"No Department found with the Id of {id}");
        //                }

        //                return Ok(department);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Post new Training Program to database
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees) OUTPUT INSERTED.Id
                                        VALUES (@Name, @StartDate, @EndDate, @MaxAttendees)";

                    cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));


                    int newId = (int)await cmd.ExecuteScalarAsync();
                    trainingProgram.Id = newId;
                    return CreatedAtRoute("GetTrainingPrograms", new { id = newId }, trainingProgram);
                }
            }
        }
        /// <summary>
        /// Post new Training Program with Employee to database
        /// </summary>
        /// 
        [HttpPost("{id}/employees")]
        public async Task<IActionResult> Put([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees) OUTPUT INSERTED.Id
                                        VALUES (@Name, @StartDate, @EndDate, @MaxAttendees)";

                    cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));


                    int newId = (int)await cmd.ExecuteScalarAsync();
                    trainingProgram.Id = newId;
                    return CreatedAtRoute("GetTrainingPrograms", new { id = newId }, trainingProgram);
                }
            } 
        }
    }
}
