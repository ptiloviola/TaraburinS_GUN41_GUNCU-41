using System.Collections.Generic;
using Gameplay.Combat.Data;

namespace Gameplay.Combat.Data
{
    public class RunProgressModel
    {
        public MapNode CurrentNode { get; set; }

        public int CurrentRunDepth { get; set; } = 0;
        
        public int CurrentRunGold { get; set; } = 0; 
        public List<string> ActiveRunItems { get; set; } = new List<string> { "StrongHeart", "CashMachine" };

        public void ResetRun()
        {
            CurrentRunDepth = 0;
            CurrentNode = null;
            CurrentRunGold = 0;
            ActiveRunItems.Clear();

            // ВРЕМЕННЫЙ ЧИТ ДЛЯ ТЕСТОВ
            ActiveRunItems.Add("StrongHeart"); 
            ActiveRunItems.Add("CashMachine");
        }
    }
}