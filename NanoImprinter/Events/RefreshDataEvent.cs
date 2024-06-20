using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoImprinter.Events
{
    /// <summary>
    /// 不同ViewModel订阅刷新事件，通过主界面的定时器调用ViewModel中需要刷新的方法。
    /// 保证只有一个定时器
    /// </summary>
    public class RefreshDataEvent : PubSubEvent<IORefreshEventArgs>
    {
    }

    public class IORefreshEventArgs
    {
        public string Name { get; set; }
        // 可以添加更多属性
    }
}
