using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

// https://www.unknowncheats.me/forum/c-/82629-basic-dll-injector.html
// https://www.pinvoke.net/default.aspx/kernel32.openprocess
// https://www.pinvoke.net/default.aspx/kernel32.virtualallocex

namespace BakkesModInjectorCs {
    public enum result : int {
        FILE_NOT_FOUND = 0,
        PROCESS_NOT_FOUND = 1,
        NO_ENTRY_POINT = 2,
        MEMORY_SPACE_FAIL = 3,
        MEMORY_WRITE_FAIL = 4,
        REMOTE_THREAD_FAIL = 5,
        NOT_SUPPORTED = 6,
        SUCCESS = 7
    }

    public class injector {
        static injector instance;

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

        private enum accessFlags : uint {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        private enum allocationType : uint {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        private enum memoryProtection : uint {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }


        public static injector injectorInstance {
            get {
                if (instance == null)
                    instance = new injector();

                return instance;
            }
        }

        private injector() { }

        public result inject(string dllPath) {
            if (!File.Exists(dllPath))
                return result.FILE_NOT_FOUND;

            uint procId = 0;
            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes) {
                string x64 = "Rocket League (64-bit, DX11, Cooked)";
                string x32 = "Rocket League (32-bit, DX9, Cooked)";
                if (p.ProcessName == "RocketLeague") {
                    if (p.MainWindowTitle == x64) {
                        procId = Convert.ToUInt32(p.Id);
                    } else if (p.MainWindowTitle == x32) {
                        return result.NOT_SUPPORTED;
                    }
                }
            }

            if (procId == 0)
                return result.PROCESS_NOT_FOUND;

            return inject(procId, dllPath);
        }

        private result inject(uint procId, string dllPath) {
            IntPtr processHandle = OpenProcess(Convert.ToUInt32(accessFlags.All), 1, procId);
            if (processHandle == IntPtr.Zero)
                return result.FILE_NOT_FOUND;

            IntPtr loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (loadLibraryAddress == IntPtr.Zero)
                return result.NO_ENTRY_POINT;

            IntPtr argAddress = VirtualAllocEx(processHandle, (IntPtr)null, (IntPtr)dllPath.Length, Convert.ToUInt32(allocationType.Commit) | Convert.ToUInt32(allocationType.Reserve), Convert.ToUInt32(memoryProtection.ExecuteReadWrite));
            if (argAddress == IntPtr.Zero)
                return result.MEMORY_SPACE_FAIL;

            byte[] bytes = Encoding.ASCII.GetBytes(dllPath);
            if (WriteProcessMemory(processHandle, argAddress, bytes, Convert.ToUInt32(bytes.Length), 0) == 0)
                return result.MEMORY_WRITE_FAIL;

            if (CreateRemoteThread(processHandle, (IntPtr)null, IntPtr.Zero, loadLibraryAddress, argAddress, 0, (IntPtr)null) == IntPtr.Zero)
                return result.REMOTE_THREAD_FAIL;

            CloseHandle(processHandle);
            return result.SUCCESS;
        }
    }
}