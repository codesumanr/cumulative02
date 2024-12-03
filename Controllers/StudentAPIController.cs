using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using cumulative01.Models;

namespace cumulative01.Controllers
{
    [Route("api/Student")]
    [ApiController]
    public class StudentAPIController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        // Dependency injection of school database context
        public StudentAPIController(SchoolDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all students from the database.
        /// </summary>
        /// <example>
        /// GET: api/Student/ListStudents -> 
        /// [
        ///   {
        ///     "StudentId": 1,
        ///     "StudentFName": "John",
        ///     "StudentLName": "Doe",
        ///     "StudentNumber": "S12345",
        ///     "EnrolDate": "2020/01/10"
        ///   },
        ///   ...
        /// ]
        /// </example>
        /// <returns>
        /// A list of all students in the system.
        /// </returns>
        [HttpGet]
        [Route("ListStudents")]
        public List<Student> ListStudents()
        {
            List<Student> Students = new List<Student>();

            // Open database connection and execute query to get all students
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "SELECT * FROM students";

                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    while (ResultSet.Read())
                    {
                        // Map the result set to a Student object
                        Student CurrentStudent = new Student
                        {
                            StudentId = Convert.ToInt32(ResultSet["studentid"]),
                            StudentFName = (ResultSet["studentfname"]).ToString(),
                            StudentLName = (ResultSet["studentlname"]).ToString(),
                            StudentNumber = (ResultSet["studentnumber"]).ToString(),
                            EnrolDate = ResultSet["enroldate"] != DBNull.Value
                                ? Convert.ToDateTime(ResultSet["enroldate"]).ToString("yyyy/MM/dd")
                                : ""
                        };
                        Students.Add(CurrentStudent); // Add to the list of students
                    }
                }
            }
            return Students; // Return the list of students
        }

        /// <summary>
        /// Retrieves details of a specific student by their ID.
        /// </summary>
        /// <param name="id">The ID of the student to retrieve.</param>
        /// <example>
        /// GET: api/Student/FindStudent/1 -> 
        /// {
        ///   "StudentId": 1,
        ///   "StudentFName": "John",
        ///   "StudentLName": "Doe",
        ///   "StudentNumber": "S12345",
        ///   "EnrolDate": "2020/01/10"
        /// }
        /// </example>
        /// <returns>
        /// A student object containing details of the specified student.
        /// </returns>
        [HttpGet]
        [Route("FindStudent/{id}")]
        public Student FindStudent(int id)
        {
            Student SelectedStudent = new Student();

            // Open database connection and execute query to get student by ID
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "SELECT * FROM students WHERE studentid=@id";
                Command.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    while (ResultSet.Read())
                    {
                        // Map the result set to a Student object
                        SelectedStudent = new Student
                        {
                            StudentId = Convert.ToInt32(ResultSet["studentid"]),
                            StudentFName = (ResultSet["studentfname"]).ToString(),
                            StudentLName = (ResultSet["studentlname"]).ToString(),
                            StudentNumber = (ResultSet["studentnumber"]).ToString(),
                            EnrolDate = ResultSet["enroldate"] != DBNull.Value
                                ? Convert.ToDateTime(ResultSet["enroldate"]).ToString("yyyy/MM/dd")
                                : ""
                        };
                    }
                }
            }
            return SelectedStudent; // Return the found student object
        }

        /// <summary>
        /// Adds a new student to the database.
        /// </summary>
        /// <param name="StudentData">The student data to be added.</param>
        /// <example>
        /// POST: api/Student/AddStudent -> 
        /// {
        ///   "StudentFName": "Jane",
        ///   "StudentLName": "Smith",
        ///   "StudentNumber": "S54321",
        ///   "EnrolDate": "2024/01/01"
        /// }
        /// Response: Returns the ID of the newly added student.
        /// </example>
        /// <returns>
        /// The ID of the newly created student.
        /// </returns>
        [HttpPost]
        [Route("AddStudent")]
        public int AddStudent([FromBody] Student StudentData)
        {
            // Open database connection and execute query to insert new student
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "INSERT INTO students (studentfname, studentlname, studentnumber, enroldate) VALUES (@studentfname, @studentlname, @studentnumber, @enroldate)";
                Command.Parameters.AddWithValue("@studentfname", StudentData.StudentFName);
                Command.Parameters.AddWithValue("@studentlname", StudentData.StudentLName);
                Command.Parameters.AddWithValue("@studentnumber", StudentData.StudentNumber);
                Command.Parameters.AddWithValue("@enroldate", StudentData.EnrolDate);

                Command.ExecuteNonQuery();
                return Convert.ToInt32(Command.LastInsertedId); // Return the ID of the newly inserted student
            }
        }

        /// <summary>
        /// Deletes a student from the database based on their student ID.
        /// </summary>
        /// <param name="StudentId">The ID of the student to be deleted.</param>
        /// <example>
        /// DELETE: api/Student/DeleteStudent/1 -> 
        /// Response: "The student with given id 1 has been removed from the DB"
        /// </example>
        /// <returns>
        /// A message indicating whether the deletion was successful or if the student was not found.
        /// </returns>
        [HttpDelete]
        [Route("DeleteStudent/{StudentId}")]
        public string DeleteStudent(int StudentId)
        {
            int RowsAffected = 0;

            // Open database connection and execute query to delete student by ID
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "DELETE FROM students WHERE studentid=@id";
                Command.Parameters.AddWithValue("@id", StudentId);
                RowsAffected = Command.ExecuteNonQuery();
            }

            // Return a message based on whether the student was successfully deleted or not
            return RowsAffected > 0
                ? $"The student with given id {StudentId} has been removed from the DB"
                : $"The student with given id {StudentId} is not found";
        }
    }
}
