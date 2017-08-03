using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask.Models
{
    public class SubjectModel
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
    }
}