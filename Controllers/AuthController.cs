// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using JWTAUTHAPP.Data;
using JWTAUTHAPP.Models;
using JWTAUTHAPP.Helpers;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthController(MongoDbContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            
            var existingUser = _context.Users.Find(u => u.Username == user.Username).FirstOrDefault();
            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }

            
            user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);


            _context.Users.InsertOne(user);

            return Ok("User registered successfully.");
        }

        
        [HttpPost("login")]
public IActionResult Login([FromBody] User loginUser)
{
    var user = _context.Users.Find(u => u.Username == loginUser.Username).FirstOrDefault();
    if (user == null)
    {
        Console.WriteLine("User not found in database.");
        return Unauthorized("Invalid username or password.");
    }

    Console.WriteLine($"Entered Password: {loginUser.PasswordHash}");
    Console.WriteLine($"Stored Hashed Password: {user.PasswordHash}");

    bool isPasswordValid = PasswordHasher.VerifyPassword(loginUser.PasswordHash, user.PasswordHash);
    Console.WriteLine($"Password Verification Result: {isPasswordValid}");

    if (!isPasswordValid)
    {
        return Unauthorized("Invalid username or password.");
    }

    var token = GenerateJwtToken(user);
    return Ok(new { Token = token });
}


        
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
