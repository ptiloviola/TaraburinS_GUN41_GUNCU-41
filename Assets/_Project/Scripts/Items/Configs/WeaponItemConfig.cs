using UnityEngine;
using TpsShooter.Weapons.Configs;
using TpsShooter.Weapons.Core;

namespace TpsShooter.Items.Configs
{
    [CreateAssetMenu(fileName = "WeaponItemConfig", menuName = "TpsShooter/Items/Weapon Drop")]
    public class WeaponItemConfig : ItemConfig
    {
        [Header("Weapon Logic")]
        [Tooltip("Ссылка на основной боевой конфиг пушки")]
        public WeaponConfig WeaponConfig; 
        public WeaponBase LiveWeaponPrefab;
    }
}