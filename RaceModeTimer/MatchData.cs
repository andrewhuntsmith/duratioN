using System;
using System.Collections.Generic;

namespace RaceModeTimer
{
    class MatchData
    {
        public List<EpisodeData> Episodes;
        public DateTime TimeStarted;

        public MatchData()
        {
            Episodes = new List<EpisodeData>();
            TimeStarted = DateTime.Now;
        }

        public double[] GetScores()
        {
            var scores = new double[4];
            foreach (var episode in Episodes)
                for (var i = 0; i < 4; i++)
                    scores[i] += episode.FinalScores[i];
            return scores;
        }

        public override string ToString()
        {
            var totalScoreStrings = new List<string>();

            var scores = GetScores();
            for (var player = 0; player < 4; player++)
                if (MainWindow.MS.PlayerActivity[player].Value)
                    totalScoreStrings.Add(MainWindow.PlayerNames[player] + ": " + scores[player]);

            var episodeStrings = new List<string>();
            foreach(var episode in Episodes)
                episodeStrings.Add(episode.ToString());

            return string.Join("\n", totalScoreStrings) + "\n\n" + string.Join(",\n\n", episodeStrings);
        }
    }
}
