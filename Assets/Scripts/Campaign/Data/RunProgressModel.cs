using System.Collections.Generic;

namespace Gameplay.Campaign.Data 
{
    public class RunProgressModel
    {
        public RunMapModel CurrentMap { get; set; }
        public MapNode CurrentNode { get; set; }

        public int CurrentRunDepth { get; set; } = 0;
        public int CurrentRunGold { get; set; } = 0; 
        public List<string> ActiveRunItems { get; set; } = new List<string>();

        public void ResetRun()
        {
            CurrentRunDepth = 0;
            CurrentNode = null;
            CurrentMap = null;
            CurrentRunGold = 0;
            ActiveRunItems.Clear();

        }
    }
}



            