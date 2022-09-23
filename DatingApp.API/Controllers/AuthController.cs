using System.Security.Cryptography;
using System.Text;
using DatingApp.API.Data;
using DatingApp.API.Data.Entities;
using DatingApp.API.DTOs;
using Microsoft.AspNetCore.Mvc;
namespace DatingApp.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly DataContext _context;
        public AuthController(DataContext context)
        {
            _context = context;
            
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] AuthUserDTO userDto)
        {
            userDto.UserName = userDto.UserName.ToLower();
            if(_context.Users.Any(u=>u.UserName==userDto.UserName)){
                return BadRequest("Username is already taken");
            }

            using var hmac = new HMACSHA512();
            var passwordBytes = Encoding.UTF8.GetBytes(userDto.Password);
            var newUser = new User
            {
                UserName = userDto.UserName,
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(passwordBytes),
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();
            return Ok(newUser);
        }


        [HttpPost("login")]
        public void Login([FromBody]  AuthUserDTO userDto)
        {
            
        }

    }
}
