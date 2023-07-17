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
                if (MS.EpisodeTimeValuesChanged || MS.PlayerActivityChanged)
                    Dispatcher.BeginInvoke(new Action(UpdateTimeDisplay));
            }
        }

        void UpdateTimeDisplay()
        {
            PlayerTimes.Items.Clear();
            var names = new List<string>();
            var scores = new List<string>();
            if (MS.P1Active)
            {
                PlayerTimes.Items.Add(P1Name + ": " + P1DisplayTime);
                names.Add(P1Name);
                scores.Add(P1DisplayTime);
            }
            if (MS.P2Active)
            {
                PlayerTimes.Items.Add(P2Name + ": " + P2DisplayTime);
                names.Add(P2Name);
                scores.Add(P2DisplayTime);
            }
            if (MS.P3Active)
            {
                PlayerTimes.Items.Add(P3Name + ": " + P3DisplayTime);
                names.Add(P3Name);
                scores.Add(P3DisplayTime);
            }
            if (MS.P4Active)
            {
                PlayerTimes.Items.Add(P4Name + ": " + P4DisplayTime);
                names.Add(P4Name);
                scores.Add(P4DisplayTime);
            }
            Log.UpdateNamesFile(names);
            Log.UpdateScoresFile(scores);
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
