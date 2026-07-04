using System;
using UnityEngine;
using MeatMushrooms.Player.Components;

namespace MeatMushrooms.Environment
{
    public class ExitZone : MonoBehaviour
    {
        // Глобальный крик: "Уровень пройден!"
        public static event Action OnLevelCompleted; 

        private void OnTriggerEnter(Collider other)
        {
            // Проверяем, что в триггер вошла именно Шапочка
            if (other.GetComponent<PlayerController>() != null)
            {
                Debug.Log("<color=green>[Exit]</color> Шапочка спаслась!");
                OnLevelCompleted?.Invoke();
            }
        }
    }
}