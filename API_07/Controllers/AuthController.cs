using API_07.Data;
using API_07.DTO;
using API_07.Model;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace API_07.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        [HttpPost("register")]
        public IActionResult Register(UserRegisDTO userDTO)
        {

            // Validasi Input
            if (string.IsNullOrWhiteSpace(userDTO.name) ||
                string.IsNullOrWhiteSpace(userDTO.username) ||
                string.IsNullOrWhiteSpace(userDTO.email) ||
                string.IsNullOrWhiteSpace(userDTO.password))
            {
                return UnprocessableEntity(new
                {
                    message = "Validation error: All fields are required."
                });
            }

            // Validasi Format Email
            if (!Regex.IsMatch(userDTO.email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return UnprocessableEntity(new
                {
                    message = "Validation error: email is invalid."
                });
            }

            // Validasi Password
            if (!Regex.IsMatch(userDTO.password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$"))
            {
                return UnprocessableEntity(new
                {
                    message = "Validation error: password must be at least 8 characters long and include uppercase, lowercase, number, and special character."
                });
            }

            // Cek Email
            var existingUser = _context.Users.FirstOrDefault(u => u.email == userDTO.email);
            if (existingUser != null)
            {
                return UnprocessableEntity(new
                {
                    message = "Validation error: email already exists."
                });
            }

            // Hash Password
            string hashpw = HashPassword(userDTO.password);

            // Buat User
            var newUser = new User
            {
                name = userDTO.name,
                username = userDTO.username,
                email = userDTO.email,
                password_hash = hashpw,
                role = "student"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new
            {
                message = "User registered successfully."
            });
        }

        private string GenerateToken(User user)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
            new System.Security.Claims.Claim("userId", user.id.ToString()),
            new System.Security.Claims.Claim("username", user.username),
            new System.Security.Claims.Claim("role", user.role)
        }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("login")]
        public IActionResult Login(UserLoginDTO loginDTO)
        {
            // Validasi Input
            if (string.IsNullOrWhiteSpace(loginDTO.email) || string.IsNullOrWhiteSpace(loginDTO.password))
            {
                return UnprocessableEntity(new
                {
                    message = "Validation error: email and password are required."
                });
            }

            if (!Regex.IsMatch(loginDTO.email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return UnprocessableEntity(new
                {
                    message = "Validation error: email is invalid."
                });
            }

            var user = _context.Users.FirstOrDefault(u => u.email == loginDTO.email);
            if (user == null || user.password_hash != HashPassword(loginDTO.password))
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Generate JWT
            var token = GenerateToken(user);

            return Ok(new
            {
                message = "Login successful.",
                data = new
                {
                    userId = user.id,
                    username = user.username,
                    role = user.role,
                    token = token
                }
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new
            {
                message = "Logout successful."
            });
        }
    
    }
}
