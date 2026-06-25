using UnityEngine;

namespace VacuumSim.Robotics.Components
{
    // Это просто скрипт-метка. Позже мы сможем добавить сюда логику зарядки.
    public class BaseStation : MonoBehaviour
    {
        public Vector3 Position => transform.position;
    }
}