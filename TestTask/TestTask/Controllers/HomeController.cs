using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestTask.Models;
using TestTask.Domain.DAO;

namespace TestTask.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext db = new ApplicationDbContext();
        public AllSubjectModelDAO AllSubjectModels = new AllSubjectModelDAO();
        public SubjectModelDAO SubjectModels = new SubjectModelDAO();
        public UserDAO Users = new UserDAO();

        public ApplicationUserManager UserManage;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListStudent(string filtering, int page)
        {
            List<ListStudentModel> Model = new List<ListStudentModel>();
            List<ApplicationUser> Students = Users.GetStudents();

            foreach (ApplicationUser user in Students)
            {
                Model.Add(new ListStudentModel { Name = user.Name, Surname = user.Surname, MiddleScores = MiddleScore(user.Id), UserId = user.Id});
            }
            
            if(filtering == "Name")
               Model = Model.OrderBy(x => x.Name).ToList();
            else if (filtering == "Up")
                Model = Model.OrderBy(x => x.MiddleScores).ToList();
            else if (filtering == "Down")
                Model = Model.OrderByDescending(x => x.MiddleScores).ToList();

            ViewBag.CountPage = (int)Math.Ceiling((double)(Students.Count)/20);
            ViewBag.TypeSort = filtering;

            int countonpage = Students.Count - (page * 20 - 20) < 20 ? Students.Count - (page * 20 - 20) : 20;

            return View(Model.GetRange(page * 20 - 20, countonpage));
        }

        public ActionResult ListTeacher(string filtering)
        {
            List<ListTeacherModel> Model = new List<ListTeacherModel>();
            ApplicationUser user;
            List<SubjectModel> Subjects = SubjectModels.GetStudents();
            foreach (SubjectModel sub in SubjectModels.GetTeachers())
            {
                user = Users.GetById(sub.UserId);
                Model.Add(new ListTeacherModel
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    CountStudent = Subjects.Where(x => x.SubjectId == sub.SubjectId).Count(),
                    Subject = AllSubjectModels.GetSub(sub.SubjectId).Subject,
                    UserId = sub.UserId,
                });
            }

            if (filtering == "Name")
                Model = Model.OrderBy(x => x.Name).ToList();
            else if (filtering == "Up")
                Model = Model.OrderBy(x => x.CountStudent).ToList();
            else if (filtering == "Down")
                Model = Model.OrderByDescending(x => x.CountStudent).ToList();

            return View(Model);
        }

        [Authorize(Roles = "Dekan")]
        public ActionResult Statistic()
        {
            StatisticModel Model = new StatisticModel();
            List<TeacherStatistic> TeacherMore = new List<TeacherStatistic>();
            List<TeacherStatistic> TeacherLess = new List<TeacherStatistic>();
            List<ListStudentModel> ListStudentModel = new List<ListStudentModel>();

            List<SubjectModel> Subjects = SubjectModels.GetAll().ToList();
            int countstudent = Users.GetStudents().Count();

            foreach (SubjectModel sub in Subjects)
            {
                if (sub.Type == "Teacher" && (Subjects.Where(x => x.SubjectId == sub.SubjectId).Count() - 1) == countstudent)
                {
                    TeacherMore.Add(new TeacherStatistic { Teacher = Users.GetById(sub.UserId),
                        Count = Subjects.Where(x => x.SubjectId == sub.SubjectId).Count() - 1});
                }
                if (sub.Type == "Teacher" && (Subjects.Where(x => x.SubjectId == sub.SubjectId).Count()) < countstudent / 2)
                {
                    TeacherLess.Add(new TeacherStatistic
                    {
                        Teacher = Users.GetById(sub.UserId),
                        Count = Subjects.Where(x => x.SubjectId == sub.SubjectId).Count() - 1
                    });
                }
            }

            foreach (ApplicationUser user in Users.GetStudents())
            {
                ListStudentModel.Add(new ListStudentModel
                {
                    MiddleScores = MiddleScore(user.Id),
                    Surname = user.Surname,
                    Name = user.Name
                });
            }

            Model.TeacherMore = TeacherMore;
            Model.ListStudentModel = ListStudentModel;
            Model.TeacherLess = TeacherLess;

            return View(Model);
        }

        public ActionResult EditStudent(string userid)
        {
            return View(GetEditModelForStudent(userid));
        }

        public ActionResult UpdateStudent(EditStudentModel Model)
        {
            Users.UpdateStudent(Model);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult EditTeacher(string userid)
        {
            return View(GetEditModelForTeacher(userid));
        }

        public ActionResult UpdateTeacher(EditTeacherModel Model)
        {
            Users.UpdateTeacher(Model);

            return RedirectToAction("Index", "Home");
        }

        public int MiddleScore(string userisd)
        {
            List<ResultModel> allresult = db.ResultModels.Where(x => x.UserId == userisd).ToList();
            int middlescore = 0;
            foreach (var step in allresult)
            {
                middlescore += step.Score;
            }

            middlescore = middlescore == 0 ? 0 : middlescore / allresult.Count;

            return middlescore;
        }

        public EditStudentModel GetEditModelForStudent(string userid)
        {
            EditStudentModel Model = new EditStudentModel();
            List<SubjectEditModel> SubModel = new List<SubjectEditModel>();
            Model.User = Users.GetById(userid);
            if (Model.User.Type == "Student")
            {
                foreach (AllSubjectModel model in AllSubjectModels.GetAll())
                {
                    List<SubjectModel> item = SubjectModels.GetAll().Where(x => x.UserId == Model.User.Id).ToList();
                    bool active = item.FirstOrDefault(x => x.SubjectId == model.Id) != null ? true : false;
                    SubModel.Add(new SubjectEditModel { Subject = model, Active = active });
                }
            }
            
            Model.Subjects = SubModel;

            return Model;
        }
        public EditTeacherModel GetEditModelForTeacher(string userid)
        {
            EditTeacherModel Model = new EditTeacherModel();
            List<ActiveStudentEdit> ActiveStudentEdit = new List<ActiveStudentEdit>();

            Model.User = Users.GetById(userid);

            int subjectid = SubjectModels.GetByUserId(userid).SubjectId;
            foreach (ApplicationUser user in Users.GetStudents())
            {
                SubjectModel sub = SubjectModels.GetAllBySubjectId(subjectid).FirstOrDefault(x => x.UserId == user.Id); 
                bool active = sub != null ? true : false;
                ActiveStudentEdit.Add(new ActiveStudentEdit { Name = user.Name, Surname = user.Surname, UserId = user.Id, Active = active });
            }

            Model.ActiveEdit = ActiveStudentEdit;

            return Model;
        }

        public static int ReturnCountStudent()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            return db.Users.Where(x => x.Type == "Student").Count();
        }
        public static int ReturnCountTeacher()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            return db.Users.Where(x => x.Type == "Teacher").Count();
        }
    }
}