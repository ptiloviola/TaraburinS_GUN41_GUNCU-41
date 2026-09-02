using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Gameplay.Enemies.Visuals
{
    public class GyroAnimator : EnemyVisualsBase
    {
        [Header("Ссылки на части модели")]
        [SerializeField] private Transform _core;
        [SerializeField] private Transform _ring1;
        [SerializeField] private Transform _ring2;
        [SerializeField] private Transform _visualRoot; 

        [Header("Настройки Ядра")]
        [SerializeField] private float _pulseDuration = 0.5f;
        [SerializeField] private float _pulseScaleMultiplier = 1.2f;

        [Header("Настройки Колец")]
        [SerializeField] private float _ring1Speed = 1.5f;
        [SerializeField] private float _ring2Speed = 2.5f;

        [Header("Левитация")]
        [SerializeField] private float _hoverHeight = 0.3f;
        [SerializeField] private float _hoverDuration = 1.5f;

        private Vector3 _initialCoreScale;
        private Vector3 _initialVisualPos;
        private Vector3 _initialVisualScale;

        protected override void Awake()
        {
            base.Awake();
            if (_core != null) _initialCoreScale = _core.localScale;
            if (_visualRoot != null) 
            {
                _initialVisualPos = _visualRoot.localPosition;
                _initialVisualScale = _visualRoot.localScale; // НОВОЕ
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (_ring1 != null) _ring1.localRotation = Quaternion.identity;
            if (_ring2 != null) _ring2.localRotation = Quaternion.identity;
            if (_visualRoot != null) 
            {
                _visualRoot.localPosition = _initialVisualPos;
                _visualRoot.localScale = _initialVisualScale; // НОВОЕ
            }
            if (_core != null) _core.localScale = _initialCoreScale;

            StartGyroAnimation();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            KillAllAnimations();
        }

        protected override void OnReachedBase() => KillAllAnimations();

        public override async UniTask PlayDeathAnimationAsync()
        {
            KillAllAnimations(); // Останавливаем пульсацию и вращение
            
            if (_visualRoot != null)
            {
                await _visualRoot.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
            }
        }

        private void StartGyroAnimation()
        {
            if (_core != null)
            {
                _core.DOScale(_initialCoreScale * _pulseScaleMultiplier, _pulseDuration)
                     .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

            if (_ring1 != null)
            {
                _ring1.DOLocalRotate(new Vector3(360f, 0f, 0f), _ring1Speed, RotateMode.FastBeyond360)
                      .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear); 
            }

            if (_ring2 != null)
            {
                _ring2.DOLocalRotate(new Vector3(0f, 360f, 360f), _ring2Speed, RotateMode.FastBeyond360)
                      .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
            }

            if (_visualRoot != null)
            {
                _visualRoot.DOLocalMoveY(_initialVisualPos.y + _hoverHeight, _hoverDuration)
                           .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        }

        private void KillAllAnimations()
        {
            if (_core != null) DOTween.Kill(_core);
            if (_ring1 != null) DOTween.Kill(_ring1);
            if (_ring2 != null) DOTween.Kill(_ring2);
            if (_visualRoot != null) DOTween.Kill(_visualRoot);
        }
    }
}