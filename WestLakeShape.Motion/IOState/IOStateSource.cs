using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using WestLakeShape.Common.WpfCommon;

namespace WestLakeShape.Motion
{
    public abstract class IOStateSource:Connectable
    {
        private IOStateSourceConfig _config;
        private List<IOState> _outputIOs;    //输出IO
        private byte[] _tempBuffer;          //
        
        protected byte[] _inputBuffer;       //输入IO的缓存状态值
        protected byte[] _outputBuffer;      //输出IO的缓存状态值
        protected byte[] _dirtyMasks;        //输出IO是否需要修改标志位

        public Dictionary<string, IOState> InputStates { get; private set; }
        public Dictionary<string, IOState> OutputStates { get; private set; }

        public byte[] InputBuffer => _inputBuffer;
        public byte[] OutputBuffer => _outputBuffer;

        public override string Name => _config.Name;

        public IOStateSourceConfig Config => _config;

     
        public IOStateSource(IOStateSourceConfig config)
        {
            _config = config;
            
            _inputBuffer = new byte[_config.InputBufferLength];
            _outputBuffer = new byte[_config.OutputBufferLength];
            _dirtyMasks = new byte[_config.OutputBufferLength];
            _tempBuffer = new byte[_config.OutputBufferLength];

            InputStates = new Dictionary<string, IOState>();
            OutputStates = new Dictionary<string, IOState>();

            LoadStates();
        }


        protected override void OnConnecting() 
        {

        }

        private void LoadStates()
        {
            foreach (var config in _config.States)
            {
                var state = new IOState(config, this);
                if (config.Type == IOType.Input)
                    InputStates.Add(state.Name, state);
                else
                    OutputStates.Add(state.Name, state);
            }
            _outputIOs = OutputStates.Values.Cast<IOState>().ToList();
        }


        protected override void RefreshStates()
        {
            while (IsConnected)
            {
                //修改标志位写入outputBuff
                RefreshOutputs();
                
                WriteOutputs(_outputBuffer);

                _outputIOs.ForEach(o => o.HasChanged());
                
                ReadInputs(_inputBuffer);
            }
           
        }

        private void RefreshOutputs()
        {
            ReadOutputs(_tempBuffer);
            for (var i = 0; i < _outputBuffer.Length; i++)
            {
                var mask = _dirtyMasks[i];
                var remote = _tempBuffer[i];
                var local = _outputBuffer[i];
                //修改byte中某个bit值，并保证不影响其它bit
                var merged = (byte)(_tempBuffer[i] ^ ((remote ^ local) & mask));
                
                _outputBuffer[i] = merged;
            }
        }

        protected abstract bool ReadInputs(byte[] buffer);

        /// <summary>
        /// 读取输出状态到缓冲区
        /// </summary>
        protected abstract bool ReadOutputs(byte[] buffer);

        /// <summary>
        /// 从缓冲区写入输出状态
        /// </summary>
        protected abstract bool WriteOutputs(byte[] buffer);

        public class IOState : BaseState
        {
            private readonly IOStateConfig _config;
            private readonly IOStateSource _source;

            public override bool ReadOnly => _config.Type == IOType.Input;
            public string Description => _config.Description;
            public bool State => Get();


            public IOState(IOStateConfig config, IOStateSource ioSource) : base(config.Name)
            {
                _config = config;
                _source = ioSource;
            }


            public override bool Get()
            {
                var buffer = _config.Type == IOType.Input
                    ? _source.InputBuffer
                    : _source.OutputBuffer;

                var data = buffer[_config.ByteIndex];
                return 0 != (data & (1 << _config.BitIndex));
            }

