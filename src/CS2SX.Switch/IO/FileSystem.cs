using System.Collections.Generic;

namespace CS2SX.Switch;

// The SD card is mounted at "/" — "/switch/myapp/save.txt" is a file on the SD card.
// These classes replace System.IO (which is not available on the Switch); the
// project imports CS2SX.Switch instead of System.IO.

/// <summary>File access on the SD card.</summary>
public static class File
{
    public static string ReadAllText(string path) => "";
    public static string[] ReadAllLines(string path) => System.Array.Empty<string>();
    public static void WriteAllText(string path, string content) { }
    public static void AppendAllText(string path, string content) { }
    public static bool Exists(string path) => false;
    public static void Delete(string path) { }
    public static void Copy(string src, string dst) { }
    /// <summary>Binary-safe copy (chunked) — preserves images, NRO, archives byte-for-byte.</summary>
    public static bool CopyBinary(string src, string dst) => false;
    /// <summary>File size in bytes (-1 if missing/unreadable).</summary>
    public static long GetSize(string path) => -1;
    /// <summary>Last-modified time as a POSIX timestamp (0 if unavailable).</summary>
    public static long GetModifiedTime(string path) => 0;
    /// <summary>Last-modified time formatted "YYYY-MM-DD HH:MM" ("" if unavailable).</summary>
    public static string GetModifiedString(string path) => "";
    /// <summary>Formatted hex dump (offset · hex · ascii) of up to maxBytes.</summary>
    public static string ReadHexDump(string path, int maxBytes) => "";
    /// <summary>Begins a chunked copy (for a progress bar). Returns total bytes, or -1 on error.</summary>
    public static long CopyBegin(string src, string dst) => -1;
    /// <summary>Copies up to chunkBytes more; returns bytes done so far (== total when finished), -1 on error.</summary>
    public static long CopyStep(int chunkBytes) => -1;
    /// <summary>Closes the in-flight chunked copy.</summary>
    public static void CopyEnd() { }
    /// <summary>Renames/moves a file within the SD card (no data copy).</summary>
    public static bool Rename(string src, string dst) => false;
}

/// <summary>Directory access on the SD card.</summary>
public static class Directory
{
    public static bool Exists(string path) => false;
    public static void CreateDirectory(string path) { }
    public static void Delete(string path) { }
    /// <summary>Recursively deletes a directory and all its contents.</summary>
    public static bool DeleteRecursive(string path) => false;
    /// <summary>Renames/moves a directory within the SD card.</summary>
    public static bool Rename(string src, string dst) => false;
    public static string[] GetFiles(string path) => System.Array.Empty<string>();
    public static string[] GetFiles(string path, string pattern) => System.Array.Empty<string>();
    public static string[] GetDirectories(string path) => System.Array.Empty<string>();
    /// <summary>Files and directories of <paramref name="path"/> (names only).</summary>
    public static List<string> GetEntries(string path) => new();
    public static IEnumerable<string> EnumerateFiles(string path) => System.Array.Empty<string>();
    public static string GetCurrentDirectory() => "/switch";
}

/// <summary>Path string helpers ('/'-separated).</summary>
public static class Path
{
    public static string Combine(string a, string b) => a + "/" + b;
    public static string GetFileName(string path) => path;
    public static string GetFileNameWithoutExtension(string path) => path;
    public static string GetExtension(string path) => "";
    public static string GetDirectoryName(string path) => path;
    public static string ChangeExtension(string path, string extension) => path;
    public static bool IsPathRooted(string path) => false;
    public static bool IsDirectory(string path) => false;
}

/// <summary>ZIP archive extraction / creation (requires the miniz extern-lib).</summary>
public static class Archive
{
    /// <summary>Extracts a .zip into destDir. Returns #files extracted, or -1 on error.</summary>
    public static int Extract(string zipPath, string destDir) => -1;
    /// <summary>Compresses a file or folder (isDir) into zipPath. Returns #files, or -1.</summary>
    public static int Compress(string srcPath, bool isDir, string zipPath) => -1;
    /// <summary>Begins a frame-stepped extract. Returns total entries (&gt;= 0), or -1.</summary>
    public static int BeginExtract(string zipPath, string destDir) => -1;
    /// <summary>Begins a frame-stepped multi-item compress. list = lines "&lt;isDir&gt;\t&lt;path&gt;\n". Returns total files, or -1.</summary>
    public static int BeginCompress(string list, string zipPath) => -1;
    /// <summary>Processes one entry. Returns 1 while more remains, 0 when finished.</summary>
    public static int Step() => 0;
    /// <summary>Processes entries until ~maxBytes handled (or done). Returns 1 more, 0 done.</summary>
    public static int StepBudget(int maxBytes) => 0;
    /// <summary>True while a stepped archive op is running.</summary>
    public static bool Busy() => false;
    /// <summary>Result after Busy() is false: #files, -1 err, -2 cancelled.</summary>
    public static int Result() => -1;
    /// <summary>Entries/files processed so far (for a progress bar).</summary>
    public static int Progress() => 0;
    /// <summary>Total entries/files (0 = unknown).</summary>
    public static int Total() => 0;
    /// <summary>Requests cancellation of the running background op.</summary>
    public static void Cancel() { }
}

/// <summary>SD-card storage information.</summary>
public static class Filesystem
{
    /// <summary>Free space on the SD card in bytes (-1 if unavailable).</summary>
    public static long GetFreeSpace() => -1;
    /// <summary>Total capacity of the SD card in bytes (-1 if unavailable).</summary>
    public static long GetTotalSpace() => -1;
    /// <summary>Recursively sums all file sizes under a directory (-1 on error).</summary>
    public static long DirSize(string path) => -1;
}

/// <summary>Reads the header of an .nsp package (PFS0). One package open at a time.</summary>
public static class Nsp
{
    /// <summary>Opens and parses an .nsp. Returns 1 on success, 0 on failure.</summary>
    public static int Open(string path) => 0;
    /// <summary>True when the last <see cref="Open"/> succeeded.</summary>
    public static bool Ok() => false;
    /// <summary>Title ID as 16 hex digits ("" if unknown).</summary>
    public static string TitleId() => "";
    /// <summary>Content type: 0 = unknown, 1 = base game, 2 = update, 3 = DLC.</summary>
    public static int Type() => 0;
    public static int NcaCount() => 0;
    /// <summary>Total package size in bytes.</summary>
    public static long TotalSize() => 0;
    public static bool HasTicket() => false;
    public static int FileCount() => 0;
    public static string FileName(int index) => "";
    public static long FileSize(int index) => 0;
}

/// <summary>USB mass-storage drives (FAT/exFAT) attached to the console.</summary>
public static class Usb
{
    /// <summary>Starts USB host support. Returns 0 on success.</summary>
    public static int Init() => -1;
    public static void Exit() { }
    /// <summary>Re-scans for attached drives; returns the number of drives.</summary>
    public static int Poll() => 0;
    public static int Count() => 0;
    public static bool Inited() => false;
    /// <summary>Mount name of drive <paramref name="index"/>, e.g. "ums0:".</summary>
    public static string Name(int index) => "";
    /// <summary>File system type of drive <paramref name="index"/>, e.g. "exFAT".</summary>
    public static string FsType(int index) => "";
    /// <summary>Capacity in bytes.</summary>
    public static long Capacity(int index) => 0;
    /// <summary>Safely unmounts drive <paramref name="index"/>. Returns 0 on success.</summary>
    public static int Unmount(int index) => -1;
}
