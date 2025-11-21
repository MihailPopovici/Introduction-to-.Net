using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OrderManagement.Common.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidISBNAttribute : ValidationAttribute, IClientModelValidator
    {
        public ValidISBNAttribute()
        {
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;
            var raw = value as string;
            if (string.IsNullOrWhiteSpace(raw)) return true; 

            var cleaned = new string(raw.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());
            if (cleaned.Length != 10 && cleaned.Length != 13) return false;
            if (!cleaned.All(char.IsDigit)) return false;

            return true;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            MergeAttribute(context.Attributes, "data-val", "true");
            var error = string.IsNullOrEmpty(ErrorMessage)
                ? "The field must be a valid ISBN (10 or 13 digits)."
                : ErrorMessage;

            MergeAttribute(context.Attributes, "data-val-validisbn", error);
            MergeAttribute(context.Attributes, "data-val-validisbn-format", "10|13");
        }

        private static void MergeAttribute(System.Collections.Generic.IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key)) return;
            attributes.Add(key, value);
        }
    }
}

