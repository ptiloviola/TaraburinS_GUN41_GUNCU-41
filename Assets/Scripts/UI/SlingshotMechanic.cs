using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bowling.UI
{
    public class SlingshotMechanic : BaseThrowMechanic
    {
        [SerializeField] private float _maxForce = 50f;
        [SerializeField] private float _dragMultiplier = 10f;

        private Vector3 _startDragWorldPos;
        private bool _isDragging = false;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (GetMouseWorldPosition(out Vector3 hitPos))
                {
                    _startDragWorldPos = hitPos;
                    _isDragging = true;
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
                    ExecuteThrow(dragVector.normalized, clampedForce);
                }
                _isDragging = false;
            }
            if (_isDragging)
            {
                
            }
            
        }

        private bool GetMouseWorldPosition(out Vector3 result)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                result = ray.GetPoint(enter);
                return true;
            }
            result = Vector3.zero;
            return false;
        }
    }
}

