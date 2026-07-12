using UnityEngine;
namespace Enemy.Config
{
    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 3.5f;
        public float wanderRadius = 10f;
        public float wanderTimer = 4f;
        public float baseOffset = 0.5f;
        
        [Header("Hop Animation")]
        public float hopHeight = 0.3f;
        public float hopDuration = 0.2f;
        
        [Header("Inertia & Physics")]
        public Vector3 startImpulse = new Vector3(-20f, 0, 0);
        public Vector3 stopImpulse = new Vector3(25f, 0, 0);
        public float turnTiltAngle = 15f;
        
        [Space]
        public float inertiaDuration = 0.5f;
        public int inertiaVibrato = 4;
        [Range(0f, 1f)] public float inertiaElasticity = 0.6f;
        public float impulseDelayStep = 0.08f;
        
        [Header("Combat")]
        public int maxHealth = 100;

        [Header("Audio")]
        public AudioClip hopSound;
    }
}