using UnityEngine;
using Gameplay.Grid;
using Gameplay.Spawning.Data;

namespace Gameplay.Cameras.Data
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "TD/Camera/Settings")]
    public class CameraSettingsConfig : ScriptableObject
    {
        [Header("Режим старта")]
        [Tooltip("Если включено, камера стартует строго по координатам ниже, игнорируя базу.")]
        public bool UseAbsoluteManualStart = true;
        
        [Header("Жесткие координаты (Заполняется кнопкой)")]
        public Vector3 AbsolutePosition;
        public Vector3 AbsoluteRotation;
        public float AbsoluteZoom = 25f;

        [Header("Динамический старт (Если галочка выше снята)")]
        [GridPointId(NodeType.Base)] 
        public string InitialFocusBaseId;
        public Vector3 BaseFocusOffset = new Vector3(0f, 0f, 10f);

        [Header("Перемещение (Pan)")]
        public float PanSpeed = 20f;
        public float PanSmoothness = 10f;
        public float DragSpeed = 0.01f; 

        [Header("Масштабирование (Zoom)")]
        public float ZoomSpeed = 30f;
        public float MinZoomY = 5f; 
        public float MaxZoomY = 50f; 

        [Header("Ограничения карты")]
        [Tooltip("Ставь минимум 10, 10! Иначе камера будет отскакивать от краев.")]
        public Vector2 BoundaryPadding = new Vector2(15f, 15f);

        [ContextMenu("🔥 Скопировать ИДЕАЛЬНЫЙ ракурс с Main Camera")]
        private void CopyFromMainCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            AbsolutePosition = cam.transform.position;
            AbsoluteRotation = cam.transform.eulerAngles;

            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            if (groundPlane.Raycast(ray, out float distance))
            {
                AbsoluteZoom = distance;
                Debug.Log($"<color=green>[Camera] СОХРАНЕНО: Позиция {AbsolutePosition}, Углы {AbsoluteRotation}. Зум: {distance}</color>");
            }
            else
            {
                AbsoluteZoom = 25f;
            }
            
            UseAbsoluteManualStart = true;
        }
    }
}