namespace API_07.DTO
{
    public class UpdateCourseDTO
    {
        public string title { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public int duration { get; set; } 
        public List<string> modules { get; set; }
        //public bool active { get; set; }
    }
}
