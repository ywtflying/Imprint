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
    /// 点胶流程
    /// </summary>
    public class GlueProcedure : WorkProcedure
    {
        private GluePlatform _gluePlatform;
        private MacroPlatform _macroPlatform;
        public GlueProcedure(IMachineModel machine, IEventAggregator eventAggregator) :base(machine,eventAggregator)
        {
            _name = "点胶流程";
            _gluePlatform = _machine.GetPlatform(typeof(GluePlatform).Name) as GluePlatform;
            _macroPlatform = _machine.GetPlatform(typeof(MacroPlatform).Name) as MacroPlatform;
        }

        protected override bool OnExecute()
        {
            var model = new DeviceModel();
            if (!CheckWorkStatus())
                return false;
            //宏动平台移动点胶位置
            _macroPlatform.MoveToGluePosition();
            
            if (!CheckWorkStatus())
                return false;
            //点胶平台Z轴移动到点胶高度
            _gluePlatform.MoveToGluePosition();

            if (!CheckWorkStatus())
                return false;
            //执行点胶
            _gluePlatform.Glue();

            if (!CheckWorkStatus())
                return false;
            //点胶平台移动到等待位置
            _gluePlatform.MoveToWaitPosition();

            return true;
        }
        protected override bool Prepare()
        {
            Thread.Sleep(1);
            return true;
        }
    }
}
