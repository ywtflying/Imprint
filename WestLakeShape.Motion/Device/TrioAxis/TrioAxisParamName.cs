using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WestLakeShape.Motion.Device
{
    public static class TrioParamName
    {
        public static string Unit = "UNITS"; //脉冲当量
        public static string Speed = "SPEED"; //速度
        public static string Creep = "CREEP";//回零蠕变
        public static string Acc = "ACCEL";  //加速度
        public static string Dec = "DECEL";  //减速度
        public static string PositiveLimit = "FS_LIMIT";     //正限位
        public static string NagetiveLimit = "RS_LIMIT";     //负限位
        public static string FollowError = "FE_LIMIT";       //跟随误差
        public static string FollowErrorRange = "FE_RANGE";  //跟随误差范围
        public static string IsLoop = "SERVO";               //闭环或开环控制(带反馈置1)
        public static string JogSpeed = "JOGSPEED";          //JOG速度
        public static string Enable = "WDOG";                //驱动器外部继电器开关
        public static string ServoOn = "AXIS_ENABLE";        //独立启用轴

        public static string TargetPosition = "DPOS";        //目标位置
        public static string CurrentPosition = "MPOS";       //当前位置
        public static string CurrentSpeed = "MSPEED";        //当前速度
        public static string MType = "MTYPE";                //移动指令状态
        public static string DatumIn = "DATUM_IN";           //
    }


    /// <summary>
    /// 轴报错码
    /// </summary>
    [Flags]
    public enum TrioErroCode
    {
        [Description("无报错")]
        None = 0,

        [Description("超过限制速度值")]
        SpeedLimit = 1 << 0,

        [Description("FE值超过了range的警告值")]
        FllowingErrowWarning = 1 << 1,

        [Description("轴通讯错误")]
        CommuError = 1 << 2,

        [Description("远程驱动器故障")]
        DeviceError = 1 << 3,

        [Description("正限位")]
        PositiveLimit = 1 << 4,

        [Description("负限位")]
        NegativeLimit = 1 << 5,

        [Description("数据采集中")]
        Datuming = 1 << 6,

        [Description("进给保持输入有效，并且轴 VP_SPEED 设置为 FHSPEED ")]
        Feedhold = 1 << 7,

        [Description("已超过 FE_LIMIT 设置的限制。")]
        FllowingErrowLimit = 1 << 8,

        [Description("正限位（Fs limit）生效")]
        FSLimit = 1 << 9,

        [Description("负限位（Rs limit）生效")]
        RSLimit = 1 << 10,

        [Description("运动取消")]
        CancleMovement = 1 << 11,

        [Description("轴的 DEMAND_SPEED 超过了步进脉冲硬件产生脉冲的能力")]
        PulsOverSpeed = 1 << 12,

        [Description("轴上的 MOVETANG 正在减速")]
        MovetangDecelerating = 1 << 13,

        [Description("过载，编码器断电")]
        EncoderPowerOverload = 1 << 18,
    }


    /// <summary>
    /// 回零模式
    /// </summary>
    public enum TrioHomeModel
    {
        //移动到零点
        MoveToZero = 0,

        //正向+Z脉冲找零
        PositiveAndZPlus = 1,
        //反向+Z脉冲找零
        NegativeAndZPlus = 2,
        //正向+限位开关
        PositiveAndLimit = 3,
        //反向+限位开关
        NegativeAndLimit = 4,
        //正向+限位开关+Z脉冲
        PositiveAndZPlusLimit = 5,
        //反向+限位开关+Z脉冲
        NegativeAndZplusLimit = 6,
    }

    /// <summary>
    /// 当前运动的命令
    /// </summary>
    public enum TrioMtypeValue
    {
        Idle = 0,
        Move =1, 
        MoveAbs =2,
        MHelical = 3,//螺旋移动
        MoveCirc = 4,
        MoveModify = 5,
        MoveSp = 6,
        MoveAbsSp = 7,
        MoveCirSp = 8,
        MHelicaSp = 9,
        Forward = 10,
        Reverse =11,
        Datum = 12,
        Cam =13,
        ForwordJog =14,
        RevJog = 15,
        Reserved = 16,
        CamBox = 20,
        Connect = 21,
        MoveLink = 22,
        ConnPath = 23,
        FlexLink = 24,
        CamLink = 25,
        CompensateXY = 26,
        Movetang = 30,
        Mspherical =31,
        Mtranscirc = 32,
        MoveSeq = 33,
        MoveAbsSeq = 34,
        MsphericalSp = 40,
        MtranscircSp = 41,
        MoveSeqSP = 42,
        MoveAbsSeqSp = 43,
        MoveJ = 44,
        MoveL = 45,
        Movec = 46,
        MoveBlend = 47,
    }
}
