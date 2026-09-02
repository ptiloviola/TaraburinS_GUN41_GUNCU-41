using UnityEngine;
using DG.Tweening;
using Gameplay.Enemies.Data.Movement;
using Cysharp.Threading.Tasks;

namespace Gameplay.Enemies.Visuals
{
    public class JumperAnimator : EnemyVisualsBase
    {
        [Header("Компоненты")]
        [SerializeField] private Transform _visualMesh;
        
        [Header("Настройки деформации")]
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _squashAmount = 0.5f;
        [SerializeField] private float _stretchAmount = 1.5f;

        private Sequence _jumpSequence;
        private Vector3 _initialScale;
        private Vector3 _initialLocalPos;
        private DiscreteMovementStrategy _discreteStrategy;
        private DiscreteMovementConfig _config;

        protected override void Awake()
        {
            base.Awake();
            if (_visualMesh == null) _visualMesh = transform.Find("Visual");
            
            if (_visualMesh != null)
            {
                _initialScale = _visualMesh.localScale;
                _initialLocalPos = _visualMesh.localPosition; 
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_visualMesh != null)
            {
                
                _visualMesh.localScale = _initialScale;
                _visualMesh.localPosition = _initialLocalPos;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_discreteStrategy != null)
            {
                _discreteStrategy.OnJumpStart -= PlayJumpAnimation;
                _discreteStrategy = null;
            }
            KillSequence();
        }

        protected override void OnMoveStart()
        {
            // Пытаемся достать дискретную стратегию и её конфиг
            if (Facade.Config.Movement is DiscreteMovementConfig config)
            {
                _config = config;
                // Чтобы получить саму стратегию, нам нужно добавить публичный геттер в Фасад.
                // В EnemyFacade.cs добавь: public IMovementStrategy MovementStrategy => _movementStrategy;
                _discreteStrategy = Facade.MovementStrategy as DiscreteMovementStrategy;

                if (_discreteStrategy != null)
                {
                    _discreteStrategy.OnJumpStart += PlayJumpAnimation;
                }
            }
        }

        private void PlayJumpAnimation()
        {
            KillSequence();
            if (_visualMesh == null || _config == null) return;

            _jumpSequence = DOTween.Sequence();

            Vector3 squashScale = new Vector3(_initialScale.x * 1.3f, _initialScale.y * _squashAmount, _initialScale.z * 1.3f);
            Vector3 stretchScale = new Vector3(_initialScale.x * 0.8f, _initialScale.y * _stretchAmount, _initialScale.z * 0.8f);

            // Синхронизируем тайминги с логикой!
            float halfFlight = _config.JumpDuration / 2f;
            float impactTime = _config.PauseDuration * 0.3f; // Четверть паузы на сплющивание
            float recoverTime = _config.PauseDuration * 0.7f; // Остаток на выпрямление

            // 1. ВЗЛЕТ И ПОЛЕТ
            _jumpSequence.Append(_visualMesh.DOScale(stretchScale, halfFlight).SetEase(Ease.OutSine));
            _jumpSequence.Join(_visualMesh.DOLocalMoveY(_initialLocalPos.y + _jumpHeight, halfFlight).SetEase(Ease.OutQuad));

            // 2. ПАДЕНИЕ
            _jumpSequence.Append(_visualMesh.DOScale(_initialScale, halfFlight).SetEase(Ease.InSine));
            _jumpSequence.Join(_visualMesh.DOLocalMoveY(_initialLocalPos.y, halfFlight).SetEase(Ease.InQuad));

            // 3. ПРИЗЕМЛЕНИЕ И СЖАТИЕ (Начинается пауза в движении агента)
            _jumpSequence.Append(_visualMesh.DOScale(squashScale, impactTime).SetEase(Ease.OutQuad));
            
            // 4. ВЫПРЯМЛЕНИЕ ВО ВРЕМЯ ПАУЗЫ
            _jumpSequence.Append(_visualMesh.DOScale(_initialScale, recoverTime).SetEase(Ease.OutBack));
        }

        public override async UniTask PlayDeathAnimationAsync()
        {
            KillSequence(); // Останавливаем прыжок, если он был в процессе
            
            if (_visualMesh != null)
            {
                // Сжимаем в ноль и ждем
                await _visualMesh.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
            }
        }

        private void KillSequence()
        {
            if (_jumpSequence != null && _jumpSequence.IsActive())
            {
                _jumpSequence.Kill();
                _jumpSequence = null;
            }
        }
    }
}