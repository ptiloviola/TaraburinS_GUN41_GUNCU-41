using Gameplay.Grid;
using UnityEngine;
using Zenject;

namespace Gameplay.Tools
{
    public class DebugCameraFitter : MonoBehaviour
    {
        private IGridService _gridService; // ИЗМЕНЕНО
        private GridSceneReferences _sceneReferences; // ИЗМЕНЕНО

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
        public void Construct(IGridService gridService, GridSceneReferences sceneReferences)
        {
            _gridService = gridService; // ИЗМЕНЕНО
            _sceneReferences = sceneReferences; // ИЗМЕНЕНО
        }

        private void Start()
        {
            FitCamera();
        }

        private void LateUpdate()
        {
            if (_updateLive)
            {
                FitCamera();
            }
        }

        private void FitCamera()
        {
            // ИЗМЕНЕНО: Проверяем, что сервисы внедрены и сетка имеет размер
            if (_gridService == null || _sceneReferences == null || _gridService.Width == 0) return;

            // 1. Узнаем реальные физические размеры сетки
            float realWidth = _gridService.Width * _sceneReferences.Spacing;
            float realHeight = _gridService.Height * _sceneReferences.Spacing;

            // 2. Вычисляем координаты центра
            float centerX = (realWidth / 2f) - (_sceneReferences.Spacing / 2f);
            float centerZ = (realHeight / 2f) - (_sceneReferences.Spacing / 2f);
            
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