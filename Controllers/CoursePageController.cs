using Microsoft.AspNetCore.Mvc;
using cumulative01.Models;

namespace cumulative01.Controllers
{
    public class CoursePageController : Controller
    {
        // The controller is dependent on the CourseAPIController to interact with course data.
        // In practice, a unified service layer would be used to interact with data sources, not the controllers directly.
        private readonly CourseAPIController _api;

        public CoursePageController(CourseAPIController api)
        {
            _api = api;
        }

        /// <summary>
        /// Displays a list of all courses available in the system.
        /// </summary>
        /// <example>
        /// GET: CoursePage/List -> 
        /// A view displaying all courses such as:
        /// [{"courseId":1,"courseCode":"http5101","teacherId":1,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Web Application Development"}, ...]
        /// </example>
        /// <returns>
        /// A view showing a list of all courses.
        /// </returns>
        // GET : CoursePage/List
        public IActionResult List()
        {
            // Fetch the list of courses from the API
            List<Course> Courses = _api.ListCourses();
            return View(Courses); // Returns the list of courses to the view
        }

        /// <summary>
        /// Displays details of a specific course based on the course ID.
        /// </summary>
        /// <param name="id">The ID of the course to display.</param>
        /// <example>
        /// GET: CoursePage/Show/7 -> Displays the details of the course with ID 7.
        /// </example>
        /// <returns>
        /// A view displaying the details of a specific course.
        /// </returns>
        // GET : CoursePage/Show/{id}
        public IActionResult Show(int id)
        {
            // Fetch the course details using the course ID
            Course SelectedCourse = _api.FindCourse(id);
            ViewData["Id"] = id; // Pass the course ID to the view
            return View(SelectedCourse); // Return the course details to the view
        }

        /// <summary>
        /// Displays the form to create a new course.
        /// </summary>
        /// <example>
        /// GET: CoursePage/New -> Displays a blank form for creating a new course.
        /// </example>
        /// <returns>
        /// A view with the form to add a new course.
        /// </returns>
        // GET: CoursePage/New
        [HttpGet]
        public IActionResult New()
        {
            return View(); // Return the view for course creation form
        }

        /// <summary>
        /// Displays the validation page if there is an error with course data.
        /// </summary>
        /// <example>
        /// GET: CoursePage/Validation -> Displays a validation page with error messages.
        /// </example>
        /// <returns>
        /// A view with error messages if there are validation issues.
        /// </returns>
        // GET: CoursePage/Validation
        [HttpGet]
        public IActionResult Validation()
        {
            // Check if there are any error messages in TempData and pass them to the view
            if (TempData["ErrorMessage"] != null)
            {
                ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            }
            return View(); // Return the validation page with the error message if present
        }

        /// <summary>
        /// Creates a new course after validating the provided course data.
        /// </summary>
        /// <param name="CourseData">The course data to be added to the system.</param>
        /// <example>
        /// POST: CoursePage/Create -> 
        /// Course data sent in the body:
        /// {
        ///   "CourseCode": "http 5110",
        ///   "TeacherId": 0,
        ///   "StartDate": "01-15-2019",
        ///   "FinishDate": "04-30-2019",
        ///   "CourseName": "Web Development"
        /// } -> Redirects to the Show page with the created course's details.
        /// </example>
        /// <returns>
        /// Redirects to the Show action to display the newly created course if successful.
        /// Redirects to the Validation page with an error message if validation fails.
        /// </returns>
        // POST: CoursePage/Create
        [HttpPost]
        public IActionResult Create(Course CourseData)
        {
            // Validate the start date (it cannot be in the future)
            if (!string.IsNullOrEmpty(CourseData.StartDate) && DateTime.Parse(CourseData.StartDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Course start date cannot be in future.";
                return RedirectToAction("Validation"); // Redirect to validation page on error
            }

            // Validate the finish date (it cannot be in the future)
            if (!string.IsNullOrEmpty(CourseData.FinishDate) && DateTime.Parse(CourseData.FinishDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Course finish date cannot be in future.";
                return RedirectToAction("Validation"); // Redirect to validation page on error
            }

            // Ensure that the course name is not empty
            if (string.IsNullOrEmpty(CourseData.CourseName))
            {
                TempData["ErrorMessage"] = "Course name cannot be empty";
                return RedirectToAction("Validation"); // Redirect to validation page on error
            }
            else
            {
                // Add the course using the API and redirect to the Show action
                int CourseId = _api.AddCourse(CourseData);
                return RedirectToAction("Show", new { id = CourseId });
            }
        }

        /// <summary>
        /// Displays a confirmation page before deleting a course.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <example>
        /// GET: CoursePage/DeleteConfirm/7 -> Displays a confirmation page to delete course with ID 7.
        /// </example>
        /// <returns>
        /// A confirmation page showing the details of the course to be deleted.
        /// </returns>
        // GET : CoursePage/DeleteConfirm/{id}
        [HttpGet]
        public IActionResult DeleteConfirm(int id)
        {
            // Fetch the course to be deleted
            Course SelectedCourse = _api.FindCourse(id);
            return View(SelectedCourse); // Return the confirmation view
        }

        /// <summary>
        /// Deletes a course from the system based on the provided course ID.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <example>
        /// POST: CoursePage/Delete -> Deletes the course with the given ID and redirects to the course list page.
        /// </example>
        /// <returns>
        /// Redirects to the List action after deletion.
        /// </returns>
        // POST: StudentPage/Delete/{id}
        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Perform the deletion operation
            string RowsAffected = _api.DeleteCourse(id);

            // Redirect to the List action to display the updated list of courses
            return RedirectToAction("List");
        }
    }
}
