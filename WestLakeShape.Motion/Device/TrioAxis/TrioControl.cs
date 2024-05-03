using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrioMotion.TrioPC_NET;
using WestLakeShape.Common.LogService;

namespace WestLakeShape.Motion.Device
{
    public class TrioControl : IDisposable
    {
        private static readonly Lazy<TrioControl> _instance = new Lazy<TrioControl>(() => new TrioControl());
        private static TrioPC _trioPC;
        private bool _isConnected;
        private string _ip = "192.168.0.250";
        private static readonly ILogger _log = LogHelper.For<TrioAxis>();
        public static TrioControl Instance => _instance.Value;

        public TrioPC TrioPC => _trioPC;

        public bool IsConnected => _isConnected;
        public bool IsError => _trioPC.IsError();

        private TrioControl()
        {
           _trioPC = new TrioPC();
        }

        public void Dispose()
        {
            if (_trioPC.IsOpen(PortId.Default))
                _trioPC.Close();
        }

        public bool Connect()
        {
            _trioPC.HostAddress = _ip;
            _isConnected = _trioPC.Open(PortType.Ethernet, PortId.Default);
            if (_isConnected)
            {
                _log.Information("控制器连接成功！");
                AllACAxesEnable();
                
                //前14个io信号为直流电机限位信号，需要取反
                for (int i = 0; i < 14; i++)
                {
                    _trioPC.InvertIn(i, 1);
                }
            }
            else {
                _log.Information("控制器连接失败！");
            }

            return _isConnected;
        }

        public bool Disconnect()
        {
            if (_trioPC.IsOpen(PortId.Default))
            {
                _trioPC.Close(PortId.Default);
                _isConnected = _trioPC.IsOpen(PortId.Default);
            }
          
            _log.Information("控制器断开！");
            
            return _isConnected;
        }

        private void AllACAxesEnable()
        {
            var ret = _trioPC.SetVariable(TrioParamName.Enable, 1);
            if (!ret)
                _log.Information("所有伺服上电失败！");
            else
                _log.Information("所有伺服上电！");
        }

        private void AllACAxesDisable()
        {
            var ret = _trioPC.SetVariable(TrioParamName.Enable, 0);
            if (!ret)
                _log.Information("所有伺服下电失败！");
            else
                _log.Information("所有伺服下电！");
        }
        //public bool SetIP(string ip)
        //{
        //    _ip = ip;
        //    _isConnected = false;
        //    Connected();
        //    return _isConnected;
        //}

        public int GetLastError()
        {
            return _trioPC.GetLastError();
        }

        public void EmegercyStop()
        {
            _trioPC.RapidStop();
        }
    }
}
