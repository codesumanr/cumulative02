using cumulative01.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text.RegularExpressions;

namespace cumulative01.Controllers
{
    public class TeacherPageController : Controller
    {
        // currently relying on the API to retrieve author information
        // this is a simplified example. In practice, both the TeacherAPI and TeacherPage controllers
        // should rely on a unified "Service", with an explicit interface
        private readonly TeacherAPIController _api;

        public TeacherPageController(TeacherAPIController api)
        {
            _api = api;
        }

        // GET : TeacherPage/List
        [HttpGet]
        public IActionResult List()
        {
            var model = new TeacherSearchModel
            {
                // Get all teachers initially
                Teachers = _api.ListTeachers()
            };

            return View(model);
        }

        // POST : TeacherPage/List
        // POST Data : TeacherSearchModel model
        [HttpPost]
        public IActionResult List(TeacherSearchModel model)
        {
            // Get all teachers from the API
            List<Teacher> Teachers = _api.ListTeachers();

            // Filter teachers by HireDate if StartDate and EndDate are provided
            if (!string.IsNullOrEmpty(model.StartDate) && !string.IsNullOrEmpty(model.EndDate))
            {
                DateTime start = DateTime.Parse(model.StartDate);
                DateTime end = DateTime.Parse(model.EndDate);

                Teachers = Teachers.Where(teacher => DateTime.Parse(teacher.HireDate) >= start && DateTime.Parse(teacher.HireDate) <= end).ToList();
            }

            // Set the filtered list of teachers and return the model to the view
            model.Teachers = Teachers;

            // Pass the model to the view
            return View(model);
        }

        // GET : TeacherPage/Show/{id}
        public IActionResult Show(int id)
        {
            Teacher SelectedTeacher = _api.FindTeacher(id);
            ViewData["Id"] = id;
            return View(SelectedTeacher);


        }

        // GET: TeacherPage/New
        [HttpGet]
        public IActionResult New(int id)
        {
            return View();
        }

        // GET: TeacherPage/Validation
        [HttpGet]
        public IActionResult Validation()
        {

            if (TempData["ErrorMessage"] != null)
            {
                ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            }
            return View();
        }

        // POST: TeacherPage/Create
        [HttpPost]
        public IActionResult Create(Teacher NewTeacher)
        {

            string EmployeeNumberPattern = @"^T\d{3}$";

            // Check for the employee number pattern
            if (!string.IsNullOrEmpty(NewTeacher.EmployeeNumber) && !Regex.IsMatch(NewTeacher.EmployeeNumber, EmployeeNumberPattern))
            {
                TempData["ErrorMessage"] = "Employee number should start with 'T' followed by 3 digits. Eg: T123";
                return RedirectToAction("Validation");
            }
            // Check for the employee number which exist already
            if (!string.IsNullOrEmpty(NewTeacher.EmployeeNumber) && Regex.IsMatch(NewTeacher.EmployeeNumber, EmployeeNumberPattern))
            {
                List<Teacher> Teachers = _api.ListTeachers();
                foreach (Teacher CurrentTeacher in Teachers)
                {
                    if (CurrentTeacher.EmployeeNumber == NewTeacher.EmployeeNumber)
                    {
                        TempData["ErrorMessage"] = "This employee number has already been taken by the teacher";
                        return RedirectToAction("Validation");
                    }
                }
            }
            // Check for future hire date
            if (!string.IsNullOrEmpty(NewTeacher.HireDate) && DateTime.Parse(NewTeacher.HireDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Hire Date cannot be in future.";
                return RedirectToAction("Validation");
            }
            // Check for teacher name field from the input and respond with appropriate error message
            if (string.IsNullOrEmpty(NewTeacher.TeacherFName) && string.IsNullOrEmpty(NewTeacher.TeacherLName))
            {
                TempData["ErrorMessage"] = "Teacher first and last name cannot be empty";
                return RedirectToAction("Validation");
            }
            else if (string.IsNullOrEmpty(NewTeacher.TeacherFName))
            {
                TempData["ErrorMessage"] = "Teacher first name cannot be empty";
                return RedirectToAction("Validation");
            }
            else if (string.IsNullOrEmpty(NewTeacher.TeacherLName))
            {
                TempData["ErrorMessage"] = "Teacher last name cannot be empty";
                return RedirectToAction("Validation");
            }
            else
            {
                int TeacherId = _api.AddTeacher(NewTeacher);

                // redirects to "Show" action on "Teacher" cotroller with id parameter supplied
                return RedirectToAction("Show", new { id = TeacherId });
            }


        }

        // GET : TeacherPage/DeleteConfirm/{id}
        [HttpGet]
        public IActionResult DeleteConfirm(int id)
        {
            Teacher SelectedTeacher = _api.FindTeacher(id);
            return View(SelectedTeacher);
        }

        // POST: TeacherPage/Delete/{id}
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string RowsAffected = _api.DeleteTeacher(id);

            // redirects to list action
            return RedirectToAction("List");
        }

    }
}
