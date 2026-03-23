// Auto-generated from kernel\condvar.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public static class Condvar
{
    public static extern Result condvarWaitTimeout(ref CondVar c, ref Mutex m, u64 timeout);
}
