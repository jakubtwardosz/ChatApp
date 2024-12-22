using ChatApp.Core.Domain.Dtos;
using ChatApp.Core.Domain.Interfaces.Services;
using ChatApp.Core.Domain.Models;
using ChatApp.Core.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Core.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettingsOption _jwtSettingOption;

        public JwtService(IOptions<JwtSettingsOption> jwtSettingOption)
        {
            _jwtSettingOption = jwtSettingOption.Value;
        }

        public AuthDto GenerateJwtToken(User user)
        {
            var claims = GetClaims(user);
            var expiryDate = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtSettingOption.ExpiryInMinutes)).AddHours(24);
            var creds = GetCredentials();

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiryDate,
                signingCredentials: creds
                );

            return new AuthDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiredDate = expiryDate,

            };
        }

        private SigningCredentials GetCredentials()
        {
            var byteSecretKey = Encoding.ASCII.GetBytes(_jwtSettingOption.SecretKey);
            var key = new SymmetricSecurityKey(byteSecretKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return creds;
        }

        private Claim[] GetClaims(User user)
        {
            return new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString())
            };
        }
    }
}
