using ChatApp.Core.Domain.Models;

namespace ChatApp.Core.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task AddUser(User user);
        Task<User?> GetUserById(Guid id);
        Task<User?> GetUserByLogin(string login);
    }
}
