using Gameplay.Grid;
using UnityEngine;
using Zenject;

namespace Gameplay.Tools
{
    public class DebugCameraFitter : MonoBehaviour
    {
        private GridGenerator _gridGenerator;

        [Header("Настройки ракурса (множители от размера карты)")]
        [SerializeField, Range(0.1f, 2f)] private float _heightMultiplier = 0.8f;
        [SerializeField, Range(0.1f, 2f)] private float _backMultiplier = 0.4f;

        [Header("Точная ручная подгонка")]
        [SerializeField] private Vector3 _positionOffset = Vector3.zero;
        [SerializeField] private Vector3 _lookAtOffset = Vector3.zero;

        [Header("Дебаг")]
        [Tooltip("Включи, чтобы двигать ползунки в Play Mode и сразу видеть результат")]
        [SerializeField] private bool _updateLive = true;

        [Inject]
        public void Construct(GridGenerator gridGenerator)
        {
            _gridGenerator = gridGenerator;
        }

        private void Start()
        {
            FitCamera();
        }

        private void LateUpdate()
        {
            // Обновляем каждый кадр только если включена галочка в инспекторе
            if (_updateLive)
            {
                FitCamera();
            }
        }

        private void FitCamera()
        {
            if (_gridGenerator.EditorConfig == null) return;

            // 1. Узнаем реальные физические размеры сетки
            float realWidth = _gridGenerator.EditorConfig.width * _gridGenerator.Spacing;
            float realHeight = _gridGenerator.EditorConfig.height * _gridGenerator.Spacing;

            // 2. Вычисляем координаты центра
            float centerX = (realWidth / 2f) - (_gridGenerator.Spacing / 2f);
            float centerZ = (realHeight / 2f) - (_gridGenerator.Spacing / 2f);
            
            // Добавляем ручное смещение фокуса, если нужно немного сдвинуть центр внимания
            Vector3 centerPoint = new Vector3(centerX, 0f, centerZ) + _lookAtOffset;

            // 3. Вычисляем масштаб
            float maxDimension = Mathf.Max(realWidth, realHeight);

            // 4. Позиционируем камеру с учетом множителей из инспектора
            float heightOffset = maxDimension * _heightMultiplier;
            float backOffset = maxDimension * _backMultiplier;

            // Применяем позицию + дополнительное ручное смещение (positionOffset)
            transform.position = centerPoint + new Vector3(0f, heightOffset, -backOffset) + _positionOffset;

            // 5. Направляем фокус
            transform.LookAt(centerPoint);
        }
    }
}