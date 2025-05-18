using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearningApp.Models
{
    public class UserProgress
    {
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string LessonId { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }
        public int EarnedPoints { get; set; }
    }

}
