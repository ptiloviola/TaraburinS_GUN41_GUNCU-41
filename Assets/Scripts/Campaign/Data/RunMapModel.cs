using System.Collections.Generic;

namespace Gameplay.Campaign.Data
{
    public class RunMapModel
    {
        public Dictionary<string, MapNode> Nodes { get; set; } = new Dictionary<string, MapNode>();
        public List<string> StartingNodeIds { get; set; } = new List<string>();
    }
}