using System.Collections.Generic;
using System.Linq;

namespace RaceModeTimer
{
    class EpisodeData
    {
        public List<LevelData> Levels;
        public double[] FinalScores;
        public double[][] TimeAdjustmentPerLevel;

        public EpisodeData()
        {
            Levels = new List<LevelData>();
            FinalScores = new double[4];
            TimeAdjustmentPerLevel = new double[5][];
        }

        public void StartNewLevel()
        {
            Levels.Add(new LevelData());
        }

        public void FinishCurrentLevel()
        {
            var priorTimes = Levels.Count > 1 ? TimeAdjustmentPerLevel[Levels.Count - 2] : new double[4] { 0, 0, 0, 0 };
            TimeAdjustmentPerLevel[Levels.Count - 1] = new double[4];
            for (var i = 0; i < 4; i++)
                TimeAdjustmentPerLevel[Levels.Count - 1][i] = priorTimes[i] + Levels.Last().TimeAdjustments[i];
            for (var i = 0; i < 4; i++)
                FinalScores[i] = TimeAdjustmentPerLevel[Levels.Count - 1][i];
        }

        public override string ToString()
        {
            var levelStrings = new List<string>();
            for (var level = 0; level < Levels.Count; level++)
            {
                levelStrings.Add("\nLevel " + level);
                var playerStrings = new List<string>();
                for (var player = 0; player < 4; player++)
                {
                    if (MainWindow.MS.PlayerActivity[player].Value)
                    {
                        var playerString = MainWindow.PlayerNames[player] + ": ";
                        playerString += Levels[level].DidFinish[player] ? "Finished" : "No finish";
                        playerString += "\n\tLevel Time Adjustment: " + Levels[level].TimeAdjustments[player];
                        if (TimeAdjustmentPerLevel[level] != null)
                            playerString += "\n\tEpisode Time Adjustement: " + TimeAdjustmentPerLevel[level][player];
                        playerString += "\n\tGold Collected: " + Levels[level].GoldCollected[player];
                        playerStrings.Add(playerString);
                    }
                }
                levelStrings.AddRange(playerStrings);
            }

            return "{" + string.Join("\n", levelStrings) + "\n}";
        }
    }
}
