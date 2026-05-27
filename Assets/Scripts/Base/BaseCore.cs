using UnityEngine;
using Gameplay.Enemies;

namespace Gameplay.Base
{
    // Требуем, чтобы на объекте обязательно был коллайдер
    [RequireComponent(typeof(Collider))]
    public class BaseCore : MonoBehaviour
    {
        [Header("Настройки Базы")]
        [SerializeField] private int _lives = 20; // Стартовое количество жизней
        private void OnTriggerEnter(Collider other)
        {

            // Лог 1: Сработало ли вообще физическое касание?
            Debug.Log($"<color=cyan>[BaseCore] Что-то коснулось базы! Имя объекта: {other.gameObject.name}</color>");
            /// Лог 2: Пытаемся найти наш фасад на объекте
            if (other.TryGetComponent(out EnemyFacade enemy))
            {
                Debug.Log($"<color=green>[BaseCore] Нашли EnemyFacade на {enemy.gameObject.name}! Уничтожаем.</color>");
                // Отнимаем жизнь и проверяем поражение
                TakeDamage(1);
                enemy.Despawn();
            }
            else
            {
                // Лог 3: Касание было, но нужного скрипта нет
                Debug.LogWarning($"<color=red>[BaseCore] На объекте {other.gameObject.name} нет компонента EnemyFacade! Может, он висит на дочернем объекте?</color>");
            }
        }

        private void TakeDamage(int amount)
        {
            _lives -= amount;
            Debug.Log($"<color=orange>[BaseCore] Пропущен враг! Осталось жизней: {_lives}</color>");

            if (_lives <= 0)
            {
                Debug.Log("<color=red>[BaseCore] ИГРА ОКОНЧЕНА (GAME OVER)!</color>");
                // В будущем мы добавим сюда паузу игры и вызов UI-экрана поражения
            }
        }
    }
}