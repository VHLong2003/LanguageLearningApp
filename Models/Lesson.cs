using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearningApp.Models
{
    public class Lesson
    {
        public string LessonId { get; set; }
        public string CourseId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<Question> Questions { get; set; }
        public int Order { get; set; }
        public int RequiredPointsToUnlock { get; set; }
    }

}
