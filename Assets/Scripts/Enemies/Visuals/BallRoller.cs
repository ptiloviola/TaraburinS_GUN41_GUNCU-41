using UnityEngine;

namespace Gameplay.Enemies.Visuals
{
    public class BallRoller : MonoBehaviour
    {
        private Transform _visualMesh; 
        private float _radius;
        private Vector3 _lastPosition;

        private void Awake()
        {
            // Awake вызывается один раз при создании префаба. 
            // Ищем компоненты здесь, чтобы не тратить ресурсы потом.
            _visualMesh = transform.Find("Visual");
            
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                _radius = sphereCollider.radius * transform.localScale.x; 
            }
            else
            {
                _radius = 0.5f; 
            }

            if (_visualMesh == null)
            {
                Debug.LogError($"[BallRoller] На объекте {gameObject.name} не найден дочерний объект 'Visual'!");
            }
        }

        private void OnEnable()
        {
            // OnEnable вызывается КАЖДЫЙ РАЗ, когда враг достается из пула (pool.Spawn)
            
            // 1. Сбрасываем позицию, чтобы избежать бешеного вращения при телепортации на точку спавна
            _lastPosition = transform.position;

            // 2. Сбрасываем вращение самой модельки в дефолтное состояние (опционально, но выглядит аккуратнее)
            if (_visualMesh != null)
            {
                _visualMesh.localRotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            if (_visualMesh == null) return;

            Vector3 movement = transform.position - _lastPosition;
            float distance = movement.magnitude;

            if (distance > 0.001f) 
            {
                Vector3 direction = movement.normalized;
                Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);

                float circumference = 2f * Mathf.PI * _radius;
                float angle = (distance / circumference) * 360f;

                _visualMesh.Rotate(rotationAxis, angle, Space.World);
            }

            _lastPosition = transform.position;
        }
    }
}