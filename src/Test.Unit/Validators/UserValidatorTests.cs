﻿using FluentValidation.Results;
using Domain;
using Domain.Validators;
using Xunit;

namespace MockExams.Test.Unit.Validators
{
    public class UserValidatorTests
    {
        UserValidator userValidation;
        User userPasswordTest;

        public UserValidatorTests()
        {
            userValidation = new UserValidator();
            userPasswordTest = new User();

        }

        [Fact]
        public void ValidEntities()
        {
			User user = new User()
			{
				Email = "joão@MockExams.com.br",
				Password = "Password.123",
				Name = "João da Silva",
				Linkedin = "linkedin.com/joao-silva",
            };

            ValidationResult result = userValidation.Validate(user);

            Assert.True(result.IsValid);
        }

		[Fact]
		public void InvalidEntities()
        {
            User user = new User()
            {
                Email = "joão@MockExams.com.br",
                Password = null,
				Name = null,
				Linkedin = "linkedin.com/joao-silva",
			};

            ValidationResult result = userValidation.Validate(user);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void PasswordOnlyNumbers()
        {
            userPasswordTest.Password = "123456";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.False(result);
        }

        [Fact]
        public void PasswordOnlyLetters()
        {
            userPasswordTest.Password = "password";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.False(result);
        }

        [Fact]
        public void PasswordLettersNumbers()
        {
            userPasswordTest.Password = "password123";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.False(result);
        }

        [Fact]
        public void PasswordSpecialCharacter()
        {
            userPasswordTest.Password = "password.123";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.False(result);
        }

        [Fact]
        public void PasswordValid()
        {
            userPasswordTest.Password = "QweRty@123!";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.True(result);
        }

        [Fact]
        public void PasswordTwoValid()
        {
            userPasswordTest.Password = "601jFy0IN#";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.True(result);
        }

        [Fact]
        public void PasswordThreeValid()
        {
            userPasswordTest.Password = "Anu-P2017";

            var result = userPasswordTest.PasswordIsStrong();

            Assert.True(result);
        }
    }
}
