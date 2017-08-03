using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class StatisticModel
    {
        public List<TeacherStatistic> TeacherMore { get; set; }
        public List<TeacherStatistic> TeacherLess { get; set; }
        public List<ListStudentModel> ListStudentModel { get; set; }
    }
}