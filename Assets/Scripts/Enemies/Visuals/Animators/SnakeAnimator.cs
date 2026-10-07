using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Gameplay.Enemies.Visuals.Animators
{
    public class SnakeAnimator : EnemyVisualsBase
    {
        [Header("Компоненты")]
        [SerializeField] private Transform _visualMesh;
        
        [Header("Настройки Змейки")]
        [SerializeField] private float _wiggleDistance = 0.5f;
        [SerializeField] private float _wiggleSpeed = 0.3f;

        private Sequence _snakeSequence;
        private Vector3 _initialLocalPos;
        private Vector3 _initialScale;

        protected override void Awake()
        {
            base.Awake();
            if (_visualMesh == null) _visualMesh = transform.Find("Visual");
            if (_visualMesh != null)
            {
                _initialLocalPos = _visualMesh.localPosition;
                _initialScale = _visualMesh.localScale;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_visualMesh != null)
            {
                _visualMesh.localPosition = _initialLocalPos;
                _visualMesh.localScale = _initialScale;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            KillSequence();
        }

        protected override void OnMoveStart()
        {
            KillSequence();
            
            if (_visualMesh != null)
            {
                _snakeSequence = DOTween.Sequence();
                
                _snakeSequence.Append(_visualMesh.DOLocalMoveX(_initialLocalPos.x + _wiggleDistance, _wiggleSpeed).SetEase(Ease.InOutSine));
                _snakeSequence.Append(_visualMesh.DOLocalMoveX(_initialLocalPos.x - _wiggleDistance, _wiggleSpeed).SetEase(Ease.InOutSine));
                
                _snakeSequence.SetLoops(-1, LoopType.Restart);
            }
        }

        protected override void OnStunned() => _snakeSequence?.Pause();
        protected override void OnReachedBase() => KillSequence();

        public override async UniTask PlayDeathAnimationAsync()
        {
            KillSequence(); 
            
            if (_visualMesh != null)
            {
                await _visualMesh.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
            }
        }

        private void KillSequence()
        {
            if (_snakeSequence != null && _snakeSequence.IsActive())
            {
                _snakeSequence.Kill();
                _snakeSequence = null;
            }
        }
    }
}