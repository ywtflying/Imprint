using NanoImprinter.Events;
using NanoImprinter.Model;
using NanoImprinter.Procedures;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WestLakeShape.Common.LogService;

namespace NanoImprinter.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private IDeviceManager _deviceManager;
    
        private static readonly ILogger Log = LogHelper.For<MainViewModel>();
        private const int Max_Log_Count = 15;
        
        private ProcedureManager _procedureManager;
        //private IEventAggregator _eventAggregator;

        private string _startContent = "启动";

        private ProcedureStatus _loadStatus;
        private ProcedureStatus _glueStatus;
        private ProcedureStatus _preprintStatus;
        private ProcedureStatus _imprintStatus;
        private ProcedureStatus _uvStatus;
        private ProcedureStatus _demoldStatus;
        private ProcedureStatus _positionStatus;
        #region property
    
        /// <summary>
        /// 启动按钮显示文字
        /// </summary>
        public string StartContent
        {
            get => _startContent;
            set => SetProperty(ref _startContent, value);
        }
       
        public IDeviceManager DeviceManager
        {
            get=> _deviceManager;
            set => SetProperty(ref _deviceManager, value);
        }

        public ObservableCollection<LogEvent> LogEvents { get; private set; }
        #endregion


        #region procedure status
        public ProcedureStatus LoadStatus
        {
            get => _loadStatus;
            set=>SetProperty(ref _loadStatus, value);
        }
        public ProcedureStatus GlueStatus
        {
            get => _glueStatus;
            set=>SetProperty(ref _glueStatus, value);
            
        }
        public ProcedureStatus PreprintStatus
        {
            get => _preprintStatus;
            set=>SetProperty(ref _preprintStatus, value);
        }
        public ProcedureStatus ImprintStatus
        {
            get => _imprintStatus;
            set => SetProperty(ref _imprintStatus, value);
        }
        public ProcedureStatus UVStatus
        {
            get => _uvStatus;
            set => SetProperty(ref _uvStatus, value);
        }
        public ProcedureStatus DemoldStatus
        {
            get => _demoldStatus;
            set=> SetProperty(ref _demoldStatus, value);
            
        }
        public ProcedureStatus PositionStatus
        {
            get => _positionStatus;
            set => SetProperty(ref _positionStatus, value);
        }
        #endregion


        #region command

        public DelegateCommand ConnectedCommand => new DelegateCommand(Connected);
      
        ////原来使用的屏幕按钮
        //public DelegateCommand StartCommand => new DelegateCommand(Start);
        //public DelegateCommand EmergencyCommand => new DelegateCommand(Emergency);
        //public DelegateCommand ResetCommand => new DelegateCommand(Reset);
        //public DelegateCommand GoHomeCommand => new DelegateCommand(GoHome);
        //public DelegateCommand VacuumCommand => new DelegateCommand(Vacuum);
        //public DelegateCommand EvacuateCommand => new DelegateCommand(Evacuate);
        #endregion


        public MainViewModel(IEventAggregator eventAggregator,
            IDeviceManager deviceManager, 
            ProcedureManager procedureManager)
        {
            _deviceManager = deviceManager;
            _procedureManager = procedureManager;
            eventAggregator.GetEvent<ProcedureInfoEvent>().Subscribe(ProcedureInfoChanged);

            LogEvents = new ObservableCollection<LogEvent>();
            if (Application.Current is App app)
            {
                app.LogEventStream
                    .ObserveOnDispatcher()
                    .Subscribe(logEvent =>
                    {
                        LogEvents.Add(logEvent);
                        if (LogEvents.Count > Max_Log_Count)
                        {
                            LogEvents.RemoveAt(0);
                        }
                    });
            }
        }

   
        /// <summary>
        /// 刷新当前各个流程状态
        /// </summary>
        /// <param name="info"></param>
        private void ProcedureInfoChanged(ProcedureInfo info)
        {
            switch (info.Name)
            {
                case "取料流程":
                    LoadStatus = info.ProcedureStatus;
                    break;
                case "点胶流程":
                    GlueStatus = info.ProcedureStatus;
                    break;
                case "预压印流程":
                    PreprintStatus = info.ProcedureStatus;
                    break;
                case "压印流程":
                    ImprintStatus = info.ProcedureStatus;
                    break;
                case "UV固化流程":
                    PreprintStatus = info.ProcedureStatus;
                    break;
                case "脱模流程":
                    DemoldStatus = info.ProcedureStatus;
                    break;
                case "定位流程":
                    PositionStatus = info.ProcedureStatus;
                    break;

                case "找圆心流程":
                    var status = info.ProcedureStatus;
                    break;
            }
        }

        /// <summary>
        /// 连接所有元器件
        /// </summary>
        private void Connected()
        {
            var task = Task.Run(() =>
            {
                _deviceManager.ConnectedPlatform();
            });
        }


        #region 原来使用的屏幕按钮的执行代码，现在移动到了procedureManager
        //private void Emergency()
        //{
        //    _deviceManager.Axes.All().ForEach(o => o.EmergencyStop());
        //}

        //private void Start()
        //{
        //    switch (_status)
        //    {
        //        case WorkStatus.Emergency:
        //            Log.Information("进入急停状态");
        //            throw new InvalidOperationException("急停状态下，必须先复位才能再次启动");
        //        case WorkStatus.Terminated:
        //            Log.Information("纳米压印启动");
        //            _workDoneEvent.Wait();
        //            Status = WorkStatus.Running;
        //            StartContent = "暂停";
        //            ThreadPool.QueueUserWorkItem(o => StartLoop());
        //            break;
        //        case WorkStatus.Running:
        //            Log.Information("纳米压印暂停");
        //            Status = WorkStatus.Paused;
        //            StartContent = "启动";
        //            return;
        //        case WorkStatus.Paused:
        //            Status = WorkStatus.Running;
        //            break;
        //    }
        //}

        //private void Reset()
        //{
        //    Log.Information("复位，所有任务终止");
        //    Status = WorkStatus.Terminated;

        //    _deviceManager.Axes.All().ForEach(o => o.ResetAlarm());
        //}

        //private void GoHome()
        //{
        //    Log.Information("系统开始回零");
        //    Status = WorkStatus.Terminated;
        //    var goHomeTask = Task.Run(() =>
        //    {
        //        //先脱模
        //        var microPlatform = _deviceManager.GetPlatform(typeof(MicroPlatform).Name) as MicroPlatform;
        //        microPlatform.Demold();

        //        foreach (var plate in _deviceManager.Platforms)
        //            plate.Value.GoHome();
        //        Log.Information("系统回零完成");
        //    });
        //}

        //private void Vacuum()
        //{
        //    Log.Information("真空吸气");
        //}

        //private void Evacuate()
        //{
        //    Log.Information("真空吹气");
        //}


        ///// <summary>
        ///// 开始执行自动流程
        ///// </summary>
        //private void StartLoop()
        //{
        //    try
        //    {
        //        //放wafe
        //        Log.Information("放Wafe");
        //        _procedureManager.ExcuteProcedureByName(typeof(LoadProcedure).Name);
        //        //定位wafe圆心
        //        Log.Information("定位Wafe圆心");
        //        _procedureManager.ExcuteProcedureByName(typeof(FindRotateCenterProcedure).Name);
        //        //定位初次压印位置
        //        Log.Information("定位Wafe初次压印位置");
        //        _procedureManager.ExcuteProcedureByName(typeof(PositionProcedure).Name);

        //        while (true)
        //        {
        //            switch (_status)
        //            {
        //                case WorkStatus.Emergency:
        //                    return;
        //                case WorkStatus.Terminated:
        //                    return;
        //                case WorkStatus.Running:
        //                    SingleLoop();        //开始执行单个图案的压印
        //                    break;
        //                case WorkStatus.Paused:
        //                    Thread.Sleep(10);
        //                    break;
        //                default:
        //                    throw new Exception("未知");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Information(ex.Message);
        //        throw ex;
        //    }
        //    finally
        //    {
        //        Log.Information("任务结束");
        //        _workDoneEvent.Release();
        //        _status = WorkStatus.Terminated;
        //    }
        //}


        ///// <summary>
        ///// 单次循环
        ///// </summary>
        //private void SingleLoop()
        //{
        //    //达到压印次数，则终止任务
        //    if (CurrentIndex >= ImprintCount)
        //    {
        //        Status = WorkStatus.Terminated;
        //        //StartContent = "启动";
        //        return;
        //    }

        //    //单个图案的压印
        //    while ( _deviceManager.ProcedureIndex < _procedureManager.AutoProcedures.Count)
        //    {
        //        var result = _procedureManager.ExecuteAutoProcedureByIndex(_deviceManager.ProcedureIndex);

        //        if (result)
        //        {
        //            //记录当前流程的步骤，报错关机后下次继续执行
        //            _deviceManager.ProcedureIndex++;

        //            //结束流程
        //            if (_deviceManager.ProcedureIndex == _procedureManager.AutoProcedures.Count)
        //            {
        //                _deviceManager.ProcedureIndex = 0;

        //                //压印图案计数; 计数条件待定。
        //                CurrentIndex++;
        //                CurrentRow = CurrentIndex / ImprintCol;
        //                CurrentCol = CurrentIndex % ImprintCol;

        //                break;
        //            } 
        //        }
        //        else
        //        {
        //            var name = _procedureManager.GetAutoProcedureName(_deviceManager.ProcedureIndex);
        //            throw new Exception($"流程{name}未正确执行完");
        //        }
        //    }
        //}
        #endregion

    }
}
