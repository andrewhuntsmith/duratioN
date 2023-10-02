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

        public void WriteCurrentMatchToFile(string matchData)
        {
            File.WriteAllText(MatchPath, matchData);
        }

        public void UpdateNamesFile(List<string> names)
        {
            File.WriteAllLines(NamesPath, names);
        }

        public void UpdateScoresFile(List<string> scores)
        {
            File.WriteAllLines(ScoresPath, scores);
        }

        public void AppendLocalStatsFile(string matchData)
        {
            File.AppendAllText(LogPath, matchData);
        }
    }
}
