using UnityEngine;
namespace Player
{
    [CreateAssetMenu(fileName = "NewPlayerConfig", menuName = "Configs/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float gravity = -9.81f;

        [Header("Look")]
        public float mouseSensitivity = 50f; // Чувствительность мыши

        [Header("Vision")]
        public float sightRadius = 15f;
        public LayerMask enemyLayer;

        [Header("Ammo & Reload")]
        public int maxAmmo = 6;
        public float reloadDuration = 1.5f; // Сколько длится перезарядка
        
        [Header("Audio")]
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip hitSound; // Сразу заложим для следующего этапа

        [Header("Weapon Settings")]
        public float fireRate = 0.5f;
        public LayerMask shootMask = ~0;
        
        [Header("Weapon Recoil")]
        public Vector3 recoilKickback = new Vector3(0, 0, -0.15f); // Толчок назад
        public Vector3 recoilRotation = new Vector3(-15f, 0, 0);   // Задирание ствола вверх
        public float recoilDuration = 0.1f;

        [Header("Weapon Positioning")]
        [Tooltip("Позиция оружия 'от бедра'")]
        public Vector3 hipPosition = new Vector3(0.3f, -0.3f, 0.6f); 
        [Tooltip("Позиция оружия в прицеле")]
        public Vector3 aimPosition = new Vector3(0f, -0.15f, 0.4f);

        [Header("Weapon Mechanics (Local Rotation)")]
        [Tooltip("Вращение спускового крючка при нажатии")]
        public Vector3 triggerPullRotation = new Vector3(-20f, 0, 0); 

        [Tooltip("Вращение бойка (курка) при ударе")]
        public Vector3 hammerStrikeRotation = new Vector3(30f, 0, 0); 

        [Tooltip("Шаг поворота барабана за один выстрел")]
        public Vector3 cylinderStepRotation = new Vector3(0, 0, 60f);

        [Header("Paintball Settings")]
        public GameObject paintballPrefab; // Префаб нашего шарика
        public float bulletDuration = 0.3f; // Время полета пули
        public float jumpPower = 0.5f;      // Высота дуги баллистики

        [Header("Reticle & Spread")]
        [Tooltip("Разброс от бедра (0.05 означает отклонение на 5% от центра экрана)")]
        public float hipSpread = 0.04f;
        [Tooltip("Разброс в прицеле")]
        public float aimSpread = 0.002f;

        [Tooltip("Размер круга прицела в пикселях от бедра")]
        public Vector2 hipReticleSize = new Vector2(100f, 100f);
        [Tooltip("Размер круга прицела в пикселях при ADS")]
        public Vector2 aimReticleSize = new Vector2(25f, 25f);
    }
}