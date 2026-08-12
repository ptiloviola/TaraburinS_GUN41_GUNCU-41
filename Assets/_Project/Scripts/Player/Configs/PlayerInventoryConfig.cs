using System;
using System.Collections.Generic;
using UnityEngine;
using TpsShooter.Player.Inventory; // Для доступа к AmmoType

namespace TpsShooter.Player.Configs
{
    [Serializable]
    public struct AmmoLimit
    {
        public AmmoType Type;
        public int MaxCapacity;
    }

    [CreateAssetMenu(fileName = "PlayerInventoryConfig", menuName = "TpsShooter/PlayerConfigs/PlayerInventoryConfig")]
    public class PlayerInventoryConfig : ScriptableObject
    {
        [Header("Health Settings")]
        public float MaxHealth = 100f;
        public float StartingHealth = 50f; // Оставим 50 для тестов аптечек

        [Header("Ammo Settings")]
        public List<AmmoLimit> AmmoLimits = new List<AmmoLimit>
        {
            // Дефолтные значения, чтобы не настраивать с нуля
            new AmmoLimit { Type = AmmoType.Pistol, MaxCapacity = 60 },
            new AmmoLimit { Type = AmmoType.Rifle, MaxCapacity = 120 },
            new AmmoLimit { Type = AmmoType.Shotgun, MaxCapacity = 24 }
        };
    }
}