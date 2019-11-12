using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// https://www.unknowncheats.me/forum/c-/82629-basic-dll-injector.html

public enum Feedback
{
    FILE_NOT_FOUND,
    PROCESS_NOT_FOUND,
    NO_ENTRY_POINT,
    MEMORY_SPACE_FAIL,
    MEMORY_WRITE_FAIL,
    REMOTE_THREAD_FAIL,
    FAIL,
    SUCCESS
}

public sealed class injector
{
    int Result;
    static readonly IntPtr IntPtr_Zero = IntPtr.Zero;
    static readonly uint Access = (0x2 | 0x8 | 0x10 | 0x20 | 0x400);
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

    public Feedback load(string Name, string Path)
    {
        if (!File.Exists(Path))
        {
            return Feedback.FILE_NOT_FOUND;
        }

        uint ProcessID = 0;
        Process[] CurrentProcesses = Process.GetProcesses();
        foreach (Process P in CurrentProcesses)
        {
            if (P.ProcessName == Name)
            {
                ProcessID = (uint)P.Id;
            }
        }

        if (ProcessID == 0) return Feedback.PROCESS_NOT_FOUND;

        injectInstance(ProcessID, Path);

        if (Result == 1) return Feedback.FAIL;

        if (Result == 2) return Feedback.NO_ENTRY_POINT;

        if (Result == 3) return Feedback.MEMORY_SPACE_FAIL;

        if (Result == 4) return Feedback.MEMORY_WRITE_FAIL;

        return Feedback.SUCCESS;
    }

    int injectInstance(uint ProcessToHook, string Path)
    {
        IntPtr processHandle = OpenProcess(Access, 1, ProcessToHook);
        if (processHandle == IntPtr_Zero) return Result = 1; // Cant open or no perms?

        IntPtr loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
        if (loadLibraryAddress == IntPtr_Zero) return Result = 2; // No entry point

        IntPtr argAddress = VirtualAllocEx(processHandle, (IntPtr)null, (IntPtr)Path.Length, (0x1000 | 0x2000), 0X40);
        if (argAddress == IntPtr_Zero) return Result = 3; // Invalid memory space, potato pc prob

        byte[] bytes = Encoding.ASCII.GetBytes(Path);

        if (WriteProcessMemory(processHandle, argAddress, bytes, (uint)bytes.Length, 0) == 0)
            return Result = 4; // Writing memory fail

        if (CreateRemoteThread(processHandle, (IntPtr)null, IntPtr_Zero, loadLibraryAddress, argAddress, 0, (IntPtr)null) == IntPtr_Zero)
        {
            return Result = 4; // Failed to create remote thread
        }

        CloseHandle(processHandle);
        return Result = 0; // Sucess
    }
}