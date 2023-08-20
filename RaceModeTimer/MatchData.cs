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

        public override string ToString()
        {
            var episodeStrings = new List<string>();
            foreach(var episode in Episodes)
                episodeStrings.Add(episode.ToString());

            return string.Join(",\n\n", episodeStrings);
        }
    }
}
