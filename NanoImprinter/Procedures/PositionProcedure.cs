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
    /// 移动到拍照高度拍照，获取补偿值，并完成补偿动作
    /// </summary>
    public class PositionProcedure : WorkProcedure
    {
        private ImprintPlatform _imprintPlatform;
        private MacroPlatform _macroPlatform;
        private ImprinterIO _io;

        public PositionProcedure(IMachineModel machine, IEventAggregator eventAggregator) :base(machine,eventAggregator)
        {
            _name = "定位流程";
            _imprintPlatform = _machine.GetPlatform(typeof(ImprintPlatform).Name) as ImprintPlatform;
            _macroPlatform = _machine.GetPlatform(typeof(MacroPlatform).Name) as MacroPlatform;
        }

        protected override bool OnExecute()
        {
            if (!CheckWorkStatus())
                return false;

            //相机动到拍照高度
            _imprintPlatform.MoveToTakePictureHeight();

            Thread.Sleep(5);
            //拍照

            //计算补偿值

            var offset = new PointXYR(0, 0, 0);
            
            if (!CheckWorkStatus())
                return false;

            //移动到相机等待拍照位
            _imprintPlatform.MoveToCameraWaitHeight();

            if (!CheckWorkStatus())
                return false;

            //移动到精确位置
            _macroPlatform.MoveTo(offset);

            return true;
        }

        protected override bool Prepare()
        {
            return true;
        }
    }
}
