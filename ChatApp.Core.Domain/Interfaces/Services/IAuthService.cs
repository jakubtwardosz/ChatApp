using ChatApp.Core.Domain.Dtos;

namespace ChatApp.Core.Domain.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthDto> GetToken(LoginDto loginModel);
        Task RegisterUser(RegisterUserDto registerUser);
    }
}
