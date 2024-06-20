using NanoImprinter.Procedures;
using NanoImprinter.ViewModels;
using Prism.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Common.LogService;
using WestLakeShape.Common.WpfCommon;

namespace NanoImprinter.Model
{
    /// <summary>
    /// 考虑Load/Unload增加工序
    /// </summary>
    public class ProcedureManager : NotifyPropertyChanged
    {
        private static readonly ILogger Log = LogHelper.For<ProcedureManager>();
        private readonly SemaphoreSlim _workDoneEvent = new SemaphoreSlim(1, 1);
        private IEventAggregator _eventAggregator;
        private readonly Dictionary<string, WorkProcedure> _procedures;//所有流程列表
        private List<WorkProcedure> _autoProcedures;                   //自动流程列表
        private readonly IDeviceManager _deviceManager;
        private IOManager _ioStates;
       
        private WorkStatus _status; //机器当前状态
        private int _currentRow;    //当前压印图案的行
        private int _currentCol;    //当前压印图案的列

        #region property
        public int CurrentRow
        {
            get => _currentRow;
            set => SetProperty(ref _currentRow, value);
        }
        public int CurrentCol
        {
            get => _currentCol;
            set => SetProperty(ref _currentCol, value);
        }

        public WorkStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        public int MaskUsageCount
        {
            get => _deviceManager.Config.MaskInfo.MaskUsageCount;
            set => _deviceManager.Config.MaskInfo.MaskUsageCount = value;
        }

        public int MaskLifetimeCount
        {
            get => _deviceManager.Config.MaskInfo.MaskLifetimeCount;
            set => _deviceManager.Config.MaskInfo.MaskLifetimeCount = value;
        }

        public int ImprintCol
        {
            get => _deviceManager.Config.MaskInfo.ImprintCol;
            set => _deviceManager.Config.MaskInfo.ImprintCol = value;
        }

        public int ImprintRow
        {
            get => _deviceManager.Config.MaskInfo.ImprintRow;
            set => _deviceManager.Config.MaskInfo.ImprintRow = value;
        }

        public int ImprintCount
        {
            get => _deviceManager.Config.MaskInfo.ImprintCount;
            set => _deviceManager.Config.MaskInfo.ImprintCount = value;
        }

        public int CurrentIndex
        {
            get => _deviceManager.Config.MaskInfo.CurrentIndex;
            set=> _deviceManager.Config.MaskInfo.CurrentIndex = value;
        }
        #endregion

        public ProcedureManager(IDeviceManager deviceManager, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _deviceManager = deviceManager;
            _ioStates = _deviceManager.IoManager;

            _autoProcedures = new List<WorkProcedure>();
            _procedures = new Dictionary<string, WorkProcedure>();
            RegisterProcesure();
            RefreshDataService.Instance.Register(RefreshSystemStatus);
        }

