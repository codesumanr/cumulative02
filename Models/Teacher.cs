namespace cumulative01.Models

{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public string TeacherFName { get; set; }
        public string TeacherLName { get; set; }
        public string EmployeeNumber { get; set; }
        public string HireDate { get; set; }
        public decimal Salary { get; set; }

        public List<Course> CoursesByTeacher { get; set; } = new List<Course>();
    }
}

