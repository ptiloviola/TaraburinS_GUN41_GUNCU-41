using UnityEngine;
using MeatMushrooms.Player.Configs;

namespace MeatMushrooms.Player.Components
{
    public class PlayerStealth : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private SphereCollider _noiseRadar;

        public SphereCollider NoiseRadar => _noiseRadar;

        public void UpdateNoiseLevel(bool isMoving, bool isRunning)
        {
            if (!isMoving)
            {
                _noiseRadar.radius = _config.IdleNoise;
            }
            else
            {
                _noiseRadar.radius = isRunning ? _config.RunNoise : _config.WalkNoise;
            }
        }

        private void OnDrawGizmos()
        {
            if (_noiseRadar != null)
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.3f); 
                Gizmos.DrawSphere(transform.position, _noiseRadar.radius);
            }
        }
    }
}