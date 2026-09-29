using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using DG.Tweening;

namespace Gameplay.Enemies.Visuals
{
    public class RiggedFlightAnimator : EnemyVisualsBase
    {
        [Header("Компоненты")]
        [SerializeField] private Animator _animator;
        [SerializeField] private EnemyFacade _facade;

        [Header("Настройки смешивания")]
        [SerializeField] private float _turnSmoothness = 5f;
        [SerializeField] private float _turnSensitivity = 50f;

        private static readonly int TurnParam = Animator.StringToHash("Turn");
        private static readonly int DeathStateHash = Animator.StringToHash("Death");
        private static readonly int MovementStateHash = Animator.StringToHash("Movement");

        private float _lastYRotation;
        private float _currentTurnValue;
        
        // Переменные для пулинга
        private Vector3 _initialScale;
        private Vector3 _initialRotation;

        protected override void Awake()
        {
            base.Awake();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_facade == null) _facade = GetComponentInParent<EnemyFacade>();
            
            _initialScale = transform.localScale;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            transform.localScale = _initialScale;
            
            _lastYRotation = transform.eulerAngles.y;
            _currentTurnValue = 0f;
            
            if (_animator != null)
            {
                _animator.speed = 1f;
                _animator.Play(MovementStateHash, 0, 0f); 
                _animator.SetFloat(TurnParam, 0f);
            }
        }

        private void Update()
        {
            if (_animator == null || _facade == null || _facade.IsDead) return;

            float currentYRotation = transform.eulerAngles.y;
            float deltaY = Mathf.DeltaAngle(_lastYRotation, currentYRotation);
            _lastYRotation = currentYRotation;

            float targetTurn = Mathf.Clamp(deltaY / (Time.deltaTime * _turnSensitivity), -1f, 1f);
            _currentTurnValue = Mathf.Lerp(_currentTurnValue, targetTurn, Time.deltaTime * _turnSmoothness);

            _animator.SetFloat(TurnParam, _currentTurnValue);
        }

        protected override void OnReachedBase()
        {
            if (_animator != null) _animator.speed = 0f;
        }

        public override async UniTask PlayDeathAnimationAsync()
        {
            if (_animator != null)
            {
                _animator.CrossFadeInFixedTime(DeathStateHash, 0.15f);
            }

            if (_facade.Agent != null)
            {
                _facade.Agent.enabled = false;
            }

            float groundY = transform.position.y - 5f; 
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 20f))
            {
                float colliderOffset = 0f;
                if (TryGetComponent(out BoxCollider col))
                {
                    colliderOffset = col.size.y / 2f;
                }
                groundY = hit.point.y + colliderOffset;
            }

            float fallDistance = transform.position.y - groundY;
            float fallDuration = Mathf.Max(0.3f, fallDistance / 10f);

            Vector3 targetRotation = transform.eulerAngles;
            targetRotation.z += 180f;
            
            transform.DORotate(targetRotation, fallDuration, RotateMode.FastBeyond360)
                     .SetEase(Ease.InQuad);

            await transform.DOMoveY(groundY, fallDuration).SetEase(Ease.InCubic).AsyncWaitForCompletion();
            
            await UniTask.Delay(TimeSpan.FromSeconds(1.2f));
            
            await transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
            
        }
    }
}