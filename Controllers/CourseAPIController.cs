using cumulative01.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace cumulative01.Controllers
{
    [Route("api/Course")]
    [ApiController]
    public class CourseAPIController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        // Constructor with dependency injection for the school database context
        public CourseAPIController(SchoolDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all courses available in the system.
        /// </summary>
        /// <example>
        /// GET api/Course/ListCourses -> 
        /// [{"courseId":1,"courseCode":"http5101","teacherId":1,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Web Application Development"},
        ///  {"courseId":2,"courseCode":"http5102","teacherId":2,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Project Management"},
        ///  {"courseId":3,"courseCode":"http5103","teacherId":5,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Web Programming"}, ...]
        /// </example>
        /// <returns>
        /// A list of Course objects containing course details.
        /// </returns>
        [HttpGet]
        [Route("ListCourses")]
        public List<Course> ListCourses()
        {
            // Initialize an empty list to store courses
            List<Course> Courses = new List<Course>();

            // Establish a database connection using the context
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection to the database
                Connection.Open();

                // Create a command to query the courses from the database
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "SELECT * FROM courses";

                // Execute the query and process the results
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    // Read through each record in the result set
                    while (ResultSet.Read())
                    {
                        // Create a new course object for each row
                        Course CurrentCourse = new Course
                        {
                            CourseId = Convert.ToInt32(ResultSet["courseid"]),
                            CourseCode = ResultSet["coursecode"].ToString(),
                            TeacherId = Convert.ToInt32(ResultSet["teacherid"]),
                            StartDate = Convert.ToDateTime(ResultSet["startdate"]).ToString("yyyy-MM-dd"),
                            FinishDate = Convert.ToDateTime(ResultSet["finishdate"]).ToString("yyyy-MM-dd"),
                            CourseName = ResultSet["coursename"].ToString()
                        };
                        // Add the current course to the list
                        Courses.Add(CurrentCourse);
                    }
                }
            }

            // Return the list of courses
            return Courses;
        }

        /// <summary>
        /// Retrieves a course by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the course to retrieve.</param>
        /// <example>
        /// GET api/Course/FindCourse/7 -> {"courseId":7,"courseCode":"http5202","teacherId":3,"startDate":"2019-01-08","finishDate":"2019-04-27","courseName":"Web Application Development 2"}
        /// </example>
        /// <returns>
        /// A course object corresponding to the provided ID, or an empty object if not found.
        /// </returns>
        [HttpGet]
        [Route("FindCourse/{id}")]
        public Course FindCourse(int id)
        {
            // Initialize an empty course object
            Course SelectedCourse = new Course();

            // Connect to the database using the context
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Create a command to retrieve a specific course by ID
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "SELECT * FROM courses WHERE courseid=@id";
                Command.Parameters.AddWithValue("@id", id);

                // Execute the query and process the result
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    while (ResultSet.Read())
                    {
                        // Populate the course object with the data from the result set
                        SelectedCourse.CourseId = Convert.ToInt32(ResultSet["courseid"]);
                        SelectedCourse.CourseCode = ResultSet["coursecode"].ToString();
                        SelectedCourse.TeacherId = Convert.ToInt32(ResultSet["teacherid"]);
                        SelectedCourse.StartDate = Convert.ToDateTime(ResultSet["startdate"]).ToString("yyyy-MM-dd");
                        SelectedCourse.FinishDate = Convert.ToDateTime(ResultSet["finishdate"]).ToString("yyyy-MM-dd");
                        SelectedCourse.CourseName = ResultSet["coursename"].ToString();
                    }
                }
            }

            // Return the selected course
            return SelectedCourse;
        }

        /// <summary>
        /// Adds a new course to the system.
        /// </summary>
        /// <param name="CourseData">The course object containing course details.</param>
        /// <example>
        /// POST api/Course/AddCourse -> 
        /// Request Body:
        /// {
        ///   "CourseCode": "http 5110",
        ///   "TeacherId": 0,
        ///   "StartDate": "01-15-2019",
        ///   "FinishDate": "04-30-2019",
        ///   "CourseName": "Web Development"
        /// } -> 25
        /// </example>
        /// <returns>
        /// The ID of the newly inserted course if successful, otherwise 0.
        /// </returns>
        [HttpPost("AddCourse")]
        public int AddCourse([FromBody] Course CourseData)
        {
            // Establish connection to the database
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Create the command to insert the course data
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "INSERT INTO courses(coursecode, teacherid, startdate, finishdate, coursename) " +
                                      "VALUES(@coursecode, @teacherid, @startdate, @finishdate, @coursename)";

                // Add parameters to the command
                Command.Parameters.AddWithValue("@coursecode", CourseData.CourseCode);
                Command.Parameters.AddWithValue("@teacherid", CourseData.TeacherId);
                Command.Parameters.AddWithValue("@startdate", CourseData.StartDate);
                Command.Parameters.AddWithValue("@finishdate", CourseData.FinishDate);
                Command.Parameters.AddWithValue("@coursename", CourseData.CourseName);

                // Execute the command and return the last inserted ID
                Command.ExecuteNonQuery();
                return Convert.ToInt32(Command.LastInsertedId);
            }
            // Return 0 if the insertion fails (error handling is minimal here)
            return 0;
        }

        /// <summary>
        /// Deletes a course from the system by its ID.
        /// </summary>
        /// <param name="CourseId">The ID of the course to delete.</param>
        /// <example>
        /// DELETE api/Course/DeleteCourse/15 -> "The course with given id 15 has been removed from the DB"
        /// </example>
        /// <returns>
        /// A message indicating whether the deletion was successful or not.
        /// </returns>
        [HttpDelete("DeleteCourse/{CourseId}")]
        public string DeleteCourse(int CourseId)
        {
            // Initialize the number of affected rows
            int RowsAffected = 0;

            // Connect to the database
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Create the command to delete the course
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "DELETE FROM courses WHERE courseid=@id";
                Command.Parameters.AddWithValue("@id", CourseId);

                // Execute the command
                RowsAffected = Command.ExecuteNonQuery();
            }

            // Return a message based on whether a course was deleted
            if (RowsAffected > 0)
            {
                return $"The course with given id {CourseId} has been removed from the DB";
            }
            else
            {
                return $"The course with given id {CourseId} is not found";
            }
        }
    }
}
