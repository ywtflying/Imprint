using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrioMotion.TrioPC_NET;
using WestLakeShape.Common;
using WestLakeShape.Common.LogService;

namespace WestLakeShape.Motion.Device
{
    /// <summary>
    /// 原计划使用异步
    /// </summary>
    public class TrioAxis : Axis<TrioAxisConfig>
    {
        private readonly TimeSpan _startWait = TimeSpan.FromMilliseconds(5);
        private static readonly ILogger _log = LogHelper.For<TrioAxis>();
        private Movement _currentMovement;
        private AxisState _state;

        protected static TrioPC _trioPC;
        protected TrioAxisConfig _config;
        protected bool _isReturnHome;

        public int Index => _config.Index;
        public override double Position => _state.Position;
        public override double Speed => _state.Speed;
        public bool IsBusy => _state.IsBusy;
        public string Name => _config.Name;
        public bool IsReturnHome => _isReturnHome;

        public TrioAxis(TrioAxisConfig config) : base(config)
        {
            _trioPC = TrioControl.Instance.TrioPC;
            _config = config;
            _state = new AxisState();
            _log.Information($"{config.Name}轴完成创建");
        }


        public void ReloadConfig(TrioAxisConfig newConfig)
        {
            _config = newConfig.DeepClone();
            InitialParameter();
        }


        /// <summary>
        /// 回零
        /// </summary>
        /// <returns></returns>
        public override bool GoHome()
        {
            if (_config.HomeModel == TrioHomeModel.MoveToZero)
            {
                MoveTo(0);
                _isReturnHome = true;
                return true;
            }
            else
            {
                //先停止轴运动
                var ret = _trioPC.Execute("RAPIDSTOP(2)");
                //执行回零动作
                _trioPC.Datum((int)(_config.HomeModel), _config.Index);
                //等待回零完成
                WaitGoHome();
                _isReturnHome = true;
                return true ;
            }
        }


        /// <summary>
        /// 绝对移动
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override bool MoveTo(double position)
        {
            Stop();
  
            var movement = new Movement(position);
            _currentMovement = movement;

            var rt = _trioPC.MoveAbs(new double[] { position }, _config.Index);
            CheckException(rt);

            GetState();
            return true ;
        }

        /// <summary>
        /// 相对移动
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override bool MoveBy(double position)
        {
            Stop();
            var targetPosition = _state.Position + position;
            var movement = new Movement(targetPosition);
            _currentMovement = movement;

            var rt = _trioPC.MoveRel(new double[] { position }, _config.Index);
            CheckException(rt);

            GetState();
            return true;
            //return await movement.TaskCompletionSource.Task.ConfigureAwait(false);
        }


