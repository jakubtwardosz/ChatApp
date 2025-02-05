﻿using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginModel)
        {
            try
            {
                var authData = await _authService.GetToken(loginModel);
                return Ok(authData);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured during login for user: {loginModel.Username}");
                return StatusCode(500, "An unexpected error occured during login");
            }
        }

        [HttpPut("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUser)
        {
            try
            {
                await _authService.RegisterUser(registerUser);
                return Ok(new { message = "User registered successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Registration attempt failed: User already exist with login: {registerUser.Username}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured during registration for user: {registerUser.Username}");
                return StatusCode(500, "An unexpected error occured during registration");
            }
        }
    }
}
