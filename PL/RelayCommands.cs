using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PL
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object ? parameter) => _canExecute == null || _canExecute((T)parameter!);

        public void Execute(object ? parameter) => _execute((T)parameter!);

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    /// <summary>
    /// Represents a command that can be executed in response to user interaction, with optional logic to determine its
    /// availability.
    /// </summary>
    /// <remarks>This class is commonly used to implement the <see cref="ICommand"/> interface in MVVM
    /// (Model-View-ViewModel) patterns. It allows you to define the execution logic and, optionally, the logic that
    /// determines whether the command can execute. The <see cref="CanExecuteChanged"/> event is automatically raised
    /// when the <see cref="CommandManager.RequerySuggested"/> event occurs.</remarks>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

