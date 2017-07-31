using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestTask.Models;

namespace TestTask.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext db = new ApplicationDbContext();
        public ApplicationUserManager UserManage;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListStudent(string filtering, int page)
        {
            List<ListStudentModel> Model = new List<ListStudentModel>();

            foreach(ApplicationUser user in db.Users.Where(x => x.Type == "Student").ToList())
            {
                Model.Add(new ListStudentModel { Name = user.Name, Surname = user.Surname, MiddleScores = MiddleScore(user.Id), UserId = user.Id});
            }
            
            if(filtering == "Name")
               Model = Model.OrderBy(x => x.Name).ToList();
            else if (filtering == "Up")
                Model = Model.OrderBy(x => x.MiddleScores).ToList();
            else if (filtering == "Down")
                Model = Model.OrderByDescending(x => x.MiddleScores).ToList();

            ViewBag.CountPage = (int)Math.Ceiling((double)(db.Users.ToList().FindAll(x => x.Type == "Student").Count)/20);
            ViewBag.TypeSort = filtering;

            int countonpage = db.Users.ToList().FindAll(x => x.Type == "Student").Count - (page * 20 - 20) < 20 ? db.Users.ToList().FindAll(x => x.Type == "Student").Count - (page * 20 - 20) : 20;

            return View(Model.GetRange(page * 20 - 20, countonpage));
        }

        public ActionResult ListTeacher(string filtering)
        {
            List<ListTeacherModel> Model = new List<ListTeacherModel>();

            foreach(SubjectModel sub in db.SubjectModels.ToList())
            {
                if (sub.Type == "Teacher")
                {
                    Model.Add(new ListTeacherModel
                    {
                        Name = db.Users.First(x => x.Id == sub.UserId).Name,
                        Surname = db.Users.First(x => x.Id == sub.UserId).Surname,
                        CountStudent = db.SubjectModels.Where(x => x.SubjectId == sub.SubjectId && x.Type == "Student").Count(),
                        Subject = db.AllSubjectModels.First(x => x.Id == sub.SubjectId).Subject,
                        UserId = sub.UserId,
                    });
                }
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
            foreach (SubjectModel sub in db.SubjectModels.ToList())
            {
                if (sub.Type == "Teacher" && (db.SubjectModels.Where(x => x.SubjectId == sub.SubjectId).Count() - 1) == db.Users.Where(x => x.Type == "Student").Count())
                {
                    TeacherMore.Add(new TeacherStatistic { Teacher = db.Users.First(x => x.Id == sub.UserId),
                        Count = db.SubjectModels.Where(x => x.SubjectId == sub.SubjectId).Count() - 1});
                }
                if (sub.Type == "Teacher" && (db.SubjectModels.Where(x => x.SubjectId == sub.SubjectId).Count()) < db.Users.Where(x => x.Type == "Student").Count() /2)
                {
                    TeacherLess.Add(new TeacherStatistic
                    {
                        Teacher = db.Users.First(x => x.Id == sub.UserId),
                        Count = db.SubjectModels.Where(x => x.SubjectId == sub.SubjectId).Count() - 1
                    });
                }
            }

            foreach (ApplicationUser user in db.Users.ToList())
            {
                if (user.Type == "Student")
                {
                    ListStudentModel.Add(new ListStudentModel
                    {
                        MiddleScores = MiddleScore(user.Id),
                        Surname = user.Surname,
                        Name = user.Name
                    });
                }
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
            db.SubjectModels.RemoveRange(db.SubjectModels.Where(x => x.UserId == Model.User.Id).ToList());
            ApplicationUser User = db.Users.FirstOrDefault(x => x.Id == Model.User.Id);
            User.Name = Model.User.Name;
            User.Surname = Model.User.Surname;

            foreach (SubjectEditModel model in Model.Subjects)
            {
                SubjectModel sub = model.Active ? new SubjectModel
                {
                    SubjectId = model.Subject.Id,
                    Type = Model.User.Type,
                    UserId = Model.User.Id
                } : null;
                if (sub != null)
                    db.SubjectModels.Add(sub);
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult EditTeacher(string userid)
        {
            return View(GetEditModelForTeacher(userid));
        }

        public ActionResult UpdateTeacher(EditTeacherModel Model)
        {
            List<SubjectModel> Add = new List<SubjectModel>();

            SubjectModel sub = new SubjectModel();
            int subid = db.SubjectModels.FirstOrDefault(i => i.UserId == Model.User.Id).SubjectId;

            foreach(ActiveStudentEdit user in Model.ActiveEdit)
            {
                sub = db.SubjectModels.FirstOrDefault(x => x.UserId == user.UserId);
                if (user.Active)
                    Add.Add(new SubjectModel { SubjectId = subid, Type = "Student", UserId = user.UserId});
            }

            db.SubjectModels.RemoveRange(db.SubjectModels.Where(x => x.SubjectId == subid && x.Type == "Student"));
            db.SubjectModels.AddRange(Add);

            db.SaveChanges();

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
            Model.User = db.Users.FirstOrDefault(x => x.Id == userid);
            if (Model.User.Type == "Student")
            {
                foreach (AllSubjectModel model in db.AllSubjectModels.ToList())
                {
                    List<SubjectModel> item = db.SubjectModels.Where(x => x.UserId == Model.User.Id).ToList();
                    bool active = item.FirstOrDefault(x => x.SubjectId == model.Id) != null ? true : false;
                    SubModel.Add(new SubjectEditModel { Subject = model, Active = active });
                }
            }
            
            Model.Subjects = SubModel;

            return Model;
        }
        public static EditTeacherModel GetEditModelForTeacher(string userid)
        {
            EditTeacherModel Model = new EditTeacherModel();
            List<ActiveStudentEdit> ActiveStudentEdit = new List<ActiveStudentEdit>();
            ApplicationDbContext db = new ApplicationDbContext();

            Model.User = db.Users.FirstOrDefault(x => x.Id == userid);

            int subjectid = db.SubjectModels.FirstOrDefault(x => x.UserId == userid).SubjectId;
            foreach (ApplicationUser user in db.Users.Where(x => x.Type == "Student").ToList())
            {
                SubjectModel sub = db.SubjectModels.FirstOrDefault(x => x.SubjectId == subjectid && x.UserId == user.Id); 
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