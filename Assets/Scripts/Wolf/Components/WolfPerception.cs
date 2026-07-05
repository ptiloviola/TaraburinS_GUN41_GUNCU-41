using MeatMushrooms.Player.Components; // Не забудь namespace игрока
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
        
        // Ссылки на игрока (теперь они заполнятся автоматически!)
        private PlayerRegistry _playerRegistry;

        public float CurrentSuspicion { get; private set; }
        public Vector3 LastKnownPosition { get; private set; }
        public bool IsTargetInSight { get; private set; }
        public bool IsEating { get; set; }

        // Zenject сам найдет PlayerController и вставит его сюда
        [Inject]
        public void Construct(WolfConfig config, WolfStats stats, PlayerRegistry playerRegistry)
        {
            _config = config;
            _stats = stats;
            _playerRegistry = playerRegistry;
        }

        private void Update()
        {
            // Теперь нам не нужно проверять _playerTransform == null, 
            // Zenject гарантирует, что игрок тут есть.
            ProcessHearing();
            ProcessVision();
        }

        private void ProcessHearing()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _playerRegistry.Controller.transform.position) - _playerRegistry.Stealth.NoiseRadar.radius;

            // Обращаемся к радару напрямую через наше новое свойство
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
            
            // Чуть приподнимаем точку, откуда пускаем луч, чтобы не стрелять из пяток волка
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

        // Метод, который вызовет другой волк при обнаружении
        public void ReceiveAlert(Vector3 targetPosition)
        {
            // Мгновенно накидываем подозрение (чтобы перебить текущие дела)
            CurrentSuspicion = Mathf.Clamp(CurrentSuspicion + 40f, 0f, 100f);
            
            // Волк узнает, где видели Шапочку, даже не видя её сам!
            LastKnownPosition = targetPosition;
        }
    }
}