using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestTask.Models;

namespace TestTask.Domain.DAO
{
    public class SubjectModelDAO : Database
    {
        public List<SubjectModel> GetTeachers()
        {
            return db.SubjectModels.SqlQuery("select * from SubjectModels where Type = 'Teacher'").ToList();
        }
        public List<SubjectModel> GetStudents()
        {
            return db.SubjectModels.SqlQuery("select * from SubjectModels where Type = 'Student'").ToList();
        }
        public List<SubjectModel> GetAll()
        {
            return db.SubjectModels.ToList();
        }
        public SubjectModel GetByUserId(string userid)
        {
            return db.SubjectModels.SqlQuery("select * from SubjectModels where UserId = '"+userid+"'").FirstOrDefault();
        }
        public SubjectModel GetBySubjectId(int subjectid)
        {
            return db.SubjectModels.SqlQuery("select * from SubjectModels where SubjectId = " + subjectid).FirstOrDefault();
        }
        public List<SubjectModel> GetAllBySubjectId(int subjectid)
        {
            return db.SubjectModels.SqlQuery("select * from SubjectModels where SubjectId = " + subjectid).ToList();
        }
    }
}