using MeatMushrooms.Player.Components;
using System;

namespace MeatMushrooms.Player
{
    public class PlayerRegistry
    {
        public PlayerController Player { get; private set; }
        
        // Событие, оповещающее о спавне игрока
        public event Action OnPlayerSpawned;

        public void Register(PlayerController player)
        {
            Player = player;
            OnPlayerSpawned?.Invoke(); // Вызываем событие при регистрации
        }
    }
}