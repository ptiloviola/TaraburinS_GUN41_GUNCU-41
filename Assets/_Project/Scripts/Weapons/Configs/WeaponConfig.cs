using UnityEngine;
using System;
using System.Collections.Generic;

namespace TpsShooter.Weapons.Configs
{
    // Структура для эффектов попадания по разным поверхностям
    [Serializable]
    public struct SurfaceEffect
    {
        public string SurfaceTag;         // Тег или слой (например, "Metal", "EnemyFlesh")
        public GameObject DecalPrefab;    // Дырка/порез
        public GameObject ParticlePrefab; // Искры/кровь
        public AudioClip ImpactSound;     // Звук попадания
    }

    // Базовый класс без CreateAssetMenu (мы будем создавать только конкретных наследников)
    public class WeaponConfig : ScriptableObject
    {
        [Header("General Stats")]
        public string WeaponName;
        public float Damage = 10f;
        public float Range = 100f; // Дальность

        [Header("Ammo & Magazine")]
        public int AmmoPerClip = 30; // Магазин
        public int MaxReserveAmmo = 90; // Запас

        [Header("Timing")]
        public float FireRate = 0.1f; // Темп стрельбы
        public float ReloadTime = 2f;

        [Header("Accuracy, Spread & Recoil")]
        public float BaseSpread = 0f;          // Базовый разброс
        public float MaxSpread = 0f;           // Макс. разброс при зажиме
        public float SpreadIncreaseRate = 0f;  // Скорость роста
        public float SpreadRecoveryRate = 0f;  // Скорость восстановления
        public float RecoilForce = 0f;         // Отдача

        [Header("Projectiles (If applicable)")]
        public GameObject ProjectilePrefab; // Префаб снаряда (для гранатомета/дробовика)

        [Header("Masks")]
        public LayerMask HitMask;

        [Header("VFX Prefabs (Pool ready)")]
        public GameObject MuzzleFlashPrefab; // Вспышка выстрела
        public GameObject TracerPrefab;      // Трассер / след луча
        public GameObject ShellCasingPrefab; // Вылетающие гильзы
        public GameObject SmokePrefab;       // Дым из ствола

        [Header("Surface Impacts")]
        public List<SurfaceEffect> SurfaceEffects; // Реакция на разные материалы

        [Header("SFX")]
        public AudioClip FireSound;
        public AudioClip EmptyClickSound;
        public AudioClip[] ReloadSounds; // Массив для многофазной перезарядки (щелчок -> затвор)
    }
}