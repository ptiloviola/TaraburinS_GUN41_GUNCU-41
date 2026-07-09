using UnityEngine;
using DG.Tweening;
using Infrastructure.Interfaces;
using Enemy.Config;
namespace Enemy.Visuals
{
    [RequireComponent(typeof(AudioSource))]
    public class SnowmanTweenView : MonoBehaviour, IEnemyView
    {
        [Header("References")]
        [SerializeField] private Transform visualsRoot;
        [SerializeField] private Transform[] bodyParts;

        private EnemyConfig _config;
        private AudioSource _audioSource;
        private Tween _hopTween;
        private float _lastYRotation;
        private bool _isMoving;
        private bool _isTurningTweenActive;

        public void Initialize(EnemyConfig config)
        {
            _config = config;
            _audioSource = GetComponent<AudioSource>();
            _audioSource.spatialBlend = 1f;
            
            visualsRoot.localScale = Vector3.zero; // Мгновенно скрываем при старте
        }

        // Реализация интерфейса (пустая заглушка, если цвет не нужен конкретно этому врагу)
        public void Initialize(Color tintColor) { }

        public void UpdateMoveAnimation(float speed, float rotationY)
        {
            bool isMovingNow = speed > 0.1f;

            if (isMovingNow && !_isMoving)
            {
                _isMoving = true;
                StartHopping();
                ApplyImpulse(_config.startImpulse);
            }
            else if (!isMovingNow && _isMoving)
            {
                _isMoving = false;
                StopHopping();
                ApplyImpulse(_config.stopImpulse);
            }

            if (isMovingNow)
            {
                float turnDelta = Mathf.DeltaAngle(_lastYRotation, rotationY);
                if (Mathf.Abs(turnDelta) > 2f && !_isTurningTweenActive)
                {
                    _isTurningTweenActive = true;
                    float tiltDirection = Mathf.Sign(turnDelta);
                    Vector3 turnForce = new Vector3(0, 0, _config.turnTiltAngle * tiltDirection);
                    ApplyImpulse(turnForce);
                    DOVirtual.DelayedCall(_config.inertiaDuration, () => _isTurningTweenActive = false);
                }
            }
            _lastYRotation = rotationY;
        }

        public void PlayHitReaction()
        {
            visualsRoot.DOKill(complete: true);
            visualsRoot.DOPunchScale(new Vector3(0.3f, -0.3f, 0.3f), 0.4f, 5, 0.5f)
                .OnComplete(() => visualsRoot.localScale = Vector3.one);
        }

        public void PlayDeathEffect()
        {
            visualsRoot.DOKill();
            visualsRoot.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        }

        public void SetVisibility(bool isVisible)
        {
            visualsRoot.DOKill(complete: true);
            float targetScale = isVisible ? 1f : 0f;
            Ease easeType = isVisible ? Ease.OutBack : Ease.InBack;
            visualsRoot.DOScale(targetScale, 0.5f).SetEase(easeType);
        }

        private void StartHopping()
        {
            float startY = visualsRoot.localPosition.y;
            Sequence hopSeq = DOTween.Sequence();
            hopSeq.Append(visualsRoot.DOLocalMoveY(startY + _config.hopHeight, _config.hopDuration).SetEase(Ease.OutQuad));
            hopSeq.Append(visualsRoot.DOLocalMoveY(startY, _config.hopDuration).SetEase(Ease.InQuad));
            hopSeq.AppendCallback(() => {
                if (_config.hopSound != null) _audioSource.PlayOneShot(_config.hopSound);
            });
            hopSeq.SetLoops(-1);
            _hopTween = hopSeq;
        }

        private void StopHopping()
        {
            _hopTween?.Kill();
            visualsRoot.DOLocalMoveY(0f, 0.2f).SetEase(Ease.OutBounce);
        }

        private void ApplyImpulse(Vector3 baseForce)
        {
            for (int i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].DOKill(complete: true);
                Vector3 appliedForce = baseForce * (1f + (i * 0.4f));
                bodyParts[i].DOPunchRotation(appliedForce, _config.inertiaDuration, _config.inertiaVibrato, _config.inertiaElasticity)
                    .SetDelay(i * _config.impulseDelayStep);
            }
        }
    }
}