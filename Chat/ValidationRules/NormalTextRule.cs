using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chat.ValidationRules
{
    internal class NormalTextRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // Setup
            string str = value as string ?? string.Empty;
            List<char> dvc = new List<char>();     // Dont valid chars

            if (string.IsNullOrEmpty(str))
                return new ValidationResult(false, "Field cannot be empty");

            // Must not contain spaces
            if (str.Contains(' '))
                return new ValidationResult(false, "Mustn't contain spaces");

            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    dvc.Add(c);
                }
            }

            if (dvc.Count != 0)
            {
                // Evaluate errors
                StringBuilder sb = new StringBuilder();
                if (dvc.Count != 1)
                {
                    // If more than one error list them
                    foreach (char c in dvc)
                    {
                        sb.Append($"'{c}', ");
                    }

                    sb.Remove(sb.Length - 3, 2);
                    sb.Append(" aren't valid letters!");
                }
                else
                    sb.Append($"'{dvc[0]}' isn't a valid letter!");

                return new ValidationResult(false, sb.ToString());
            }
            else
                return ValidationResult.ValidResult;
        }
    }
}
