using System.Security.Claims;
using API_07.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_07.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public TransactionsController(AppDbContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }


        [HttpGet]
        [Authorize]
        public IActionResult GetTransactions([FromQuery] string? courseName, [FromQuery] string? sortBy = "asc", [FromQuery] string? userEmail = null, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            //var role = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (!int.TryParse(userIdStr, out int currentUserId))
            {
                return Unauthorized(new
                {
                    message = "Invalid User"
                });
            }

            // Validasi Sort By
            if (sortBy != "asc" && sortBy != "desc")
            {
                return UnprocessableEntity(new
                {
                    message = "sortBy must be 'asc' or 'desc'."
                });
            }

            var query = _context.Purchases
                .Include(p => p.course)
                .Include(p => p.user)
                .Include(p => p.coupon)
                .AsQueryable();

            // Untuk User 
            if (role == "student")
            {
                query = query.Where(p => p.user_id == currentUserId);

                if (!string.IsNullOrWhiteSpace(courseName))
                {
                    query = query.Where(p => p.course.title.Contains(courseName));
                }

                query = sortBy == "desc" 
                    ? query.OrderByDescending(p => p.purchased_at) : query.OrderBy(p => p.purchased_at);
            } else if (role == "admin")
            {
                if (!string.IsNullOrWhiteSpace(userEmail))
                {
                    query = query.Where(p => p.user.email.Contains(userEmail));
                }

                if (!string.IsNullOrWhiteSpace(courseName))
                {
                    query = query.Where(p => p.course.title.Contains(courseName));
                }

                query = query.OrderByDescending(p => p.purchased_at);
            } else
            {
                //return Forbid();
                return NotFound(new
                {
                    message = "eqeqwhieqie2r"
                });
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)size);

            //var data = query.Skip((page - 1) * size)
            //                .Take(size)
            //                .Select(p => new
            //                {
            //                    transactionId = p.id,
            //                    userEmail = role == "admin" ? p.user.email : null,
            //                    courseTitle = p.course.title,
            //                    purchaseDate = p.purchased_at,
            //                    amount = p.course.price,
            //                    couponCode = p.coupon!= null ? p.coupon.code : "",
            //                    paidAmount = p.price_paid
            //                }).ToList();
            //                

            List<object> data;

            if (role == "admin")
            {
                data = query.Skip((page - 1) * size)
                            .Take(size)
                            .Select(p => new
                            {
                                transactionId = p.id,
                                userEmail = p.user.email,
                                courseId = p.course.id,
                                courseTitle = p.course.title,
                                purchaseDate = p.purchased_at,
                                amount = p.course.price,
                                couponCode = p.coupon != null ? p.coupon.code : "",
                                paidAmount = p.price_paid
                            }).Cast<object>().ToList();
            }
            else
            {
                data = query.Skip((page - 1) * size)
                            .Take(size)
                            .Select(p => new
                            {
                                transactionId = p.id,
                                courseTitle = p.course.title,
                                purchaseDate = p.purchased_at,
                                amount = p.course.price,
                                couponCode = p.coupon != null ? p.coupon.code : "",
                                paidAmount = p.price_paid
                            }).Cast<object>().ToList();
            }

            return Ok(new
            {
                data = data,
                pagination = new
                {
                    page = page,
                    size = size,
                    totalPages = totalPages
                }
            });
        }

        //[HttpGet("check-claims")]
        //[Authorize]
        //public IActionResult CheckClaims()
        //{
        //    var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        //    return Ok(claims);
        //}
    }
}
