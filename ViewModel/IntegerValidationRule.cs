using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PunchingFoundRebarModule.ViewModel
{
    public class IntegerValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = (value ?? "").ToString();

            if (int.TryParse(input, out _))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, "Введите целое число");
        }
    }
}
