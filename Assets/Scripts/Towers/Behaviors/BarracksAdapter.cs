using UnityEngine;
using Zenject;
using Gameplay.Towers.Factories;

namespace Gameplay.Towers.Behaviors
{
    public class BarracksAdapter : MonoBehaviour, IBehaviorAdapter
    {
        private DefenderFactory _defenderFactory;

        // Открываем доступ для чистого контроллера
        public DefenderFactory DefenderFactory => _defenderFactory;
        public Vector3 SpawnPoint => transform.position + transform.forward * 2f;
        public Vector3 Center => transform.position;
        public Vector3 Forward => transform.forward;

        [Inject]
        public void Construct(DefenderFactory defenderFactory)
        {
            _defenderFactory = defenderFactory;
        }

        public ITowerBehavior CreateBehavior()
        {
            // Передаем адаптер внутрь чистого класса
            return new BarracksController(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + transform.forward * 2f, 0.5f);
        }
    }
}