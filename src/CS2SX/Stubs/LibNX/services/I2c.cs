// Auto-generated from services\i2c.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum I2cDevice
{
    I2cDevice_DebugPad = 0,
    I2cDevice_TouchPanel = 1,
    I2cDevice_Tmp451 = 2,
    I2cDevice_Nct72 = 3,
    I2cDevice_Alc5639 = 4,
    I2cDevice_Max77620Rtc = 5,
    I2cDevice_Max77620Pmic = 6,
    I2cDevice_Max77621Cpu = 7,
    I2cDevice_Max77621Gpu = 8,
    I2cDevice_Bq24193 = 9,
    I2cDevice_Max17050 = 10,
    I2cDevice_Bm92t30mwv = 11,
    I2cDevice_Ina226Vdd15v0Hb = 12,
    I2cDevice_Ina226VsysCpuDs = 13,
    I2cDevice_Ina226VsysGpuDs = 14,
    I2cDevice_Ina226VsysDdrDs = 15,
    I2cDevice_Ina226VsysAp = 16,
    I2cDevice_Ina226VsysBlDs = 17,
    I2cDevice_Bh1730 = 18,
    I2cDevice_Ina226VsysCore = 19,
    I2cDevice_Ina226Soc1V8 = 20,
    I2cDevice_Ina226Lpddr1V8 = 21,
    I2cDevice_Ina226Reg1V32 = 22,
    I2cDevice_Ina226Vdd3V3Sys = 23,
    I2cDevice_HdmiDdc = 24,
    I2cDevice_HdmiScdc = 25,
    I2cDevice_HdmiHdcp = 26,
    I2cDevice_Fan53528 = 27,
    I2cDevice_Max77812_3 = 28,
    I2cDevice_Max77812_2 = 29,
    I2cDevice_Ina226VddDdr0V6 = 30,
    I2cDevice_Count,
}

public enum I2cTransactionOption
{
    I2cTransactionOption_Start = (1 << 0),
    I2cTransactionOption_Stop = (1 << 1),
    I2cTransactionOption_All = I2cTransactionOption_Start | I2cTransactionOption_Stop,
}

public unsafe struct I2cSession
{
    public Service s;
}

public static class I2c
{
    public static extern Result i2cInitialize();
    public static extern void i2cExit();
    public static extern IntPtr i2cGetServiceSession();
    public static extern Result i2cOpenSession(ref I2cSession @out, I2cDevice dev);
    public static extern Result i2csessionSendAuto(ref I2cSession s, IntPtr buf, ulong size, I2cTransactionOption option);
    public static extern Result i2csessionReceiveAuto(ref I2cSession s, IntPtr buf, ulong size, I2cTransactionOption option);
    public static extern Result i2csessionExecuteCommandList(ref I2cSession s, IntPtr dst, ulong dst_size, IntPtr cmd_list, ulong cmd_list_size);
    public static extern void i2csessionClose(ref I2cSession s);
}
