using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WestLakeShape.Common;

namespace WestLakeShape.Motion
{
    public static class StateExtension
    {

        /// <summary>
        /// 等待直到可刷新的布尔状态变为 true
        /// </summary>
        public static async Task AsyncWait(this IOStateSource.IOState state)
        {
            while (!state.Get())
            {
                await Task.Delay(Constants.DelayInterval).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// 等待直到状态变成指定值
        /// </summary>
        public static async Task AsyncWait(this IOStateSource.IOState state, bool value)
        {
            while (!state.Get().Equals(value))
            {
                await Task.Delay(Constants.DelayInterval).ConfigureAwait(false);
            }
        }


        public static void SyncWait(this IOStateSource.IOState state)
        {
            while (!state.Get())
            {
                Thread.Sleep(Constants.DelayInterval);
            }
        }
        public static void SyncWait(this IOStateSource.IOState state, bool value)
        {
            while (!state.Get().Equals(value))
            {
                Thread.Sleep(Constants.DelayInterval);
            }
        }
    }
}
