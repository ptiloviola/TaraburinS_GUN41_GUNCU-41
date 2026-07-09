using UnityEngine;
using DG.Tweening;

namespace Enemy
{
    [RequireComponent(typeof(AudioSource))]
    public class EnemyAnimator : MonoBehaviour
    {
        [Header("References")]
        public Transform visualsRoot;
        [Tooltip("Порядок важен: от нижнего шара к верхнему")]
        public Transform[] bodyParts; // Перетащи сюда Sphere.003, Sphere.002, Sphere.001

        private EnemyConfig _config;
        private bool _isMoving;
        private Tween _hopTween;
        
        // Для отслеживания поворотов
        private float _lastYRotation;
        private bool _isTurningTweenActive;
        private AudioSource _audioSource;

        public void Initialize(EnemyConfig config)
        {
            _config = config;
            // Получаем или добавляем AudioSource
            _audioSource = GetComponent<AudioSource>();
            
            // Настраиваем AudioSource как 3D-звук (чтобы он был тише издалека)
            _audioSource.spatialBlend = 1f;
        }

        // Теперь принимаем еще и текущий угол поворота по Y
        public void UpdateAnimation(float currentSpeed, float currentYRotation)
        {
            bool isMovingNow = currentSpeed > 0.1f; 

            // 1. Проверка старта/остановки
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

            // 2. Проверка резких поворотов (работает только в движении)
            if (isMovingNow)
            {
                float turnDelta = Mathf.DeltaAngle(_lastYRotation, currentYRotation);
                
                // Если повернули резко (больше 2 градусов за кадр) и сейчас нет активной анимации поворота
                if (Mathf.Abs(turnDelta) > 2f && !_isTurningTweenActive)
                {
                    _isTurningTweenActive = true;
                    
                    // Вычисляем крен. Если повернули направо, кренимся влево (инерция)
                    float tiltDirection = Mathf.Sign(turnDelta); 
                    Vector3 turnForce = new Vector3(0, 0, _config.turnTiltAngle * tiltDirection);
                    
                    ApplyImpulse(turnForce);

                    // Сбрасываем флаг через время анимации, чтобы можно было снова крениться
                    DOVirtual.DelayedCall(_config.inertiaDuration, () => _isTurningTweenActive = false);
                }
            }

            _lastYRotation = currentYRotation;
        }

        private void StartHopping()
        {
            float startY = visualsRoot.localPosition.y;
            
            // Создаем секвенцию для точного контроля
            Sequence hopSeq = DOTween.Sequence();

            // 1. Прыжок вверх
            hopSeq.Append(visualsRoot.DOLocalMoveY(startY + _config.hopHeight, _config.hopDuration).SetEase(Ease.OutQuad));
            
            // 2. Падение вниз
            hopSeq.Append(visualsRoot.DOLocalMoveY(startY, _config.hopDuration).SetEase(Ease.InQuad));
            
            // 3. Звук приземления (вызывается строго 1 раз за цикл)
            hopSeq.AppendCallback(PlayHopSound);

            // Зацикливаем всю секвенцию бесконечно
            hopSeq.SetLoops(-1);
            
            _hopTween = hopSeq; // Сохраняем в ту же переменную, чтобы StopHopping() мог её убить
        }

        private void PlayHopSound()
        {
            if (_config.hopSound != null)
            {
                // Та самая магия рандома: звук каждый раз чуть выше или чуть ниже
                _audioSource.pitch = Random.Range(0.85f, 1.15f);
                _audioSource.PlayOneShot(_config.hopSound);
            }
        }

        private void StopHopping()
        {
            _hopTween?.Kill();
            visualsRoot.DOLocalMoveY(0f, 0.2f).SetEase(Ease.OutBounce);
        }

        // Тот самый метод цепной реакции
        private void ApplyImpulse(Vector3 baseForce)
        {
            for (int i = 0; i < bodyParts.Length; i++)
            {
                // Завершаем предыдущие твины на этом шаре, чтобы они не конфликтовали
                bodyParts[i].DOKill(complete: true);

                // Чем выше шар, тем сильнее он реагирует на импульс (множитель увеличивается)
                Vector3 appliedForce = baseForce * (1f + (i * 0.4f));
                
                // Чем выше шар, тем позже до него доходит волна (задержка)
                float delay = i * _config.impulseDelayStep;

                bodyParts[i].DOPunchRotation(
                    appliedForce, 
                    _config.inertiaDuration, 
                    _config.inertiaVibrato, 
                    _config.inertiaElasticity
                ).SetDelay(delay);
            }
        }

        // Внутри EnemyAnimator.cs:

        public void AnimateVisibility(bool isVisible)
        {
            // Убиваем предыдущую анимацию масштаба, если она не успела закончиться
            visualsRoot.DOKill(complete: true);

            float targetScale = isVisible ? 1f : 0f;
            float duration = 0.5f;
            
            // Если появляемся - пружиним (OutBack). Если исчезаем - втягиваемся (InBack)
            Ease easeType = isVisible ? Ease.OutBack : Ease.InBack;

            visualsRoot.DOScale(targetScale, duration).SetEase(easeType);
        }

        // Полезно добавить метод для мгновенного скрытия при старте игры
        public void SetInvisibleInstant()
        {
            visualsRoot.localScale = Vector3.zero;
        }

        public void PlayHitReaction()
        {
            // Завершаем предыдущие анимации масштаба (например, если он только что появился из-под земли)
            visualsRoot.DOKill(complete: true);
            
            // Эффект удара: снеговик резко сплющивается по Y и раздается вширь по X/Z, 
            // а затем пружинисто возвращается в норму.
            visualsRoot.DOPunchScale(new Vector3(0.3f, -0.3f, 0.3f), 0.4f, vibrato: 5, elasticity: 0.5f)
                .OnComplete(() => visualsRoot.localScale = Vector3.one); // Гарантируем, что он вернет нормальный размер
        }

    }
}