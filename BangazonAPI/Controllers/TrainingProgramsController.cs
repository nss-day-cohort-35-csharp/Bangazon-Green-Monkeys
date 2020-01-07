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
        /// Get  By Id
        /// </summary>
        /// 
        [HttpGet("{id}", Name = "TrainingProgram")]

        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (id != null)
                    {
                        cmd.CommandText = @"SELECT Id, [Name], StartDate, EndDate, MaxAttendees
                                            FROM TrainingProgram 
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                        TrainingProgram trainingProgram = null;

                        if (reader.Read())
                        {
                            trainingProgram = new TrainingProgram
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

                        if (trainingProgram == null)
                        {
                            return NotFound($"No Training Program found with the Id of {id}");
                        }

                        return Ok(trainingProgram);

                    }

                    else
                    {
                        cmd.CommandText = "SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));


                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        TrainingProgram trainingProgram = null;

                        if (reader.Read())
                        {
                            trainingProgram = new TrainingProgram
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                            };
                        }

                        reader.Close();

                        if (trainingProgram == null)
                        {
                            return NotFound($"No Department found with the Id of {id}");
                        }

                        return Ok(trainingProgram);
                    }
                }
            }
        }
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
        public async Task<IActionResult> Post([FromRoute] int id, [FromBody] Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())

                    //if statement
                {
                    cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId) OUTPUT INSERTED.Id
                                        VALUES (@EmployeeId, @TrainingProgramId)";

                    cmd.Parameters.Add(new SqlParameter("@EmployeeId", employee.Id));
                    cmd.Parameters.Add(new SqlParameter("@TrainingProgramId", id));

                    //new Employee Training
                    EmployeeTraining employeeTraining = new EmployeeTraining();
     ;

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    employeeTraining.Id = newId;
                    return CreatedAtRoute("GetTrainingPrograms", new { id = newId }, employeeTraining);

                    
                }
            }
        }
        private async Task<bool> EmployeeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT *
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
