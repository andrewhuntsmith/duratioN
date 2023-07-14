using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace RaceModeTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // offsets from npp.dll to find player timers
        public const int DllOffset = 0x5E2628;
        public const int P1Offset = 0x7D00;
        public const int P2Offset = P1Offset + 0x8;
        public const int P3Offset = P2Offset + 0x8;
        public const int P4Offset = P3Offset + 0x8;

        public static int FinalMemoryOffset;

        // variables that store memory access information
        public static Process NppProcess;
        public static IntPtr NppProcessHandle;
        public static ProcessModule NppProcessModule;
        public static ProcessModuleCollection NppProcessModuleCollection;
        public static IntPtr NppdllBaseAddress;

        const int PROCESS_VM_READ = 0x0010;

        double P1Time = 0;
        double P2Time = 0;
        double P3Time = 0;
        double P4Time = 0;

        double P1CurrentTime = 0;
        double P2CurrentTime = 0;
        double P3CurrentTime = 0;
        double P4CurrentTime = 0;

        string P1Name = "P1";
        string P2Name = "P2";
        string P3Name = "P3";
        string P4Name = "P4";

        string P1DisplayTime => (P1Time + P1CurrentTime).ToString("0.000");
        string P2DisplayTime => (P2Time + P2CurrentTime).ToString("0.000");
        string P3DisplayTime => (P3Time + P3CurrentTime).ToString("0.000");
        string P4DisplayTime => (P4Time + P4CurrentTime).ToString("0.000");

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public MainWindow()
        {
            InitializeComponent();
            HookMemory();
        }

        void HookMemory()
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

            FinalMemoryOffset = BitConverter.ToInt32(offsetPointer, 0);
            ReadProcessMemory((int)NppProcessHandle, (int)(FinalMemoryOffset + P1Offset), p1Buffer, p1Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(FinalMemoryOffset + P2Offset), p2Buffer, p2Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(FinalMemoryOffset + P3Offset), p3Buffer, p3Buffer.Length, ref bytesRead);
            ReadProcessMemory((int)NppProcessHandle, (int)(FinalMemoryOffset + P4Offset), p4Buffer, p4Buffer.Length, ref bytesRead);

            var p1CurrentTime = BitConverter.ToDouble(p1Buffer, 0);
            var p2CurrentTime = BitConverter.ToDouble(p2Buffer, 0);
            var p3CurrentTime = BitConverter.ToDouble(p3Buffer, 0);
            var p4CurrentTime = BitConverter.ToDouble(p4Buffer, 0);

            Thread t = new Thread(UpdateThread);
            t.Start();
            UpdateTimeDisplay();
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

        bool Loop;
        void UpdateThread()
        {
            Loop = true;
            while (Loop)
            {
                int bytesRead = 0;
                byte[] p1Buffer = new byte[8];
                byte[] p2Buffer = new byte[8];
                byte[] p3Buffer = new byte[8];
                byte[] p4Buffer = new byte[8];

                ReadProcessMemory((int)NppProcessHandle, (int)(FinalMemoryOffset + P1Offset), p1Buffer, p1Buffer.Length, ref bytesRead);
                ReadProcessMemory((int)NppProcessHandle, (int)(FinalMemoryOffset + P2Offset), p2Buffer, p2Buffer.Length, ref bytesRead);
                ReadProcessMemory((int)NppProcessHandle, (int)(FinalMemoryOffset + P3Offset), p3Buffer, p3Buffer.Length, ref bytesRead);
                ReadProcessMemory((int)NppProcessHandle, (int)(FinalMemoryOffset + P4Offset), p4Buffer, p4Buffer.Length, ref bytesRead);

                var p1ReadTime = BitConverter.ToDouble(p1Buffer, 0);
                var p2ReadTime = BitConverter.ToDouble(p2Buffer, 0);
                var p3ReadTime = BitConverter.ToDouble(p3Buffer, 0);
                var p4ReadTime = BitConverter.ToDouble(p4Buffer, 0);

                var StartNewEpisode = p1ReadTime < P1CurrentTime
                                || p2ReadTime < P2CurrentTime
                                || p3ReadTime < P3CurrentTime
                                || p4ReadTime < P4CurrentTime;

                var valuesChanged = p1ReadTime != P1CurrentTime
                                || p2ReadTime != P2CurrentTime
                                || p3ReadTime != P3CurrentTime
                                || p4ReadTime != P4CurrentTime;

                if (StartNewEpisode)
                {
                    P1Time += P1CurrentTime;
                    P2Time += P2CurrentTime;
                    P3Time += P3CurrentTime;
                    P4Time += P4CurrentTime;
                }

                P1CurrentTime = p1ReadTime;
                P2CurrentTime = p2ReadTime;
                P3CurrentTime = p3ReadTime;
                P4CurrentTime = p4ReadTime;

                if (valuesChanged)
                    Dispatcher.BeginInvoke(new Action(UpdateTimeDisplay));
            }
        }

        void UpdateTimeDisplay()
        {
            PlayerTimes.Items.Clear();
            PlayerTimes.Items.Add(P1Name + ": " + P1DisplayTime);
            PlayerTimes.Items.Add(P2Name + ": " + P2DisplayTime);
            PlayerTimes.Items.Add(P3Name + ": " + P3DisplayTime);
            PlayerTimes.Items.Add(P4Name + ": " + P4DisplayTime);
        }

        void ResetTimes(object sender, RoutedEventArgs e)
        {
            P1Time = 0;
            P2Time = 0;
            P3Time = 0;
            P4Time = 0;
            UpdateTimeDisplay();
        }

        void UpdateNames(object sender, RoutedEventArgs e)
        {
            P1Name = string.IsNullOrEmpty(P1Text.Text) ? "P1" : P1Text.Text;
            P2Name = string.IsNullOrEmpty(P2Text.Text) ? "P2" : P2Text.Text;
            P3Name = string.IsNullOrEmpty(P3Text.Text) ? "P3" : P3Text.Text;
            P4Name = string.IsNullOrEmpty(P4Text.Text) ? "P4" : P4Text.Text;
            UpdateTimeDisplay();
        }

        void HookToGame(object sender, RoutedEventArgs e)
        {

        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Loop = false;
        }
    }
}
