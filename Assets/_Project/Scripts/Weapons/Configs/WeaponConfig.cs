using UnityEngine;

namespace TpsShooter.Weapons.Configs
{
    [CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "TpsShooter/Weapons/BaseConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("General Stats")]
        public string WeaponName;
        public float Damage = 10f;
        public float Range = 100f;
        
        [Header("Ammo")]
        public int AmmoPerClip = 30; // В магазине
        public int MaxReserveAmmo = 90; // Запас патронов (в рюкзаке)
        
        [Header("Timing")]
        public float FireRate = 0.1f;
        public float ReloadTime = 2f;
        
        [Header("Masks & Physics")]
        // Маска, чтобы пуля не попадала в самого игрока или в триггеры
        public LayerMask HitMask; 
        
        [Header("VFX & SFX Prefabs")]
        public GameObject MuzzleFlashPrefab; 
        public GameObject BulletDecalPrefab; // Дырка от пули (для пула)
        public AudioClip FireSound;
        public AudioClip EmptyClickSound; // Звук "нет патронов"
        public AudioClip ReloadSound;
    }
}