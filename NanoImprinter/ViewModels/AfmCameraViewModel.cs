using NanoImprinter.Model;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WestLakeShape.Common;
using WestLakeShape.Motion;

namespace NanoImprinter.ViewModels
{
    public class AfmCameraViewModel : BindableBase
    {
        private IDeviceManager _deviceManager;
        private IDialogService _dialogService;
        private AfmPlatform _plate;
        private AfmPlatformConfig _config;

        private Point3D _waitPosition;
        private Point3D _workPosition;

        #region property
        public Point3D WaitPosition
        {
            get => _waitPosition;
            set => SetProperty(ref _waitPosition, value);
        }
        public Point3D WorkPosition
        {
            get => _workPosition;
            set => SetProperty(ref _workPosition, value);
        }

        public ObservableCollection<IAxis> Axes { get; set; }
        #endregion

        #region
        public DelegateCommand GoHomeCommand => new DelegateCommand(GoHome);
        public DelegateCommand ResetAlarmCommand => new DelegateCommand(ResetAlarm);
        public DelegateCommand SaveParamCommand => new DelegateCommand(SaveParam);
        public DelegateCommand ReloadParamCommand => new DelegateCommand(ReloadParam);
        public DelegateCommand MoveToLoadPositionCommand => new DelegateCommand(MoveToWaitPosition);
        #endregion

        public AfmCameraViewModel(IDeviceManager deviceManager, IDialogService dialogService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _plate = deviceManager.GetPlatform(typeof(AfmPlatform).Name) as AfmPlatform;
            _config = _deviceManager.Config.AfmPlatform;

            Axes = new ObservableCollection<IAxis>();
            Axes.Add(_plate.XAxis);
            Axes.Add(_plate.YAxis);
            Axes.Add(_plate.ZAxis);

            ReloadParam();
        }

        private void SaveParam()
        {
            try
            {
                _config.WaitPosition = WaitPosition;
                _config.WorkPosition = WorkPosition;

                _deviceManager.SaveParam();
                //_plate.LoadAxesVelocity();
            }
            catch (Exception e)
            {
                ShowDialog(e.Message);
            }
        }

        private void ReloadParam()
        {
            WaitPosition = _config.WaitPosition;
            WorkPosition = _config.WorkPosition;
        }
        private void GoHome()
        {
            var task = Task.Run(() =>
            {
                _plate.GoHome();
            });
        }
        private void ResetAlarm()
        {

        }

        private void MoveToWaitPosition()
        {
            _plate.MoveToWaitPosition();
        }

        private void MoveToWorkPosition()
        {
            _plate.MoveToWorkPosition();
        }

        private void ShowDialog(string message, Action onOkClicked = null)
        {
            var parameters = new DialogParameters { { "message", message } };
            _dialogService.ShowDialog("MessageDialog", parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    onOkClicked?.Invoke();
                }
            });
        }
    }
}
