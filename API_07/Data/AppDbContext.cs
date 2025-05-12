using System.Reflection;
using API_07.Model;
using Microsoft.EntityFrameworkCore;

namespace API_07.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        //public DbSet<System.Reflection.Module> Modules { get; set; }
        public DbSet<CourseModule> Modules { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
    }
}
