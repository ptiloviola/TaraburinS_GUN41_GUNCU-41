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
        [SerializeField] private Transform _visualsRoot;
        [SerializeField] private Transform[] _bodyParts;

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
            
            _visualsRoot.localScale = Vector3.zero;
        }

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
                    // виртуальный твин-таймер, ждем заданное время, вызываем колл-бек
                    DOVirtual.DelayedCall(_config.inertiaDuration, () => _isTurningTweenActive = false);
                }
            }
            _lastYRotation = rotationY;
        }

        public void PlayHitReaction()
        {
            // прерываем текущие твины на объекте, предварительно завершая их
            _visualsRoot.DOKill(complete: true);
            // упругая деформация, сбрасываем масштаб после завершения
            _visualsRoot.DOPunchScale(new Vector3(0.3f, -0.3f, 0.3f), 0.4f, 5, 0.5f)
                .OnComplete(() => _visualsRoot.localScale = Vector3.one);
        }

        public void PlayDeathEffect()
        {
            
            _visualsRoot.DOKill();
            _visualsRoot.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
        }

        public void SetVisibility(bool isVisible)
        {
            _visualsRoot.DOKill(complete: true);
            float targetScale = isVisible ? 1f : 0f;
            // выбираем кривую интерполяции
            Ease easeType = isVisible ? Ease.OutBack : Ease.InBack;
            // плавно масштабируем объект в зависимости от выбранно кривой
            _visualsRoot.DOScale(targetScale, 0.5f).SetEase(easeType);
        }

        private void StartHopping()
        {
            float startY = _visualsRoot.localPosition.y;
            // создаем последовательность твинов
            Sequence hopSeq = DOTween.Sequence();
            // добавляем анимации в очередб
            hopSeq.Append(_visualsRoot.DOLocalMoveY(startY + _config.hopHeight, _config.hopDuration).SetEase(Ease.OutQuad));
            hopSeq.Append(_visualsRoot.DOLocalMoveY(startY, _config.hopDuration).SetEase(Ease.InQuad));
            // вставляем вызов колл-бека в таймлайн последовательности
            hopSeq.AppendCallback(() => {
                if (_config.hopSound != null) _audioSource.PlayOneShot(_config.hopSound);
            });
            // зацикливаем последовательность
            hopSeq.SetLoops(-1);
            // сохраняем ссылку на последовательность, чтобы остановить её в StopHopping()
            _hopTween = hopSeq;
        }

        private void StopHopping()
        {
            _hopTween?.Kill();
            //плавно возвращаем объект на уровень замли
            _visualsRoot.DOLocalMoveY(0f, 0.2f).SetEase(Ease.OutBounce);
        }

        private void ApplyImpulse(Vector3 baseForce)
        {
            for (int i = 0; i < _bodyParts.Length; i++)
            {
                _bodyParts[i].DOKill(complete: true);
                Vector3 appliedForce = baseForce * (1f + (i * 0.4f));
                // наносим вращательный удар по углам Эйлера
                _bodyParts[i].DOPunchRotation(appliedForce, _config.inertiaDuration, _config.inertiaVibrato, _config.inertiaElasticity)
                    // делаем каскадный эффект: каждая следующая часть начинает отклоняться чуть позже
                    .SetDelay(i * _config.impulseDelayStep);
            }
        }
    }
}