using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Modifiers.Data
{
    [CreateAssetMenu(fileName = "ItemRegistry", menuName = "TD/Modifiers/Item Registry")]
    public class ItemRegistry : ScriptableObject
    {
        [SerializeField] private List<ItemConfig> _items = new List<ItemConfig>();

        public ItemConfig GetItem(string id)
        {
            return _items.FirstOrDefault(item => item.ItemId == id);
        }
    }
}