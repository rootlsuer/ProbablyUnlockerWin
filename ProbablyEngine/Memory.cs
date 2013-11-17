using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace MemoryControl
{
    public class MemC
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, ulong nSize, out long lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        public static extern long CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ulong dwDesiredAccess, long bInheritHandle, ulong dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern long ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, ulong size, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern unsafe bool VirtualProtect(IntPtr lpAddress, ulong dwSize, ulong flNewProtect, ulong* lpflOldProtect);
        [DllImport("kernel32")]
        public static extern UInt32 VirtualAlloc(UInt32 lpStartAddr, UInt32 size, UInt32 flAllocationType, UInt32 flProtect);
        [DllImport("kernel32")]
        public static extern bool VirtualFree(IntPtr lpAddress, UInt32 dwSize, UInt32 dwFreeType);
        [DllImport("kernel32")]
        public static extern IntPtr CreateThread(UInt32 lpThreadAttributes, UInt32 dwStackSize, UInt32 lpStartAddress, IntPtr param, UInt32 dwCreationFlags, ref UInt32 lpThreadId);
        [DllImport("kernel32")]
        public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
        [DllImport("kernel32")]
        public static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32")]
        public static extern UInt32 GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32")]
        public static extern UInt32 LoadLibrary(string lpFileName);
        [DllImport("kernel32")]
        public static extern UInt32 GetLastError();
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESSOR_INFO
        {
            public UInt32 dwMax;
            public UInt32 id0;
            public UInt32 id1;
            public UInt32 id2;

            public UInt32 dwStandard;
            public UInt32 dwFeature;

            // If AMD
            public UInt32 dwExt;
        }

        public enum AccessProcessTypes
        {
            PROCESS_CREATE_PROCESS = 0x80,
            PROCESS_CREATE_THREAD = 2,
            PROCESS_DUP_HANDLE = 0x40,
            PROCESS_QUERY_INFORMATION = 0x400,
            PROCESS_SET_INFORMATION = 0x200,
            PROCESS_SET_QUOTA = 0x100,
            PROCESS_SET_SESSIONID = 4,
            PROCESS_TERMINATE = 1,
            PROCESS_VM_OPERATION = 8,
            PROCESS_VM_READ = 0x10,
            PROCESS_VM_WRITE = 0x20
        }

        public enum VirtualProtectAccess
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        public enum VirtualProtectSize
        {
            INT = sizeof(long),
            FLOAT = sizeof(float),
            DOUBLE = sizeof(double),
            CHAR = sizeof(char)
        }

        public static IntPtr cProcessHandle;

        public static void cOpenProcess(string ProcessX)
        {
            var ApplicationXYZ = Process.GetProcessesByName(ProcessX)[0];
            AccessProcessTypes toAccess = AccessProcessTypes.PROCESS_VM_WRITE | AccessProcessTypes.PROCESS_VM_READ |
                                          AccessProcessTypes.PROCESS_VM_OPERATION;
            cProcessHandle = OpenProcess((ulong)toAccess, 1, (ulong)ApplicationXYZ.Id);
        }

        public static void cOpenProcessId(int ProcessX)
        {
            var ApplicationXYZ = Process.GetProcessById(ProcessX);
            AccessProcessTypes toAccess = AccessProcessTypes.PROCESS_VM_WRITE | AccessProcessTypes.PROCESS_VM_READ |
                                          AccessProcessTypes.PROCESS_VM_OPERATION;
            cProcessHandle = OpenProcess((ulong)toAccess, 1, (ulong)ApplicationXYZ.Id);
        }

        public static void cCloseProcess()
        {
            try
            {
                CloseHandle(cProcessHandle);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static long getProcessID(string AppX)
        {
            var AppToID = Process.GetProcessesByName(AppX)[0];
            long ID = AppToID.Id;
            return ID;
        }

        private static void WriteThis(IntPtr Address, byte[] XBytesToWrite, long nSizeToWrite)
        {
            long writtenBytes;
            WriteProcessMemory(cProcessHandle, (ulong)Address, XBytesToWrite, (ulong)nSizeToWrite, out writtenBytes);
        }

        public static void WriteXNOP(long desiredAddress, long noOfNOPsToWrite)
        {
            byte aNOP = 0x90;
            List<byte> nopList = new List<byte>();
            for (long i = 0; i < noOfNOPsToWrite; i++)
                nopList.Add(aNOP);
            byte[] nopBuffer = nopList.ToArray();
            WriteThis((IntPtr)desiredAddress, nopBuffer, noOfNOPsToWrite);
        }

        public static void WriteXInt(long desiredAddrsss, long valToWrite)
        {
            byte[] valueToWrite = BitConverter.GetBytes(valToWrite);
            WriteThis((IntPtr)desiredAddrsss, valueToWrite, valueToWrite.Length);
        }

        public static void WriteXFloat(long desiredAddrsss, float valToWrite)
        {
            byte[] valueToWrite = BitConverter.GetBytes(valToWrite);
            WriteThis((IntPtr)desiredAddrsss, valueToWrite, valueToWrite.Length);
        }

        public static void WriteXDouble(long desiredAddrsss, double valToWrite)
        {
            byte[] valueToWrite = BitConverter.GetBytes(valToWrite);
            WriteThis((IntPtr)desiredAddrsss, valueToWrite, valueToWrite.Length);
        }

        public static void WriteXString(long desiredAddrsss, string valToWrite)
        {
            byte[] valueToWrite = Encoding.ASCII.GetBytes(valToWrite);
            WriteThis((IntPtr)desiredAddrsss, valueToWrite, valueToWrite.Length);
        }

        public static unsafe void WriteXBytes(long desiredAddress, byte[] bytesToWrite)
        {
            ulong protection;
            VirtualProtect((IntPtr)desiredAddress, (ulong)bytesToWrite.Length,  (ulong)0x40, &protection);
            WriteThis((IntPtr)desiredAddress, bytesToWrite, bytesToWrite.Length);
            VirtualProtect((IntPtr)desiredAddress, (ulong)bytesToWrite.Length, protection, &protection);
        }

        public static byte[] readXBytes(long desiredAddress, long noOfBytesToRead)
        {
            byte[] buffer = new byte[noOfBytesToRead];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (ulong)noOfBytesToRead, out noOfBytesRead);
            return buffer;
        }

        public static long readXInt(long desiredAddress)
        {
            byte[] buffer = new byte[0xFF];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (ulong)4, out noOfBytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static float readXFloat(long desiredAddress)
        {
            byte[] buffer = new byte[0xFF];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (ulong)4, out noOfBytesRead);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static double readXDouble(long desiredAddress)
        {
            byte[] buffer = new byte[0xFF];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (ulong)4, out noOfBytesRead);
            return BitConverter.ToDouble(buffer, 0);
        }

        public static string readXString(long desiredAddress, long sizeOfString)
        {
            byte[] buffer = new byte[sizeOfString];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (ulong)sizeOfString, out noOfBytesRead);
            return buffer.ToString();
        }

    }
}