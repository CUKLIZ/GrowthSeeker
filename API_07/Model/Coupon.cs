namespace API_07.Model
{
    public class Coupon
    {
        public int id { get; set; }
        public string code { get; set; }
        public decimal discount_pct { get; set; }
        public int quota { get; set; }
        public DateTime expiry_date { get; set; }
        public DateTime created_at { get; set; }
    }
}
