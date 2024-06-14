using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrioMotion.TrioPC_NET;

namespace WestLakeShape.Motion.Device
{
    /// <summary>
    /// 临时添加的直流电机
    /// </summary>
    public class DCAxis : TrioAxis
    {
        private DCAxisConfig _config;
        public DCAxis(DCAxisConfig config) : base(config)
        {
            _config = config;
        }

        public override void ServoOn()
        {
            var rt = _trioPC.SetAxisVariable(TrioParamName.ServoOn, (short)_config.Index, 1);//上使能
            if (_config.Index == 4 || _config.Index == 5 || _config.Index == 6)
            {
                Thread.Sleep(5000);//轴使能需要延时
                CheckException(rt);
                rt = _trioPC.CoWrite(0, (short)(_config.Index + 1), 0x60FE, 2, 7, 0X10000);
                CheckException(rt);
                rt = _trioPC.CoWrite(0, (short)(_config.Index + 1), 0x60FE, 1, 7, 0X0);//输出抱闸
                CheckException(rt);

                rt = _trioPC.CoWrite(0, (short)(_config.Index + 1), 0x60FE, 1, 7, 0X10000);//关闭抱闸
                CheckException(rt);               
            }
            rt = _trioPC.SetAxisParameter(AxisParameter.SERVO, _config.Index, 1); //所有轴打开编码器
            GetState();
        }
        public override void InitialParameter()
        {
            base.InitialParameter();

            //完成直流点击正负限位
            if (_config.IsFwdEnable)
            {
                _trioPC.SetAxisParameter(AxisParameter.FWD_IN, _config.Index, _config.FwdIndex);
            }
            if (_config.IsRevEnable)
            {
                _trioPC.SetAxisParameter(AxisParameter.REV_IN, _config.Index, _config.RevIndex);
            }
        }


        public override void ServoOff()
        {
            var rt = _trioPC.CoWrite(0, 1, 0x60FE, 2, 7, 0X10000);
            CheckException(rt);
            rt = _trioPC.CoWrite(0, 1, 0x60FE, 1, 7, 0X0);//输出抱闸
            CheckException(rt);
            rt = _trioPC.SetAxisVariable("AXIS_ENABLE", 0, 0);//断使能
            CheckException(rt);
            rt = _trioPC.CoWrite(0, 1, 0x60FE, 1, 7, 0);//关闭抱闸
            CheckException(rt);
        }

