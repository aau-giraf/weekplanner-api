using System.ComponentModel.DataAnnotations;
using Xunit;
using System.Collections.Generic;
using GirafAPI.Entities.Users.DTOs;

namespace GirafAPI.UnitTests.Entities.Users
{
    public class CreateUserDTOTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void UserName_IsRequired()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                UserName = null, // Missing the required value
                Password = "validpassword"
            };            

            // Act
            var validationResults = ValidateModel(createUserDTO);

            // Log
            foreach (var result in validationResults)
            {
                Console.WriteLine($"{result.ErrorMessage} - {string.Join(",", result.MemberNames)}");
            }

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateUserDTO.UserName)));
            Assert.Equal("The UserName field is required.", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void Password_IsRequired()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                UserName = "ValidUsername", 
                Password = null // Missing the required value
            };            

            // Act
            var validationResults = ValidateModel(createUserDTO);

            // Log
            foreach (var result in validationResults)
            {
                Console.WriteLine($"{result.ErrorMessage} - {string.Join(",", result.MemberNames)}");
            }

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateUserDTO.Password)));
            Assert.Equal("The Password field is required.", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void UserName_CannotExceedMaxLength()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                UserName = new string('a', 51), // Exceeds max length
                Password = "validpassword"
            };

            // Act
            var validationResults = ValidateModel(createUserDTO);

            // Log
            foreach (var result in validationResults)
            {
                Console.WriteLine($"{result.ErrorMessage} - {string.Join(",", result.MemberNames)}");
            }

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateUserDTO.UserName)));
            Assert.Equal("The field UserName must be a string with a maximum length of 50.", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void Password_CannotExceedMaxLength()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                UserName = "validusername",
                Password = new string('a', 101) // Exceeds max length
            };

            // Act
            var validationResults = ValidateModel(createUserDTO);

            // Log
            foreach (var result in validationResults)
            {
                Console.WriteLine($"{result.ErrorMessage} - {string.Join(",", result.MemberNames)}");
            }

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateUserDTO.Password)));
            Assert.Equal("The field Password must be a string with a maximum length of 100.", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void ValidCreateUserDTO_PassesValidation()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                UserName = "validusername",
                Password = "validpassword"
            };

            // Act
            var validationResults = ValidateModel(createUserDTO);

            // Log
            foreach (var result in validationResults)
            {
                Console.WriteLine($"{result.ErrorMessage} - {string.Join(",", result.MemberNames)}");
            }

            // Assert
            Assert.Empty(validationResults); // Expect no validation errors
        }
    }
}