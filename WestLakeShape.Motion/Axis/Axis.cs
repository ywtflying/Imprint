using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WestLakeShape.Common.WpfCommon;

namespace WestLakeShape.Motion
{
    public abstract class Axis<TAxisConfig> : IAxis<TAxisConfig>
        where TAxisConfig : AxisConfig
    { 
        public abstract double Position { get; }
        public abstract double Speed { get; }
        public TAxisConfig Config { get; set; }
        public string Name { get; }

        public Axis(TAxisConfig config)
        {
            Config = config;
        }

        public abstract bool GoHome();

        public abstract bool MoveTo(double position);
        public abstract bool MoveBy(double position);

        public abstract void ServoOff();

        public abstract void ServoOn();

        public abstract void Stop();
        public abstract void InitialParameter();

        public abstract void EmergencyStop();
        public abstract void ResetAlarm();

        public abstract void LoadVelocity(double vel);
    }

    public class AxisConfig:NotifyPropertyChanged
    {
        private double _speed = 10;
        private string _name;
        private int _index =0;
        private short _cardIndex = 1;

        [Category("Axis"), Description("当前速度"), DefaultValue(10)]
        [RefreshProperties(RefreshProperties.All)]
        [DisplayName("当前速度")]
        public double Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        [Category("Axis"), Description("轴号"), DefaultValue(1)]
        [DisplayName("轴号")]
        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        [Category("Axis"), Description("轴名字")]
        [DisplayName("轴名字")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [Category("Axis"), Description("板卡索引")]
        [DisplayName("板卡索引")]
        public short CardIndex
        {
            get => _cardIndex;
            set => SetProperty(ref _cardIndex, value);
        } 
    }
}
