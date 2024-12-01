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

        public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
        {
            _logger = logger;
            _userRepository = userRepository;
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