        /// <summary>
        /// 通过流程名执行单个流程
        /// </summary>
        /// <param name="name"></param>
        public void ExcuteProcedureByName(string name)
        {
            WorkProcedure procedure;
            if (_procedures.TryGetValue(name, out procedure))
            {
                procedure.Execute();
            }
            else
            {
                throw new Exception("不存在该流程");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="procedureIndex"></param>
        /// <returns></returns>
        public bool ExecuteAutoProcedureByIndex(int procedureIndex)
        {
            try
            {
                var op = _autoProcedures[procedureIndex];
                var result = op.Execute();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetAutoProcedureName(int procedureIndex)
        {
            return _autoProcedures[procedureIndex].Name;
        }


        /// <summary>
        /// 注册流程列表
        /// </summary>
        private void RegisterProcesure()
        {
            //注册到自动执行流程
            _autoProcedures.Add(new GlueProcedure(_deviceManager, _eventAggregator));
            _autoProcedures.Add(new PreprintProcedure(_deviceManager, _eventAggregator));
            _autoProcedures.Add(new PositionProcedure(_deviceManager, _eventAggregator));
            _autoProcedures.Add(new ImprintProcedure(_deviceManager, _eventAggregator));
            _autoProcedures.Add(new UVCurePorcedure(_deviceManager, _eventAggregator));
            _autoProcedures.Add(new DemoldProcedure(_deviceManager, _eventAggregator));

            //注册到所有流程列表
            _autoProcedures.ForEach(op => AddProcedures(op));
            AddProcedures(new LoadProcedure(_deviceManager, _eventAggregator));
            AddProcedures(new FindRotateCenterProcedure(_deviceManager, _eventAggregator));
            AddProcedures(new CalibProcedure(_deviceManager, _eventAggregator));
            AddProcedures(new PutProcesure(_deviceManager, _eventAggregator));
        }

        /// <summary>
        /// 添加到所有流程列表中
        /// </summary>
        /// <param name="procedure"></param>
        private void AddProcedures(WorkProcedure procedure)
        {
            _procedures.Add(procedure.GetType().Name, procedure);
        }

 
        private void Stop()
        {
            foreach (var kvp in _procedures)
            {
                kvp.Value.Status = WorkStatus.Terminated;
            }
        }
        
        private void Pause()
        {
            foreach (var kvp in _procedures)
            {
                kvp.Value.Status = WorkStatus.Paused;
            }
        }

        private void Emergency()
        {
            _deviceManager.AxesManager.All().ForEach(o => o.EmergencyStop());
        }

        private void Start()
        {
            switch (_status)
            {
                case WorkStatus.Emergency:
                    Log.Information("进入急停状态");
                    throw new InvalidOperationException("急停状态下，必须先复位才能再次启动");
                case WorkStatus.Terminated:
                    Log.Information("纳米压印启动");
                    _workDoneEvent.Wait();
                    Status = WorkStatus.Running;
                    //StartContent = "暂停";
                    ThreadPool.QueueUserWorkItem(o => StartLoop());
                    break;
                case WorkStatus.Running:
                    Log.Information("纳米压印暂停");
                    Status = WorkStatus.Paused;
                    //StartContent = "启动";
                    return;
                case WorkStatus.Paused:
                    Status = WorkStatus.Running;
                    break;
            }
        }

        private void Reset()
        {
            Log.Information("复位，所有任务终止");
            Status = WorkStatus.Terminated;

            _deviceManager.AxesManager.All().ForEach(o => o.ResetAlarm());
        }

        private void GoHome()
        {
            Log.Information("系统开始回零");
            Status = WorkStatus.Terminated;
            var goHomeTask = Task.Run(() =>
            {
                //先脱模
                var microPlatform = _deviceManager.GetPlatform(typeof(MicroPlatform).Name) as MicroPlatform;
                microPlatform.Demold();

                foreach (var plate in _deviceManager.Platforms)
                    plate.Value.GoHome();
                Log.Information("系统回零完成");
            });
        }

        private void Vacuum()
        {
            Log.Information("真空吸气");
        }

        private void Evacuate()
        {
            Log.Information("真空吹气");
        }


        /// <summary>
        /// 开始执行自动流程
        /// </summary>
        private void StartLoop()
        {
            try
            {
                //放wafe
                Log.Information("放Wafe");
                ExcuteProcedureByName(typeof(LoadProcedure).Name);
                //定位wafe圆心
                Log.Information("定位Wafe圆心");
                ExcuteProcedureByName(typeof(FindRotateCenterProcedure).Name);
                //定位初次压印位置
                Log.Information("定位Wafe初次压印位置");
                ExcuteProcedureByName(typeof(PositionProcedure).Name);

                while (true)
                {
                    switch (_status)
                    {
                        case WorkStatus.Emergency:
                            return;
                        case WorkStatus.Terminated:
                            return;
                        case WorkStatus.Running:
                            SingleLoop();        //开始执行单个图案的压印
                            break;
                        case WorkStatus.Paused:
                            Thread.Sleep(10);
                            break;
                        default:
                            throw new Exception("未知");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Information(ex.Message);
                throw ex;
            }
            finally
            {
                Log.Information("任务结束");
                _workDoneEvent.Release();
                _status = WorkStatus.Terminated;
            }
        }


        /// <summary>
        /// 单次循环
        /// </summary>
        private void SingleLoop()
        {
            //达到压印次数，则终止任务
            if (CurrentIndex >= ImprintCount)
            {
                Status = WorkStatus.Terminated;
                //StartContent = "启动";
                return;
            }

            //单个图案的压印
            while (_deviceManager.ProcedureIndex < _autoProcedures.Count)
            {
                var result = ExecuteAutoProcedureByIndex(_deviceManager.ProcedureIndex);

                if (result)
                {
                    //记录当前流程的步骤，报错关机后下次继续执行
                    _deviceManager.ProcedureIndex++;

                    //结束流程
                    if (_deviceManager.ProcedureIndex == _autoProcedures.Count)
                    {
                        _deviceManager.ProcedureIndex = 0;

                        //压印图案计数; 计数条件待定。
                        CurrentIndex++;
                        CurrentRow = CurrentIndex / ImprintCol;
                        CurrentCol = CurrentIndex % ImprintCol;

                        break;
                    }
                }
                else
                {
                    var name = GetAutoProcedureName(_deviceManager.ProcedureIndex);
                    throw new Exception($"流程{name}未正确执行完");
                }
            }
        }


        private void RefreshSystemStatus()
        {
            if (_ioStates.GetInputIOStatus(InputIOName.Stop.ToString()))
            {
                Stop();
            }
            else if (_ioStates.GetInputIOStatus(InputIOName.Start.ToString()))
            {
                var isHomeComplete = _deviceManager.Platforms.Any(platform => platform.Value.IsHomeComplete == true);
                if (isHomeComplete)
                {
                    Start();
                }
                else
                {
                    throw new Exception("系统回零未完成");
                }
            }
            else if (_ioStates.GetInputIOStatus(InputIOName.GoHome.ToString()))
            {
                GoHome();
            }
            //else if (_ioStates.GetInputIOStatus(InputIOName.Reset.ToString()))
            //{
            //    Reset();
            //}
        }
    }

    public enum WorkStatus
    {
        /// <summary>
        /// 终止
        /// </summary>
        Terminated,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,
        /// <summary>
        /// 暂停中
        /// </summary>
        Paused,
        /// <summary>
        /// 急停中
        /// </summary>
        Emergency,
    }
}
