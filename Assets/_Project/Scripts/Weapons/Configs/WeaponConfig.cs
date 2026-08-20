using UnityEngine;
using System;
using System.Collections.Generic;
using TpsShooter.Player.Inventory;

namespace TpsShooter.Weapons.Configs
{
    [Serializable]
    public struct SurfaceEffect
    {
        public string SurfaceTag;         
        public GameObject DecalPrefab;    
        public GameObject ParticlePrefab; 
        public string ImpactSoundId;
    }

    public class WeaponConfig : ScriptableObject
    {
        [Header("General Stats")]
        public string WeaponName;
        public float Damage = 10f;
        public float Range = 100f; 

        [Header("Firing Mode")]
        public bool IsAutomatic = true; 

        [Header("Ammo & Magazine")]
        public AmmoType WeaponAmmoType;
        public int AmmoPerClip = 30; 
        public int MaxReserveAmmo = 90; 

        [Header("Timing")]
        public float FireRate = 0.1f; 
        public float ReloadTime = 2f;

        [Header("Accuracy, Spread & Recoil")]
        public float BaseSpread = 0f;          
        public float MaxSpread = 0f;           
        public float SpreadIncreaseRate = 0f;  
        public float SpreadRecoveryRate = 0f;  
        public float RecoilForce = 0f;         

        [Header("Projectiles (If applicable)")]
        public GameObject ProjectilePrefab; 

        [Header("Masks")]
        public LayerMask HitMask;

        [Header("VFX Prefabs")]
        public GameObject MuzzleFlashPrefab; 
        public GameObject TracerPrefab;      
        public GameObject ShellCasingPrefab; 
        public GameObject SmokePrefab;       

        [Header("Surface Impacts")]
        public List<SurfaceEffect> SurfaceEffects; 

        [Header("SFX (Audio IDs)")]
        public string FireSoundId = "Rifle_Fire";
        public string EmptyClickSoundId = "Weapon_Empty";
        public string ReloadSoundId = "Weapon_Reload";
        public string PickupSoundId = "Item_Pickup";
    }
}