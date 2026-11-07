namespace CS2SX.Switch;

/// <summary>
/// Minimal FTP server (passive mode) serving the SD card, running on a background
/// thread. Connect with FileZilla/WinSCP using a single connection.
/// </summary>
public static class Ftp
{
    /// <summary>Starts the server on <paramref name="port"/>. Returns 0 on success.</summary>
    public static int Start(int port) => -1;
    public static void Stop() { }
    public static bool IsRunning() => false;
    /// <summary>The console's IP address, e.g. "192.168.0.23".</summary>
    public static string GetIP() => "";
    public static int GetPort() => 0;
    /// <summary>Number of connected clients (0 or 1).</summary>
    public static int Clients() => 0;
}
