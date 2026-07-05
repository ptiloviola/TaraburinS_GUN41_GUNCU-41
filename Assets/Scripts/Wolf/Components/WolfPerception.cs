using MeatMushrooms.Player.Components;
using MeatMushrooms.Wolf.Configs;
using UnityEngine;
using Zenject;
using MeatMushrooms.Player;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfPerception : MonoBehaviour
    {
        private WolfConfig _config;
        private WolfStats _stats;
        
        private PlayerRegistry _playerRegistry;

        public float CurrentSuspicion { get; private set; }
        public Vector3 LastKnownPosition { get; private set; }
        public bool IsTargetInSight { get; private set; }
        public bool IsEating { get; set; }

        [Inject]
        public void Construct(WolfConfig config, WolfStats stats, PlayerRegistry playerRegistry)
        {
            _config = config;
            _stats = stats;
            _playerRegistry = playerRegistry;
        }

        private void Update()
        {
            ProcessHearing();
            ProcessVision();
        }

        private void ProcessHearing()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _playerRegistry.Controller.transform.position) - _playerRegistry.Stealth.NoiseRadar.radius;

            if (distanceToPlayer <= _playerRegistry.Stealth.NoiseRadar.radius)
            {
                float buildRate = _config.Perception.SuspicionBuildRate * Time.deltaTime;
                
                if (_stats.Hunger > _config.Perception.HungerThreshold) 
                    buildRate *= _config.Perception.HungrySuspicionMultiplier;
                
                if (IsEating) 
                    buildRate *= _config.Perception.EatingDistractionMultiplier;

                CurrentSuspicion = Mathf.Clamp(CurrentSuspicion + buildRate, 0f, 100f);
                LastKnownPosition = _playerRegistry.Controller.transform.position;
            }
            else
            {
                if (!IsTargetInSight)
                {
                    CurrentSuspicion = Mathf.Clamp(CurrentSuspicion - (_config.Perception.SuspicionDecayRate * Time.deltaTime), 0f, 100f);
                }
            }
        }

        private void ProcessVision()
        {
            IsTargetInSight = false;

            Vector3 dirToPlayer = (_playerRegistry.Controller.transform.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, _playerRegistry.Controller.transform.position);

            if (distanceToPlayer > _config.Perception.SightDistance) return;

            float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            if (angleToPlayer > _config.Perception.SightAngle / 2f) return;

            LayerMask combinedMask = _config.Perception.PlayerMask | _config.Perception.ObstacleMask;
            
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer, out RaycastHit hit, distanceToPlayer, combinedMask))
            {
                if ((_config.Perception.PlayerMask.value & (1 << hit.collider.gameObject.layer)) > 0)
                {
                    IsTargetInSight = true;
                    CurrentSuspicion = 100f; 
                    LastKnownPosition = _playerRegistry.Controller.transform.position;
                }
            }
        }

        public void ClearSuspicion()
        {
            CurrentSuspicion = 0f;
        }

        public void ReceiveAlert(Vector3 targetPosition)
        {
            CurrentSuspicion = Mathf.Clamp(CurrentSuspicion + 40f, 0f, 100f);
            
            LastKnownPosition = targetPosition;
        }
    }
}