using NanoImprinter.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NanoImprinter.Procedures
{
    /// <summary>
    /// 脱模流程
    /// </summary>
    public class DemoldProcedure : WorkProcedure
    {
        private MicroPlatform _microPlatform;
        private ImprintPlatform _imprintPlatform;
        public DemoldProcedure(IMachineModel machine, IEventAggregator eventAggregator) :base(machine,eventAggregator)
        {
            _name = "脱模流程";
            _microPlatform = _machine.GetPlatform(typeof(MicroPlatform).Name) as MicroPlatform;
            _imprintPlatform = _machine.GetPlatform(typeof(ImprintPlatform).Name) as ImprintPlatform;
        }

        protected override bool OnExecute()
        {
            if (!CheckWorkStatus())
                return false;
            _microPlatform.Demold();
            Thread.Sleep(10);

            if (!CheckWorkStatus())
                return false;
            _imprintPlatform.MoveToMaskWaitHeight();

            return true;
        }
        protected override bool Prepare()
        {
            Thread.Sleep(1);
            return true;
        }
    }
}
