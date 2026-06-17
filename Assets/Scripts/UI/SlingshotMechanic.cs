using UnityEngine;

namespace Bowling.UI
{
    public class SlingshotMechanic : BaseThrowMechanic
    {
        [Header("Настройки физики")]
        [SerializeField] private float _maxForce = 50f;
        [SerializeField] private float _dragMultiplier = 10f;

        [Header("Визуализация (Резинка)")]
        [SerializeField] private LineRenderer _lineRenderer;

        [SerializeField] private Transform _ballTransform;
        [SerializeField] private float _visualLineMultiplier = 3f;
        [Range(0f, 1f)]
        [SerializeField] private float _shakeThreshold = 0.6f; // Начинаем дрожать после 60% натяжения
        [SerializeField] private float _shakeAmplitude = 2f;   // Насколько сильно уводит прицел (в метрах)
        [SerializeField] private float _shakeSpeed = 25f;      // Как быстро дергается рука

        // Скрытая переменная: сюда будем сохранять финальное дрожащее направление
        private Vector3 _currentAimDirection;

        private Vector3 _startDragWorldPos;
        private bool _isDragging = false;

        private void OnEnable()
        {
            if (_lineRenderer != null) _lineRenderer.enabled = false;
        }

        private void OnDisable()
        {
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            _isDragging = false;
        }

        private void Update()
        {
            if (_isThrowExecuted) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (GetMouseWorldPosition(out Vector3 hitPos))
                {
                    _startDragWorldPos = hitPos;
                    _isDragging = true;

                    if (_lineRenderer != null)
                    {
                        _lineRenderer.enabled = true;
                        Vector3 startPoint = _ballTransform != null ? _ballTransform.position : _startDragWorldPos;
                        startPoint.y = 0.5f; 
                        
                        _lineRenderer.SetPosition(0, startPoint);
                        _lineRenderer.SetPosition(1, startPoint);
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0) && _isDragging)
            {
                if (GetMouseWorldPosition(out Vector3 hitPos))
                {
                    Vector3 dragVector = _startDragWorldPos - hitPos;
                    dragVector.y = 0;
                    float rawForce = dragVector.magnitude * _dragMultiplier;
                    float clampedForce = Mathf.Clamp(rawForce, 0, _maxForce);
                    
                    // КРИТИЧЕСКОЕ ИЗМЕНЕНИЕ: 
                    // Мы стреляем не по идеальному вектору, а по нашему сохраненному ДРОЖАЩЕМУ!
                    ExecuteThrow(_currentAimDirection, clampedForce);
                }
                
                _isDragging = false;
                if (_lineRenderer != null) _lineRenderer.enabled = false;
            }

            // --- БЛОК ОТРИСОВКИ ЛАЗЕРА С ТРЕМОРОМ ---
            if (_isDragging)
            {
                if (GetMouseWorldPosition(out Vector3 currentHitPos))
                {
                    Vector3 dragVector = _startDragWorldPos - currentHitPos;
                    dragVector.y = 0;

                    float maxDragDistance = _maxForce / _dragMultiplier; 
                    Vector3 clampedDragVector = Vector3.ClampMagnitude(dragVector, maxDragDistance);

                    // 1. Считаем процент натяжения (от 0.0 до 1.0)
                    float tensionPercent = clampedDragVector.magnitude / maxDragDistance;
                    
                    // 2. Готовим вектор отклонения (по умолчанию нулевой)
                    Vector3 shakeOffset = Vector3.zero;

                    // 3. Если натянули сильнее порога — включаем тремор!
                    if (tensionPercent > _shakeThreshold)
                    {
                        // Считаем интенсивность от 0 до 1 (на пороге 0, на максимуме 1)
                        float intensity = (tensionPercent - _shakeThreshold) / (1f - _shakeThreshold);

                        // Генерируем Шум Перлина (-1 до 1) для осей X и Z. 
                        // Time.time заставляет шум постоянно меняться с течением времени
                        float noiseX = (Mathf.PerlinNoise(Time.time * _shakeSpeed, 0f) - 0.5f) * 2f;
                        float noiseZ = (Mathf.PerlinNoise(0f, Time.time * _shakeSpeed) - 0.5f) * 2f;

                        // Умножаем шум на максимальную амплитуду и на текущую интенсивность
                        shakeOffset = new Vector3(noiseX, 0f, noiseZ) * (_shakeAmplitude * intensity);
                    }

                    Vector3 startPoint = _ballTransform != null ? _ballTransform.position : _startDragWorldPos;
                    startPoint.y = 0.5f;

                    // 4. Прибавляем дрожание к идеальной точке прицеливания
                    Vector3 aimPoint = startPoint + (clampedDragVector * _visualLineMultiplier) + shakeOffset;
                    aimPoint.y = 0.5f;

                    // 5. СОХРАНЯЕМ испорченный вектор, чтобы при отпускании мыши шар полетел именно по нему
                    _currentAimDirection = (aimPoint - startPoint).normalized;

                    if (_lineRenderer != null)
                    {
                        _lineRenderer.SetPosition(0, startPoint);
                        _lineRenderer.SetPosition(1, aimPoint);
                    }
                }
            }
        }

        private bool GetMouseWorldPosition(out Vector3 result)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                result = hit.point;
                return true;
            }
            result = Vector3.zero;
            return false;
        }
    }
}
