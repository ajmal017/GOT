using System;
using System.Windows.Input;

namespace GOT.UI.Common
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute == null) {
                    return;
                }

                CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute == null) {
                    return;
                }

                CommandManager.RequerySuggested -= value;
            }
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute = (Predicate<T>) null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T) parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute == null) {
                    return;
                }

                CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute == null) {
                    return;
                }

                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            _execute((T) parameter);
        }
    }
}