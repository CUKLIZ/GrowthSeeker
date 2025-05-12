namespace API_07.DTO
{
    public class GetCourseDetailDTO
    {
        public int id {  get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public string duration { get; set; }
        public List<string> modules { get; set; }
    }
}
