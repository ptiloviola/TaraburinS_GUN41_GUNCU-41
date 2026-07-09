using UnityEngine;
namespace Player.Config
{
    [CreateAssetMenu(fileName = "NewRadarData", menuName = "Configs/Radar Data")]
    public class RadarConfig : ScriptableObject
    {
        [Header("Vision")]
        public float sightRadius = 10f;
        public LayerMask enemyLayer;
    }
}