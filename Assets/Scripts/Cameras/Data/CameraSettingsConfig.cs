using UnityEngine;
using Gameplay.Grid; // Подключаем твой домен с NodeType
using Gameplay.Spawning.Data;


namespace Gameplay.Cameras.Data
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "TD/Camera/Settings")]
    public class CameraSettingsConfig : ScriptableObject
    {
        [Header("Стартовый вид")]
        [GridPointId(NodeType.Base)] // ТВОЯ МАГИЯ: Теперь тут выпадающий список!
        public string InitialFocusBaseId;
        
        [Header("Ракурс (Pitch/Yaw)")]
        [Range(10f, 85f)] public float CameraPitch = 50f;
        [Range(-180f, 180f)] public float CameraYaw = 0f;

        [Header("Кадрирование (Сдвиг фокуса)")]
        [Tooltip("Сдвигает фокус относительно базы. Z > 0 опустит базу вниз экрана.")]
        public Vector3 BaseFocusOffset = new Vector3(0f, 0f, 10f);

        [Header("Перемещение (Pan)")]
        public float PanSpeed = 20f;
        public float PanSmoothness = 10f;
        public float DragSpeed = 0.05f; 

        [Header("Масштабирование (Zoom)")]
        public float ZoomSpeed = 15f;
        public float MinZoomY = 5f; 
        public float MaxZoomY = 25f; 
        public float ZoomSmoothness = 8f;

        [Header("Ограничения карты")]
        [Tooltip("ВАЖНО: Должно быть достаточно большим (например, 15-20), чтобы камера могла отъехать от краев карты!")]
        public Vector2 BoundaryPadding = new Vector2(15f, 15f);

        // СКРИПТ-ПОМОЩНИК: Копирует настройки со сцены
        [ContextMenu("Скопировать ракурс с Main Camera")]
        private void CopyFromMainCamera()
        {
            if (Camera.main != null)
            {
                CameraPitch = Camera.main.transform.eulerAngles.x;
                CameraYaw = Camera.main.transform.eulerAngles.y;
                Debug.Log($"<color=green>[CameraSettings] Ракурс скопирован: Pitch = {CameraPitch:F1}, Yaw = {CameraYaw:F1}</color>");
            }
        }
    }
}