            public override void Set(bool value)
            {
                if (_config.Type == IOType.Input)
                    throw new InvalidOperationException("输入状态不允许写入");

                var buffer = _source.OutputBuffer;
                var data = buffer[_config.ByteIndex];
                if (value)
                {
                    data |= (byte)(1 << _config.BitIndex);
                }
                else
                {
                    data &= (byte)~(1 << _config.BitIndex);
                }
                buffer[_config.ByteIndex] = data;

                var masks = _source._dirtyMasks;
                var mask = masks[_config.ByteIndex];
                mask |= (byte)(1 << _config.BitIndex);
                _source._dirtyMasks[_config.ByteIndex] = mask;
                Dirty = true;
            }
        }
    }

    public class IOStateSourceConfig: NotifyPropertyChanged
    {
        private string _name;
        private int _inputBufferLength = 1;
        private int _outputBufferLength = 1;
        private Collection<IOStateConfig> _states = new Collection<IOStateConfig>();

        [Category("程序"), Description("状态包名称，如：IO卡1")]
        [DisplayName("状态包名称"), Required, StringLength(32)]
        public string Name 
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }


        [Category("I/O"), Description("输入缓冲区字节数"), DefaultValue(1)]
        [DisplayName("输入缓冲区字节数"), Range(0, 256)]
        public int InputBufferLength
        {
            get => _inputBufferLength;
            set => SetProperty(ref _inputBufferLength, value);
        }


        [Category("I/O"), Description("输出缓冲区字节数"), DefaultValue(1)]
        [DisplayName("输出缓冲区字节数"), Range(0, 256)]
        public int OutputBufferLength
        {
            get => _outputBufferLength;
            set => SetProperty(ref _outputBufferLength, value);
        } 


        [Category("IO口"), Description("IO口")]
        [DisplayName("IO口")]
        public Collection<IOStateConfig> States
        {
            get => _states;
            set => SetProperty(ref _states, value);
        } 

    }


    public class IOStateConfig:NotifyPropertyChanged
    {
        private string _name;
        private IOType _type = IOType.Input;
        private int _byteIndex = 0;
        private int _bitIndex = 0;
        private IOActiveLevel _activeLevel = IOActiveLevel.Lower;

        [Category("I/O"), Description("IO名称")]
        [DisplayName("IO名称"), Required, StringLength(32)]
        public string Name 
        {
            get => _name;
            set => SetProperty(ref _name, value);
        } 

        /// <summary>
        /// 是否输入状态。True 代表输入状态，False 代表输出状态
        /// <para>输入状态只读，输出状态可读写</para>
        /// </summary>
        [Category("I/O"), Description("状态类别，输入还是输出"), DefaultValue(IOType.Input)]
        [DisplayName("状态类别")]
        public IOType Type 
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        /// <summary>
        /// 对应 Buffer 中存储字节索引
        /// </summary>
        [Category("I/O"), Description("字节索引。该状态存储于对应缓冲区第几字节，基于 0"), DefaultValue(0)]
        [DisplayName("字节索引"), Range(0, 255)]
        public int ByteIndex 
        {
            get => _byteIndex;
            set => SetProperty(ref _byteIndex, value);
        } 

        /// <summary>
        /// 对应 Buffer 中存储字节中的位(Bit)索引
        /// </summary>
        [Category("I/O"), Description("字位索引。该状态存储于所在字节的第几位，基于 0"), DefaultValue(0)]
        [DisplayName("字位索引"), Range(0, 7)]
        public int BitIndex
        {
            get => _bitIndex;
            set => SetProperty(ref _byteIndex, value);
        }

        /// <summary>
        /// 有效电平
        /// </summary>
        [Category("I/O"), Description("有效电平,例如低电平有效"), DefaultValue(IOActiveLevel.Lower)]
        [DisplayName("有效电平"), StringLength(128)]
        public IOActiveLevel ActiveLevel 
        {
            get => _activeLevel;
            set => SetProperty(ref _activeLevel, value);
        } 

        /// <summary>
        /// 说明
        /// </summary>
        [Category("其他"), Description("状态说明")]
        [DisplayName("状态说明"), StringLength(128)]
        public string Description { get; set; } = "";

        internal virtual int GetDataBits()
        {
            return 1;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
