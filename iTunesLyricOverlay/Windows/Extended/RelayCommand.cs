using System;
using System.Windows.Input;

namespace iTunesLyricOverlay.Windows.Extended
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object>     m_execute;
        private readonly Func<object, bool> m_canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.m_execute    = execute;
            this.m_canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
            => this.m_canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter)
            => this.m_execute(parameter);
    }
}
