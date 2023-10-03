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

        MatchData CurrentMatch;
        EpisodeData CurrentEpisode => CurrentMatch.Episodes.Any() ? CurrentMatch.Episodes.Last() : null;
        LevelData CurrentLevel;

        public string P1Name = "P1";
        public string P2Name = "P2";
        public string P3Name = "P3";
        public string P4Name = "P4";

        string P1DisplayTime => CurrentMatch.GetScores()[0].ToString("0.000");
        string P2DisplayTime => CurrentMatch.GetScores()[1].ToString("0.000");
        string P3DisplayTime => CurrentMatch.GetScores()[2].ToString("0.000");
        string P4DisplayTime => CurrentMatch.GetScores()[3].ToString("0.000");

        public MainWindow()
        {
            PlayerNames = new List<string> { P1Name, P2Name, P3Name, P4Name };
            
            InitializeComponent();
            MS.HookMemory();
            MS.PlayerFinished += UpdatePlayersFinished;
            MS.LevelFinished += FinishCurrentLevel;
            MS.StartNewLevel += StartNewLevel;
            MS.EpisodeStarted += StartNewEpisode;
            CurrentMatch = new MatchData();
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

            names.Add(P1Name);
            scores.Add(P1DisplayTime);
            names.Add(P2Name);
            scores.Add(P2DisplayTime);
            names.Add(P3Name);
            scores.Add(P3DisplayTime);
            names.Add(P4Name);
            scores.Add(P4DisplayTime);
            
            var matchStrings = CurrentMatch.ToString().Split('\n');
            foreach(var line in matchStrings)
                PlayerTimes.Items.Add(line);
            Log.UpdateNamesFile(names);
            Log.UpdateScoresFile(scores);
            Log.WriteCurrentMatchToFile(CurrentMatch.TimeStarted + "\n" + string.Join("\n", CurrentMatch));
        }

        void ResetMatchButtonPressed(object sender, RoutedEventArgs e)
        {
            ResetMatch();
        }

        void ResetEpisodeButtonPressed(object sender, RoutedEventArgs e)
        {
            MS.ResetValues();
            CurrentMatch.Episodes.Remove(CurrentMatch.Episodes.Last());
            UpdateTimeDisplay();
        }

        void EndMatchButtonPressed(object sender, RoutedEventArgs e)
        {
            Log.AppendLocalStatsFile("\n\n" + CurrentMatch.TimeStarted + "\n" + string.Join("\n", CurrentMatch));
            ResetMatch();
        }

        void ResetMatch()
        {
            MS.ResetValues();
            CurrentMatch = new MatchData();
            UpdateTimeDisplay();
        }

        void UpdateNamesButtonPressed(object sender, RoutedEventArgs e)
        {
            P1Name = string.IsNullOrEmpty(P1Text.Text) ? "P1" : P1Text.Text;
            P2Name = string.IsNullOrEmpty(P2Text.Text) ? "P2" : P2Text.Text;
            P3Name = string.IsNullOrEmpty(P3Text.Text) ? "P3" : P3Text.Text;
            P4Name = string.IsNullOrEmpty(P4Text.Text) ? "P4" : P4Text.Text;
            PlayerNames = new List<string> { P1Name, P2Name, P3Name, P4Name };
            UpdateTimeDisplay();
        }

        void HookToGameButtonPressed(object sender, RoutedEventArgs e)
        {

        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Loop = false;
        }

        void FinishCurrentLevel()
        {
            if (!CurrentEpisode.Levels.Any())
                StartNewLevel();
            CurrentEpisode?.FinishCurrentLevel();
            Dispatcher.BeginInvoke(new Action(UpdateTimeDisplay));
        }

        void StartNewEpisode()
        {
            CurrentMatch.Episodes.Add(new EpisodeData());
        }

        void StartNewLevel()
        {
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
