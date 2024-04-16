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
    /// 完成纳米压印
    /// </summary>
    public class ImprintProcedure : WorkProcedure
    {
        private MicroPlatform _microPlatform;
        private ImprintPlatform _imprintPlatform;
        private double _maxValue;
        private double _minValue;
        public ImprintProcedure(IDeviceManager machine, IEventAggregator eventAggregator) :base(machine,eventAggregator)
        {
            _name = "压印流程";
            _microPlatform = _machine.GetPlatform(typeof(MicroPlatform).Name) as MicroPlatform;
            _imprintPlatform = _machine.GetPlatform(typeof(ImprintPlatform).Name) as ImprintPlatform;
        }
        protected override bool OnExecute()
        {
            if (!CheckWorkStatus())
                return false;
            //移动到掩膜跟胶水接触位置
            _microPlatform.MoveToContactPosition();

            double foreValue = 0;
            var dis = _microPlatform.Config.ZCreepDistance;
            //计算1N等于多少纳米；
            var forceRatio = _maxValue / dis;
                      
            do
            {
                if (!CheckWorkStatus())
                    return false;

                //移动一段距离
                _microPlatform.Creep(_microPlatform.ZAxis, dis);

                Thread.Sleep(5);

                //获取压力传感器压力值
                var forceValues = new double[] {_imprintPlatform.ForceValue0,
                                                _imprintPlatform.ForceValue1,
                                                _imprintPlatform.ForceValue2 };
                //数据处理
                foreValue = CalculateForce(forceValues);
                //换算成数据
                dis = (_maxValue - foreValue) * forceRatio;

            } while (foreValue < _maxValue);

            return true;
        }

        protected override bool Prepare()
        {
            var foreValue = _machine.Config.MaskInfo.ForceValue;
            var percentage = _machine.Config.MaskInfo.ForceRangePercentage;
            _minValue = foreValue * (100 - percentage) / 100;
            _maxValue = foreValue * (100 + percentage) / 100;
            return true;
        }

        private double CalculateForce(double[] values)
        {
            double average = 0;
            foreach (var v in values)
            {
                average += v;
            }
            average = average / values.Length;
            var max = values.Max();
            var min = values.Min();
            return average;
        }
    }
}
