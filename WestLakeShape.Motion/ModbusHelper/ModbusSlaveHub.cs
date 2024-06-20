using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WestLakeShape.Motion
{
    public class ModbusSlaveHub : IModbusSlaveHub
    {
        private readonly Dictionary<int, List<PendingRequest>> _pendingTasks = new Dictionary<int, List<PendingRequest>>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        protected readonly ModbusSlaveChannel _channel;
        protected readonly ModbusHubConfig _config;
        protected readonly int _pduOffset;
        protected Stream _stream;
        private bool _closing;


        public ModbusSlaveHub(ModbusHubConfig config)
        {

            _config = config;
            _channel = _config.Channel == ModbusChannel.RTU
                ? new RtuChannel(_config.RtuConfig)
                : new TcpChannel(_config.TcpConfig) as ModbusSlaveChannel;
            _pduOffset = _channel.HeadLength;
        }


        public string Name => _config.Channel == ModbusChannel.RTU
            ? _config.RtuConfig.PortName
            : $"{_config.TcpConfig.IpAddress}:{_config.TcpConfig.Port:d}";

        public string Key => _config.GetKey();

        /// <summary>
        /// 消息字节数组中, PDU所处位置(即Function Code的索引)
        /// </summary>
        public int PduOffset => _pduOffset;


        public void OnConnecting()
        {
            Open();
        }


        public void OnDisconnecting()
        {
            Close();
        }

        private void Open()
        {
            _stream = _channel.Connect();

            ThreadPool.QueueUserWorkItem(_ => ProcessIncomingData().GetAwaiter().GetResult());
        }

        private void Close()
        {
            if (_closing || _stream == null)
                return;

            _closing = true;
            try
            {
                _channel.Disconnect();
                ClearPendingTasks();
            }
            catch (Exception exception)
            {
                if (!HandleException(exception))
                    throw;
            }
            finally
            {
                _closing = false;
            }
        }

        private bool HandleException(Exception exception)
        {
            if (_closing || !_channel.IsConnected)
            {
                if (exception is IOException ||
                    exception is SocketException ||
                    exception is ObjectDisposedException |
                    exception is OperationCanceledException ||
                    exception is InvalidOperationException)
                    return true;
            }

            return false;
        }


        public Task WriteSingleRegister(byte slaveId, ushort address, ushort value)
        {
            var request = CreateRequest(5);
            request[_pduOffset + 0] = FunctionCodes.WriteSingleHoldingRegister;
            request[_pduOffset + 1] = (byte)(address >> 8);
            request[_pduOffset + 2] = (byte)(address & 0xff);
            request[_pduOffset + 3] = (byte)(value >> 8);
            request[_pduOffset + 4] = (byte)(value & 0xff);

            return SendRequest(slaveId, request);
        }

        public async Task<ushort[]> ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort count)
        {
            int offset;
            var response = await ReadHoldingRegistersRaw(slaveId, startAddress, count, out offset).ConfigureAwait(false);
            var result = new ushort[count];
            for (var i = 0; i < count; i++)
                result[i] = (ushort)((response[i * 2 + offset] << 8) + response[i * 2 + offset + 1]);

            return result;
        }

        public Task<byte[]> ReadHoldingRegistersRaw(byte slaveId, ushort startAddress, ushort count, out int dataOffset)
        {
            var request = CreateRequest(5);
            request[_pduOffset + 0] = FunctionCodes.ReadHoldingRegisters;
            request[_pduOffset + 1] = (byte)(startAddress >> 8);
            request[_pduOffset + 2] = (byte)(startAddress & 0xff);
            request[_pduOffset + 3] = (byte)(count >> 8);
            request[_pduOffset + 4] = (byte)(count & 0xff);

            // 0: Function Code, 1: Byte Count
            dataOffset = _pduOffset + 2;

            return SendRequest(slaveId, request);
        }

        public async Task<bool[]> ReadCoils(byte slaveId, ushort startAddress, ushort count)
        {
            int offset;
            var response = await ReadCoilsRaw(slaveId, startAddress, count, out offset);
            var result = new bool[count];
            for (var i = 0; i < count; i++)
            {
                var byteIndex = offset + (i / 8);
                var bitIndex = i % 8;

                result[i] = 0 != (response[byteIndex] & (1 << bitIndex));
            }

            return result;
        }

        public Task<byte[]> ReadCoilsRaw(byte slaveId, ushort startAddress, ushort count, out int dataOffset)
        {
            var request = CreateRequest(5);
            request[_pduOffset + 0] = FunctionCodes.ReadCoils;
            request[_pduOffset + 1] = (byte)(startAddress >> 8);
            request[_pduOffset + 2] = (byte)(startAddress & 0xff);
            request[_pduOffset + 3] = (byte)(count >> 8);
            request[_pduOffset + 4] = (byte)(count & 0xff);

            // 0: Function Code 1: Byte Count
            dataOffset = _pduOffset + 2;

            return SendRequest(slaveId, request);
        }

        public Task WriteSingleCoil(byte slaveId, ushort address, bool value)
        {
            var request = CreateRequest(5);
            request[_pduOffset + 0] = FunctionCodes.WriteSingleCoil;
            request[_pduOffset + 1] = (byte)(address >> 8);
            request[_pduOffset + 2] = (byte)(address & 0xff);
            request[_pduOffset + 3] = value ? (byte)0xff : (byte)0x00;
            request[_pduOffset + 4] = 0x00;

            return SendRequest(slaveId, request);
        }

        public Task WriteCoils(byte slaveId, ushort startAddress, IReadOnlyList<bool> values)
        {
            var count = values.Count;
            var dataBytes = (count + 7) / 8;
            var data = new byte[dataBytes];
            for (var i = 0; i < dataBytes; i++)
            {
                var value = 0;
                for (var bit = 0; bit < 8; bit++)
                {
                    var index = i * 8 + bit;
                    if (index >= values.Count)
                        break;
                    if (values[index])
                        value |= 1 << bit;
                }
                data[i] = (byte)value;
            }

            return WriteCoilsRaw(slaveId, startAddress, count, data, 0);
        }

        public Task WriteCoilsRaw(byte slaveId, ushort startAddress, int count, byte[] data, int offset)
        {
            var dataBytes = (count + 7) / 8;
            var request = CreateRequest(6 + dataBytes);
            request[_pduOffset + 0] = FunctionCodes.WriteCoils;
            request[_pduOffset + 1] = (byte)(startAddress >> 8);
            request[_pduOffset + 2] = (byte)(startAddress & 0xff);
            request[_pduOffset + 3] = (byte)(count >> 8);
            request[_pduOffset + 4] = (byte)(count & 0xff);
            request[_pduOffset + 5] = (byte)dataBytes;
            for (var i = 0; i < dataBytes; i++)
                request[_pduOffset + 6 + i] = data[offset + i];

            return SendRequest(slaveId, request);
        }

        public async Task<bool[]> ReadDiscreteInputs(byte slaveId, ushort startAddress, ushort count)
        {
            int offset;
            var response = await ReadDiscreteInputsRaw(slaveId, startAddress, count, out offset).ConfigureAwait(false);
            var result = new bool[count];
            for (var i = 0; i < count; i++)
            {
                var byteIndex = offset + (i / 8);
                var bitIndex = i % 8;

                result[i] = 0 != (response[byteIndex] & (1 << bitIndex));
            }

            return result;
        }

        public Task<byte[]> ReadDiscreteInputsRaw(byte slaveId, ushort startAddress, ushort count, out int dataOffset)
        {
            var request = CreateRequest(5);
            request[_pduOffset + 0] = FunctionCodes.ReadDiscreteInputs;
            request[_pduOffset + 1] = (byte)(startAddress >> 8);
            request[_pduOffset + 2] = (byte)(startAddress & 0xff);
            request[_pduOffset + 3] = (byte)(count >> 8);
            request[_pduOffset + 4] = (byte)(count & 0xff);

            // 0: Function Code 1: Byte Count
            dataOffset = _pduOffset + 2;
            return SendRequest(slaveId, request);
        }

        public Task WriteRegisters(byte slaveId, ushort startAddress, IReadOnlyList<ushort> values)
        {
            var data = new byte[values.Count * 2];
            for (var i = 0; i < values.Count; i++)
            {
                data[i * 2] = (byte)(values[i] >> 8);
                data[i * 2 + 1] = (byte)(values[i] & 0xff);
            }

            return WriteRegistersRaw(slaveId, startAddress, values.Count, data, 0);
        }

        public Task WriteRegistersRaw(byte slaveId, ushort startAddress, int count, byte[] data, int offset)
        {
            var dataBytes = count * 2;
            var request = CreateRequest(6 + dataBytes);
            request[_pduOffset + 0] = FunctionCodes.WriteRegisters;
            request[_pduOffset + 1] = (byte)(startAddress >> 8);
            request[_pduOffset + 2] = (byte)(startAddress & 0xff);
            request[_pduOffset + 3] = (byte)(count >> 8);
            request[_pduOffset + 4] = (byte)(count & 0xff);
            request[_pduOffset + 5] = (byte)dataBytes;
            for (var i = 0; i < dataBytes; i++)
                request[_pduOffset + 6 + i] = data[offset + i];

            return SendRequest(slaveId, request);
        }

        public async Task<ushort[]> ReadInputRegisters(byte slaveId, ushort startAddress, ushort count)
        {
            int offset;
            var response = await ReadInputRegistersRaw(slaveId, startAddress, count, out offset).ConfigureAwait(false);
            var result = new ushort[count];

            for (var i = 0; i < count; i++)
                result[i] = (ushort)((response[i * 2 + offset] << 8) + response[i * 2 + offset + 1]);

            return result;
        }

        public Task<byte[]> ReadInputRegistersRaw(byte slaveId, ushort startAddress, ushort count, out int dataOffset)
        {
            var request = CreateRequest(5);
            request[_pduOffset + 0] = FunctionCodes.ReadInputRegisters;
            request[_pduOffset + 1] = (byte)(startAddress >> 8);
            request[_pduOffset + 2] = (byte)(startAddress & 0xff);
            request[_pduOffset + 3] = (byte)(count >> 8);
            request[_pduOffset + 4] = (byte)(count & 0xff);

            // 0: Function Code 1: Byte Count
            dataOffset = _pduOffset + 2;
            return SendRequest(slaveId, request);
        }


        private byte[] CreateRequest(int pduLength)
        {
            return new byte[_pduOffset + pduLength + _channel.TailLength];
        }

        protected async Task<byte[]> SendRequest(byte slaveId, byte[] request)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            var tid = _channel.PrepareRequest(request, slaveId);
            var task = RegisterRequest(tid, request);
            try
            {
                await _stream.WriteAsync(request, 0, request.Length).ConfigureAwait(false);
                if (_config.OperationDelay > 0)
                    await Task.Delay(_config.OperationDelay).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (!HandleException(exception))
                    throw;
            }
            finally
            {
                _semaphore.Release();
            }

            return await task.ConfigureAwait(false);
        }


        private async Task ProcessIncomingData()
        {
            var s = _stream;
            try
            {
                await ReadPortData().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                if (!HandleException(exception))
                    throw;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 循环读取串口数据
        /// </summary>
        private async Task ReadPortData()
        {
            var buffer = new byte[1024];
            var left = 0;
            var right = 0;
            while (true)
            {
                var count = buffer.Length - right;
                if (count <= 0)
                    throw new BufferFullException();

                int read;
                try
                {
                    read = await _stream.ReadAsync(buffer, right, count).ConfigureAwait(false);
                }
                catch (IOException ex)
                {
                    // IO Abort, Port Closing/Disconnecting
                    if (ex.HResult == unchecked((int)0x800703e3))
                        read = 0;
                    else
                        throw;
                }

                if (read == 0)
                    break;
                right += read;

                if (buffer.Length - right < 256)
                {
                    Array.Copy(buffer, left, buffer, 0, right - left);
                    right -= left;
                    left = 0;
                }

                var faulted = false;
                while (true)
                {
                    try
                    {
                        if (!TryTakeMessage(buffer, ref left, right))
                            break;
                    }
                    catch (UnexpectedValueException)
                    {
                        faulted = true;
                        break;
                    }
                    catch (CrcValidateException)
                    {
                        faulted = true;
                        break;
                    }
                }

                if (faulted)
                {
                    ClearPendingTasks();

                    left = 0;
                    right = 0;
                }
            }

            ClearPendingTasks();
        }

        protected void ClearPendingTasks()
        {
            List<PendingRequest> pendings;

            lock (_pendingTasks)
            {
                pendings = _pendingTasks.SelectMany(x => x.Value).ToList();
                _pendingTasks.Clear();
            }

            foreach (var pending in pendings)
            {
                pending.TaskCompletionSource.TrySetCanceled();
            }
        }

        /// <summary>
        /// 试图从缓冲区取出一条完整的回复消息
        /// </summary>
        private bool TryTakeMessage(byte[] buffer, ref int start, int stop)
        {
            var offset = _pduOffset;
            var tail = _channel.TailLength;

            var minLength = offset + tail + 1;
            if (stop - start < minLength)
                return false;

            var functionCode = buffer[start + offset];
            int messageLength;
            if (functionCode < 0x80)
            {
                // No exception
                switch (functionCode)
                {
                    case FunctionCodes.ReadHoldingRegisters:
                    case FunctionCodes.ReadCoils:
                    case FunctionCodes.ReadDiscreteInputs:
                    case FunctionCodes.ReadInputRegisters:
                        {
                            // 0: Function Code, 1: Byte Count, 2+ Data
                            var byteCount = buffer[start + offset + 1];
                            messageLength = offset + 2 + byteCount + tail;
                        }
                        break;
                    case FunctionCodes.WriteSingleHoldingRegister:
                    case FunctionCodes.WriteSingleCoil:
                        {
                            // 0: Function Code, 1-2: Register Address, 3-4: Data Value
                            messageLength = offset + 5 + tail;
                        }
                        break;
                    case FunctionCodes.WriteCoils:
                    case FunctionCodes.WriteRegisters:
                        {
                            // 0: Function Code, 1-2: Start Address, 3-4: Data Count
                            messageLength = offset + 5 + tail;
                        }
                        break;
                    default:
                        throw new UnexpectedValueException(functionCode);
                }
            }
            else
            {
                functionCode -= 0x80;
                switch (functionCode)
                {
                    case FunctionCodes.ReadCoils:
                    case FunctionCodes.WriteSingleCoil:
                    case FunctionCodes.WriteCoils:

                    case FunctionCodes.ReadDiscreteInputs:

                    case FunctionCodes.ReadHoldingRegisters:
                    case FunctionCodes.WriteSingleHoldingRegister:
                    case FunctionCodes.WriteRegisters:

                    case FunctionCodes.ReadInputRegisters:
                        // 0: Error Code, 1: Exception Code
                        messageLength = offset + 2 + tail;
                        break;

                    default:
                        throw new UnexpectedValueException(functionCode);
                }
            }

            if (messageLength > 256)
                throw new UnexpectedValueException(messageLength);

            if (0 < messageLength && messageLength <= stop - start)
            {
                var message = new byte[messageLength];
                Array.Copy(buffer, start, message, 0, messageLength);

                start += messageLength;

                DisptachResponse(message);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 将请求添加到未完成的任务中
        /// </summary>
        private Task<byte[]> RegisterRequest(ushort transactionId, byte[] request)
        {
            var pending = new PendingRequest();

            lock (_pendingTasks)
            {
                List<PendingRequest> list;
                if (!_pendingTasks.TryGetValue(transactionId, out list))
                {
                    list = new List<PendingRequest>();
                    _pendingTasks.Add(transactionId, list);
                }
                list.Add(pending);
                return pending.TaskCompletionSource.Task;
            }
        }

        /// <summary>
        /// 将回复交给先前的请求。假设消息 ID (由 slave address 和 function code 组成)相同的请求，先发送的先被回复
        /// </summary>
        protected void DisptachResponse(byte[] response)
        {
            var id = _channel.PrepareResponse(response);
            PendingRequest pending = null;
            lock (_pendingTasks)
            {
                List<PendingRequest> list;
                if (_pendingTasks.TryGetValue(id, out list) &&
                    list.Count > 0)
                {
                    pending = list[0];
                    list.RemoveAt(0);
                }
            }

            if (pending != null)
            {
                if (response[_pduOffset] < 0x80)
                {
                    pending.TaskCompletionSource.TrySetResult(response);
                }
                else
                {
                    var exceptionCode = response[_pduOffset + 1];
                    pending.TaskCompletionSource.TrySetException(new ModbusResponseException(exceptionCode));
                }
            }
            else
            {
                Debug.WriteLine("忽略串口回复: " + string.Join(" ", response.Select(x => x.ToString("x2"))));
            }
        }

        public Task RefreshStates()
        {
            return Task.CompletedTask;
        }

        public void OnTicking()
        {
            if (_config.ResponseTimeout > 0)
            {
                var utcNow = DateTime.UtcNow;
                var threshold = utcNow.AddMilliseconds(-_config.ResponseTimeout);
                var expiredRequests = new List<PendingRequest>();
                lock (_pendingTasks)
                {
                    // 清理超时请求
                    foreach (var list in _pendingTasks.Values)
                    {
                        for (var i = 0; i < list.Count; i++)
                        {
                            var pending = list[i];
                            // 找到第一个未超时的请求后，在它后面的更不会超时
                            if (pending.SendTimeUtc > threshold)
                                break;

                            expiredRequests.Add(pending);
                            list.RemoveAt(i);
                            i--;
                        }
                    }
                }

                foreach (var request in expiredRequests)
                {
                    request.TaskCompletionSource.TrySetException(new TimeoutException());
                }
            }
        }


        /// <summary>
        /// 未完成（被回复）的请求
        /// </summary>
        class PendingRequest
        {
            public PendingRequest()
            {
                // DateTime.UtcNow 比 DateTime.Now 执行速度快
                SendTimeUtc = DateTime.UtcNow;
                TaskCompletionSource = new TaskCompletionSource<byte[]>();
            }

            public TaskCompletionSource<byte[]> TaskCompletionSource { get; private set; }

            public DateTime SendTimeUtc { get; private set; }
        }
    }
    public enum ModbusChannel
    {
        RTU,
        TCP
    }

    public class ModbusHubConfig
    {
        [Category("Modbus"), Description("设备连接方式"), DefaultValue(ModbusChannel.RTU)]
        [RefreshProperties(RefreshProperties.All)]
        [DisplayName("连接方式")]
        public ModbusChannel Channel { get; set; } = ModbusChannel.RTU;

        /// <summary>
        /// 写操作后等待时间（毫秒）
        /// </summary>
        [Category("通信"), Description("写操作后等待时间（毫秒）"), DefaultValue(5)]
        [DisplayName("写入延迟"), Range(0, 10000)]
        public int OperationDelay { get; set; } = 5;

        /// <summary>
        /// 请求发送后，等待回复超时时间（毫秒）
        /// </summary>
        [Category("通信"), Description("请求发送后，等待回复超时时间（毫秒）"), DefaultValue(2000)]
        [Display(Name = "回复超时"), Range(1, 100000)]
        public int ResponseTimeout { get; set; } = 2000;

        /// <summary>
        /// 设备端口配置
        /// </summary>
        [Category("通信"), Description("设备端口配置，仅在 RTU 方式下生效")]
        [Display(Name = "RTU 配置")]
        public PortConfig RtuConfig { get; set; } = new PortConfig();

        /// <summary>
        /// TCP 地址配置
        /// </summary>
        [Category("通信"), Description("TCP 通信配置，仅在 TCP 方式下生效")]
        [Display(Name = "TCP 配置")]
        public TcpEndPoint TcpConfig { get; set; } = new TcpEndPoint();

        public override string ToString()
        {
            return GetKey();
        }

        public string GetKey()
        {
            return Channel == ModbusChannel.RTU
                ? RtuConfig.PortName
                : $"{TcpConfig.IpAddress}:{TcpConfig.Port:d}";
        }
    }
}
