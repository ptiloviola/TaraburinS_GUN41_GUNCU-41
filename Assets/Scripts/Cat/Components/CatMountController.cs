using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using VacuumSim.Cat.Brain;
using VacuumSim.Cat.Contracts;
using Zenject;
using VacuumSim.Robotics.Contracts;
using VacuumSim.GameConfigs;

namespace VacuumSim.Cat
{
    [RequireComponent(typeof(CatBrain), typeof(ICatObstacle), typeof(ICatView))]
    public class CatMountController : MonoBehaviour
    {
        [Header("Настройки Радара")]
        [SerializeField] private float _radarRadius = 3.0f;
        [SerializeField] private float _radarScanInterval = 0.5f;
        [SerializeField] private LayerMask _robotLayer;

        [Header("Настройки Езды")]
        [SerializeField] private float _rideDuration = 10.0f;
        [SerializeField] private float _jumpDuration = 0.4f;
        [SerializeField] private Vector3 _mountOffset = new Vector3(0, 0.35f, 0);

        [Inject] private IVacuumMotor _motor;
        [Inject] private IVacuumBattery _battery; // Убедись, что IVacuumBattery импортирован
        [Inject] private GameConfig _gameConfig;

        private CatBrain _brain;
        private ICatObstacle _obstacle;
        private ICatView _view;
        private Collider _physicalCollider; // Ссылка на физическое тело кота

        private bool _isRidingOrJumping = false;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _brain = GetComponent<CatBrain>();
            _obstacle = GetComponent<ICatObstacle>();
            _view = GetComponent<ICatView>();
            _physicalCollider = GetComponent<Collider>();
        }

        private void Start()
        {
            _cts = new CancellationTokenSource();
            RadarLoopAsync(_cts.Token).Forget();
        }

        private async UniTask RadarLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!_isRidingOrJumping)
                {
                    Collider[] hits = Physics.OverlapSphere(transform.position, _radarRadius, _robotLayer);
                    
                    if (hits.Length > 0)
                    {
                        Transform robotTransform = hits[0].transform;
                        MountRobotAsync(robotTransform).Forget();
                    }
                }
                await UniTask.Delay(System.TimeSpan.FromSeconds(_radarScanInterval), cancellationToken: token);
            }
        }

        private async UniTask MountRobotAsync(Transform robotTransform)
        {
            _isRidingOrJumping = true;
            Debug.Log("<color=orange>[Cat] Прыжок!</color>");

            _brain.StopBrain();
            _obstacle.SetObstacleActive(false);

            // ВЫКЛЮЧАЕМ ФИЗИКУ КОТА! Чтобы не взрывать физику робота изнутри.
            if (_physicalCollider != null) _physicalCollider.enabled = false;

            transform.SetParent(robotTransform);

            // НАЧИНАЕМ СИДЕТЬ ПРЯМО В ПОЛЕТЕ
            _view.PlaySitDown();
            _motor.SetSpeedMultiplier(_gameConfig.CatSpeedMultiplier);
            _battery.SetLoadMultiplier(_gameConfig.CatBatteryMultiplier);

            Vector3 startLocalPos = transform.localPosition;
            Quaternion startLocalRot = transform.localRotation;
            
            float elapsed = 0f;
            while (elapsed < _jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _jumpDuration;
                
                transform.localPosition = Vector3.Lerp(startLocalPos, _mountOffset, t);
                transform.localRotation = Quaternion.Slerp(startLocalRot, Quaternion.identity, t);
                
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            transform.localPosition = _mountOffset;
            transform.localRotation = Quaternion.identity;

            await UniTask.Delay(System.TimeSpan.FromSeconds(_rideDuration));

            if (this != null) 
            {
                DismountRobotAsync().Forget(); // Теперь спрыгивание асинхронное
            }
        }

        private async UniTask DismountRobotAsync()
        {
            Debug.Log("<color=orange>[Cat] Поездка окончена. Кот встает...</color>");
            
            // Сначала встаем, будучи еще на роботе
            _view.PlayStandUp();
            await UniTask.Delay(1000); // Ждем секунду, пока проиграется анимация

            // Теперь отвязываемся и спрыгиваем
            transform.SetParent(null);
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            _motor.SetSpeedMultiplier(1.0f);
            _battery.SetLoadMultiplier(1.0f);
            
            // ВКЛЮЧАЕМ ФИЗИКУ ОБРАТНО
            if (_physicalCollider != null) _physicalCollider.enabled = true;
            _obstacle.SetObstacleActive(true);
            
            _brain.StartBrain();
            
            UniTask.Delay(5000).ContinueWith(() => _isRidingOrJumping = false);
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, _radarRadius);
        }
    }
}