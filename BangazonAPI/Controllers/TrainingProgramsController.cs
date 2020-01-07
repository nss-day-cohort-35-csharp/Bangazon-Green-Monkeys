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
        /// <summary>
        /// Edit/Update Department in database
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] TrainingProgram trainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE TrainingProgram 
                                            SET Name = @name,
                                                StartDate = @startDate,
                                                EndDate = @endDate,
                                                MaxAttendees = @maxAttendees
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", trainingProgram.Name));
                        cmd.Parameters.Add(new SqlParameter("@startDate", trainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@endDate", trainingProgram.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

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
                bool exists = await TrainingProgramExists(id);
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

        [HttpDelete("{id}")] //Code for deleting an 
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM TrainingProgram WHERE Id = @id";
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
                bool exists = await TrainingProgramExists(id);
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

        [HttpDelete("{tpId/employees/{eId}")] //Code for deleting an 
        public async Task<IActionResult> Delete([FromRoute] int tpId, [FromRoute] int eId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM EmployeeTraining WHERE EmployeeId = @eId AND TrainingProgramId = @tpId";
                        cmd.Parameters.Add(new SqlParameter("@id", eId));
                        cmd.Parameters.Add(new SqlParameter("@id", tpId));
                        

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
                bool exists = await EmployeeTrainingExists(tpId, eId);
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
        private async Task<bool> TrainingProgramExists(int id)
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                        SELECT Id, [Name], StartDate, EndDate, MaxAttendees
                        FROM TrainingPoogram
                        WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        SqlDataReader reader = await cmd.ExecuteReaderAsync();
                        return reader.Read();
                    }
                }
            }
        private async Task<bool> EmployeeTrainingExists(int tpId, int eId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, EmployeeId, TrainingProgramId
                        FROM EmployeeTraining
                        WHERE EmployeeId = @eId AND TrainingProgramId = @tpId";
                    
                    cmd.Parameters.Add(new SqlParameter("@tpId", tpId));
                    cmd.Parameters.Add(new SqlParameter("@eId", eId));

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    return reader.Read();
                }
            }
        }
    }

    }

