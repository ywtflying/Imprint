﻿using NanoImprinter.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Motion;
using WestLakeShape.Motion.Device;

namespace NanoImprinter.Procedures
{
    /// <summary>
    /// 取料流程
    /// </summary>
    public class LoadProcedure : WorkProcedure
    {
        private MacroPlatform _platform;
        private IOManager _io;
        public LoadProcedure(IDeviceManager device, IEventAggregator eventAggregator) :base(device, eventAggregator)
        {
            _name = "取料流程";
            _platform = _device.GetPlatform(typeof(MacroPlatform).Name) as MacroPlatform;
            _io = _device.IoManager;
        }

        protected override bool OnExecute()
        {
            if (!CheckWorkStatus())
                return false;

            //移动到放料位
            _platform.MoveToLoadPosition();

            if (!CheckWorkStatus())
                return false;

            //等待取走晶圆
            _io.GetInputIO(InputIOName.HasWafe.ToString()).SyncWait();

            //间隔一定时间
            Thread.Sleep(10);
            
            if (!CheckWorkStatus())
                return false;

            //等待放入新的晶圆
            _io.GetInputIO(InputIOName.HasWafe.ToString()).SyncWait(false);

            return true;
        }


        protected override bool Prepare()
        {
            return true;
        }
    }
}
