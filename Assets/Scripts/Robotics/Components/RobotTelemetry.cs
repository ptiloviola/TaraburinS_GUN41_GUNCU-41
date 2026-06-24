using UnityEngine;

namespace VacuumSim.Robotics.Components
{
    // Этот скрипт нужен только для отладки, потом мы его удалим
    [RequireComponent(typeof(Rigidbody))]
    public class RobotTelemetry : MonoBehaviour
    {
        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnGUI()
        {
            GUI.contentColor = Color.yellow;
            GUI.skin.label.fontSize = 20;

            // Выводим реальную физическую скорость робота
            GUI.Label(new Rect(20, 20, 400, 30), $"Скорость Rigidbody: {_rb.velocity.magnitude:F3}");
            
            // Проверяем, не выключила ли Unity физику для этого объекта
            if (_rb.IsSleeping())
            {
                GUI.contentColor = Color.red;
                GUI.Label(new Rect(20, 50, 400, 30), "ОШИБКА: RIGIDBODY УСНУЛ!");
            }
        }
    }
}
