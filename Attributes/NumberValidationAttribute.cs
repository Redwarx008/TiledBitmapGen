using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledBitmapGen.ViewModels;

namespace TiledBitmapGen.Attributions
{
    internal class NumberValidationAttribute : ValidationAttribute
    {
        private float _min;
        private float _max;
        public NumberValidationAttribute(float min, float max)
        {
            _min = min;
            _max = max;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string numStr)
            {
                if (string.IsNullOrEmpty(numStr)) 
                {
                    return ValidationResult.Success;
                }
                if (float.TryParse(numStr, out float num))
                { 
                    if (num >= _min && num <= _max)
                        return ValidationResult.Success;
                }
            }
            return new ValidationResult($"Input value must be a number within the range [{_min}, {_max}]");
        }
    }
}
