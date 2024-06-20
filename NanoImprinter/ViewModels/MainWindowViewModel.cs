using NanoImprinter.Model;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows;
using WestLakeShape.Motion;
using WestLakeShape.Motion.Device;

namespace NanoImprinter.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IDeviceManager _deviceManager;
        private readonly ProcedureManager _procedureManager;
        
        public MicroPlatform MicroPlatform { get; private set; }
        public MacroPlatform MacroPlatform { get; private set; }
        public ImprintPlatform ImprintPlatform { get; private set; }
        public GluePlatform GluePlatform { get; private set; }
        public AfmPlatform AfmPlatform { get; private set; }

        public DelegateCommand<string> NavigateCommand { get; private set; }


        public MainWindowViewModel(IRegionManager regionManager,
            IDeviceManager deviceManager,
            ProcedureManager procedureManager)
        {
            _regionManager = regionManager;
            _deviceManager = deviceManager;
            _procedureManager = procedureManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
            MicroPlatform = _deviceManager.GetPlatform(typeof(MicroPlatform).Name) as MicroPlatform;
            MacroPlatform = _deviceManager.GetPlatform(typeof(MacroPlatform).Name) as MacroPlatform;
            ImprintPlatform = _deviceManager.GetPlatform(typeof(ImprintPlatform).Name) as ImprintPlatform;
            GluePlatform =_deviceManager.GetPlatform(typeof(GluePlatform).Name) as GluePlatform;
            AfmPlatform = _deviceManager.GetPlatform(typeof(AfmPlatform).Name) as AfmPlatform;
        }


        private void Navigate(string navigatePath)
        {
            if (navigatePath != null)
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
        }

    }
}
