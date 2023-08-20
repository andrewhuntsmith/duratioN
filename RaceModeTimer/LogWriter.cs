using System.Collections.Generic;
using System.IO;

namespace RaceModeTimer
{
    class LogWriter
    {
        public string LogPath = "TimerLog.txt";
        public string NamesPath = "Names.txt";
        public string ScoresPath = "Scores.txt";

        public void WriteStatsToFile(string matchData)
        {
            File.WriteAllText(LogPath, matchData);
        }

        public void UpdateNamesFile(List<string> names)
        {
            File.WriteAllLines(NamesPath, names);
        }

        public void UpdateScoresFile(List<string> scores)
        {
            File.WriteAllLines(ScoresPath, scores);
        }
    }
}
