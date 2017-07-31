using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class SubjectEditModel
    {
        public AllSubjectModel Subject { get; set; }
        public bool Active { get; set; }
    }
}