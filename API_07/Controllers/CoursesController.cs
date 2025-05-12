using System.Reflection;
using API_07.Data;
using API_07.DTO;
using API_07.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_07.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public CoursesController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null) return null;
            return int.Parse(userIdClaim.Value);
        }

        [HttpGet]
        public IActionResult GetCourse([FromQuery] string? title, [FromQuery] string sort = "desc", [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            if (page <= 0)
            {
                return UnprocessableEntity(new
                {
                    message = "Validation Error: 'Page' Must Be a Positive Integer"
                });
            }

            if (size <= 0) size = 10;

            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(c => c.title.Contains(title));
            }

            if (sort.ToLower() == "asc")
            {
                query = query.OrderBy(c => c.created_at);
            }
            else
            {
                query = query.OrderByDescending(c => c.created_at);
            }

            var totalCourses = query.Count();
            var totalPages = (int)Math.Ceiling(totalCourses / (double)size);

            var courses = query.Skip((page - 1) * size).Take(size).Select(c => new GetCourseDTO
            {
                id = c.id,
                title = c.title,
                description = c.description,
                price = c.price
            }).ToList();

            return Ok(new
            {
                data = courses,
                pagination = new
                {
                    page,
                    size,
                    totalPages
                }
            });
        }

        [HttpGet("{courseId}")]
        public IActionResult GetCourseDetail(string courseId)
        {
            if (!int.TryParse(courseId, out var id))
            {
                return UnprocessableEntity(new
                {
                    message = "CourseId Must Be Numeric"
                });
            }

            var course = _context.Courses.Include(c => c.modules).FirstOrDefault(c => c.id == id);

            if (course == null)
            {
                return NotFound(new
                {
                    message = "Course Not Found"
                });
            }

            var responeses = new GetCourseDetailDTO
            {
                id = course.id,
                title = course.title,
                description = course.description,
                price = course.price,
                duration = $"{course.duration} minutes",
                modules = course.modules.Select(m => m.title).ToList()
            };

            return Ok(new
            {
                data = responeses
            });
        }

        [HttpPost("{courseId}/purchase")]
        [Authorize]
        public IActionResult PurchaseCourse(int courseId, [FromBody] PurchaseCourseRequestDTO request)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Please Login To Purchase "
                });
            }

            if (string.IsNullOrWhiteSpace(request.paymentMethod) ||
                !new[] {"credit_card", "debit_card","paypal"}.Contains(request.paymentMethod.ToLower()))
            {
                return UnprocessableEntity(new
                {
                    message = "Payment Method Not Suppoert"
                });
            }

            var course = _context.Courses.FirstOrDefault(c => c.id == courseId);
            if (course == null)
            {
                return NotFound(new
                {
                    message = "Course Not Found"
                });
            }

            decimal discount = 0;
            int? couponId = null;

            if (!string.IsNullOrWhiteSpace(request.couponCode))
            {
                var coupon = _context.Coupons.FirstOrDefault(c =>
                    c.code == request.couponCode &&
                    c.expiry_date >= DateTime.UtcNow &&
                    c.quota > 0
                );

                if (coupon == null)
                {
                    return UnprocessableEntity(new
                    {
                        message = "Coupon Code Has Expired Or Quota Exceeded"
                    });
                }

                discount = (course.price * coupon.discount_pct) / 100;
                couponId = coupon.id;

                // Kurangi Quota
                coupon.quota -= 1;
            }

            var pricePaid = course.price - discount;

            var purchase = new Purchase
            {
                user_id = userId.Value,
                course_id = courseId,
                coupon_id = couponId,
                price_paid = pricePaid,
                payment_method = request.paymentMethod.ToLower(),
                purchased_at = DateTime.UtcNow
            };

            _context.Purchases.Add(purchase);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Course purchased successfully.",
                data = new
                {
                    purchaseId = purchase.id,
                    courseId = courseId,
                    userId = userId.Value,
                    purchaseDate = purchase.purchased_at.ToString("o"), 
                    paymentMethod = request.paymentMethod.ToLower(),
                    originalPrice = course.price,
                    discountApplied = discount,
                    paidAmount = pricePaid
                }
            });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult CreateCourse([FromBody] CreateCourseDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.title) || string.IsNullOrEmpty(dto.description) || dto.price <= 0 || dto.duration <= 0)
            {
                return UnprocessableEntity(new
                {
                    message = "price and duration must be numeric and greater than 0"
                });
            }

            if (dto.modules == null || dto.modules.Count < 3)
            {
                return UnprocessableEntity(new
                {
                    message = "Module Must Contain At Leash 3 Items"
                });
            }

            var course = new Course
            {
                title = dto.title,
                description = dto.description,
                price = dto.price,
                duration = dto.duration,
                created_at = DateTime.UtcNow,
                modules = dto.modules.Select(title => new CourseModule
                {
                    title = title
                }).ToList()
            };

            _context.Courses.Add(course);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Course created successfully.",
                data = new
                {
                    courseId = course.id,
                    course.title,
                    course.description,
                    course.price,
                    duration = $"{course.duration} minutes",
                    modules = dto.modules
                }
            });
        }

        [HttpPut("{courseId}")]
        [Authorize(Roles = "admin")]
        public IActionResult UpdateCourse(int courseId, [FromBody] UpdateCourseDTO dto)
        {
            var course = _context.Courses.Include(c => c.modules).FirstOrDefault(c => c.id == courseId);

            if (course == null)
            {
                return NotFound(new
                {
                    message = "Course Not Found"
                });
            }

            if (dto.modules == null || dto.modules.Count < 3)
            {
                return UnprocessableEntity(new
                {
                    message = "Module Must Contain At Leash 3 Items"
                });
            }

            if (dto.price <= 0 || dto.duration <= 0)
            {
                return UnprocessableEntity(new
                {
                    messgae = "price and duration must be numeric and greater than 0"
                });
            }

            // Update If Value Changed
            if (!string.IsNullOrWhiteSpace(dto.title) && dto.title != course.title)
            {
                course.title = dto.title;
            }

            if (!string.IsNullOrWhiteSpace(dto.description) || dto.description != course.description)
            {
                course.description = dto.description;
            }        
            
            if (dto.price != course.price)
            {
                course.price = dto.price;
            }

            if (dto.duration != course.duration)
            {
                course.duration = dto.duration;
            }

            course.modules.Clear();
            foreach (var title in dto.modules)
            {
                course.modules.Add(new CourseModule { title = title });
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Course updated successfully.",
                data = new
                {
                    courseId = course.id,
                    title = course.title,
                    description = course.description,
                    price = course.price,
                    duration = $"{course.duration}",
                    modules = course.modules.Select(m => m.title).ToList()
                }
            });
        }
    }
}
