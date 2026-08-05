using UnityEngine;

namespace TpsShooter.Weapons.Configs
{
    // Добавляем атрибут, чтобы создавать конфиги через Create -> TpsShooter -> Weapons -> BaseConfig
    [CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "TpsShooter/Weapons/BaseConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("General Weapon Stats")]
        public string WeaponName;
        public int MaxAmmo = 30;
        public int AmmoPerClip = 30;
        public float FireRate = 0.1f;
        public float ReloadTime = 2f;
        
        [Header("VFX & SFX Prefabs")]
        public GameObject MuzzleFlashPrefab; 
        public AudioClip FireSound;
    }
}