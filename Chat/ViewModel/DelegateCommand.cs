using System;
using System.Windows.Input;

namespace Chat.ViewModel
{
    /// <summary>
    /// A command to run in MVVM
    /// </summary>
    internal class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Command to execute
        /// </summary>
        private readonly Action<object> _execute;

        /// <summary>
        /// Fuction to check if command is executable
        /// </summary>
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Define a command that run without any condition
        /// </summary>
        /// <param name="execute">Action to run</param>
        public DelegateCommand(Action<object> execute) : this(null, execute)
        {
        }
        
        /// <summary>
        /// Define a command that run with a condition
        /// </summary>
        /// <param name="canExecute">Condition</param>
        /// <param name="execute">Action to run when condition is <see langword="true"/></param>
        public DelegateCommand(Predicate<object> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        /// <summary>
        /// Update source button
        /// </summary>
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        bool ICommand.CanExecute(object parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        void ICommand.Execute(object parameter) =>
            _execute?.Invoke(parameter);
    }
}
