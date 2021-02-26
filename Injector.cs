using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

// https://www.unknowncheats.me/forum/c-/82629-basic-dll-injector.html
// https://www.pinvoke.net/default.aspx/kernel32.openprocess
// https://www.pinvoke.net/default.aspx/kernel32.virtualallocex

namespace BakkesModInjectorCs
{
    public enum InjectorResult : Int16
    {
        FILE_NOT_FOUND = 0,
        PROCESS_NOT_FOUND = 1,
        PROCESS_NOT_SUPPORTED = 2,
        PROCESS_HANDLE_NOT_FOUND = 3,
        LOADLIBRARY_NOT_FOUND = 4,
        VIRTUAL_ALLOCATE_FAIL = 5,
        WRITE_MEMORY_FAIL = 6,
        CREATE_THREAD_FAIL = 7,
        SUCCESS = 8
    }

    public class Injector
    {
        private static Injector InjectorInstance;

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

        private enum AccessFlags : uint
        {
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

        private enum AllocationType : uint
        {
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

        private enum MemoryProtection : uint
        {
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

        public static Injector Instance
        {
            get
            {
                if (InjectorInstance == null)
                {
                    InjectorInstance = new Injector();
                }

                return InjectorInstance;
            }
        }

        public InjectorResult InjectDLL(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return InjectorResult.FILE_NOT_FOUND;
            }

            uint processId = 0;

            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes)
            {
                if (p.ProcessName == "RocketLeague")
                {
                    if (p.MainWindowTitle == "Rocket League (64-bit, DX11, Cooked)")
                    {
                        processId = Convert.ToUInt32(p.Id);
                    }
                    else
                    {
                        return InjectorResult.PROCESS_NOT_SUPPORTED;
                    }
                }
            }

            if (processId == 0)
            {
                return InjectorResult.PROCESS_NOT_FOUND;
            }

            return InjectDLL(processId, filePath);
        }

        private InjectorResult InjectDLL(uint processId, string filePath)
        {
            IntPtr processHandle = OpenProcess(Convert.ToUInt32(AccessFlags.All), 1, processId);

            if (processHandle == IntPtr.Zero)
            {
                return InjectorResult.PROCESS_HANDLE_NOT_FOUND;
            }

            IntPtr loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (loadLibraryAddress == IntPtr.Zero)
            {
                CloseHandle(processHandle);

                return InjectorResult.LOADLIBRARY_NOT_FOUND;
            }

            IntPtr allocatedAddress = VirtualAllocEx(processHandle, (IntPtr)null, (IntPtr)filePath.Length, Convert.ToUInt32(AllocationType.Commit) | Convert.ToUInt32(AllocationType.Reserve), Convert.ToUInt32(MemoryProtection.ExecuteReadWrite));
           
            if (allocatedAddress == IntPtr.Zero)
            {
                CloseHandle(processHandle);

                return InjectorResult.VIRTUAL_ALLOCATE_FAIL;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(filePath);
            int bWroteMemory = WriteProcessMemory(processHandle, allocatedAddress, bytes, Convert.ToUInt32(bytes.Length), 0);

            if (bWroteMemory == 0)
            {
                CloseHandle(processHandle);

                return InjectorResult.WRITE_MEMORY_FAIL;
            }

            IntPtr threadHandle = CreateRemoteThread(processHandle, (IntPtr)null, IntPtr.Zero, loadLibraryAddress, allocatedAddress, 0, (IntPtr)null);

            if (threadHandle == IntPtr.Zero)
            {
                CloseHandle(processHandle);

                return InjectorResult.CREATE_THREAD_FAIL;
            }

            CloseHandle(threadHandle);
            CloseHandle(processHandle);

            return InjectorResult.SUCCESS;
        }
    }
}