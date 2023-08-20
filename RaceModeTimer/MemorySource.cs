using System;
using System.Collections.Generic;
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

        public const int P1FinishedOffset = 0x7DE0;
        public const int P2FinishedOffset = P1FinishedOffset + 0x4;
        public const int P3FinishedOffset = P2FinishedOffset + 0x4;
        public const int P4FinishedOffset = P3FinishedOffset + 0x4;

        public const int P1BonusOffset = 0x7E40;
        public const int P2BonusOffset = P1BonusOffset + 0x8;
        public const int P3BonusOffset = P2BonusOffset + 0x8;
        public const int P4BonusOffset = P3BonusOffset + 0x8;

        public const int EpisodeFrameCounterOffset = 0x7CC8;

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

        public BoolAddressValue P1Active;
        public BoolAddressValue P2Active;
        public BoolAddressValue P3Active;
        public BoolAddressValue P4Active;

        public BoolAddressValue P1Finished;
        public BoolAddressValue P2Finished;
        public BoolAddressValue P3Finished;
        public BoolAddressValue P4Finished;

        public double P1Time = 0;
        public double P2Time = 0;
        public double P3Time = 0;
        public double P4Time = 0;

        public DoubleAddressValue P1EpisodeTime;
        public DoubleAddressValue P2EpisodeTime;
        public DoubleAddressValue P3EpisodeTime;
        public DoubleAddressValue P4EpisodeTime;

        public DoubleAddressValue P1BonusTime;
        public DoubleAddressValue P2BonusTime;
        public DoubleAddressValue P3BonusTime;
        public DoubleAddressValue P4BonusTime;

        public int CurrentFrame;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        //first parameter player number, second parameter is current frame count, third parameter is player bonus time
        public Action<int, int, double> PlayerFinished;
        public Action LevelFinished;
        public Action EpisodeFinished;

        public void HookMemory()
        {
            int bytesRead = 0;
            byte[] offsetPointer = new byte[8];

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
            InitializeAllValues();

            P1EpisodeTime.UpdateValue();
            P2EpisodeTime.UpdateValue();
            P3EpisodeTime.UpdateValue();
            P4EpisodeTime.UpdateValue();
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

        void InitializeAllValues()
        {
            P1Active = new BoolAddressValue() { Offsets = new List<int> { StatBlockOffset + P1ActiveOffset } };
            P2Active = new BoolAddressValue() { Offsets = new List<int> { StatBlockOffset + P2ActiveOffset } };
            P3Active = new BoolAddressValue() { Offsets = new List<int> { StatBlockOffset + P3ActiveOffset } };
            P4Active = new BoolAddressValue() { Offsets = new List<int> { StatBlockOffset + P4ActiveOffset } };

            P1Finished = new BoolAddressValue() { Offsets = new List<int> { StatBlockOffset + P1FinishedOffset } };
            P2Finished = new BoolAddressValue() { Offsets = new List<int> { StatBlockOffset + P2FinishedOffset } };
            P3Finished = new BoolAddressValue() { Offsets = new List<int> { StatBlockOffset + P3FinishedOffset } };
            P4Finished = new BoolAddressValue() { Offsets = new List<int> { StatBlockOffset + P4FinishedOffset } };

            P1EpisodeTime = new DoubleAddressValue() { Offsets = new List<int> { StatBlockOffset + P1EpisodeTimeOffset } };
            P2EpisodeTime = new DoubleAddressValue() { Offsets = new List<int> { StatBlockOffset + P2EpisodeTimeOffset } };
            P3EpisodeTime = new DoubleAddressValue() { Offsets = new List<int> { StatBlockOffset + P3EpisodeTimeOffset } };
            P4EpisodeTime = new DoubleAddressValue() { Offsets = new List<int> { StatBlockOffset + P4EpisodeTimeOffset } };

            P1BonusTime = new DoubleAddressValue() { Offsets = new List<int> { StatBlockOffset + P1BonusOffset } };
            P2BonusTime = new DoubleAddressValue() { Offsets = new List<int> { StatBlockOffset + P2BonusOffset } };
            P3BonusTime = new DoubleAddressValue() { Offsets = new List<int> { StatBlockOffset + P3BonusOffset } };
            P4BonusTime = new DoubleAddressValue() { Offsets = new List<int> { StatBlockOffset + P4BonusOffset } };
        }

        public void UpdateThread()
        {
            UpdateFinishedPlayers();
            UpdateEpisodeTime();
            UpdatePlayerActivity();
        }

        void UpdateFinishedPlayers()
        {
            P1Finished.UpdateValue();
            P2Finished.UpdateValue();
            P3Finished.UpdateValue();
            P4Finished.UpdateValue();

            if (P1Finished.Value && !P1Finished.PreviousValue)
            {
                P1BonusTime.UpdateValue();
                PlayerFinished(0, CurrentFrame, P1BonusTime.Value);
            }
            if (P2Finished.Value && !P2Finished.PreviousValue)
            {
                P2BonusTime.UpdateValue();
                PlayerFinished(1, CurrentFrame, P2BonusTime.Value);
            }
            if (P3Finished.Value && !P3Finished.PreviousValue)
            {
                P3BonusTime.UpdateValue();
                PlayerFinished(2, CurrentFrame, P3BonusTime.Value);
            }
            if (P4Finished.Value && !P4Finished.PreviousValue)
            {
                P4BonusTime.UpdateValue();
                PlayerFinished(3, CurrentFrame, P4BonusTime.Value);
            }

            if ((!P1Finished.Value && P1Finished.PreviousValue) || (!P2Finished.Value && P2Finished.PreviousValue)
                || (!P3Finished.Value && P3Finished.PreviousValue) || (!P4Finished.Value && P4Finished.PreviousValue))
            {
                LevelFinished();
            }
        }

        void UpdateEpisodeTime()
        {
            P1EpisodeTime.UpdateValue();
            P2EpisodeTime.UpdateValue();
            P3EpisodeTime.UpdateValue();
            P4EpisodeTime.UpdateValue();

            StartNewEpisode = P1EpisodeTime.Value < P1EpisodeTime.PreviousValue
                            || P2EpisodeTime.Value < P2EpisodeTime.PreviousValue
                            || P3EpisodeTime.Value < P3EpisodeTime.PreviousValue
                            || P4EpisodeTime.Value < P4EpisodeTime.PreviousValue;

            EpisodeTimeValuesChanged = P1EpisodeTime.Value != P1EpisodeTime.PreviousValue
                            || P2EpisodeTime.Value != P2EpisodeTime.PreviousValue
                            || P3EpisodeTime.Value != P3EpisodeTime.PreviousValue
                            || P4EpisodeTime.Value != P4EpisodeTime.PreviousValue;

            if (StartNewEpisode)
            {
                P1Time += P1EpisodeTime.Value;
                P2Time += P2EpisodeTime.Value;
                P3Time += P3EpisodeTime.Value;
                P4Time += P4EpisodeTime.Value;
            }
        }

        void UpdatePlayerActivity()
        {
            P1Active.UpdateValue();
            P2Active.UpdateValue();
            P3Active.UpdateValue();
            P4Active.UpdateValue();

            PlayerActivityChanged = P1Active.Value != P1Active.PreviousValue || P2Active.Value != P2Active.PreviousValue
                || P3Active.Value != P3Active.PreviousValue || P4Active.Value != P4Active.PreviousValue;
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
