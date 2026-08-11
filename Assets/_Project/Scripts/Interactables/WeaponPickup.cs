using UnityEngine;
using TpsShooter.Weapons.Core;

namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour
    {
        // Храним саму "живую" пушку
        public WeaponBase WeaponInstance { get; private set; }

        public void Initialize(WeaponBase weaponInstance)
        {
            WeaponInstance = weaponInstance;
        }

        public void Collect()
        {
            // Пушку мы не трогаем (она уходит в руки игрока),
            // уничтожаем только этот пустой объект-триггер, который служил для нее контейнером на земле
            Destroy(gameObject);
        }
    }
}