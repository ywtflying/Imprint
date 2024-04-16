using NanoImprinter.Events;
using NanoImprinter.Model;
using NanoImprinter.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NanoImprinter.Procedures
{
    public interface IWorkProcedure
    {
        string Name { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        TimeSpan Deration { get; }
        WorkStatus Status { get; set; }
        bool Execute();
    }

    public abstract class WorkProcedure : IWorkProcedure
    {
        private DateTime _startTime;
        private DateTime _endTime;
        

        protected string _name;
        protected IDeviceManager _machine;
        protected WorkStatus _workStatus;
        protected readonly IEventAggregator _eventAggregator;

        public string Name => _name;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _endTime;
        public TimeSpan Deration => _endTime - _startTime;

        public WorkStatus Status
        {
            get => _workStatus;
            set => _workStatus = value;
        }

        public WorkProcedure(IDeviceManager machine,IEventAggregator eventAggregator)
        {
            _machine = machine;
            _eventAggregator = eventAggregator;
        }

        public bool Execute()
        {
        
            if (!CheckWorkStatus())
                return false;
            
            _startTime = DateTime.Now;
            RaiseProcedureResult(Name, ProcedureStatus.Running);
            try
            {
                Prepare();
                var result = OnExecute();
                if (result)
                {
                    _endTime = DateTime.Now;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                RaiseProcedureResult(Name, ProcedureStatus.Failed);
                throw new Exception($"{_name}：{ex.Message}");
            }
        }

        /// <summary>
        /// 检查机器当前运行状态
        /// </summary>
        /// <returns></returns>
        protected bool CheckWorkStatus()
        {
            if (_workStatus == WorkStatus.Emergency || _workStatus == WorkStatus.Terminated)
            {
                RaiseProcedureResult(Name, ProcedureStatus.Cancelled);
                return false;
            }
               
            else if (_workStatus == WorkStatus.Paused)
            {
                RaiseProcedureResult(Name, ProcedureStatus.Stopped);
                do
                {
                    Thread.Sleep(10);
                }
                while (_workStatus == WorkStatus.Paused);
            }
            RaiseProcedureResult(Name, ProcedureStatus.Running);
            return true;
        }

        protected void RaiseProcedureResult(string procedureName, ProcedureStatus status)
        {
            var procedureInfo = new ProcedureInfo() { Name = procedureName, ProcedureStatus = status };
            _eventAggregator.GetEvent<ProcedureInfoEvent>().Publish(procedureInfo);
        }

        protected abstract bool Prepare();

        protected abstract bool OnExecute();

    }

    public enum ProcedureStatus
    {
        None,
        Stopped,
        Running,
        Succeeded,
        Failed,
        Cancelled
    }
}
