using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class TeacherStatistic
    {
        public ApplicationUser Teacher { get; set; }
        public int Count { get; set; }
    }
}