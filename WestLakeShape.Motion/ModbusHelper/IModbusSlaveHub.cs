using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WestLakeShape.Motion
{
    public interface IModbusSlaveHub
    {
        string Key { get; }

        /// <summary>
        /// 1.读取线圈状态(bool Input/Output)
        /// </summary>
        Task<bool[]> ReadCoils(byte slaveId, ushort startAddress, ushort count);
        /// <summary>
        /// 1.读取线圈状态(bool Input/Output)，返回原始回复（字节数组）
        /// </summary>
        Task<byte[]> ReadCoilsRaw(byte slaveId, ushort startAddress, ushort count, out int dataOffset);
        /// <summary>
        /// 2.读取输入线圈状态(bool Input)
        /// </summary>
        Task<bool[]> ReadDiscreteInputs(byte slaveId, ushort startAddress, ushort count);
        /// <summary>
        /// 2.读取输入线圈状态(bool Input)，返回原始回复（字节数组）
        /// </summary>
        Task<byte[]> ReadDiscreteInputsRaw(byte slaveId, ushort startAddress, ushort count, out int dataOffset);
        /// <summary>
        /// 3.读取多个保持寄存器
        /// </summary>
        /// <exception cref="IOException">端口关闭</exception>
        /// <exception cref="TimeoutException">等待回复超时</exception>
        /// <exception cref="TaskCanceledException">收到无法解析的回复</exception>
        Task<ushort[]> ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort count);
        /// <summary>
        /// 3.读取多个保持寄存器，返回原始回复（字节数组）
        /// </summary>
        /// <exception cref="IOException">端口关闭</exception>
        /// <exception cref="TimeoutException">等待回复超时</exception>
        /// <exception cref="TaskCanceledException">收到无法解析的回复</exception>
        Task<byte[]> ReadHoldingRegistersRaw(byte slaveId, ushort startAddress, ushort count, out int dataOffset);
        /// <summary>
        /// 4.读取多个输入寄存器
        /// </summary>
        /// <exception cref="IOException">端口关闭</exception>
        /// <exception cref="TimeoutException">等待回复超时</exception>
        /// <exception cref="TaskCanceledException">收到无法解析的回复</exception>
        Task<ushort[]> ReadInputRegisters(byte slaveId, ushort startAddress, ushort count);
        /// <summary>
        /// 4.读取多个输入寄存器，返回原始回复（字节数组）
        /// </summary>
        /// <exception cref="IOException">端口关闭</exception>
        /// <exception cref="TimeoutException">等待回复超时</exception>
        /// <exception cref="TaskCanceledException">收到无法解析的回复</exception>
        Task<byte[]> ReadInputRegistersRaw(byte slaveId, ushort startAddress, ushort count, out int dataOffset);
        /// <summary>
        /// 15.写入多个线圈
        /// </summary>
        Task WriteCoils(byte slaveId, ushort startAddress, IReadOnlyList<bool> values);
        /// <summary>
        /// 15.写入多个线圈，使用原始数据（字节数组）
        /// </summary>
        Task WriteCoilsRaw(byte slaveId, ushort startAddress, int count, byte[] data, int offset);
        /// <summary>
        /// 16.写入多个寄存器
        /// </summary>
        Task WriteRegisters(byte slaveId, ushort startAddress, IReadOnlyList<ushort> values);
        /// <summary>
        /// 16.写入多个寄存器，使用原始数据（字节数组）
        /// </summary>
        Task WriteRegistersRaw(byte slaveId, ushort startAddress, int count, byte[] data, int offset);
        /// <summary>
        /// 5.写入单个线圈
        /// </summary>
        Task WriteSingleCoil(byte slaveId, ushort address, bool value);
        /// <summary>
        /// 6.写入单个寄存器
        /// </summary>
        /// <exception cref="IOException">端口关闭</exception>
        /// <exception cref="TimeoutException">等待回复超时</exception>
        /// <exception cref="TaskCanceledException">收到无法解析的回复</exception>
        Task WriteSingleRegister(byte slaveId, ushort address, ushort value);
    }
}
