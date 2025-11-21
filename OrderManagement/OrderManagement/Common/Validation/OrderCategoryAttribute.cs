using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using OrderManagement.Features;
using OrderManagement.Features.Orders;

namespace OrderManagement.Common.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class OrderCategoryAttribute(params OrderCategory[] allowed) : ValidationAttribute
    {
        private readonly OrderCategory[] _allowed = allowed ?? [];

        public override bool IsValid(object? value)
        {
            if (value == null) return true;

            if (value is OrderCategory cat)
            {
                if (_allowed.Length == 0) return Enum.IsDefined(typeof(OrderCategory), cat);
                return _allowed.Contains(cat);
            }
            
            if (Enum.IsDefined(typeof(OrderCategory), value))
            {
                var parsed = (OrderCategory)Enum.ToObject(typeof(OrderCategory), value);
                if (_allowed.Length == 0) return true;
                return _allowed.Contains(parsed);
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            var allowedText = _allowed.Length == 0
                ? string.Join(", ", Enum.GetNames(typeof(OrderCategory)))
                : string.Join(", ", _allowed.Select(a => a.ToString()));

            return string.IsNullOrEmpty(ErrorMessage)
                ? $"{name} must be one of the following categories: {allowedText}."
                : ErrorMessage;
        }
    }
}

