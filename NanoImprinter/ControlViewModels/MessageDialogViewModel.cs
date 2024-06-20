using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoImprinter.ControlViewModels
{
    public class MessageDialogViewModel : BindableBase, IDialogAware
    {
        public string Title =>"提示";
        public string Message { get; set; }
        public event Action<IDialogResult> RequestClose;
        
        public DelegateCommand CloseCommand => new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("message"))
            {
                Message = parameters.GetValue<string>("message");
            }
        }
    }
}
