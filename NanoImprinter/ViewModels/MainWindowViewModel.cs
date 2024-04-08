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
        private readonly IMachineModel _machine;
        private readonly ProcedureManager _manager;
        
        public MicroPlatform MicroPlatform { get; private set; }
        public MacroPlatform MacroPlatform { get; private set; }
        public ImprintPlatform ImprintPlatform { get; private set; }
        public GluePlatform GluePlatform { get; private set; }

        public DelegateCommand<string> NavigateCommand { get; private set; }


        public MainWindowViewModel(IRegionManager regionManager,
            IMachineModel machine,
            ProcedureManager manager)
        {
            _regionManager = regionManager;
            _machine = machine;
            _manager = manager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
            MicroPlatform = _machine.GetPlatform(typeof(MicroPlatform).Name) as MicroPlatform;
            MacroPlatform = _machine.GetPlatform(typeof(MacroPlatform).Name) as MacroPlatform;
            ImprintPlatform = _machine.GetPlatform(typeof(ImprintPlatform).Name) as ImprintPlatform;
            GluePlatform =_machine.GetPlatform(typeof(GluePlatform).Name) as GluePlatform;
        }


        private void Navigate(string navigatePath)
        {
            if (navigatePath != null)
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
        }

    }
}
