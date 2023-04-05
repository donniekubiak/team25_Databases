using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from e in db.Enrolleds where e.Class.Course.Subject == subject && e.Class.Course.Number == num && e.Class.Season.Equals(season) && e.Class.Year == year select new { fname = e.UidNavigation.FirstName, lname = e.UidNavigation.LastName, uid = e.Uid, dob = e.UidNavigation.Dob, grade = e.Grade };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category != null)
            {
                var query = from a in db.Assignments where a.Category.Class.Course.Subject == subject && a.Category.Class.Course.Number == num && a.Category.Class.Season.Equals(season) && a.Category.Class.Year == year && a.Category.Name == category select new { aname = a.Name, cname = a.Category.Name, due = a.Due, submissions = a.Submissions.Count() };
                return Json(query.ToArray());
            }
            else
            {
                var query = from a in db.Assignments where a.Category.Class.Course.Subject == subject && a.Category.Class.Course.Number == num && a.Category.Class.Season.Equals(season) && a.Category.Class.Year == year select new { aname = a.Name, cname = a.Category.Name, due = a.Due, submissions = a.Submissions.Count() };
                return Json(query.ToArray());
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from c in db.AssignmentCategories where c.Class.Course.Subject == subject && c.Class.Course.Number == num && c.Class.Season.Equals(season) && c.Class.Year == year select new {name = c.Name, weight = c.Weight};
            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var query = from c in db.AssignmentCategories where c.Class.Course.Subject == subject && c.Class.Course.Number == num && c.Class.Season.Equals(season) && c.Class.Year == year && c.Name == category select c;
            if (query.Any())
            {
                return Json(new { success = false });
            }
            AssignmentCategory newCategory = new AssignmentCategory();
            newCategory.Name = category;
            newCategory.Weight = (byte) catweight;

            var classIDQuery = from c in db.Classes where c.Course.Subject == subject && c.Course.Number == num && c.Season.Equals(season) && c.Year == year select c.ClassId;
            newCategory.ClassId = classIDQuery.First();

            db.AssignmentCategories.Add(newCategory);
            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var query = from a in db.Assignments where a.Category.Class.Course.Subject == subject && a.Category.Class.Course.Number == num
                                                    && a.Category.Class.Season.Equals(season) && a.Category.Class.Year == year
                                                    && a.Category.Name == category && a.Name == asgname select a;
            if (query.Any())
            {
                return Json(new { success = false });
            }
            Assignment newAssign = new Assignment();
            newAssign.Name = asgname;
            newAssign.Contents = asgcontents;
            newAssign.Due = asgdue;
            newAssign.Points = (uint) asgpoints;

            var categoryIDQuery = from c in db.AssignmentCategories
                                  where c.Class.Course.Subject == subject && c.Class.Course.Number == num
                                  && c.Class.Season.Equals(season) && c.Class.Year == year
                                  && c.Name == category
                                  select c.CategoryId;
            newAssign.CategoryId = categoryIDQuery.First();

            db.Assignments.Add(newAssign);
            db.SaveChanges();

            var update_query = from e in db.Enrolleds
                        where e.Class.Course.Subject == subject && e.Class.Course.Number == num
                                                    && e.Class.Season.Equals(season) && e.Class.Year == year
                        select new { student = e.UidNavigation, e_class = e.Class };

            foreach(var sc in update_query)
            {
                SetGradeInClass(sc.student, sc.e_class);
            }


            return Json(new { success = true });



        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from s in db.Submissions where s.Assignment.Name == asgname && s.Assignment.Category.Name == category && s.Assignment.Category.Class.Season.Equals(season)
                        && s.Assignment.Category.Class.Year == year && s.Assignment.Category.Class.Course.Number == num && s.Assignment.Category.Class.Course.Subject == subject
                        select new { fname = s.UidNavigation.FirstName, lname = s.UidNavigation.LastName, uid = s.Uid, time = s.Time, score = s.Score };
            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = from s in db.Submissions where s.Assignment.Name == asgname && s.Assignment.Category.Name == category && s.Assignment.Category.Class.Season.Equals(season)
                        && s.Assignment.Category.Class.Year == year && s.Assignment.Category.Class.Course.Number == num && s.Assignment.Category.Class.Course.Subject == subject
                        && s.Uid == uid select new { submission = s, student = s.UidNavigation, s_class = s.Assignment.Category.Class };
            if(!query.Any())
                return Json(new { success = false });
            query.First().submission.Score = (uint)score;
            db.Submissions.Update(query.First().submission);
            db.SaveChanges();

            SetGradeInClass(query.First().student, query.First().s_class);
            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Classes where c.Professor.Uid == uid select new { subject = c.Course.Subject, number = c.Course.Number, name = c.Course.Name, season = c.Season, year = c.Year };
            return Json(query.ToArray());
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
            db.SaveChanges();
            double totalweight = 0;
            
            foreach (AssignmentCategory ac in query.ToList())
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
            for (int i = 0; i < results.Count(); i++)
            {
                total += results[i] * (weights[i] / totalweight);
            }
            string lettergrade = ConvertPercentageToLetterGrade(total);
            var equery = from en in db.Enrolleds where en.Uid == student.Uid && en.ClassId == grade_class.ClassId select en;
            Enrolled e = equery.First();
            e.Grade = lettergrade;
            db.Enrolleds.Update(e);
            db.SaveChanges();
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
            var cquery = from a in db.Assignments
                        where a.CategoryId == category.CategoryId select
                        (double)(a.Submissions.Where(s => s.Uid == student.Uid).Count() > 0 ? a.Submissions.Where(s => s.Uid == student.Uid).First().Score : 0) / (double)a.Points;
            if (cquery.ToArray().Length == 0)
            {
                return -1;
            }
            foreach (double i in cquery)
            {
                total += i;
            }
            total /= cquery.Count();
            return total;
        }

        /*******End code to modify********/
    }
}

