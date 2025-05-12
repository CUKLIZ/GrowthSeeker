namespace API_07.Model
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password_hash { get; set; }
        public string role { get; set; } = "student";
    }
}
