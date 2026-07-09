using UnityEngine;
namespace Player.Weapon.Config
{
    [CreateAssetMenu(fileName = "NewRangedWeaponData", menuName = "Configs/Ranged Weapon Data")]
    public class RangedWeaponConfig : ScriptableObject
    {
        [Header("General")]
        public int maxAmmo = 6;
        public float fireRate = 0.5f;
        public float reloadDuration = 2f;

        [Header("Shooting & Projectile")]
        public GameObject paintballPrefab;
        public float bulletDuration = 0.3f;
        public float jumpPower = 0.5f;
        public LayerMask shootMask = ~0;

        [Header("Spread & Crosshair")]
        public float hipSpread = 0.04f;
        public float aimSpread = 0.002f;
        public Vector2 hipReticleSize = new Vector2(100f, 100f);
        public Vector2 aimReticleSize = new Vector2(25f, 25f);

        [Header("Transform Positions")]
        public Vector3 hipPosition;
        public Vector3 aimPosition;

        [Header("Recoil Animation")]
        public Vector3 recoilKickback;
        public Vector3 recoilRotation;
        public float recoilDuration = 0.1f;

        [Header("Mechanics Animation")]
        public Vector3 triggerPullRotation;
        public Vector3 hammerStrikeRotation;
        public Vector3 cylinderStepRotation;

        [Header("Audio")]
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip hitSound;
    }
}