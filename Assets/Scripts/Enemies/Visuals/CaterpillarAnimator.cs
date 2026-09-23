using UnityEngine;
using DG.Tweening;
using Gameplay.Enemies.Data.Movement;
using Cysharp.Threading.Tasks;

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
            transform.localScale = Vector3.one;
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
                    _discreteStrategy.OnJumpStart += PlayImpulseAnimation;
                    _discreteStrategy.OnPauseStart += PlayPreparationAnimation;
                    
                    PlayPreparationAnimation();
                }
            }
        }

        private void PlayPreparationAnimation()
        {
            KillSequence();
            if (_config == null) return;

            _crawlSequence = DOTween.Sequence();
            float duration = _config.PauseDuration;

            for (int i = 1; i < _segments.Length; i++)
            {
                if (_segments[i] == null) continue;

                
                float progress = (float)i / (_segments.Length - 1); 
                
                Vector3 targetPos = _initialLocalPos[i];
                
                targetPos.z += _tailPullDistance * progress; 
                
                targetPos.y += Mathf.Sin(progress * Mathf.PI) * _archHeight;

                _crawlSequence.Join(_segments[i].DOLocalMove(targetPos, duration).SetEase(Ease.InOutQuad));
            }
        }

        private void PlayImpulseAnimation()
        {
            KillSequence();
            if (_config == null) return;

            _crawlSequence = DOTween.Sequence();
            float duration = _config.JumpDuration;

            for (int i = 1; i < _segments.Length; i++)
            {
                if (_segments[i] == null) continue;
                _crawlSequence.Join(_segments[i].DOLocalMove(_initialLocalPos[i], duration).SetEase(Ease.OutQuad));
            }
        }

        protected override void OnStunned() => _crawlSequence?.Pause();
        protected override void OnReachedBase() => KillSequence();

        public override async UniTask PlayDeathAnimationAsync()
        {
            KillSequence();
            
            await transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
        }

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