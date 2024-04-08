using NanoImprinter.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoImprinter.Procedures
{
    /// <summary>
    /// 标定
    /// </summary>
    public class CalibProcedure:WorkProcedure
    {
        public CalibProcedure(IMachineModel machine, IEventAggregator eventAggregator) :base(machine,eventAggregator)
        {
            _name = "标定流程";
        }

        protected override bool OnExecute()
        {
            if (!CheckWorkStatus())
                return false;
            throw new NotImplementedException();
        }

        protected override bool Prepare()
        {
            throw new NotImplementedException();
        }
    }
}
