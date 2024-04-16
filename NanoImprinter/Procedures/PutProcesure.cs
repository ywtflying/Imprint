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
    /// 放料流程
    /// </summary>
    public class PutProcesure : WorkProcedure
    {

        public PutProcesure(IDeviceManager machine, IEventAggregator eventAggregator) :base(machine,eventAggregator)
        {
            _name = "放料流程";
        }

        protected override bool OnExecute()
        {
            Thread.Sleep(1);

            if (!CheckWorkStatus())
                return false;

            return true;

        }

        protected override bool Prepare()
        {
            Thread.Sleep(1);
            return true;
        }
    }
}
