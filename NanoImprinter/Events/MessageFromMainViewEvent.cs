using NanoImprinter.Procedures;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoImprinter.Events
{
    public class ProcedureInfoEvent:PubSubEvent<ProcedureInfo>
    {
        
    }

    public class ProcedureInfo
    {
        public string Name { get; set; }
        public ProcedureStatus ProcedureStatus { get; set; }
    }
}
