using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
// https://www.unknowncheats.me/forum/c-/82629-basic-dll-injector.html

public enum feedback
{
    FILE_NOT_FOUND,
    PROCESS_NOT_FOUND,
    NO_ENTRY_POINT,
    MEMORY_SPACE_FAIL,
    MEMORY_WRITE_FAIL,
    REMOTE_THREAD_FAIL,
    NOT_SUPPORTED,
    SUCCESS
}

public sealed class injector
{
    static readonly IntPtr IntPtr_Zero = IntPtr.Zero;
    static readonly uint Access = 0x001F0FFF;
    static injector _instance;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress,
        IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern int CloseHandle(IntPtr hObject);

    public static injector instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new injector();
            }
            return _instance;
        }
    }

    private injector() { }

    public feedback load(string name, string path)
    {
        if (!File.Exists(path))
        {
            return feedback.FILE_NOT_FOUND;
        }

        uint ProcessID = 0;
        Process[] CurrentProcesses = Process.GetProcesses();
        foreach (Process P in CurrentProcesses)
        {
            string x64 = "Rocket League (64-bit, DX11, Cooked)";
            string x32 = "Rocket League (32-bit, DX9, Cooked)";

            if (P.ProcessName == name)
            {
                if (P.MainWindowTitle == x64)
                {
                    ProcessID = (uint)P.Id;
                }
                else if (P.MainWindowTitle == x32)
                {
                    return feedback.NOT_SUPPORTED;
                }
            }
        }

        if (ProcessID == 0) return feedback.PROCESS_NOT_FOUND;

        return injectInstance(ProcessID, path);
    }

    feedback injectInstance(uint processId, string path)
    {
        IntPtr processHandle = OpenProcess(Access, 1, processId);
        if (processHandle == IntPtr_Zero) return feedback.FILE_NOT_FOUND;

        IntPtr loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
        if (loadLibraryAddress == IntPtr_Zero) return feedback.NO_ENTRY_POINT;

        IntPtr argAddress = VirtualAllocEx(processHandle, (IntPtr)null, (IntPtr)path.Length, (0x1000 | 0x2000), 0X40);
        if (argAddress == IntPtr_Zero) return feedback.MEMORY_SPACE_FAIL;

        byte[] bytes = Encoding.ASCII.GetBytes(path);

        if (WriteProcessMemory(processHandle, argAddress, bytes, (uint)bytes.Length, 0) == 0) return feedback.MEMORY_WRITE_FAIL;
        if (CreateRemoteThread(processHandle, (IntPtr)null, IntPtr_Zero, loadLibraryAddress, argAddress, 0, (IntPtr)null) == IntPtr_Zero) return feedback.REMOTE_THREAD_FAIL;

        CloseHandle(processHandle);
        return feedback.SUCCESS;
    }
}