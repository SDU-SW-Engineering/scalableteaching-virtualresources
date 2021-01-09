using backend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Helpers
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
                new Claim("Cn", User.Cn),
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

        public static UserDTO Decode(String Token)
        {
            return null;
        }
    }
}
