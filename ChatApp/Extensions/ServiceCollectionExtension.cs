using ChatApp.Core.Application.Services;
using ChatApp.Core.Domain;
using ChatApp.Core.Domain.Interfaces.Repositories;
using ChatApp.Core.Domain.Interfaces.Services;
using ChatApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlServer(connectionString));
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IAuthService, AuthService>();
            return services;
        }
    }
}
