using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Schema;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrolleds where e.Uid == uid select new { subject = e.Class.Course.Subject, number = e.Class.Course.Number, name = e.Class.Course.Name,
                season = e.Class.Season, year = e.Class.Year, grade = e.Grade == null ? "--" : e.Grade };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query = from a in db.Assignments
                        where a.Category.Class.Course.Subject == subject
                        && a.Category.Class.Course.Number == num && a.Category.Class.Season.Equals(season)
                        && a.Category.Class.Year == year
                        select new
                        {
                            aname = a.Name,
                            cname = a.Category.Name,
                            due = a.Due,
                            score = a.Submissions.Where(s => s.Uid == uid).Count() > 0 ? (uint?)a.Submissions.Where(s => s.Uid == uid).First().Score : null
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var query = from s in db.Submissions
                        where s.Assignment.Category.Class.Course.Subject == subject
                        && s.Assignment.Category.Class.Course.Number == num
                        && s.Assignment.Category.Class.Season.Equals(season)
                        && s.Assignment.Category.Class.Year == year
                        && s.Assignment.Category.Name == category
                        && s.Assignment.Name == asgname
                        && s.Uid == uid
                        select s;
            // Update existing submission
            if (query.Any())
            {
                Submission existing = query.First();
                existing.Contents = contents;
                existing.Time = DateTime.Now;
                db.Submissions.Update(existing);
            }
            // Create new submission
            else
            {
                Submission newSubmission = new Submission();
                var assignmentIDQuery = from a in db.Assignments
                                        where a.Category.Class.Course.Subject == subject
                                        && a.Category.Class.Course.Number == num
                                        && a.Category.Class.Season.Equals(season)
                                        && a.Category.Class.Year == year
                                        && a.Category.Name == category
                                        && a.Name == asgname
                                        select a.AssignmentId;
                newSubmission.AssignmentId = assignmentIDQuery.First();
                newSubmission.Time = DateTime.Now;
                newSubmission.Contents = contents;
                newSubmission.Score = 0;
                newSubmission.Uid = uid;
            }
            db.SaveChanges();
            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var query = from e in db.Enrolleds where e.Uid == uid
                        && e.Class.Course.Subject == subject && e.Class.Course.Number == num
                        && e.Class.Season.Equals(season) && e.Class.Year == year select e;
            if (query.Any())
            {
                return Json(new { success = false });
            }
            Enrolled newEnrollment = new Enrolled();
            newEnrollment.Uid = uid;
            var classIDQuery = from c in db.Classes
                               where c.Course.Subject == subject && c.Course.Number == num
                               && c.Season.Equals(season) && c.Year == year
                               select c.ClassId;
            newEnrollment.ClassId = classIDQuery.First();
            newEnrollment.Grade = "--";

            db.Enrolleds.Add(newEnrollment);
            db.SaveChanges();
            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {

            var query = from e in db.Enrolleds where e.Uid == uid select e.Grade;
            double GPA = 0;
            double classesWithGrades = 0;
            foreach (var g in query)
            {
                double result = ConvertLetterGradeToGPA(g);
                if (result != -1)
                {
                    GPA += ConvertLetterGradeToGPA(g);
                    classesWithGrades++;
                }
            }
            if (classesWithGrades == 0)
            {
                return Json(new { gpa = 0 });
            }
            GPA /= classesWithGrades;
            return Json(new {gpa = GPA });
        }

        private double ConvertLetterGradeToGPA(string grade)
        {
            switch(grade)
            {
                case "A":
                    return 4.0;
                case "A-":
                    return 3.7;
                case "B+":
                    return 3.3;
                case "B":
                    return 3.0;
                case "B-":
                    return 2.7;
                case "C+":
                    return 2.3;
                case "C":
                    return 2.0;
                case "C-":
                    return 1.7;
                case "D+":
                    return 1.3;
                case "D":
                    return 1.0;
                case "D-":
                    return 0.7;
                case "E":
                    return 0.0;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Returns the student's letter grade for the given class
        /// </summary>
        /// <param name="student">The student object</param>
        /// <param name="grade_class">The class that the student is enrolled in</param>
        /// <returns>The student's letter grade for the given class</returns>
        private string SetGradeInClass(Student student, Class grade_class)
        {
            double total = 0;
            var query = from ac in db.AssignmentCategories where ac.ClassId == grade_class.ClassId select ac;
            List<double> results = new List<double>();
            List<double> weights = new List<double>();
            double totalweight = 0;
            foreach(AssignmentCategory ac in query)
            {
                double result = GetGradeInAssignmentCategory(student, ac);
                if (result < 0)
                {
                    results.Add(0);
                    weights.Add(0);
                }
                else
                {
                    results.Add(result);
                    weights.Add(ac.Weight);
                    totalweight += ac.Weight;
                }
            }
            for(int i = 0; i < results.Count(); i++)
            {
                total += results[i] * (weights[i] / totalweight);
            }
            return ConvertPercentageToLetterGrade(total);
        }

        /// <summary>
        /// Converts the given grade (from 0 to 1) to its corresponding letter grade
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string ConvertPercentageToLetterGrade(double grade)
        {
            if (grade >= .93)
            {
                return "A";
            } 
            else if (grade >= 90)
            {
                return "A-";
            }
            else if (grade >= 87)
            {
                return "B+";
            }
            else if (grade >= 83)
            {
                return "B";
            }
            else if (grade >= 80)
            {
                return "B-";
            }
            else if (grade >= 77)
            {
                return "C+";
            }
            else if (grade >= 73)
            {
                return "C";
            }
            else if (grade >= 70)
            {
                return "C-";
            }
            else if (grade >= 67)
            {
                return "D+";
            }
            else if (grade >= 63)
            {
                return "D";
            }
            else if (grade >= 60)
            {
                return "D-";
            }
            else
            {
                return "E";
            }
        }

        /// <summary>
        /// takes student and category object and returns total of their scores on all assignments, counting their score as 0 if no submission.
        /// </summary>
        /// <param name="student"></param>
        /// <param name="category"></param>
        /// <returns>-1 if category has no assignments, total percent of category grade otherwise</returns>
        private double GetGradeInAssignmentCategory(Student student, AssignmentCategory category)
        {
            double total = 0;
            var query = from a in db.Assignments
                        where a.CategoryId == category.CategoryId
                        select (double)(a.Submissions.Where(s => s.Uid == student.Uid).Count() > 0 ? a.Submissions.Where(s => s.Uid == student.Uid).First().Score : 0) / (double)a.Points;
            if (!query.Any())
            {
                return -1;
            }
            foreach(double i in query)
            {
                total += i;
            }
            total /= query.Count();
            return total;
        }

        /*******End code to modify********/

    }
}

