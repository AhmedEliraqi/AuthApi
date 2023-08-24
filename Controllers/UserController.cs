using AuthApi.Context;
using AuthApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace AuthApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly AppDbContext _authContext;
        public UserController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;

        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] Users userObj)
        {
            if (userObj == null)

                return BadRequest();

            var user = await _authContext.User.FirstOrDefaultAsync(x => x.UserName == userObj.UserName && x.Password == userObj.Password);
            if (user == null)
                return NotFound(new { Message = "User Not Found" });



            user.Token = CreateJwt(user);
            return Ok(new
            {
                Token = user.Token,
                Message = "Login Success"
            });
        }


        [HttpPost("register")]

        public async Task<IActionResult> RegisterUser([FromBody] Users userObj)
        {
            if (userObj == null)
                return BadRequest();

            if (await CheckUserNameExistAsync(userObj.UserName))
                return BadRequest(new { Message = "Username Alraedy Exist" });

            if (await CheckEmailExistAsync(userObj.Email))
                return BadRequest(new { Message = "Email Alraedy Exist" });

            

            await _authContext.User.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "User Registered"
            });
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] Users userObj)
        {
            if (userObj == null)
                return BadRequest();

            if (await CheckUserNameExistAsync(userObj.UserName))
                return BadRequest(new { Message = "Username Alraedy Exist" });

            if (await CheckEmailExistAsync(userObj.Email))
                return BadRequest(new { Message = "Email Alraedy Exist" });

            

            await _authContext.User.AddAsync(userObj);
            await _authContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "Admin Registered"
            });
        }

        private Task<bool> CheckUserNameExistAsync(string userName)
            => _authContext.User.AnyAsync(x => x.UserName == userName);

        private Task<bool> CheckEmailExistAsync(string email)
            => _authContext.User.AnyAsync(x => x.Email == email);

        private string CreateJwt(Users user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysecret..xcgdfxgdfgxgxdgdsgrsg...");
            var identity = new ClaimsIdentity(new Claim[]
            {
               
                new Claim(ClaimTypes.Name,$"{user.FirstName}{user.LastName}"),
                new Claim(ClaimTypes.Email,$"{user.Email}"),
                
                
            });
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescripter = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescripter);
            return jwtTokenHandler.WriteToken(token);

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new { Message = "You have admin access" });
        }

        
        [HttpGet]
        public async Task<ActionResult<Users>> GetAllUsers()
        {
            return Ok(await _authContext.User.ToListAsync());
        }
    }
}
