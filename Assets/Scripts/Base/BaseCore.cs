using UnityEngine;
using Gameplay.Enemies;

namespace Gameplay.Base
{
    // Требуем, чтобы на объекте обязательно был коллайдер
    [RequireComponent(typeof(Collider))]
    public class BaseCore : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {

            // Лог 1: Сработало ли вообще физическое касание?
            Debug.Log($"<color=cyan>[BaseCore] Что-то коснулось базы! Имя объекта: {other.gameObject.name}</color>");
            /// Лог 2: Пытаемся найти наш фасад на объекте
            if (other.TryGetComponent(out EnemyFacade enemy))
            {
                Debug.Log($"<color=green>[BaseCore] Нашли EnemyFacade на {enemy.gameObject.name}! Уничтожаем.</color>");
                enemy.Despawn();
            }
            else
            {
                // Лог 3: Касание было, но нужного скрипта нет
                Debug.LogWarning($"<color=red>[BaseCore] На объекте {other.gameObject.name} нет компонента EnemyFacade! Может, он висит на дочернем объекте?</color>");
            }
        }
    }
}