using System.Collections.Generic;
using UnityEngine;
using Gameplay.Modifiers.Data;

namespace Gameplay.Combat.Data
{
    [CreateAssetMenu(fileName = "NewShopConfig", menuName = "TD/Campaign/Shop Config")]
    public class ShopConfig : ScriptableObject
    {
        [Header("Ассортимент магазина")]
        public List<ItemConfig> AvailableItems = new List<ItemConfig>();
    }
}