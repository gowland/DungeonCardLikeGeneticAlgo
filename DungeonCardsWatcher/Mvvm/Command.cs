using System;
using System.Windows.Input;

namespace DungeonCardsWatcher.Mvvm
{
    public class Command : ICommand
    {
        private readonly Func<object, bool> _canExecuteFunc;
        private readonly Action<object> _executeAction;

        public Command(Action<object> executeAction, Func<object, bool> canExecuteFunc)
        {
            _executeAction = executeAction;
            _canExecuteFunc = canExecuteFunc;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class Command<T> : ICommand
    {
        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteFunc;

        public Command(Action<T> executeAction, Func<T, bool> canExecuteFunc) : base()
        {
            _executeAction = executeAction;
            _canExecuteFunc = canExecuteFunc;
        }

        public bool CanExecute(object parameter)
        {
            return AsT(parameter, out T parameterAsT) && _canExecuteFunc(parameterAsT);
        }

        public void Execute(object parameter)
        {
            if (AsT(parameter, out T parameterAsT))
            {
                _executeAction(parameterAsT);
            }
        }

        public event EventHandler CanExecuteChanged;

        private static bool AsT(object parameter, out T paramterAsT)
        {
            if (parameter is T converted)
            {
                paramterAsT = converted;
                return true;
            }

            try
            {
                paramterAsT = (T)Convert.ChangeType(parameter, typeof(T));
                return paramterAsT != null;
            }
            catch (InvalidCastException)
            {
                paramterAsT = default(T);
            }

            return false;
        }
    }
}