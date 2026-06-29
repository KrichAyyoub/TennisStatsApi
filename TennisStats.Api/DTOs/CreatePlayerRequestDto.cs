using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TennisStats.Api.DTOs
{
    public class CreatePlayerRequestDto
    {
        [Required(ErrorMessage = "First name is required.")]
        public string Firstname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string Lastname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Short name is required.")]
        public string Shortname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sex is required.")]
        [RegularExpression("^[MF]$", ErrorMessage = "Sex must be 'M' or 'F'.")]
        public string Sex { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Country code must be exactly 3 characters.")]
        public string CountryCode { get; set; } = string.Empty;

        public string CountryPicture { get; set; } = string.Empty;

        public string Picture { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Rank must be greater than 0.")]
        public int Rank { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Points must be greater than or equal to 0.")]
        public int Points { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Weight must be greater than 0.")]
        public int Weight { get; set; } // in grams

        [Range(1, int.MaxValue, ErrorMessage = "Height must be greater than 0.")]
        public int Height { get; set; } // in cm

        [Range(1, int.MaxValue, ErrorMessage = "Age must be greater than 0.")]
        public int Age { get; set; }

        [ValidLastMatches]
        public List<int> Last { get; set; } = new();
    }

    public class ValidLastMatchesAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is List<int> list)
            {
                foreach (var match in list)
                {
                    if (match != 0 && match != 1)
                    {
                        return new ValidationResult("The last matches list must only contain 0 (defeat) or 1 (victory).");
                    }
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid type for last matches.");
        }
    }
}
