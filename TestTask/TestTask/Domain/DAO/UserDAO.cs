using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestTask.Models;

namespace TestTask.Domain.DAO
{
    public class UserDAO : Database
    {
        SubjectModelDAO SubjectModels = new SubjectModelDAO();

        public List<ApplicationUser> GetTeachers()
        {
            return db.Database.SqlQuery<ApplicationUser>("select * from AspNetUsers where Type = 'Teacher'").ToList();
        }
        public List<ApplicationUser> GetStudents()
        {
            return db.Database.SqlQuery<ApplicationUser>("select * from AspNetUsers where Type = 'Student'").ToList();
        }
        public List<ApplicationUser> GetAll()
        {
            return db.Users.ToList();
        }
        public ApplicationUser GetById(string userid)
        {
            return db.Database.SqlQuery<ApplicationUser>("select * from AspNetUsers where Id = '" + userid + "'").FirstOrDefault();
        }
        public void UpdateTeacher(EditTeacherModel Model)
        {
            List<SubjectModel> Add = new List<SubjectModel>();

            SubjectModel sub = new SubjectModel();
            int subid = SubjectModels.GetByUserId(Model.User.Id).SubjectId;

            foreach (ActiveStudentEdit user in Model.ActiveEdit)
            {
                sub = SubjectModels.GetByUserId(Model.User.Id);
                if (user.Active)
                    Add.Add(new SubjectModel { SubjectId = subid, Type = "Student", UserId = user.UserId });
            }

            db.SubjectModels.RemoveRange(db.SubjectModels.Where(x => x.SubjectId == subid && x.Type == "Student"));
            db.SubjectModels.AddRange(Add);

            db.SaveChanges();
        }
        public void UpdateStudent(EditStudentModel Model)
        {
            db.SubjectModels.RemoveRange(db.SubjectModels.Where(x => x.UserId == Model.User.Id).ToList());
            ApplicationUser User = GetById(Model.User.Id);
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

        }
    }
}