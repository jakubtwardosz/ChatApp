using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Interfaces.Repositories;
using ChatApp.Core.Domain.Interfaces.Services;
using ChatApp.Core.Domain.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ChatApp.Core.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, ILogger<AuthService> logger, IJwtService jwtService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthDto> GetToken(LoginDto loginModel)
        {
            try
            {
                var user = await _userRepository.GetUserByLogin(loginModel.Username);

                if (user == null)
                {
                    _logger.LogWarning($"User with login {loginModel.Username} not found");
                    throw new InvalidOperationException("User with this login does not exist");
                }

                if (VerifyPassword(loginModel.Password, user.Password))
                {
                    var authData = _jwtService.GenerateJwtToken(user);

                    return authData;
                }
                else
                {
                    throw new UnauthorizedAccessException("Invalid credentials");
                }               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Authentication failed for user: {loginModel.Username}");
                throw new InvalidProgramException();
            }
        }

        public bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            var parts = storedPassword.Split(':');

            if (parts.Length != 2)
            {
                throw new FormatException("Unexpected hash format. The stored hash should be in format 'salt:hashedPassword'");
            }

            var salt = Convert.FromBase64String(parts[0]);
            var storedHashedPassword = parts[1];

            var enteredHashedPassword = Hash(enteredPassword, salt);

            return storedHashedPassword == enteredHashedPassword;
        }

        public async Task RegisterUser(RegisterUserDto registerUser)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByLogin(registerUser.Username);
                if (existingUser != null)
                {
                    _logger.LogError($"User with this login {registerUser.Username} already exists");
                    throw new InvalidOperationException("User with this login already exists");
                }

                var user = new User(registerUser.Username, HashPassword(registerUser.Password));
                await _userRepository.AddUser(user);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while registering user: {registerUser.Username}");
                throw new InvalidProgramException();
            }
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[128/8];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Hash(password, salt);

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        private string Hash(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}
