using System;

namespace Gameplay.Core.Data
{
    [Serializable]
    public class PlayerProfileModel
    {
        public int MaxCompletedLevelIndex;
        public int TotalRunsPlayed;
        
        public int MetaCurrency;

        public PlayerProfileModel()
        {
            MaxCompletedLevelIndex = 0;
            TotalRunsPlayed = 0;
            MetaCurrency = 0;
        }
    }
}