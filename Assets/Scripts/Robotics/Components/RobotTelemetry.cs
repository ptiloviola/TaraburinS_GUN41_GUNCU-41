using UnityEngine;

namespace VacuumSim.Robotics.Components
{
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

            GUI.Label(new Rect(20, 20, 400, 30), $"Скорость Rigidbody: {_rb.velocity.magnitude:F3}");
            
            if (_rb.IsSleeping())
            {
                GUI.contentColor = Color.red;
                GUI.Label(new Rect(20, 50, 400, 30), "ОШИБКА: RIGIDBODY УСНУЛ!");
            }
        }
    }
}
