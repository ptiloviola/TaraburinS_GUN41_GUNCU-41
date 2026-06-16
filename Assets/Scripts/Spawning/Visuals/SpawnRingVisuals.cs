using System.Collections;
using UnityEngine;

namespace Gameplay.Spawning.Visuals
{
    public class SpawnRingVisuals : MonoBehaviour, ISpawnVisuals
    {
        [Header("Ссылки")]
        [Tooltip("Плоский цилиндр, который будет расширяться")]
        [SerializeField] private Renderer _ringRenderer; 

        [Header("Настройки колец")]
        [SerializeField] private Color _ringColor = new Color(1f, 0f, 0f, 0.8f); // Красный
        [SerializeField] private float _maxScale = 3f; // Насколько широко расходится круг
        [SerializeField] private float _pingSpeed = 0.5f; // Время одного расширения (секунды)

        private Material _ringMaterial;
        private Coroutine _warningRoutine;
        private Vector3 _initialScale;

        private void Awake()
        {
            if (_ringRenderer != null)
            {
                _ringMaterial = _ringRenderer.material;
                _ringMaterial.color = new Color(_ringColor.r, _ringColor.g, _ringColor.b, 0f);
                _initialScale = _ringRenderer.transform.localScale;
                _ringRenderer.gameObject.SetActive(false);
            }
        }

        public void PlayWarningEffect(float duration)
        {
            if (_warningRoutine != null) StopCoroutine(_warningRoutine);
            _warningRoutine = StartCoroutine(WarningRoutine(duration));
        }

        private IEnumerator WarningRoutine(float duration)
        {
            if (_ringRenderer == null) yield break;
            
            _ringRenderer.gameObject.SetActive(true);

            float totalElapsed = 0f;
            float currentPingTimer = 0f;

            // Крутимся, пока не выйдет общее время предупреждения (например, 2 секунды)
            while (totalElapsed < duration)
            {
                totalElapsed += Time.deltaTime;
                currentPingTimer += Time.deltaTime;

                // Если один "пинг" закончился, начинаем кольцо заново
                if (currentPingTimer >= _pingSpeed)
                {
                    currentPingTimer = 0f;
                }

                // Вычисляем прогресс текущего круга (от 0 до 1)
                float progress = currentPingTimer / _pingSpeed;

                // 1. Увеличиваем масштаб
                float currentScale = Mathf.Lerp(0f, _maxScale, progress);
                // Сохраняем высоту (Y) неизменной, чтобы диск оставался плоским
                _ringRenderer.transform.localScale = new Vector3(currentScale, _initialScale.y, currentScale);

                // 2. Растворяем цвет (в начале яркий, к концу прозрачный)
                Color c = _ringMaterial.color;
                // Используем небольшую математическую хитрость: плавно гасим альфу к краям
                c.a = Mathf.Lerp(_ringColor.a, 0f, progress); 
                _ringMaterial.color = c;

                yield return null;
            }

            // Выключаем кольцо по завершении
            _ringRenderer.gameObject.SetActive(false);
        }
    }
}