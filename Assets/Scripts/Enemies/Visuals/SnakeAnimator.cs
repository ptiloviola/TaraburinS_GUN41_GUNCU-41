using UnityEngine;
using DG.Tweening;

namespace Gameplay.Enemies.Visuals
{
    public class SnakeAnimator : EnemyVisualsBase
    {
        [Header("Компоненты")]
        [SerializeField] private Transform _visualMesh;
        
        [Header("Настройки Змейки")]
        [SerializeField] private float _wiggleDistance = 0.5f; // Насколько сильно отклоняется в стороны
        [SerializeField] private float _wiggleSpeed = 0.3f; // Скорость одного колебания

        private Sequence _snakeSequence;
        private Vector3 _initialLocalPos;

        protected override void Awake()
        {
            base.Awake();
            if (_visualMesh == null) _visualMesh = transform.Find("Visual");
            if (_visualMesh != null) _initialLocalPos = _visualMesh.localPosition;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_visualMesh != null)
            {
                _visualMesh.localPosition = _initialLocalPos;
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
                
                // Двигаем меш по локальной оси X влево, затем вправо
                _snakeSequence.Append(_visualMesh.DOLocalMoveX(_initialLocalPos.x + _wiggleDistance, _wiggleSpeed).SetEase(Ease.InOutSine));
                _snakeSequence.Append(_visualMesh.DOLocalMoveX(_initialLocalPos.x - _wiggleDistance, _wiggleSpeed).SetEase(Ease.InOutSine));
                
                _snakeSequence.SetLoops(-1, LoopType.Restart);
            }
        }

        protected override void OnStunned() => _snakeSequence?.Pause();
        protected override void OnDeath() => KillSequence();
        protected override void OnReachedBase() => KillSequence();

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