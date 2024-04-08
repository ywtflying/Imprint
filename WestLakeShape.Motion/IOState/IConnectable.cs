using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WestLakeShape.Motion
{
    public interface IConnectable
    {
        bool IsConnected { get; }

        void Connect();

        void Disconnect();
    }

    public abstract class Connectable:IConnectable
    {
        public abstract string Name { get; }

        public bool IsConnected { get; private set; }


        public void Connect()
        {
            if (!IsConnected)
            {
                OnConnecting();
                IsConnected = true;
                ThreadPool.QueueUserWorkItem(_ => TickProc());
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                OnDisconnecting();
                IsConnected = false;
            }
        }


        private void TickProc()
        {
            while (IsConnected)
            {
                RefreshStates();
                Thread.Sleep(1);
            }
        }


        protected virtual void OnConnecting()
        {

        }
        protected virtual void OnDisconnecting()
        { 

        }

        protected abstract void RefreshStates();
    }

}
