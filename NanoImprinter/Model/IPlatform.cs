using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoImprinter.Model
{
    public interface IPlatform
    {
        bool IsConnected { get; }
        bool GoHome();
        void Connected();
        void Disconnected();
    }
    
    //public interface IComponentFactory
    //{
    //    IComponent CreateComponent();
    //}

    //public class AfmPlatformFactory : IComponentFactory
    //{
    //    private readonly AfmPlatformConfig _config;
    //    public AfmPlatformFactory(IMachineModel machine)
    //    {
    //        _config = machine.Config.AfmPlatform;
    //    }
    //    public IComponent CreateComponent()
    //    {
    //        return new AfmPlatform(_config);
    //    }
    //}

    //public class GluePlatformFactory : IComponentFactory
    //{
    //    private readonly GluePlatformConfig _config;

    //    public GluePlatformFactory(IMachineModel machine)
    //    {
    //        _config = machine.Config.GluePlatform;
    //    }
    //    public IComponent CreateComponent()
    //    {
    //        return new GluePlatform(_config);
    //    }
    //}

    //public class ImprintPlatformFactory : IComponentFactory
    //{
    //    private readonly ImprintPlatformConfig _config;

    //    public ImprintPlatformFactory(IMachineModel machine)
    //    {
    //        _config = machine.Config.ImprintPlatform;
    //    }

    //    public IComponent CreateComponent()
    //    {
    //        return new ImprintPlatform(_config);
    //    }
    //}

    //public class MacroPlatformFactory : IComponentFactory
    //{
    //    private readonly MacroPlatformConfig _config;
    //    public MacroPlatformFactory(IMachineModel machine)
    //    {
    //        _config = machine.Config.MacroPlatform;    
    //    }

    //    public IComponent CreateComponent()
    //    {
    //        return new MacroPlatform(_config);
    //    }
    //}


    //public class MicroPlatformFactory : IComponentFactory
    //{
    //    private MicroPlatformConfig _config;
    //    public MicroPlatformFactory(IMachineModel machine)
    //    {
    //        _config = machine.Config.MicroPlatform;
    //    }
    //    public IComponent CreateComponent()
    //    {
    //        return new MicroPlatform(_config);
    //    }
    //}
}
