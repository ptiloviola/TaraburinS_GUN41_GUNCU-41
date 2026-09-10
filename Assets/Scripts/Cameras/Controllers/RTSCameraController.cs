using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Input;
using Gameplay.Cameras.Data;
using Gameplay.Grid;
using Gameplay.Base;

namespace Gameplay.Cameras.Controllers
{
    public class RTSCameraController : IInitializable, ILateTickable
    {
        private readonly Camera _camera;
        private readonly IInputService _inputService;
        private readonly CameraSettingsConfig _config;
        private readonly IGridService _gridService;
        private readonly GridSceneReferences _sceneReferences;
        private readonly BaseRegistry _baseRegistry;

        // НОВОЕ: Мы больше не двигаем саму камеру. Мы двигаем точку на земле!
        private Vector3 _focusPoint; 
        private float _currentZoomDistance; // Дистанция от камеры до точки на земле

        private float _minX, _maxX, _minZ, _maxZ;

        public RTSCameraController(
            Camera camera,
            IInputService inputService,
            CameraSettingsConfig config,
            IGridService gridService,
            GridSceneReferences sceneReferences,
            BaseRegistry baseRegistry)
        {
            _camera = camera;
            _inputService = inputService;
            _config = config;
            _gridService = gridService;
            _sceneReferences = sceneReferences;
            _baseRegistry = baseRegistry;
        }

        public void Initialize()
        {
            // 1. Задаем поворот
            _camera.transform.rotation = Quaternion.Euler(_config.CameraPitch, _config.CameraYaw, 0f);
            CalculateGridBounds();

            // 2. Устанавливаем средний зум
            _currentZoomDistance = Mathf.Lerp(_config.MinZoomY, _config.MaxZoomY, 0.5f);

            // 3. Ищем стартовую базу
            BaseCore targetBase = _baseRegistry.GetBaseById(_config.InitialFocusBaseId);
            if (targetBase != null)
            {
                Quaternion yawRotation = Quaternion.Euler(0f, _config.CameraYaw, 0f);
                _focusPoint = targetBase.transform.position + (yawRotation * _config.BaseFocusOffset);
                _focusPoint.y = 0f; // Точка фокуса ВСЕГДА лежит плоско на земле
            }
            else
            {
                _focusPoint = Vector3.zero;
            }

            // 4. Мгновенно ставим камеру в правильную позицию без интерполяции (Lerp)
            ApplyCameraPosition(1f);
        }

        public void LateTick()
        {
            HandlePanning();
            HandleDragPanning();
            HandleZooming();
            
            ClampFocusPoint();

            // Плавно двигаем камеру за точкой фокуса
            ApplyCameraPosition(Time.deltaTime * _config.PanSmoothness);
        }

        private void HandlePanning()
        {
            Vector2 input = _inputService.PanDelta;
            if (input == Vector2.zero) return;

            Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
            Quaternion yawRotation = Quaternion.Euler(0f, _config.CameraYaw, 0f);
            
            // Двигаем невидимую точку по земле
            _focusPoint += (yawRotation * moveDirection) * (_config.PanSpeed * Time.deltaTime);
        }

        private void HandleDragPanning()
        {
            if (!_inputService.IsDragPanning) return;
            Vector2 delta = _inputService.PointerDelta;
            if (delta == Vector2.zero) return;

            Vector3 dragDirection = new Vector3(-delta.x, 0f, -delta.y);
            Quaternion yawRotation = Quaternion.Euler(0f, _config.CameraYaw, 0f);
            
            _focusPoint += (yawRotation * dragDirection) * _config.DragSpeed;
        }

        private void HandleZooming()
        {
            float scroll = _inputService.ZoomDelta;
            if (Mathf.Approximately(scroll, 0f)) return;

            // Изменяем саму дистанцию зума, а не координаты
            float zoomDir = scroll > 0 ? -1f : 1f; // Вверх - приближение (уменьшение дистанции)
            _currentZoomDistance += zoomDir * _config.ZoomSpeed * Time.deltaTime;
            
            // Ограничиваем дистанцию
            _currentZoomDistance = Mathf.Clamp(_currentZoomDistance, _config.MinZoomY, _config.MaxZoomY);
        }

        private void ClampFocusPoint()
        {
            // Ограничиваем только точку на земле! Камера в небе может быть где угодно.
            _focusPoint.x = Mathf.Clamp(_focusPoint.x, _minX, _maxX);
            _focusPoint.z = Mathf.Clamp(_focusPoint.z, _minZ, _maxZ);
            _focusPoint.y = 0f; 
        }

        private void ApplyCameraPosition(float interpolationT)
        {
            // Позиция камеры = Точка фокуса МИНУС вектор взгляда, умноженный на дистанцию зума
            Vector3 desiredPosition = _focusPoint - (_camera.transform.forward * _currentZoomDistance);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, desiredPosition, interpolationT);
        }

        private void CalculateGridBounds()
        {
            if (_gridService.Width == 0) return;
            float realWidth = _gridService.Width * _sceneReferences.Spacing;
            float realLength = _gridService.Height * _sceneReferences.Spacing;

            _minX = -_config.BoundaryPadding.x;
            _maxX = realWidth + _config.BoundaryPadding.x;
            _minZ = -_config.BoundaryPadding.y;
            _maxZ = realLength + _config.BoundaryPadding.y;
        }
    }
}