using NanoImprinter.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Common;

namespace NanoImprinter.Procedures
{
    /// <summary>
    /// 压印
    /// </summary>
    public class PreprintProcedure : WorkProcedure
    {
        private ImprintPlatform _imprintPlatform;
        private MacroPlatform _macroPlatform;
        public PreprintProcedure(IDeviceManager machine, IEventAggregator eventAggregator) :base(machine,eventAggregator)
        {
            _name = "预压印流程";
            _device = machine;
            _imprintPlatform =_device.GetPlatform(typeof(ImprintPlatform).Name) as ImprintPlatform;
            _macroPlatform = _device.GetPlatform(typeof(MacroPlatform).Name) as MacroPlatform;
        }

        protected override bool OnExecute()
        {
            if (!CheckWorkStatus())
                return false;
            //宏动平台移动到压印位置
            _macroPlatform.MoveToImprintPosition();

            if (!CheckWorkStatus())
                return false;
            ///移动到预压印高度，等待相机拍照
            _imprintPlatform.MoveToMaskPrintHeight();

            return true;
        }
        protected override bool Prepare()
        {
            Thread.Sleep(1);
            return true;
        }
    }
}
