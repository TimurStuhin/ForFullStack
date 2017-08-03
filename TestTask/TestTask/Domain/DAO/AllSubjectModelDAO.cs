using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestTask.Models;

namespace TestTask.Domain.DAO
{
    public class AllSubjectModelDAO : Database
    {
        public List<AllSubjectModel> GetAll()
        {
            return db.AllSubjectModels.ToList();
        }
        public AllSubjectModel GetSub(int id)
        {
            return db.Database.SqlQuery<AllSubjectModel>("select * from AllSubjectModels where Id = "+id).FirstOrDefault();
        }
    }
}