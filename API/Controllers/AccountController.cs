using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            this._tokenService = tokenService;
            this._context = context;
        }

        [HttpPost("register")]

        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO){

            if(await UserExists(registerDTO.Username)) return BadRequest("UserName already exists");

            using var hmac = new HMACSHA512();
            
            var user = new AppUser{
                UserName = registerDTO.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTO{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

         [HttpPost("login")]
         public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){

            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDTO.Username);

            using var hmac = new HMACSHA512(user.PasswordSalt); // As it gets generated from random key so to avoid that.

            var computedvalue = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for (int i = 0; i < computedvalue.Length; i++)
            {
                if(computedvalue[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }
            return new UserDTO{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        public async Task<bool> UserExists(string Username){
            return await _context.Users.AnyAsync(x => x.UserName == Username);
        }
    }
}