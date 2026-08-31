using UnityEngine;
using DG.Tweening;

namespace Gameplay.Enemies.Visuals
{
    public class SlinkyAnimator : EnemyVisualsBase
    {
        [Header("Настройки анимации")]
        [SerializeField] private Transform _visualMesh;
        [SerializeField] private float _stepDuration = 1f; 
        [SerializeField] private float _stretchMultiplier = 2f; 

        private Sequence _slinkySequence;
        private Vector3 _initialScale;

        protected override void Awake()
        {
            base.Awake();
            if (_visualMesh == null) _visualMesh = transform.Find("Visual");
            if (_visualMesh != null) _initialScale = _visualMesh.localScale; 
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_visualMesh != null)
            {
                _visualMesh.localRotation = Quaternion.identity;
                _visualMesh.localScale = _initialScale; 
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            KillSequence();
        }

        // --- РЕАКЦИИ НА СМЕНУ СОСТОЯНИЙ ---
        protected override void OnMoveStart()
        {
            if (_slinkySequence != null && !_slinkySequence.IsPlaying())
            {
                _slinkySequence.Play();
            }
            else if (_slinkySequence == null)
            {
                StartSlinkyAnimation();
            }
        }

        protected override void OnStunned() => _slinkySequence?.Pause();
        protected override void OnDeath() => KillSequence();
        protected override void OnReachedBase() => KillSequence();

        private void StartSlinkyAnimation()
        {
            KillSequence();
            
            _slinkySequence = DOTween.Sequence();

            _slinkySequence.Append(_visualMesh.DORotate(new Vector3(180f, 0, 0), _stepDuration, RotateMode.LocalAxisAdd)
                           .SetEase(Ease.InOutSine)); 

            _slinkySequence.Join(_visualMesh.DOScale(new Vector3(1f, _stretchMultiplier, 1f), _stepDuration / 2f)
                           .SetLoops(2, LoopType.Yoyo)
                           .SetEase(Ease.OutQuad));

            _slinkySequence.SetLoops(-1, LoopType.Restart);
        }

        private void KillSequence()
        {
            if (_slinkySequence != null)
            {
                _slinkySequence.Kill();
                _slinkySequence = null;
            }
        }
    }
}