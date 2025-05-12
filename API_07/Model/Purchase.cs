using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_07.Model
{
    public class Purchase
    {
        [Key]
        public int id { get; set; }

        [Column("user_id")]
        public int user_id { get; set; }

        [Column("course_id")]
        public int course_id { get; set; }

        [Column("coupon_id")]
        public int? coupon_id { get; set; }

        [Column("price_paid")]
        public decimal price_paid { get; set; }

        [Column("payment_method")]
        public string payment_method { get; set; }

        [Column("purchased_at")]
        public DateTime purchased_at { get; set; }

        [ForeignKey("user_id")]
        public User user { get; set; }

        [ForeignKey("course_id")]
        public Course course { get; set; }

        [ForeignKey("coupon_id")]
        public Coupon coupon { get; set; }
    }
}
