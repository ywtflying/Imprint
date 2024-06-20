using NanoImprinter.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Common;

namespace NanoImprinter.Procedures
{
    public class FindRotateCenterProcedure : WorkProcedure
    {
        private Point2D _wafeCenter;
        private MacroPlatform _platform;
        private DeviceManager device;
        public FindRotateCenterProcedure(IDeviceManager device, IEventAggregator eventAggregator) :base(device,eventAggregator)
        {
            _wafeCenter = new Point2D(0, 0);
            _platform = _device.GetPlatform(typeof(MacroPlatform).Name) as MacroPlatform;
            _name = "找圆心流程";
        }
        protected override bool OnExecute()
        {
            if (!CheckWorkStatus())
                return false;
            //移动到圆心左监测点 
            _platform.MoveTo(_platform.Config.LeftCenterPosition);

            //等待相机检测到圆心



            if (!CheckWorkStatus())
                return false;
            //移动到圆心右监测点
            _platform.MoveTo(_platform.Config.RightCenterPosition);

            //等待相机检测到圆心



            if (!CheckWorkStatus())
                return false;
            //移动到圆心上监测点
            _platform.MoveTo(_platform.Config.UpCenterPosition);

            //等待相机检测到圆心


            if (!CheckWorkStatus())
                return false;
            //移动到圆心下监测点
            _platform.MoveTo(_platform.Config.DownCenterPosition);

            //等待相机检测到圆心



            ///图像算法获取圆心位置
            _device.Config.WafeInfo.Center = new Point2D(1.0, 1.0);


            return true;
        }

        protected override bool Prepare()
        {
            Debug.WriteLine($"Prepare {typeof(FindRotateCenterProcedure)}");
            Thread.Sleep(1);
            return true;
        }

    }
}
