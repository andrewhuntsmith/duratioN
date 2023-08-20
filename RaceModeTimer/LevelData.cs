using System.Collections.Generic;

namespace RaceModeTimer
{
    class LevelData
    {
        public int[] FinishTimes;
        public bool[] DidFinish;
        public int[] GoldCollected;
        public double[] TimeAdjustments;

        public LevelData()
        {
            FinishTimes = new int[4];
            DidFinish = new bool[4];
            GoldCollected = new int[4];
            TimeAdjustments = new double[4];
        }

        public override string ToString()
        {
            var playerStrings = new List<string>();
            for (var i = 0; i < 4; i++)
            {
                var player = "p" + (i+1) + ": ";
                player += DidFinish[i] ? "Finished, " : "No finish, ";
                player += TimeAdjustments[i];
                playerStrings.Add(player);
            }

            return "[\n" + string.Join(",\n", playerStrings) + "\n]";
        }
    }
}
