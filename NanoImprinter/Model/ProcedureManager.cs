using NanoImprinter.Procedures;
using NanoImprinter.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NanoImprinter.Model
{
    public class ProcedureManager
    {
        /// <summary>
        /// 考虑Load/Unload增加工序
        /// </summary>
        private readonly Dictionary<string, WorkProcedure> _procedures;
        private List<WorkProcedure> _autoProcedures;
        private readonly IDeviceManager _machine;
        private IEventAggregator _eventAggregator;
        public int AutoPorcedureCount => _autoProcedures.Count;
        public List<WorkProcedure> AutoProcedures => _autoProcedures;
        
        public ProcedureManager(IDeviceManager machine, IEventAggregator eventAggregator)
        {
            _machine = machine;
            _eventAggregator = eventAggregator;
            _autoProcedures = new List<WorkProcedure>();
            _procedures = new Dictionary<string, WorkProcedure>();
            RegisterProcesure();
        }

        private void RegisterProcesure()
        {
            _autoProcedures.Add(new GlueProcedure(_machine, _eventAggregator));
            _autoProcedures.Add(new PreprintProcedure(_machine, _eventAggregator));
            _autoProcedures.Add(new PositionProcedure(_machine, _eventAggregator));
            _autoProcedures.Add(new ImprintProcedure(_machine, _eventAggregator));
            _autoProcedures.Add(new UVCurePorcedure(_machine, _eventAggregator));
            _autoProcedures.Add(new DemoldProcedure(_machine, _eventAggregator));

            _autoProcedures.ForEach(o => AddProcedures(o));

            AddProcedures(new LoadProcedure(_machine, _eventAggregator));
            AddProcedures(new FindRotateCenterProcedure(_machine, _eventAggregator));
            AddProcedures(new CalibProcedure(_machine, _eventAggregator));
            AddProcedures(new PutProcesure(_machine, _eventAggregator));
        }

        private void AddProcedures(WorkProcedure procedure)
        {
            _procedures.Add(procedure.GetType().Name, procedure);
        }

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

        public string GetProcedureName(int procedureIndex)
        {
            return _autoProcedures[procedureIndex].Name;
        }
        public void StopProcedure()
        {
            foreach (var kvp in _procedures)
            {
                kvp.Value.Status = WorkStatus.Terminated;
            }
        }
        public void PauseProcedure()
        {
            foreach (var kvp in _procedures)
            {
                kvp.Value.Status = WorkStatus.Paused;
            }
        }
    }
}
