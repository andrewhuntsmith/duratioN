using System.Collections.Generic;
using System.IO;

namespace RaceModeTimer
{
    class LogWriter
    {
        public string MatchPath = "LastMatch.txt";
        public string NamesPath = "Names.txt";
        public string ScoresPath = "Scores.txt";
        public string LogPath = "TimerLog.txt";

        public string P1Name = "1p name.txt";
        public string P2Name = "2p name.txt";
        public string P3Name = "3p name.txt";
        public string P4Name = "4p name.txt";

        public string P1Score = "1p score.txt";
        public string P2Score = "2p score.txt";
        public string P3Score = "3p score.txt";
        public string P4Score = "4p score.txt";

        public void WriteCurrentMatchToFile(string matchData)
        {
            File.WriteAllText(MatchPath, matchData);
        }

        public void UpdateNamesFile(List<string> names)
        {
            File.WriteAllText(P1Name, names[0]);
            File.WriteAllText(P2Name, names[1]);
            File.WriteAllText(P3Name, names[2]);
            File.WriteAllText(P4Name, names[3]);
        }

        public void UpdateScoresFile(List<string> scores)
        {
            File.WriteAllText(P1Score, scores[0]);
            File.WriteAllText(P2Score, scores[1]);
            File.WriteAllText(P3Score, scores[2]);
            File.WriteAllText(P4Score, scores[3]);
        }

        public void AppendLocalStatsFile(string matchData)
        {
            File.AppendAllText(LogPath, matchData);
        }
    }
}
