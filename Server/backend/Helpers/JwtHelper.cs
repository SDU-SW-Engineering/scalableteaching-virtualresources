using Microsoft.IdentityModel.Tokens;
using ScalableTeaching.DTO;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ScalableTeaching.Helpers
{
    public class JwtHelper
    {
        public static String Create(UserDTO User, String issuer)
        {
            var key = new RsaSecurityKey(CryptoHelper.Instance.Rsa);

            var claims = new Claim[]
            {
                new Claim("username", User.Username),
                new Claim("mail", User.Mail),
                new Claim("sn", User.Sn),
                new Claim("gn", User.Gn),
                new Claim("cn", User.Cn),
                new Claim("account_type", User.AccountType)
            };
            var token = new JwtSecurityToken(
                issuer,
                issuer,
                claims,
                DateTime.UtcNow.AddMinutes(-1d),
                DateTime.UtcNow.AddHours(8),
                new SigningCredentials(key, SecurityAlgorithms.RsaSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
