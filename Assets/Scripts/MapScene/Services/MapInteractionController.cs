using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using Gameplay.MapScene.Views;

namespace Gameplay.MapScene.Services
{
    public class MapInteractionController : ITickable
    {
        private readonly Camera _camera;
        private Vector2 _lastMousePos;
        private bool _isDragging;
        
        private readonly float _dragThreshold = 10f; 
        private readonly float _minY = -2f;
        private readonly float _maxY = 15f; 

        private MapNodeView _hoveredNode;

        public MapInteractionController()
        {
            _camera = Camera.main;
        }

        public void Tick()
        {
            if (Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            bool isPressed = Mouse.current.leftButton.isPressed;
            bool wasPressed = Mouse.current.leftButton.wasPressedThisFrame;
            bool wasReleased = Mouse.current.leftButton.wasReleasedThisFrame;


            if (!isPressed)
            {
                Ray ray = _camera.ScreenPointToRay(mousePos);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                MapNodeView hitNode = hit.collider != null ? hit.collider.GetComponent<MapNodeView>() : null;

                if (hitNode != _hoveredNode)
                {
                    if (_hoveredNode != null) _hoveredNode.OnPointerExit();
                    _hoveredNode = hitNode;
                    if (_hoveredNode != null) _hoveredNode.OnPointerEnter();
                }
            }


            if (wasPressed)
            {
                _lastMousePos = mousePos;
                _isDragging = false;
            }

            if (isPressed)
            {
                Vector2 delta = mousePos - _lastMousePos;
                if (delta.sqrMagnitude > _dragThreshold) _isDragging = true;

                if (_isDragging)
                {
                    Vector3 camPos = _camera.transform.position;
                    camPos.y -= delta.y * (_camera.orthographicSize / Screen.height * 2f);
                    camPos.y = Mathf.Clamp(camPos.y, _minY, _maxY);
                    _camera.transform.position = camPos;
                    _lastMousePos = mousePos; 
                }
            }

            if (wasReleased && !_isDragging && _hoveredNode != null)
            {
                _hoveredNode.OnPointerClick();
            }
        }
    }
}