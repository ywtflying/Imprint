using NanoImprinter.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoImprinter.Procedures
{
    public class UVCurePorcedure : WorkProcedure
    {
        private ImprintPlatform _platform;
        public UVCurePorcedure(IDeviceManager machine, IEventAggregator eventAggregator) : base(machine,eventAggregator)
        {
            _name = "UV固化流程";
            _platform = _machine.GetPlatform(typeof(ImprintPlatform).Name) as ImprintPlatform;
        }

        protected override bool OnExecute()
        {
            if (!CheckWorkStatus())
                return false;
            //移动到照射位
            _platform.MoveToUVIrradiationPosition();

            if (!CheckWorkStatus())
                return false;

            //开始照射
            _platform.UVIrradiate();

            if (!CheckWorkStatus())
                return false;
            //移动到等待位
            _platform.MoveToUVWaitPositon();

            return true;
        }

        protected override bool Prepare()
        {

            return true;
        }
    }
}
