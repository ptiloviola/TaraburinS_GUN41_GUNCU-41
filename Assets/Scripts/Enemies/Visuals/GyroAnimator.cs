using UnityEngine;
using DG.Tweening;

namespace Gameplay.Enemies.Visuals
{
    public class GyroAnimator : MonoBehaviour
    {
        [Header("Ссылки на части модели")]
        [SerializeField] private Transform _core;
        [SerializeField] private Transform _ring1;
        [SerializeField] private Transform _ring2;
        [SerializeField] private Transform _visualRoot; // Твой объект Visual

        [Header("Настройки Ядра")]
        [SerializeField] private float _pulseDuration = 0.5f;
        [SerializeField] private float _pulseScaleMultiplier = 1.2f;

        [Header("Настройки Колец (Время на один оборот)")]
        [SerializeField] private float _ring1Speed = 1.5f;
        [SerializeField] private float _ring2Speed = 2.5f;

        [Header("Левитация")]
        [SerializeField] private float _hoverHeight = 0.3f;
        [SerializeField] private float _hoverDuration = 1.5f;

        private Sequence _gyroSequence;
        private Vector3 _initialCoreScale;
        private Vector3 _initialVisualPos;

        private void Awake()
        {
            if (_core != null) _initialCoreScale = _core.localScale;
            if (_visualRoot != null) _initialVisualPos = _visualRoot.localPosition;
        }

        private void OnEnable()
        {
            // Сбрасываем вращения при спавне из пула
            if (_ring1 != null) _ring1.localRotation = Quaternion.identity;
            if (_ring2 != null) _ring2.localRotation = Quaternion.identity;
            if (_visualRoot != null) _visualRoot.localPosition = _initialVisualPos;
            if (_core != null) _core.localScale = _initialCoreScale;

            StartGyroAnimation();
        }

        private void StartGyroAnimation()
        {
            _gyroSequence = DOTween.Sequence();

            // 1. Пульсация ядра (Сердцебиение)
            if (_core != null)
            {
                _core.DOScale(_initialCoreScale * _pulseScaleMultiplier, _pulseDuration)
                     .SetLoops(-1, LoopType.Yoyo)
                     .SetEase(Ease.InOutSine);
            }

            // 2. Вращение внутреннего кольца (например, по оси X)
            // RotateMode.FastBeyond360 гарантирует, что кольцо сделает полный оборот, а не пойдет по кратчайшему пути к 0.
            if (_ring1 != null)
            {
                _ring1.DOLocalRotate(new Vector3(360f, 0f, 0f), _ring1Speed, RotateMode.FastBeyond360)
                      .SetLoops(-1, LoopType.Restart)
                      .SetEase(Ease.Linear); // Linear дает ровное механическое вращение без ускорений
            }

            // 3. Вращение внешнего кольца (например, по осям Y и Z одновременно)
            if (_ring2 != null)
            {
                _ring2.DOLocalRotate(new Vector3(0f, 360f, 360f), _ring2Speed, RotateMode.FastBeyond360)
                      .SetLoops(-1, LoopType.Restart)
                      .SetEase(Ease.Linear);
            }

            // 4. Мягкая левитация всей модели вверх-вниз
            if (_visualRoot != null)
            {
                _visualRoot.DOLocalMoveY(_initialVisualPos.y + _hoverHeight, _hoverDuration)
                           .SetLoops(-1, LoopType.Yoyo)
                           .SetEase(Ease.InOutSine);
            }
        }

        private void OnDisable()
        {
            // DOTween.Kill(transform) безопасно убивает все анимации на объекте, чтобы избежать утечек в пуле
            if (_core != null) DOTween.Kill(_core);
            if (_ring1 != null) DOTween.Kill(_ring1);
            if (_ring2 != null) DOTween.Kill(_ring2);
            if (_visualRoot != null) DOTween.Kill(_visualRoot);
        }
    }
}