using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Interfaces.Repositories;
using ChatApp.Core.Domain.Interfaces.Services;
using ChatApp.Core.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _userService;

        public AuthController(IAuthService userService)
        {
            _userService = userService;
        }

        [HttpPut("register")]
        public async Task<JsonResult> GetUser()
        {
            await _userService.RegisterUser(new RegisterUserDto
            {
                Username = "tes1t",
                Password = "tes2t"
            });

            return Json("User registered");
        }
    }
}
