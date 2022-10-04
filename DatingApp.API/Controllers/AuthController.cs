using DatingApp.API.Data;
using DatingApp.API.Data.Entities;
using DatingApp.API.DTOs;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AuthController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] AuthUserDTO userDto)
        {
            userDto.UserName = userDto.UserName.ToLower();
            if (_context.Users.Any(u => u.UserName == userDto.UserName))
            {
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
            var token = _tokenService.CreateToken(newUser.UserName);
            return Ok(token);
        }

        
        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthUserDTO userDto)
        {
            userDto.UserName = userDto.UserName.ToLower();
            var currentUser = _context.Users.FirstOrDefault(u => u.UserName == userDto.UserName);
            if (currentUser == null)
                return Unauthorized("Username is invalid");
            using var hmac = new HMACSHA512(currentUser.PasswordSalt);
            var passwordBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password));
            for (int i = 0; i < currentUser.PasswordHash.Length; i++)
            {
                if (currentUser.PasswordHash[i] != passwordBytes[i])
                    return Unauthorized("Password is invalid");
            }

            var token = _tokenService.CreateToken(currentUser.UserName);
            return Ok(token);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_context.Users.ToList());
        }
    }
}