using UnityEngine;
using Gameplay.Enemies;
using Gameplay.Combat.Statuses;
using Gameplay.Combat;

public class DebugStatusTester : MonoBehaviour
{
    [Header("Настройки теста")]
    [Tooltip("На сколько секунд заморозить")]
    public float FreezeDuration = 3f;
    [Tooltip("Сила замедления (0.5 = на 50%)")]
    public float SlowAmount = 0.5f;

    private void Update()
    {
        // По нажатию на Пробел имитируем попадание ледяного снаряда
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Находим первого попавшегося врага на сцене
            EnemyFacade enemy = FindObjectOfType<EnemyFacade>();
            
            if (enemy != null && enemy.StatusController != null)
            {
                // Накладываем статус
                var freeze = new FreezeStatus(FreezeDuration, SlowAmount);
                enemy.StatusController.AddStatus(freeze);
                
                // Для красоты нанесем 1 единицу взрывного урона, чтобы увидеть мигание
                enemy.GetComponent<DamageReceiver>()?.TakeDamage(new DamagePayload(1f, DamageType.Explosive));
            }
        }
    }
}