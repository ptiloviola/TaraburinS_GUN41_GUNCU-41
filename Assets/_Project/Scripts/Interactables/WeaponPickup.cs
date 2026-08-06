using UnityEngine;
using TpsShooter.Weapons.Core;
using TpsShooter.Weapons.Configs;

namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour
    {
        // Храним данные, которые отдадим Инвентарю
        public WeaponConfig Config { get; private set; }
        public WeaponBase WeaponPrefab { get; private set; }

        public void Initialize(WeaponConfig config, WeaponBase weaponPrefab)
        {
            Config = config;
            WeaponPrefab = weaponPrefab;
        }

        // Этот метод вызовет игрок, когда подберет предмет
        public void Collect()
        {
            // Позже, когда добавим Object Pool, мы будем возвращать пикап в пул,
            // а не уничтожать. Пока для прототипа используем Destroy.
            Destroy(gameObject);
        }
    }
}