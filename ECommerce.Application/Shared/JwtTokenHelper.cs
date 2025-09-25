using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Shared
{
    public class JwtTokenHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;


        public JwtTokenHelper(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> GenerateTokenAsync(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName??string.Empty),
                new Claim(ClaimTypes.Email, user.Email??string.Empty),
                new Claim("uid", user.Id.ToString())
            };
            claims.AddRange(userRoles.Select(role=>new Claim(ClaimTypes.Role, role)));
            claims.AddRange(userClaims);

            var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var credentials=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var token=new JwtSecurityToken(
                issuer:_configuration["JwtSettings:Issuer"],
                audience:_configuration["JwtSettings:Audience"],
                claims:claims,
                expires: GetTokenExpiryDate(),
                signingCredentials:credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
        public DateTime GetTokenExpiryDate()
        {
            return DateTime.UtcNow.AddDays(double.Parse(_configuration["JwtSettings:ExpirationInDays"]!));
        }
    }
}
