using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WestLakeShape.Common.LogService
{
    public class LogHelper
    {
        public static ILogger For<T>()=>Log.ForContext<T>();
    }
}
