using System;
using System.Windows.Controls;
using System.Globalization;

namespace RTX2CSV_Converter
{
    public class Input_Validate : ValidationRule
    {
        public double Min { get; set; }
        public double Max { get; set; }

        public Input_Validate()
        {

        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double val = 0;

            try
            {
                if (((string)value).Length > 0)
                    val = Double.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Illegal characters or {e.Message}");
            }

            if ((val < Min) || (val > Max))
            {
                return new ValidationResult(false,
                  $"Please enter within permitted range.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
