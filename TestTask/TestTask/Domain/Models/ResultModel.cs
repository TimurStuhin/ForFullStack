using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class ResultModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SubjectID { get; set; }
        public int Score { get; set; }
    }
}