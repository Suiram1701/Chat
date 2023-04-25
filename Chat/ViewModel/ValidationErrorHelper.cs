using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Chat.ViewModel
{
    internal class ValidationErrorHelper : INotifyDataErrorInfo
    {
        public bool HasErrors =>
            Errors.Count > 0;

        /// <summary>
        /// Contains all errors
        /// </summary>
        private IDictionary<string, List<string>> Errors { get; } = new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Get the next error for the property
        /// </summary>
        /// <param name="propertyName">Property</param>
        /// <returns>Error message</returns>
        public string this[string propertyName]
        {
            get
            {
                if (Errors.ContainsKey(propertyName))
                    return Errors[propertyName].First();
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Check if the given property has an error
        /// </summary>
        /// <param name="propertyName">Property</param>
        /// <returns><see langword="true"/> if the poroperty has an error</returns>
        public bool HasError([CallerMemberName] string propertyName = "") =>
            Errors.ContainsKey(propertyName);

        public IEnumerable GetErrors([CallerMemberName] string propertyName = "")
        {
            if (Errors.ContainsKey(propertyName))
                return Errors[propertyName];
            else
                return Array.Empty<string>();
        }

        /// <summary>
        /// Add an error to the error list for the property
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="propertyName">Property</param>
        public void AddError(string message, [CallerMemberName] string propertyName = "")
        {
            if (!Errors.ContainsKey(propertyName))
                Errors.Add(propertyName, new List<string>());

            Errors[propertyName].Add(message);
        }

        /// <summary>
        /// Remove all errors for the property
        /// </summary>
        /// <param name="propertyName">Property</param>
        public void ClearErrors([CallerMemberName] string propertyName = "")
        {
            Errors.Remove(propertyName);
            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        /// Notify that the errors has been changed
        /// </summary>
        /// <param name="propertyName">Property</param>
        public void RaiseErrorsChanged([CallerMemberName] string propertyName = "")
        {
            if (!string.IsNullOrEmpty(propertyName))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
