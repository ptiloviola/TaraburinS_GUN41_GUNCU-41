using System;
using System.Collections.Generic;

namespace Gameplay.Campaign.Data
{
    [Serializable]
    public class RunSaveData
    {
        public string CurrentNodeId = string.Empty; 
        
        public int CurrentRunDepth = 0;
        public int CurrentRunGold = 0;
        
        public List<string> PathHistory = new List<string>();
        public List<string> ActiveRunItems = new List<string>();
    }
}