using Microsoft.AspNetCore.Mvc;
using cumulative01.Models;
using System.Text.RegularExpressions;

namespace cumulative01.Controllers
{
    public class StudentPageController : Controller
    {
        // The StudentAPIController is used to interact with the API for retrieving and manipulating student data.
        // In a more robust implementation, both StudentAPI and StudentPage controllers should share a common "Service" layer for data operations.
        private readonly StudentAPIController _api;

        public StudentPageController(StudentAPIController api)
        {
            _api = api;
        }

        /// <summary>
        /// Displays a list of all students in the system.
        /// </summary>
        /// <example>
        /// GET: StudentPage/List -> 
        /// [
        ///   {
        ///     "StudentId": 1,
        ///     "StudentFName": "John",
        ///     "StudentLName": "Doe",
        ///     "StudentNumber": "N1234",
        ///     "EnrolDate": "2023/09/15"
        ///   },
        ///   ...
        /// ]
        /// </example>
        /// <returns>
        /// A view displaying all the students in a list.
        /// </returns>
        public IActionResult List()
        {
            List<Student> Students = _api.ListStudents();
            return View(Students); // Return the list of students to the view
        }

        /// <summary>
        /// Displays detailed information about a single student.
        /// </summary>
        /// <param name="id">The ID of the student to display.</param>
        /// <example>
        /// GET: StudentPage/Show?id=1 -> 
        /// {
        ///     "StudentId": 1,
        ///     "StudentFName": "John",
        ///     "StudentLName": "Doe",
        ///     "StudentNumber": "N1234",
        ///     "EnrolDate": "2023/09/15"
        /// }
        /// </example>
        /// <returns>
        /// A view displaying the details of a selected student.
        /// </returns>
        public IActionResult Show(int id)
        {
            Student SelectedStudent = _api.FindStudent(id);
            ViewData["Id"] = id; // Pass the ID to the view for further use
            return View(SelectedStudent); // Return the selected student to the view
        }

        /// <summary>
        /// Displays the form to create a new student.
        /// </summary>
        /// <returns>
        /// A view displaying the form to enter a new student's data.
        /// </returns>
        [HttpGet]
        public IActionResult New()
        {
            return View(); // Return the view with the form for creating a new student
        }

        /// <summary>
        /// Displays a validation page with error messages if any validation fails during student creation.
        /// </summary>
        /// <returns>
        /// A view displaying validation error messages, if applicable.
        /// </returns>
        [HttpGet]
        public IActionResult Validation()
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewData["ErrorMessage"] = TempData["ErrorMessage"]; // Display the error message if any
            }
            return View(); // Return the validation view
        }

        /// <summary>
        /// Creates a new student with the provided data.
        /// </summary>
        /// <param name="StudentData">The data for the new student to be created.</param>
        /// <example>
        /// POST: StudentPage/Create -> 
        /// {
        ///   "StudentFName": "Jane",
        ///   "StudentLName": "Smith",
        ///   "StudentNumber": "N9876",
        ///   "EnrolDate": "2024/01/01"
        /// }
        /// </example>
        /// <returns>
        /// Redirects to the "Show" action, displaying the newly created student's details.
        /// </returns>
        [HttpPost]
        public IActionResult Create(Student StudentData)
        {
            string EmployeeNumberPattern = @"^N\d{4}$";

            // Validate student number format (should start with 'N' followed by 4 digits)
            if (!string.IsNullOrEmpty(StudentData.StudentNumber) && !Regex.IsMatch(StudentData.StudentNumber, EmployeeNumberPattern))
            {
                TempData["ErrorMessage"] = "Student number should start with 'N' followed by 4 digits. Eg: N1234";
                return RedirectToAction("Validation"); // Redirect to validation view with error message
            }

            // Check if the student number already exists in the system
            if (!string.IsNullOrEmpty(StudentData.StudentNumber) && Regex.IsMatch(StudentData.StudentNumber, EmployeeNumberPattern))
            {
                List<Student> Students = _api.ListStudents();
                foreach (Student CurrentStudent in Students)
                {
                    if (CurrentStudent.StudentNumber == StudentData.StudentNumber)
                    {
                        TempData["ErrorMessage"] = "This student number has already been taken by another student";
                        return RedirectToAction("Validation"); // Redirect to validation view with error message
                    }
                }
            }

            // Ensure enrolment date is not in the future
            if (!string.IsNullOrEmpty(StudentData.EnrolDate) && DateTime.Parse(StudentData.EnrolDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Enrol Date cannot be in the future.";
                return RedirectToAction("Validation"); // Redirect to validation view with error message
            }

            // Check if student first and last names are provided
            if (string.IsNullOrEmpty(StudentData.StudentFName) && string.IsNullOrEmpty(StudentData.StudentLName))
            {
                TempData["ErrorMessage"] = "Student first and last name cannot be empty";
                return RedirectToAction("Validation"); // Redirect to validation view with error message
            }
            else if (string.IsNullOrEmpty(StudentData.StudentFName))
            {
                TempData["ErrorMessage"] = "Student first name cannot be empty";
                return RedirectToAction("Validation"); // Redirect to validation view with error message
            }
            else if (string.IsNullOrEmpty(StudentData.StudentLName))
            {
                TempData["ErrorMessage"] = "Student last name cannot be empty";
                return RedirectToAction("Validation"); // Redirect to validation view with error message
            }

            // Add student to the database and get the student ID
            int StudentId = _api.AddStudent(StudentData);

            // Redirect to the "Show" action with the newly created student ID
            return RedirectToAction("Show", new { id = StudentId });
        }

        /// <summary>
        /// Displays a confirmation view before deleting a student.
        /// </summary>
        /// <param name="id">The ID of the student to confirm deletion.</param>
        /// <returns>
        /// A view displaying the student details with an option to confirm deletion.
        /// </returns>
        [HttpGet]
        public IActionResult DeleteConfirm(int id)
        {
            Student SelectedStudent = _api.FindStudent(id);
            return View(SelectedStudent); // Return the confirmation view with the student's details
        }

        /// <summary>
        /// Deletes a student from the system.
        /// </summary>
        /// <param name="id">The ID of the student to be deleted.</param>
        /// <example>
        /// POST: StudentPage/Delete?id=1 -> 
        /// Redirects to the "List" view after deletion.
        /// </example>
        /// <returns>
        /// Redirects to the "List" action after deleting the student.
        /// </returns>
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string RowsAffected = _api.DeleteStudent(id); // Call the API to delete the student

            // Redirect back to the list of students
            return RedirectToAction("List");
        }
    }
}