        /// <summary>
        /// 以jog速度持续移动
        /// </summary>
        /// <param name="jogSpeed"></param>
        /// <param name="isForward"></param>
        public void ContinueMove(double jogSpeed, bool isForward)
        {
            SetAxisParameter(AxisParameter.JOGSPEED, jogSpeed);
            if (isForward)
                _trioPC.Forward(_config.Index);
            else
                _trioPC.Reverse(_config.Index);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        public override void Stop()
        {
            if (!IsBusy)
                return;

            var rt = _trioPC.Cancel(0, _config.Index);
            CheckException(rt);

            var movement = _currentMovement;
            _currentMovement = null;
            
            if (movement != null)
            {
                if (movement.TaskCompletionSource.TrySetResult(false))
                    throw new Exception("运动终止失败");
            }

            while (IsBusy)
            {
                Thread.Sleep(5);
                GetState();
            }
        }

        /// <summary>
        /// 急停
        /// </summary>
        public override void EmergencyStop()
        {
            _trioPC.Cancel(2, _config.Index);
        }


        protected void GetState()
        {
            double positionValue, speedValue;
            double moveValue, stopValue;
            _state.IsBusy = true;

            do
            {
                //获取目标位置
                //GetAxisParameter(AxisParameter.DPOS, out targetPostion);               
                GetAxisParameter(AxisParameter.MPOS, out positionValue); //当前的速度
                GetAxisParameter(AxisParameter.MSPEED, out speedValue);  //当前位置
                GetAxisParameter(AxisParameter.IDLE, out stopValue);//运动否已停止
                GetAxisParameter(AxisParameter.MTYPE, out moveValue);//当前的运动指令,空闲为Idle

                _state.Speed = speedValue;
                _state.Position = positionValue;
                _state.Command = (TrioMtypeValue)moveValue;
                _state.IsBusy = stopValue == 0 ? true : false; //0(false)代表正在移动

                var movement = _currentMovement;
                if (movement != null)
                {
                    if (DateTime.UtcNow - movement.StartTimeUtc > _startWait)
                    {
                        if (!_state.IsBusy &&
                           Math.Abs(positionValue - movement.TargetPostion) <= 10)
                        {
                            if (!movement.TaskCompletionSource.TrySetResult(true))
                                throw new Exception("轴运动完成赋值出错");
                            lock (_state)
                            {
                                if (movement == _currentMovement)
                                    _currentMovement = null;
                            }
                        }
                        else if (!_state.IsBusy && Math.Abs(positionValue - movement.TargetPostion) > 10)
                        {
                            throw new Exception($"轴移动出错，目标位置{_currentMovement.TargetPostion},实际位置{_state.Position}");
                        }
                    }
                }
            }
            while (_state.IsBusy);
        }

        public Task RefreshState()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    double positionValue, speedValue;
                    double moveValue, stopValue;

                    //获取目标位置
                    //GetAxisParameter(AxisParameter.DPOS, out targetPostion);

                    //当前的速度和位置
                    GetAxisParameter(AxisParameter.MPOS, out positionValue);
                    GetAxisParameter(AxisParameter.MSPEED, out speedValue);
                    //运动否已停止
                    GetAxisParameter(AxisParameter.IDLE, out stopValue);
                    //当前的运动指令
                    GetAxisParameter(AxisParameter.MTYPE, out moveValue);

                    _state.Speed = speedValue;
                    _state.Position = positionValue;
                    _state.Command = (TrioMtypeValue)moveValue;
                    
                    //0(false)代表正在移动
                    _state.IsBusy = stopValue == 0 ? true : false;


                    var movement = _currentMovement;
                    if (movement != null)
                    {
                        if (DateTime.UtcNow - movement.StartTimeUtc > _startWait)
                        {
                            if (!_state.IsBusy &&
                               Math.Abs(positionValue - movement.TargetPostion) <= 10)
                            {
                                if (!movement.TaskCompletionSource.TrySetResult(true))
                                    throw new Exception("轴运动完成赋值出错");
                                lock (_state)
                                {
                                    if (movement == _currentMovement)
                                        _currentMovement = null;
                                }
                            }
                        }
                    }
                }
               
            });
        }

        /// <summary>
        /// 清除报警
        /// </summary>
        public override void ResetAlarm()
        {
            // Tbi_AxisAlarmReset(Axis, AlarmclrChannel)
            //var ret = _trioPC.Datum(0);
            //CheckException(ret);
            //var command = $"datum({_config.Index})";
            //_trioPC.Execute(command);
        }

        public string GetErrorCode()
        {
            double errorCodeValue;
            //获取状态
            GetAxisParameter(AxisParameter.AXISSTATUS, out errorCodeValue);

            string errorMsg = $"错误码:{errorCodeValue}; ";
            var errorCode = (TrioErroCode)errorCodeValue;

            foreach (TrioErroCode flag in Enum.GetValues(typeof(TrioErroCode)))
            {
                if (errorCode.HasFlag(flag))
                    errorMsg += GetErrorCode(flag);
            }

            return errorMsg;
        }

        public override void ServoOff()
        {
            var ret = _trioPC.SetAxisVariable(TrioParamName.ServoOn, _config.Index, 0);
            //var ret = _trioPC.SetVariable(TrioParamName.Enable, 0);
            CheckException(ret);
        }
        public override void ServoOn()
        {
            if (_config.Index != 3)
            {
                var ret = _trioPC.SetAxisVariable(TrioParamName.IsLoop, _config.Index, 1);
                CheckException(ret);
                ret = _trioPC.SetAxisVariable(TrioParamName.ServoOn, _config.Index, 1);
                CheckException(ret);
                GetState();
            }
        }

        private bool WaitGoHome()
        {
            var commValue = TrioMtypeValue.Datum;
            double typeValue = 1;

            while (commValue == TrioMtypeValue.Datum)
            {
                Thread.Sleep(50);
                //获取当前运动指令
                GetAxisParameter(AxisParameter.MTYPE, out typeValue);

                typeValue = Math.Round(typeValue, 0);
                Enum.TryParse(typeValue.ToString(), out commValue);

                //回零后将vr101置2
                if (commValue == TrioMtypeValue.Idle)
                    _trioPC.SetVr(101, 2);
            }
            return true;
        }

        /// <summary>
        /// 写入轴默认参数
        /// </summary>
        public override void InitialParameter()
        {
            SetAxisParameter(AxisParameter.UNITS, _config.PlusEquivalent);
            var acc = _config.Speed * 10;     //加速度=10*Vel
            var jogSpeed = _config.Speed / 2; //jog速度=vel/2
            SetAxisParameter(AxisParameter.ACCEL,  acc);
            SetAxisParameter(AxisParameter.DECEL, acc);
            SetAxisParameter(AxisParameter.SPEED, _config.Speed);
            SetAxisParameter(AxisParameter.JOGSPEED, jogSpeed);
            SetAxisParameter(AxisParameter.CREEP, _config.Creep);//触发原点后，移动到限位的速度
            SetAxisParameter(AxisParameter.FE_LIMIT, 10);
            SetAxisParameter(AxisParameter.FE_RANGE, 10);

            if (_config.Index == 3)
            {
                //R轴设置正负限位值，防止角度过大，微动平台线缆扯断
                //因为离线编程，后续加入到config类中
                var ret = _trioPC.SetAxisParameter(AxisParameter.FS_LIMIT, _config.Index, 10);
                CheckException(ret);
                ret = _trioPC.SetAxisParameter(AxisParameter.RS_LIMIT, _config.Index, -10);
                CheckException(ret);
            }
        }

        public override void LoadVelocity(double vel)
        {
            var acc = vel * 10;
            SetAxisParameter(AxisParameter.ACCEL, acc);
            SetAxisParameter(AxisParameter.DECEL, acc);
            SetAxisParameter(AxisParameter.SPEED, vel);
        }

        /// <summary>
        /// 设置轴参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private void SetAxisParameter(AxisParameter name, double value)
        {
            var ret = _trioPC.SetAxisParameter(name, _config.Index, value);
            CheckException(ret);
        }

        /// <summary>
        /// 获取轴参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private void GetAxisParameter(AxisParameter name, out double value)
        {
            var ret= _trioPC.GetAxisParameter(name, _config.Index, out value);
            CheckException(ret);
        }

        private void SetAxisVariable(string name, double value)
        {
            var ret = _trioPC.SetAxisVariable(name, _config.Index, value);
            CheckException(ret);
        }

        private void GetAxisVariable(string name, out double value)
        {
            var ret = _trioPC.GetAxisVariable(name, _config.Index, out value);
            CheckException(ret);
        }

        /// <summary>
        /// 获取报警说明
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        private static string GetErrorCode(TrioErroCode errorCode)
        {
            var file = errorCode.GetType().GetField(errorCode.ToString());
            var attributs = (DescriptionAttribute[])file.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributs != null && attributs.Length > 0)
                return attributs[0].ToString();
            else
                return errorCode.ToString();
        }

        protected static void CheckException(bool ret)
        {
            if (!ret)
                throw new Exception("dll的Api函数使用报错");
        }

        class Movement
        {
            public Movement(double position)
            {
                TargetPostion = position;
                StartTimeUtc = DateTime.UtcNow;
                TaskCompletionSource = new TaskCompletionSource<bool>();
            }

            public double TargetPostion { get; private set; }

            public DateTime StartTimeUtc { get; private set; }

            public TaskCompletionSource<bool> TaskCompletionSource { get; private set; }
        }

        public class AxisState
        {
            public double Position { get; internal set; }

            public double Speed { get; internal set; }

            public TrioMtypeValue Command { get; internal set; }
            public bool IsBusy { get; set; }
        }
    }


    public class TrioAxisConfig: AxisConfig
    {
        private double _plusEquivalent;
        private double _acc;
        private double _dec;
        private double _startSpeed;
        private double _workSpeed;
        private TrioHomeModel _homeModel;
        private double _creep;

        [Category("TrioAxis"), Description("脉冲当量"), DefaultValue(10)]
        [DisplayName("脉冲当量")]
        public double PlusEquivalent
        {
            get => _plusEquivalent;
            set => SetProperty(ref _plusEquivalent, value);
        }

        [Category("TrioAxis"), Description("加速度"), DefaultValue(100)]
        [DisplayName("加速度")]
        public double Acc
        {
            get => _acc;
            set => SetProperty(ref _acc, value);
        }
        
        [Category("TrioAxis"), Description("减速度"), DefaultValue(100)]
        [DisplayName("减速度")]
        public double Dec
        {
            get => _dec;
            set => SetProperty(ref _dec, value);
        }

        [Category("TrioAxis"), Description("启动速度"), DefaultValue(1000)]
        [DisplayName("启动速度")]
        public double StartSpeed
        {
            get => _startSpeed;
            set => SetProperty(ref _startSpeed, value);
        }

        [Category("TrioAxis"), Description("工作速度"), DefaultValue(1000)]
        [DisplayName("工作速度")]
        public double WorkSpeed
        {
            get => _workSpeed;
            set => SetProperty(ref _workSpeed, value);
        }

        [Category("TrioAxis"), Description("回零方式"), DefaultValue(TrioHomeModel.MoveToZero)]
        [DisplayName("回零方式")]
        public TrioHomeModel HomeModel 
        {
            get => _homeModel;
            set => SetProperty(ref _homeModel, value);
        }

        [Category("TrioAxis"), Description("回零触碰零点后到负限位的速度"), DefaultValue(10)]
        [DisplayName("回零蠕动速度")]
        public double Creep
        {
            get => _creep;
            set => SetProperty(ref _creep, value);
        }        
    }
}
