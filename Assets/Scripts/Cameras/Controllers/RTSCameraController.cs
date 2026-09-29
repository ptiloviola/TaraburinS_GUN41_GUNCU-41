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

        private Vector3 _focusPoint; 
        private float _currentZoomDistance;
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
            CalculateGridBounds();

            if (_config.UseAbsoluteManualStart)
            {
                _camera.transform.position = _config.AbsolutePosition;
                _camera.transform.rotation = Quaternion.Euler(_config.AbsoluteRotation);
                _currentZoomDistance = _config.AbsoluteZoom;
                
                _focusPoint = _camera.transform.position + (_camera.transform.forward * _currentZoomDistance);
                _focusPoint.y = 0f;
            }
            else
            {
                _camera.transform.rotation = Quaternion.Euler(_config.AbsoluteRotation.x, _config.AbsoluteRotation.y, 0f);
                _currentZoomDistance = _config.AbsoluteZoom;
                
                Quaternion yawRotation = Quaternion.Euler(0f, _config.AbsoluteRotation.y, 0f);


                if (_config.FocusMode == CameraFocusMode.GridBottomCenter)
                {

                    float centerX = (_gridService.Width * _sceneReferences.Spacing) / 2f;

                    float bottomZ = _gridService.Height * _sceneReferences.Spacing;
                    
                    Vector3 targetPoint = new Vector3(centerX, 0f, bottomZ);
                    _focusPoint = targetPoint + (yawRotation * _config.BaseFocusOffset);
                }
                else if (_config.FocusMode == CameraFocusMode.GridCenter)
                {
                    float centerX = (_gridService.Width * _sceneReferences.Spacing) / 2f;
                    float centerZ = (_gridService.Height * _sceneReferences.Spacing) / 2f;
                    
                    Vector3 targetPoint = new Vector3(centerX, 0f, centerZ);
                    _focusPoint = targetPoint + (yawRotation * _config.BaseFocusOffset);
                }
                else
                {
                    BaseCore targetBase = _baseRegistry.GetBaseById(_config.InitialFocusBaseId);
                    
                    if (targetBase == null)
                    {
                        targetBase = Object.FindObjectOfType<BaseCore>();
                    }

                    if (targetBase != null)
                    {
                        _focusPoint = targetBase.transform.position + (yawRotation * _config.BaseFocusOffset);
                    }
                }

                _focusPoint.y = 0f;
                ApplyCameraPosition(1f);
            }
        }

        public void LateTick()
        {
            HandlePanning();
            HandleDragPanning();
            HandleZooming();
            
            ClampFocusPoint();
            ApplyCameraPosition(Time.deltaTime * _config.PanSmoothness);
        }

        private void HandlePanning()
        {
            Vector2 input = _inputService.PanDelta;
            if (input == Vector2.zero) return;

            Vector3 moveDir = new Vector3(input.x, 0f, input.y);
            Quaternion yaw = Quaternion.Euler(0f, _camera.transform.eulerAngles.y, 0f);
            _focusPoint += (yaw * moveDir) * (_config.PanSpeed * Time.deltaTime);
        }

        private void HandleDragPanning()
        {
            if (!_inputService.IsDragPanning) return;
            Vector2 delta = _inputService.PointerDelta;
            if (delta == Vector2.zero) return;

            Vector3 dragDir = new Vector3(-delta.x, 0f, -delta.y);
            Quaternion yaw = Quaternion.Euler(0f, _camera.transform.eulerAngles.y, 0f);
            _focusPoint += (yaw * dragDir) * _config.DragSpeed;
        }

        private void HandleZooming()
        {
            float scroll = _inputService.ZoomDelta;
            if (Mathf.Approximately(scroll, 0f)) return;

            float zoomDir = scroll > 0 ? -1f : 1f; 
            _currentZoomDistance += zoomDir * _config.ZoomSpeed * Time.deltaTime;
            _currentZoomDistance = Mathf.Clamp(_currentZoomDistance, _config.MinZoomY, _config.MaxZoomY);
        }

        private void ClampFocusPoint()
        {
            _focusPoint.x = Mathf.Clamp(_focusPoint.x, _minX, _maxX);
            _focusPoint.z = Mathf.Clamp(_focusPoint.z, _minZ, _maxZ);
            _focusPoint.y = 0f; 
        }

        private void ApplyCameraPosition(float t)
        {
            Vector3 desiredPosition = _focusPoint - (_camera.transform.forward * _currentZoomDistance);
            float lerpFactor = 1f - Mathf.Exp(-t); 
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, desiredPosition, lerpFactor);
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