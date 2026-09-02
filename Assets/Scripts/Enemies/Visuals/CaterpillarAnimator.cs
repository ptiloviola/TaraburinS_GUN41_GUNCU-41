using UnityEngine;
using DG.Tweening;
using Gameplay.Enemies.Data.Movement;

namespace Gameplay.Enemies.Visuals
{
    public class CaterpillarAnimator : EnemyVisualsBase
    {
        [Header("Сегменты (От головы [0] к хвосту)")]
        [SerializeField] private Transform[] _segments;

        [Header("Настройки Гусеничного Хода")]
        [Tooltip("Насколько хвост подтягивается к голове при подготовке (по локальной оси Z)")]
        [SerializeField] private float _tailPullDistance = 0.8f; 
        [Tooltip("Высота 'горба' в центре тела при сжатии (по локальной оси Y)")]
        [SerializeField] private float _archHeight = 0.6f;

        private Sequence _crawlSequence;
        private Vector3[] _initialLocalPos;
        
        private DiscreteMovementStrategy _discreteStrategy;
        private DiscreteMovementConfig _config;

        protected override void Awake()
        {
            base.Awake();
            
            _initialLocalPos = new Vector3[_segments.Length];
            
            for (int i = 0; i < _segments.Length; i++)
            {
                if (_segments[i] != null)
                {
                    _initialLocalPos[i] = _segments[i].localPosition;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            for (int i = 0; i < _segments.Length; i++)
            {
                if (_segments[i] != null)
                {
                    _segments[i].localPosition = _initialLocalPos[i];
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_discreteStrategy != null)
            {
                _discreteStrategy.OnJumpStart -= PlayImpulseAnimation;
                _discreteStrategy.OnPauseStart -= PlayPreparationAnimation;
                _discreteStrategy = null;
            }
            KillSequence();
        }

        protected override void OnMoveStart()
        {
            if (Facade.Config.Movement is DiscreteMovementConfig config)
            {
                _config = config;
                _discreteStrategy = Facade.MovementStrategy as DiscreteMovementStrategy;

                if (_discreteStrategy != null)
                {
                    // Подписываемся на обе фазы движения
                    _discreteStrategy.OnJumpStart += PlayImpulseAnimation;
                    _discreteStrategy.OnPauseStart += PlayPreparationAnimation;
                    
                    // При спавне стратегия сразу находится в состоянии паузы, поэтому дергаем ручку сами
                    PlayPreparationAnimation();
                }
            }
        }

        // ФАЗА 1: Сжатие (Агент стоит на месте)
        private void PlayPreparationAnimation()
        {
            KillSequence();
            if (_config == null) return;

            _crawlSequence = DOTween.Sequence();
            float duration = _config.PauseDuration;

            // Начинаем с i=1, так как Голова (i=0) остается на месте
            for (int i = 1; i < _segments.Length; i++)
            {
                if (_segments[i] == null) continue;

                // Вычисляем процент от головы до хвоста (0 - голова, 1 - самый кончик хвоста)
                float progress = (float)i / (_segments.Length - 1); 
                
                Vector3 targetPos = _initialLocalPos[i];
                
                // 1. Подтягиваем сегмент вперед (по оси Z)
                targetPos.z += _tailPullDistance * progress; 
                
                // 2. Формируем дугу (Парабола через синус: в центре максимум, по краям 0)
                targetPos.y += Mathf.Sin(progress * Mathf.PI) * _archHeight;

                _crawlSequence.Join(_segments[i].DOLocalMove(targetPos, duration).SetEase(Ease.InOutQuad));
            }
        }

        // ФАЗА 2: Выпрямление (Агент совершает рывок вперед)
        private void PlayImpulseAnimation()
        {
            KillSequence();
            if (_config == null) return;

            _crawlSequence = DOTween.Sequence();
            float duration = _config.JumpDuration;

            // Возвращаем все сегменты на их изначальные локальные места.
            // Так как корень летит вперед, возврат хвоста назад создаст иллюзию неподвижности.
            for (int i = 1; i < _segments.Length; i++)
            {
                if (_segments[i] == null) continue;
                _crawlSequence.Join(_segments[i].DOLocalMove(_initialLocalPos[i], duration).SetEase(Ease.OutQuad));
            }
        }

        protected override void OnStunned() => _crawlSequence?.Pause();
        protected override void OnDeath() => KillSequence();
        protected override void OnReachedBase() => KillSequence();

        private void KillSequence()
        {
            if (_crawlSequence != null && _crawlSequence.IsActive())
            {
                _crawlSequence.Kill();
                _crawlSequence = null;
            }
        }
    }
}