using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class ListTeacherModel : ListModel
    {
        public string Subject { get; set; }
        public int CountStudent { get; set; }
    }
}