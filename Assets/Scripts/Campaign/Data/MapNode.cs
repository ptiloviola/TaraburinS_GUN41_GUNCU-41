using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Campaign.Data
{
    [Serializable]
    public class MapNode
    {
        public string Id;
        public int Depth;
        public Vector2 RenderPosition; 
        public List<string> NextNodeIds = new List<string>();

        [SerializeReference] 
        public INodeEncounter Encounter;
    }
}