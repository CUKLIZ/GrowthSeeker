namespace API_07.Model
{
    public class Course
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public int duration { get; set; }
        public DateTime created_at { get; set; }       
        public List<CourseModule> modules { get; set; }
    }
}
