using API_07.Data;
using API_07.DTO;
using API_07.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_07.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CouponsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult getCoupons()
        {
            var coupons = _context.Coupons.OrderByDescending(c => c.expiry_date).Select(c => new GetCouponDTO
            {
                couponId = c.id,
                couponCode = c.code,
                discountValue = c.discount_pct,
                expiryDate = c.expiry_date,
                quota = c.quota
            }).ToList();

            return Ok(new
            {
                data = coupons
            });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult CreateCoupont([FromBody] CreateCouponDTO dto)
        {
            // Cek Admin
            //var role = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            //if (role != "admin")
            //{
            //    return BadRequest(new
            //    {
            //        message = "Access Denied."
            //    });
            //}

            //foreach (var claim in User.Claims)
            //{
            //    Console.WriteLine($"CLAIM TYPE: {claim.Type}, VALUE: {claim.Value}");
            //}           

            // Validasi Input
            if (string.IsNullOrWhiteSpace(dto.couponCode) || dto.discountValue <= 0 || dto.quota <= 0 || dto.expiryDate <= DateTime.UtcNow)
            {
                return UnprocessableEntity(new
                {
                    message = "Make Sure All Field Are Valid"
                });
            }

            // Cek Coupon 
            var exists = _context.Coupons.Any(c => c.code == dto.couponCode);
            if (exists)
            {
                return UnprocessableEntity(new
                {
                    message = "Coupon Code Must Be Unique"
                });
            }

            var coupon = new Coupon
            {
                code = dto.couponCode,
                discount_pct = dto.discountValue,
                expiry_date = dto.expiryDate,
                quota = dto.quota,
                created_at = DateTime.UtcNow
            };

            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Coupon created successfully.",
                data = new
                {
                    couponId = coupon.id,
                    couponCode = coupon.code,
                    discountValue = coupon.discount_pct,
                    expiryDate = coupon.expiry_date,
                    quota = coupon.quota
                }
            });
        }

        [HttpPut("{couponId}")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateCoupon(int couponId, [FromBody] CreateCouponDTO dto)
        {
            // Validasi Input
            if (string.IsNullOrWhiteSpace(dto.couponCode) || dto.discountValue <= 0 || dto.quota <= 0 || dto.expiryDate <= DateTime.UtcNow)
            {
                return UnprocessableEntity(new
                {
                    message = "Make Sure All Field Are Valid"
                });
            }

            var coupon = _context.Coupons.FirstOrDefault(c => c.id == couponId);
            if (coupon == null)
            {
                return NotFound(new
                {
                    message = "Coupon Not Found"
                });
            }

            if (!string.Equals(dto.couponCode, coupon.code, StringComparison.OrdinalIgnoreCase) &&
                    _context.Coupons.Any(c => c.code == dto.couponCode && c.id != couponId))
            {
                return UnprocessableEntity(new { message = "Coupon Code Must Be Unique" });
            }

            if (coupon.code != dto.couponCode) coupon.code = dto.couponCode;
            if (coupon.discount_pct != dto.discountValue) coupon.discount_pct = dto.discountValue;
            if (coupon.quota != dto.quota) coupon.quota = dto.quota;
            if (coupon.expiry_date != dto.expiryDate) coupon.expiry_date = dto.expiryDate;

            _context.SaveChanges();

            return Ok(new
            {
                message = "Copun Updated",
                data = new
                {
                    couponId = coupon.id,
                    couponCode = coupon.code,
                    discountValue = coupon.discount_pct,
                    expiryDate = coupon.expiry_date,
                    quota = coupon.quota
                }
            });
        }
    }
}
