using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace WestLakeShape.Motion
{
    public class TcpChannel : ModbusSlaveChannel
    {
        private int _transactionId = 1;
        private readonly TcpEndPoint _config;
        private readonly TcpClient _client;

        public override bool IsConnected => _client.Connected;

        public override ushort HeadLength => 7;

        public override ushort TailLength => 0;

        public TcpChannel(TcpEndPoint config)
        {
            _config = config;
            _client = new TcpClient();
        }

        public override Stream Connect()
        {
            _client.Connect(_config.IpAddress, _config.Port);
            return _client.GetStream();
        }

        public override void Disconnect()
        {
            _client.Close();
        }

        public override ushort PrepareRequest(byte[] requestAdu, byte slaveID)
        {
            var id = Interlocked.Increment(ref _transactionId);
            var length = requestAdu.Length - 6;
            requestAdu[0] = (byte)((id >> 8) & 0xff);
            requestAdu[1] = (byte)(id & 0xff);
            requestAdu[2] = 0;
            requestAdu[3] = 0;
            requestAdu[4] = (byte)((length >> 8) & 0xff);
            requestAdu[5] = (byte)(length & 0xff);
            requestAdu[6] = slaveID;

            return unchecked((ushort)id);
        }

        public override ushort PrepareResponse(byte[] responseAdu)
        {
            return (ushort)((responseAdu[0] << 8) | responseAdu[1]);
        }
    }

    public class TcpEndPoint
    {
        /// <summary>
        /// IP 地址
        /// </summary>
        [Category("TCP/IP"), Description("IP 地址"), DefaultValue("192.168.1.200")]
        [RefreshProperties(RefreshProperties.All)]
        [DisplayName("IP 地址")]
        public string IpAddress { get; set; } = "192.168.1.200";

        /// <summary>
        /// TCP 端口
        [Category("TCP/IP"), Description("TCP 通信端口"), DefaultValue(502)]
        [RefreshProperties(RefreshProperties.All)]
        [DisplayName("通信端口"), Range(1, 65535)]
        public int Port { get; set; } = 502;


        public override string ToString()
        {
            return $"{IpAddress}:{Port}";
        }
    }
}
