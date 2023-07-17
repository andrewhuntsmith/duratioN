using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace RaceModeTimer
{
    class MemorySource
    {
        // offsets from npp.dll to find player timers
        public const int DllOffset = 0x5E2628;
        public const int P1EpisodeTimeOffset = 0x7D00;
        public const int P2EpisodeTimeOffset = P1EpisodeTimeOffset + 0x8;
        public const int P3EpisodeTimeOffset = P2EpisodeTimeOffset + 0x8;
        public const int P4EpisodeTimeOffset = P3EpisodeTimeOffset + 0x8;

        public const int P1ActiveOffset = 0x7BE8;
        public const int P2ActiveOffset = P1ActiveOffset + 0x4;
        public const int P3ActiveOffset = P2ActiveOffset + 0x4;
        public const int P4ActiveOffset = P3ActiveOffset + 0x4;

        public static int StatBlockOffset;

        // variables that store memory access information
        public static Process NppProcess;
        public static IntPtr NppProcessHandle;
        public static ProcessModule NppProcessModule;
        public static ProcessModuleCollection NppProcessModuleCollection;
        public static IntPtr NppdllBaseAddress;

        const int PROCESS_VM_READ = 0x0010;

        public bool StartNewEpisode;
        public bool EpisodeTimeValuesChanged;
        public bool PlayerActivityChanged;

        public bool P1Active;
        public bool P2Active;
        public bool P3Active;
        public bool P4Active;
       
        public double P1Time = 0;
        public double P2Time = 0;
        public double P3Time = 0;
        public double P4Time = 0;

        public double P1EpisodeTime = 0;
        public double P2EpisodeTime = 0;
        public double P3EpisodeTime = 0;
        public double P4EpisodeTime = 0;

        public double P1EpisodeTimeNewFrame = 0;
        public double P2EpisodeTimeNewFrame = 0;
        public double P3EpisodeTimeNewFrame = 0;
        public double P4EpisodeTimeNewFrame = 0;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public void HookMemory()
        {
            int bytesRead = 0;
            byte[] offsetPointer = new byte[8];
            byte[] p1Buffer = new byte[8];
            byte[] p2Buffer = new byte[8];
            byte[] p3Buffer = new byte[8];
            byte[] p4Buffer = new byte[8];

            if (!FindProcessNPP()) { return; }

            NppProcessHandle = OpenProcess(PROCESS_VM_READ, false, NppProcess.Id);
            // if OpenProcess failed
            if (NppProcessHandle == (IntPtr)0)
            {
                string errorMessage = "Cannot access N++ process!";
                string caption = "Error in accessing application";
                MessageBox.Show(errorMessage, caption);
                return;
            }

            if (!FindnppModule()) { return; }

            ReadProcessMemory((int)NppProcessHandle, (int)(NppdllBaseAddress + DllOffset), offsetPointer, offsetPointer.Length, ref bytesRead);

            StatBlockOffset = BitConverter.ToInt32(offsetPointer, 0);
            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P1EpisodeTimeOffset), p1Buffer, p1Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P2EpisodeTimeOffset), p2Buffer, p2Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P3EpisodeTimeOffset), p3Buffer, p3Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P4EpisodeTimeOffset), p4Buffer, p4Buffer.Length, ref bytesRead);

            P1EpisodeTime = BitConverter.ToDouble(p1Buffer, 0);
            P2EpisodeTime = BitConverter.ToDouble(p2Buffer, 0);
            P3EpisodeTime = BitConverter.ToDouble(p3Buffer, 0);
            P4EpisodeTime = BitConverter.ToDouble(p4Buffer, 0);
        }

        // finds the N++ process
        // returns true if process is found
        // returns false if process is not found and user quits application
        bool FindProcessNPP()
        {
            while (Process.GetProcessesByName("N++").Length == 0)
            {
                // if GetProcessesByName failed
                string errorMessage = "Cannot find application N++!\nOpen the game and click 'Yes', or give up and click 'No'.";
                string caption = "Error in finding application";
                var result = MessageBox.Show(errorMessage, caption, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return false;
                }
            }
            NppProcess = Process.GetProcessesByName("N++")[0];
            return true;
        }

        // finds the base address for npp.dll
        // returns true if found
        // returns false if error
        bool FindnppModule()
        {
            string nppdllFilePath = "npp.dll";
            NppdllBaseAddress = (IntPtr)0;
            NppProcessModuleCollection = NppProcess.Modules;

            //Console.WriteLine("Base addresses of modules associated with N++ are:");
            for (int i = 0; i < NppProcessModuleCollection.Count; i++)
            {
                NppProcessModule = NppProcessModuleCollection[i];
                //Console.WriteLine(nppProcessModule.FileName+" : "+nppProcessModule.BaseAddress);
                if (NppProcessModule.FileName.Contains(nppdllFilePath))
                {
                    NppdllBaseAddress = NppProcessModule.BaseAddress;
                }
            }

            // if the npp.dll module was not found
            if (NppdllBaseAddress == (IntPtr)0)
            {
                string errorMessage = "Cannot access npp.dll module!";
                string caption = "Error in accessing memory";
                MessageBox.Show(errorMessage, caption);
                return false;
            }
            return true;
        }

        public void UpdateThread()
        {
            UpdateEpisodeTime();
            UpdatePlayerActivity();
        }

        void UpdateEpisodeTime()
        {
            int bytesRead = 0;
            byte[] p1Buffer = new byte[8];
            byte[] p2Buffer = new byte[8];
            byte[] p3Buffer = new byte[8];
            byte[] p4Buffer = new byte[8];

            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P1EpisodeTimeOffset), p1Buffer, p1Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P2EpisodeTimeOffset), p2Buffer, p2Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P3EpisodeTimeOffset), p3Buffer, p3Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P4EpisodeTimeOffset), p4Buffer, p4Buffer.Length, ref bytesRead);

            P1EpisodeTimeNewFrame = BitConverter.ToDouble(p1Buffer, 0);
            P2EpisodeTimeNewFrame = BitConverter.ToDouble(p2Buffer, 0);
            P3EpisodeTimeNewFrame = BitConverter.ToDouble(p3Buffer, 0);
            P4EpisodeTimeNewFrame = BitConverter.ToDouble(p4Buffer, 0);

            StartNewEpisode = P1EpisodeTimeNewFrame < P1EpisodeTime
                            || P2EpisodeTimeNewFrame < P2EpisodeTime
                            || P3EpisodeTimeNewFrame < P3EpisodeTime
                            || P4EpisodeTimeNewFrame < P4EpisodeTime;

            EpisodeTimeValuesChanged = P1EpisodeTimeNewFrame != P1EpisodeTime
                            || P2EpisodeTimeNewFrame != P2EpisodeTime
                            || P3EpisodeTimeNewFrame != P3EpisodeTime
                            || P4EpisodeTimeNewFrame != P4EpisodeTime;

            if (StartNewEpisode)
            {
                P1Time += P1EpisodeTime;
                P2Time += P2EpisodeTime;
                P3Time += P3EpisodeTime;
                P4Time += P4EpisodeTime;
            }

            P1EpisodeTime = P1EpisodeTimeNewFrame;
            P2EpisodeTime = P2EpisodeTimeNewFrame;
            P3EpisodeTime = P3EpisodeTimeNewFrame;
            P4EpisodeTime = P4EpisodeTimeNewFrame;
        }

        void UpdatePlayerActivity()
        {
            int bytesRead = 0;
            byte[] activeBuffer = new byte[16];
            ReadProcessMemory((int)NppProcessHandle, (int)(StatBlockOffset + P1ActiveOffset), activeBuffer, activeBuffer.Length, ref bytesRead);

            var p1 = BitConverter.ToBoolean(activeBuffer, 0);
            var p2 = BitConverter.ToBoolean(activeBuffer, 4);
            var p3 = BitConverter.ToBoolean(activeBuffer, 8);
            var p4 = BitConverter.ToBoolean(activeBuffer, 12);

            PlayerActivityChanged = p1 != P1Active || p2 != P2Active || p3 != P3Active || p4 != P4Active;

            P1Active = p1;
            P2Active = p2;
            P3Active = p3;
            P4Active = p4;
        }

        public void ResetValues()
        {
            P1Time = 0;
            P2Time = 0;
            P3Time = 0;
            P4Time = 0;
        }
    }
}
