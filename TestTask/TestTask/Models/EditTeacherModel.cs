using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class EditTeacherModel
    {
        public List<ActiveStudentEdit> ActiveEdit { get; set; }
        public ApplicationUser User { get; set; }
    }
}