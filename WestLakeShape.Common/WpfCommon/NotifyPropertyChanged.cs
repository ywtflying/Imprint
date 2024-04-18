using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WestLakeShape.Common.WpfCommon
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly Dispatcher _dispatcher;

        public NotifyPropertyChanged()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
        //protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        //protected void SetProperty<T>(ref T properValue, T newValue, [CallerMemberName] string properName = "")
        //{
        //    if (object.Equals(properValue, newValue))
        //        return;

        //    properValue = newValue;
        //    OnPropertyChanged(properName);
        //}

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handlers = PropertyChanged;
            if (handlers != null)
            {
                if (_dispatcher.CheckAccess())
                {
                    handlers(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    _dispatcher.Invoke(()=>handlers(this,new PropertyChangedEventArgs(propertyName)));
                }
            }
        }
        protected void SetProperty<T>(ref T propertyValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(propertyValue, newValue))
                return;
            propertyValue = newValue;
            OnPropertyChanged(propertyName);
            
        }
    }
}
