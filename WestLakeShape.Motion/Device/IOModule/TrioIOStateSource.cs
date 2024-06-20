using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrioMotion.TrioPC_NET;

namespace WestLakeShape.Motion.Device
{
    public class TrioIOStateSource : IOStateSource
    {
        private readonly int Btye_Size = 8;
        private readonly int Bit_Size = 1;

        private TrioPC _trioPC;
        public TrioIOStateSource(IOStateSourceConfig config) :base( config)
        {
            _trioPC = TrioControl.Instance.TrioPC;
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            LoadInputs(_inputBuffer);
            LoadOutputs(_outputBuffer);
        }

        protected override bool ReadInputs(byte[] buff)
        {
            LoadInputs(buff);
            return true;
        }

        protected override bool ReadOutputs(byte[] buff)
        {
            LoadOutputs(buff);
            return true;
        }

        protected override bool WriteOutputs(byte[] buff)
        {
            SaveOutput(buff);
            return true;
        }

        private void LoadInputs(byte[] buff)
        {
            double value = -1;
            for (var i = 0; i < buff.Length; i++)
            {
                var startIndex = 8 * i;
                var stopIndex = startIndex + 7;
                _trioPC.In(startIndex, stopIndex, out value);
                buff[i] = (byte)value;
            }
        }

        private void LoadOutputs(byte[] buff)
        {
            int value = -1;            
            for (var i = 0; i < buff.Length; i++)
            {
                var startIndex = 8 * i;
                var stopIndex = startIndex + 7;
                _trioPC.ReadOp(startIndex, stopIndex, out value);
                buff[i] = (byte)(value & 0xFF);
            }
        }

        private void SaveOutput(byte[] buff)
        {
            for (var i = 0; i < buff.Length; i++)
            {
                var startIndex = 8 * i;
                var stopIndex = startIndex + 7;
                _trioPC.Op(startIndex, stopIndex, buff[i]);
            }
        }
    }
}
