using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLearningApp.Models
{
    public class Course
    {
        public string CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public CourseType Type { get; set; } // Loại khóa học
        public List<Lesson> Lessons { get; set; }
    }

    public enum CourseType
    {
        Language,
        Programming,
        Business,
        Science,
        Arts
    }

}
