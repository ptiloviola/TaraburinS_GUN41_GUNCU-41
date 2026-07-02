using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;
using UnityEngine.AI;

namespace MeatMushrooms.Wolf.States
{
    public class WanderState : IWolfState
    {
        private readonly WolfStats _stats;
        private readonly WolfLocomotion _locomotion;
        
        private const float WanderRadius = 15f; // Радиус поиска случайной точки
        private bool _isWaiting; // Флаг для небольшой паузы между перебежками
        private float _waitTimer;

        // Zenject автоматически передаст сюда и Статы, и Локомоцию (которую мы забиндили через FromComponentOnRoot)
        public WanderState(WolfStats stats, WolfLocomotion locomotion)
        {
            _stats = stats;
            _locomotion = locomotion;
        }

        public float CalculateScore()
        {
            // Базовое желание размять лапы = 20 (чтобы не падать в ноль).
            // Но чем сильнее голод, тем яростнее волк ищет еду (прибавляем до 40 сверху).
            return 20f + (_stats.Hunger * 0.4f); 
        }

        public void Enter()
        {
            Debug.Log("[WanderState] Волк начал блуждание.");
            _isWaiting = false;
            SetNewRandomDestination();
        }

        public void Tick()
        {
            // Если волк сейчас стоит и ждет (например, принюхивается)
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0)
                {
                    _isWaiting = false;
                    SetNewRandomDestination(); // Ждать закончили, идем в новую точку
                }
                return; // Выходим из Tick, чтобы не проверять движение
            }

            // Проверяем, дошел ли волк до цели
            if (_locomotion.HasReachedDestination())
            {
                // Останавливаемся и стоим от 1 до 3 секунд перед следующим шагом
                _locomotion.Stop();
                _isWaiting = true;
                _waitTimer = Random.Range(1f, 3f);
            }
        }

        public void Exit()
        {
            Debug.Log("[WanderState] Волк прекратил блуждание.");
            _locomotion.Stop(); // Обязательно тормозим волка при смене состояния!
        }

        // Метод поиска случайной точки на NavMesh
        private void SetNewRandomDestination()
        {
            // Берем случайную точку в сфере вокруг текущей позиции волка
            Vector3 randomDirection = Random.insideUnitSphere * WanderRadius;
            randomDirection += _locomotion.transform.position;

            // Спрашиваем NavMesh, есть ли рядом с этой случайной точкой проходимая зона
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, WanderRadius, NavMesh.AllAreas))
            {
                _locomotion.MoveTo(hit.position);
            }
            else
            {
                // Если точка оказалась внутри камня, просто ждем секунду и пробуем снова
                _isWaiting = true;
                _waitTimer = 1f;
            }
        }
    }
}