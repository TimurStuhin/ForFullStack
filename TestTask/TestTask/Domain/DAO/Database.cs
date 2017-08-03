using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestTask.Models;

namespace TestTask.Domain.DAO
{
    public class Database
    {
        public ApplicationDbContext db = new ApplicationDbContext();
    }
}