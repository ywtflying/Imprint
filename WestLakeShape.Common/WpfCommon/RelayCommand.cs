using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WestLakeShape.Common
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executWithParameter;
        private readonly Action _executNoParameter;
        private readonly Predicate<object> _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// 有形参的
        /// </summary>
        /// <param name="execut"></param>
        /// <param name="canExecute"></param>
        public RelayCommand(Action<object> execut, Predicate<object> canExecute = null)
        {
            _executWithParameter = execut;
            _canExecute = canExecute;
        }


        public RelayCommand(Action execut, Predicate<object> canExecute = null)
        {
            _executNoParameter = execut;
            _canExecute = canExecute;
        }



        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter) && !_isExecuting;
        }

        public void Execute(object parameter)
        {
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            try
            {
                _executWithParameter?.Invoke(parameter);
                _executNoParameter?.Invoke();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }

        }
    }
}
