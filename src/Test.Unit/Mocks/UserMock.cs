﻿using Domain;
using Domain.Enums;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MockExams.Test.Unit.Mocks
{
    public class UserMock
    {
        private const string PASSWORD_HASH = "9XurTqQsYQY1rtAGXRfwEWO/ROghN3DFx9lTT75i/0s=";
        private const string PASSWORD_SALT = "1x7XxoaSO5I0QGIdARCh5A==";

        public ClaimsPrincipal GetClaimsUser()
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, new Guid().ToString())
                    }
                );

            identity.AddClaim(new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            return principal;
        }

        public static User GetDonor()
        {
            return  new User()
            {
                Id = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3"),
                Name = "Rodrigo",
                Password = PASSWORD_HASH,
                PasswordSalt = PASSWORD_SALT,
                Email = "rodrigo@MockExams.com.br",
                Linkedin = "linkedin.com/rodrigo",
                Profile = Profile.Usuario,
                LastLogin = DateTime.UtcNow.AddMinutes(-60)
            };
        }

        public static User GetGrantee()
        {
            return new User()
            {

                Id = new Guid("5489A967-9320-4350-FFFF-08D5CC8498F3"),
                Name = "Walter Vinicius",
                Password = PASSWORD_HASH,
                PasswordSalt = PASSWORD_SALT,
                Email = "walter@MockExams.com.br",
                Linkedin = "linkedin.com/walter",
                Profile = Profile.Usuario,
                LastLogin = DateTime.UtcNow.AddMinutes(-60),
            };
        }

        public static  User GetAdmin()
        {
            return new User()
            {
                Id = new Guid("5489A967-AAAA-BBBB-CCCC-08D5CC8498F3"),
                Name = "Cussa Mitre",
                Password = PASSWORD_HASH,
                PasswordSalt = PASSWORD_SALT,
                Email = "cussa@MockExams.com.br",
                Profile = Profile.Admin,
                LastLogin = DateTime.UtcNow.AddMinutes(-60)
            };
        }
    }
}
