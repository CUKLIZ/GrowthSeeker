using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_07.Model
{
    public class CourseModule
    {
        [Key]
        public int id {  get; set; }
        [ForeignKey("course")]
        public int course_id { get; set; }
        public string title { get; set; }
        public string content { get; set; }

        public Course course { get; set; }
    }
}
