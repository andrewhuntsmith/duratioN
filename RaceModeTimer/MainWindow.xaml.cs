using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace RaceModeTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<string> PlayerNames;

        public static MemorySource MS = new MemorySource();
        LogWriter Log = new LogWriter();
        List<MatchData> LocalMatches = new List<MatchData>();

        MatchData CurrentMatch;
        EpisodeData CurrentEpisode;
        LevelData CurrentLevel;

        public string P1Name = "P1";
        public string P2Name = "P2";
        public string P3Name = "P3";
        public string P4Name = "P4";

        string P1DisplayTime => (MS.P1Time + MS.P1EpisodeTime.Value).ToString("0.000");
        string P2DisplayTime => (MS.P2Time + MS.P2EpisodeTime.Value).ToString("0.000");
        string P3DisplayTime => (MS.P3Time + MS.P3EpisodeTime.Value).ToString("0.000");
        string P4DisplayTime => (MS.P4Time + MS.P4EpisodeTime.Value).ToString("0.000");

        public MainWindow()
        {
            PlayerNames = new List<string> { P1Name, P2Name, P3Name, P4Name };
            
            InitializeComponent();
            MS.HookMemory();
            MS.PlayerFinished += UpdatePlayersFinished;
            MS.LevelFinished += FinishCurrentLevel;
            MS.StartNewLevel += StartNewLevel;
            CurrentMatch = new MatchData();
            StartNewLevel();
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
            var matchStrings = CurrentMatch.ToString().Split('\n');
            foreach(var line in matchStrings)
                PlayerTimes.Items.Add(line);
            Log.UpdateNamesFile(names);
            Log.UpdateScoresFile(scores);
            Log.WriteStatsToFile(CurrentMatch.TimeStarted + "\n" + string.Join("\n", CurrentMatch));
        }

        void ResetTimes(object sender, RoutedEventArgs e)
        {
            MS.ResetValues();
            UpdateTimeDisplay();
            CurrentMatch = new MatchData();
            CurrentEpisode = new EpisodeData();
            CurrentLevel = new LevelData();
            CurrentEpisode.Levels.Add(CurrentLevel);
            CurrentMatch.Episodes.Add(CurrentEpisode);
        }

        void UpdateNames(object sender, RoutedEventArgs e)
        {
            P1Name = string.IsNullOrEmpty(P1Text.Text) ? "P1" : P1Text.Text;
            P2Name = string.IsNullOrEmpty(P2Text.Text) ? "P2" : P2Text.Text;
            P3Name = string.IsNullOrEmpty(P3Text.Text) ? "P3" : P3Text.Text;
            P4Name = string.IsNullOrEmpty(P4Text.Text) ? "P4" : P4Text.Text;
            PlayerNames = new List<string> { P1Name, P2Name, P3Name, P4Name };
            UpdateTimeDisplay();
        }

        void HookToGame(object sender, RoutedEventArgs e)
        {

        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Loop = false;
        }

        void FinishCurrentLevel()
        {
            CurrentEpisode?.FinishCurrentLevel();
            Dispatcher.BeginInvoke(new Action(UpdateTimeDisplay));
        }

        void StartNewLevel()
        {
            if (CurrentEpisode == null || CurrentEpisode.Levels.Count % 5 == 0)
            {
                CurrentMatch.Episodes.Add(new EpisodeData());
                CurrentEpisode = CurrentMatch.Episodes.Last();
            }

            CurrentEpisode.StartNewLevel();
            CurrentLevel = CurrentEpisode.Levels.Last();
            Dispatcher.BeginInvoke(new Action(UpdateTimeDisplay));
        }

        void UpdatePlayersFinished(int playerIndex, int frameCount, double bonus, int goldCollected)
        {
            CurrentLevel.DidFinish[playerIndex] = true;
            CurrentLevel.FinishTimes[playerIndex] = frameCount;
            CurrentLevel.TimeAdjustments[playerIndex] = bonus;
            CurrentLevel.GoldCollected[playerIndex] = goldCollected;
        }
    }
}
