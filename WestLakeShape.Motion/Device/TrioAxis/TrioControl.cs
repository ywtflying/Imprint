using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public bool IsConnected => _trioPC.IsOpen(PortId.Default);
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
            var isOK = CheckControlStatue();          
            if (_isConnected&&isOK)
            {
                _log.Information("控制器连接成功！");
                AllACAxesEnable();
                
                //前14个io信号为直流电机限位信号，需要取反
                for (int j = 0; j < 14; j++)
                {
                    _trioPC.InvertIn(j, 1);
                }
            }
            else {
                _log.Information($"控制器连接状态{_isConnected};控制器工作状态{isOK}");
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

        private bool CheckControlStatue()
        {
            double state = 1;
            _trioPC.SetVr(0, -1);
            //将EthercAT的状态返回到VR(0)中。EtherCAT指令详见TriO BASIC
            _trioPC.Execute("ETHERCAT($22,0,0)");
            _trioPC.GetVr(0, out state);
            int i = 0;
            while (state != 3 && i < 3)//控制器未连接驱动器，则重新初始化EC
            {
                _trioPC.Execute("ETHERCAT(0,0)");//重新初始化EC"
                Thread.Sleep(3000);
                _trioPC.Execute("ETHERCAT($22, 0, 0)");
                _trioPC.GetVr(0, out state);
                i++;
            }
            //3正常，否则异常
            return state == 3 ? true : false;
        }

        private void AllACAxesEnable()
        {
            var ret = _trioPC.SetVariable(TrioParamName.Enable, 1);
            if (!ret)
            {
                _log.Information("伺服不能上电！");
                throw new Exception("轴上电总开关打开失败");
            }               
            else
                _log.Information("伺服开始上电！");
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
