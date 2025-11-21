using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace OrderManagement.Common.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PriceRangeAttribute : ValidationAttribute
    {
        private readonly decimal _min;
        private readonly decimal _max;

        public PriceRangeAttribute(double min, double max)
        {
            _min = Convert.ToDecimal(min);
            _max = Convert.ToDecimal(max);
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true; // let [Required] handle nulls

            if (value is decimal dec)
            {
                return dec >= _min && dec <= _max;
            }

            if (value is double dbl)
            {
                var d = Convert.ToDecimal(dbl);
                return d >= _min && d <= _max;
            }

            if (value is float f)
            {
                var d = Convert.ToDecimal(f);
                return d >= _min && d <= _max;
            }

            if (decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed >= _min && parsed <= _max;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var minText = _min.ToString("C", CultureInfo.CurrentCulture);
            var maxText = _max.ToString("C", CultureInfo.CurrentCulture);
            return string.IsNullOrEmpty(ErrorMessage)
                ? $"{name} must be between {minText} and {maxText}."
                : ErrorMessage;
        }
    }
}

