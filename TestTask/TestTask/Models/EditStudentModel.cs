using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class EditStudentModel
    {
        public ApplicationUser User { get; set; }
        public List<SubjectEditModel> Subjects { get; set; }
    }
}