using UnityEngine;
using MeatMushrooms.Player.Components;
using System;

namespace MeatMushrooms.Player
{
    public class PlayerRegistry
    {
        // Кэшированные ссылки на все важные модули Шапочки
        public PlayerController Controller { get; private set; }
        public PlayerHealth Health { get; private set; }
        public PlayerStealth Stealth { get; private set; }
        
        public event Action OnPlayerSpawned;

        // Передаем сюда весь объект при спавне
        public void Register(GameObject playerInstance)
        {
            // Делаем GetComponent РОВНО ОДИН РАЗ за всю игру!
            Controller = playerInstance.GetComponent<PlayerController>();
            Health = playerInstance.GetComponent<PlayerHealth>();
            Stealth = playerInstance.GetComponent<PlayerStealth>();
            
            OnPlayerSpawned?.Invoke(); 
        }
    }
}