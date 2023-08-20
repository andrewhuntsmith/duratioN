using System.Collections.Generic;

namespace RaceModeTimer
{
    class EpisodeData
    {
        public List<LevelData> Levels;
        public float[] FinalScores;

        public EpisodeData()
        {
            Levels = new List<LevelData>();
            FinalScores = new float[4];
        }

        public override string ToString()
        {
            var levelStrings = new List<string>();
            foreach (var level in Levels)
                levelStrings.Add(level.ToString());

            return "{\n" + string.Join(",\n", levelStrings) + "\n}";
        }
    }
}
