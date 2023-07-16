using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace RaceModeTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MemorySource MS = new MemorySource();
        LogWriter Log = new LogWriter();

        public string P1Name = "P1";
        public string P2Name = "P2";
        public string P3Name = "P3";
        public string P4Name = "P4";

        string P1DisplayTime => (MS.P1Time + MS.P1EpisodeTime).ToString("0.000");
        string P2DisplayTime => (MS.P2Time + MS.P2EpisodeTime).ToString("0.000");
        string P3DisplayTime => (MS.P3Time + MS.P3EpisodeTime).ToString("0.000");
        string P4DisplayTime => (MS.P4Time + MS.P4EpisodeTime).ToString("0.000");

        public MainWindow()
        {
            InitializeComponent();
            MS.HookMemory();
            UpdateTimeDisplay();
            Thread t = new Thread(UpdateThread);
            t.Start();
        }

        bool Loop;
        void UpdateThread()
        {
            Loop = true;
            while (Loop)
            {
                MS.UpdateThread();
                if (MS.ValuesChanged)
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
            Log.UpdateNamesFile(new List<string> { P1Name, P2Name, P3Name, P4Name });
            Log.UpdateScoresFile(new List<string> { P1DisplayTime, P2DisplayTime, P3DisplayTime, P4DisplayTime });
        }

        void ResetTimes(object sender, RoutedEventArgs e)
        {
            MS.ResetValues();
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
