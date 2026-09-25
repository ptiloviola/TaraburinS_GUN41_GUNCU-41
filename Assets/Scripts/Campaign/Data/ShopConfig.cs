using System.Collections.Generic;
using UnityEngine;
using Gameplay.Modifiers.Data;

namespace Gameplay.Campaign.Data
{
    [CreateAssetMenu(fileName = "NewShopConfig", menuName = "TD/Campaign/Shop Config")]
    public class ShopConfig : ScriptableObject
    {
        [Header("Визуал на Карте")]
        public Sprite MapIcon;

        [Header("Ассортимент магазина")]
        public List<ItemConfig> AvailableItems = new List<ItemConfig>();
    }
}