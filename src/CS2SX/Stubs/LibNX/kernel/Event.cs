// Auto-generated from kernel\event.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public unsafe struct Event
{
    public Handle revent;
    public Handle wevent;
    public bool autoclear;
}

public static class Event
{
    public static extern Result eventCreate(ref Event t, bool autoclear);
    public static extern void eventLoadRemote(ref Event t, Handle handle, bool autoclear);
    public static extern void eventClose(ref Event t);
    public static extern Result eventWait(ref Event t, u64 timeout);
    public static extern Result eventFire(ref Event t);
    public static extern Result eventClear(ref Event t);
}
