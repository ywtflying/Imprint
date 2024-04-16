using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WestLakeShape.Common.WpfCommon
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<Task<bool>> _executeWithResult;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _executeWithResult = null;
            _canExecute = canExecute;
        }
        public AsyncRelayCommand(Func<Task<bool>> executeWithResult, Func<bool> canExecute = null)
        {
            _executeWithResult = executeWithResult ?? throw new ArgumentNullException(nameof(executeWithResult));
            _execute = null;
            _canExecute = canExecute;
        }


        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public async void Execute(object parameter)
        {
            if (_execute != null)
                await _execute();
            else if (_executeWithResult != null)
                await _executeWithResult();

        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