        public override bool GoHome()
        {
            bool isHomeFinish = false;
            double value;
            double datumIOIndex;
            double fwdIOIndex;//正限位IO端口号
            double revIOIndex;//负限位IO端口号

            var acc = _config.GoHomeSpeed * 10;     //加速度=10*Vel
            SetAxisParameter(AxisParameter.ACCEL, acc);
            SetAxisParameter(AxisParameter.DECEL, acc);
            SetAxisParameter(AxisParameter.SPEED, _config.GoHomeSpeed);

            //获取轴的回零模式
            var ret =_trioPC.GetAxisParameter(AxisParameter.AXISSTATUS, _config.Index, out value);
            CheckException(ret);
            switch (_config.HomeModel)
            {
                //正向回原到正限位
                case TrioHomeModel.PositiveAndLimit:
                    // 获取正限位输入IO端口号
                    ret = _trioPC.GetAxisParameter(AxisParameter.FWD_IN, _config.Index, out fwdIOIndex);
                    CheckException(ret);
                    // 获取原点的输入IO端口号
                    ret = _trioPC.GetAxisParameter(AxisParameter.DATUM_IN, _config.Index, out datumIOIndex);
                    CheckException(ret);
                    //取消正限位关联IO
                    ret = _trioPC.SetAxisParameter(AxisParameter.FWD_IN, _config.Index, -1);          
                    CheckException(ret);
                    //原正限位关联IO作为原点IO
                    ret = _trioPC.SetAxisParameter(AxisParameter.DATUM_IN, _config.Index, fwdIOIndex);
                    CheckException(ret);
                    //开始回零
                    ret = _trioPC.Datum(3, _config.Index);
                    CheckException(ret);
                    //获取轴回零状态
                    ret = _trioPC.GetAxisParameter(AxisParameter.MTYPE, _config.Index, out value);
                    CheckException(ret);
                    //判断回零完成
                    while (value == 0) 
                    {
                        ret = _trioPC.GetAxisParameter(AxisParameter.MTYPE, _config.Index, out value);
                        CheckException(ret);
                    }
                    if (value != 0)
                        isHomeFinish = true;

                    //获取轴位置到达信号
                    ret = _trioPC.GetAxisParameter(AxisParameter.AXISSTATUS, _config.Index, out value);
                    CheckException(ret);
                    while (((int)value >> 6) == 1)
                    {
                        ret = _trioPC.GetAxisParameter(AxisParameter.AXISSTATUS, _config.Index, out value);
                        CheckException(ret);
                    }

                    //回绝对零点
                    if (isHomeFinish)
                    {
                        ret = _trioPC.MoveAbs(new double[] { 0 }, _config.Index);
                        CheckException(ret);
                    }
                    
                    //恢复原点和限位设定
                    ret = _trioPC.SetAxisParameter(AxisParameter.DATUM_IN, _config.Index, datumIOIndex);
                    CheckException(ret);
                    ret = _trioPC.SetAxisParameter(AxisParameter.FWD_IN, _config.Index, fwdIOIndex);
                    CheckException(ret);
                    
                    break;
                
                //反向回原到负限位
                //case TrioHomeModel.NegativeAndLimit:                    
                //    ret =_trioPC.GetAxisParameter(AxisParameter.REV_IN, _config.Index, out revIOIndex);
                //    CheckException(ret);
                //    ret =_trioPC.GetAxisParameter(AxisParameter.DATUM_IN, _config.Index, out datumIOIndex);
                //    CheckException(ret);
                //    //取消负限位关联IO
                //    ret = _trioPC.SetAxisParameter(AxisParameter.REV_IN, _config.Index, -1);
                //    CheckException(ret);
                //    //原负限位关联IO作为原点IO
                //    ret = _trioPC.SetAxisParameter(AxisParameter.DATUM_IN, _config.Index, revIOIndex);
                //    CheckException(ret);
                //    ret = _trioPC.Datum(4, _config.Index);
                //    CheckException(ret);
                //    ret = _trioPC.GetAxisParameter(AxisParameter.MTYPE, _config.Index, out value);
                //    CheckException(ret);
                //    while (value == 0)
                //    {
                //        ret =_trioPC.GetAxisParameter(AxisParameter.MTYPE, _config.Index, out value);
                //        CheckException(ret);
                //    }
                //    ret = _trioPC.GetAxisParameter(AxisParameter.AXISSTATUS, _config.Index, out value);
                //    CheckException(ret);
                //    while (((int)value >> 6) == 1)
                //    {
                //        ret = _trioPC.GetAxisParameter(AxisParameter.AXISSTATUS, _config.Index, out value);
                //        CheckException(ret);
                //    }
                //    //恢复原点和负限位IO设定
                //    ret = _trioPC.SetAxisParameter(AxisParameter.DATUM_IN, _config.Index, datumIOIndex);
                //    CheckException(ret);
                //    ret = _trioPC.SetAxisParameter(AxisParameter.REV_IN, _config.Index, revIOIndex);
                //    CheckException(ret);
                //    break;
            }


            ret = _trioPC.SetAxisParameter(AxisParameter.FS_LIMIT, _config.Index, _config.SoftPositiveDistance);
            CheckException(ret);
            ret = _trioPC.SetAxisParameter(AxisParameter.RS_LIMIT, _config.Index, _config.SoftNegativeDistance);
            CheckException(ret);
            return true;
        }

    }


    public class DCAxisConfig : TrioAxisConfig
    {
        [Category("DCAxis"), Description("正限位IO索引"), DefaultValue(10)]
        [DisplayName("正限位IO索引")]
        public double FwdIndex { get; set; }


        [Category("DCAxis"), Description("负限位IO索引"), DefaultValue(10)]
        [DisplayName("负限位IO索引")]
        public double RevIndex { get; set; }
        

        [Category("DCAxis"), Description("存在负限位"), DefaultValue(10)]
        [DisplayName("存在负限位")]
        public bool IsFwdEnable { get; set; }


        [Category("DCAxis"), Description("存在正限位"), DefaultValue(10)]
        [DisplayName("存在正限位")]
        public bool IsRevEnable { get; set; }
        [Category("DCAxis"), Description("软正限位值，行程值"), DefaultValue(10)]
        [DisplayName("软正限位值")]
        public double SoftPositiveDistance { get; set; }

        [Category("DCAxis"), Description("软正限位值，行程值"), DefaultValue(10)]
        [DisplayName("软负限位")]
        public double SoftNegativeDistance { get; set; }
    }
}
