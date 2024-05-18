using System.Threading.Tasks;

namespace WestLakeShape.Motion
{
    public interface IAxis
    {
        bool Direction { get; }
        string Name { get; }
        /// <summary>
        /// 当前位置
        /// </summary>
        double Position { get; }

        double Speed { get; }

        void ServoOn();

        void ServoOff();

        bool GoHome();

        bool MoveTo(double position);

        bool MoveBy(double position);
        void InitialParameter();
        void Stop();
        void EmergencyStop();
        void ResetAlarm();
        void LoadVelocity(double vel);
    }

    public interface IAxis<TAxisConfig> : IAxis
    where TAxisConfig : AxisConfig
    {
    }

}
