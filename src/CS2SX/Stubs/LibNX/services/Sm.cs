// Auto-generated from services\sm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct SmServiceName
{
    public fixed byte name[8];
}

public static class Sm
{
    public static extern Result smInitialize();
    public static extern void smExit();
    public static extern Result smGetServiceWrapper(ref Service service_out, SmServiceName name);
    public static extern Result smGetServiceOriginal(ref Handle handle_out, SmServiceName name);
    public static extern Handle smGetServiceOverride(SmServiceName name);
    public static extern Result smRegisterService(ref Handle handle_out, SmServiceName name, bool is_light, s32 max_sessions);
    public static extern Result smRegisterServiceCmif(ref Handle handle_out, SmServiceName name, bool is_light, s32 max_sessions);
    public static extern Result smRegisterServiceTipc(ref Handle handle_out, SmServiceName name, bool is_light, s32 max_sessions);
    public static extern Result smUnregisterService(SmServiceName name);
    public static extern Result smUnregisterServiceCmif(SmServiceName name);
    public static extern Result smUnregisterServiceTipc(SmServiceName name);
    public static extern Result smDetachClient();
    public static extern Result smDetachClientCmif();
    public static extern Result smDetachClientTipc();
    public static extern void smAddOverrideHandle(SmServiceName name, Handle handle);
}
