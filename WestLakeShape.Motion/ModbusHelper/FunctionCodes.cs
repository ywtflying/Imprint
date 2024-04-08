namespace WestLakeShape.Motion
{
    /// <summary>
    /// 功能码
    /// </summary>
    public static class FunctionCodes
    {
        public const byte ReadCoils = 1;
        public const byte WriteSingleCoil = 5;
        public const byte WriteCoils = 15;
        public const byte ReadDiscreteInputs = 2;

        public const byte ReadHoldingRegisters = 3;
        public const byte WriteSingleHoldingRegister = 6;
        public const byte WriteRegisters = 16;
        public const byte ReadInputRegisters = 4;
    }
}
