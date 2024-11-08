using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using GirafAPI.Entities.Citizens.DTOs;
using Xunit;

namespace GirafAPI.UnitTests.Entities.Resources
{
    public class CitizenDTOTests
    {
        
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            
            // Create a validation context for the model
            var validationContext = new ValidationContext(model);
            
            // Validate the model using Validator.TryValidateObject
            Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Additionally, manually validate each property if needed
            var properties = model.GetType().GetProperties();
            
            foreach (var property in properties)
            {
                var value = property.GetValue(model);
                var validationAttributes = property.GetCustomAttributes(typeof(ValidationAttribute), true);
                
                foreach (ValidationAttribute attribute in validationAttributes)
                {
                    var result = attribute.GetValidationResult(value, validationContext);
                    if (result != ValidationResult.Success)
                    {
                        validationResults.Add(result);
                    }
                }
            }

            return validationResults;
        }

        [Fact]
        public void CitizenDTO_ValidData()
        {
            // Arrange
            var citizen = new CitizenDTO(1, "John", "Doe");

            // Act & Assert
            Assert.Equal(1, citizen.Id);
            Assert.Equal("John", citizen.FirstName);
            Assert.Equal("Doe", citizen.LastName);
        }
    }
}