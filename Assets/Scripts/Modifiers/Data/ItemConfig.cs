using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Modifiers.Data
{
    [CreateAssetMenu(fileName = "NewItemConfig", menuName = "TD/Modifiers/Item Config")]
    public class ItemConfig : ScriptableObject
    {
        public string ItemId;
        public string DisplayName;
        [TextArea] public string Description;

        public int BaseCost = 50; 
        public Sprite Icon;
        
        public List<StatModifierData> Modifiers = new List<StatModifierData>();
    }
}