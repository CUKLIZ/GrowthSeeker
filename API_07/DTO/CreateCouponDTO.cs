namespace API_07.DTO
{
    public class CreateCouponDTO
    {
        public string couponCode { get; set; }
        public decimal discountValue { get; set; }
        public DateTime expiryDate { get; set; }
        public int quota { get; set; }
    }
}
