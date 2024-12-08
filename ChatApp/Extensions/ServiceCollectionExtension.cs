using ChatApp.Core.Application.Services;
using ChatApp.Core.Domain;
using ChatApp.Core.Domain.Interfaces.Repositories;
using ChatApp.Core.Domain.Interfaces.Services;
using ChatApp.Core.Domain.Options;
using ChatApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ChatApp.API.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            AddCustomAuthentication(services, configuration);

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ChatDbContext>(options =>
                options.UseSqlServer(connectionString));
            return services;
        }

        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettingOption>(options => configuration.GetSection(nameof(JwtSettingOption)).Bind(options));
            return services;
        }

        private static void AddCustomAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(nameof(JwtSettingOption)).Get<JwtSettingOption>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
                throw new ArgumentException("Secret Key is Empty");

            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = GetTokenValidationParameters(key);
            });
        }

        private static TokenValidationParameters GetTokenValidationParameters(byte[] key)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(120)
            };
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IChatRepository, ChatRepository>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IJwtService, JwtService>();

            return services;
        }
    }
}
