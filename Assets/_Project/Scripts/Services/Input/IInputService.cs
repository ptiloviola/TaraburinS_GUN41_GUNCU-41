using System;
using UnityEngine;

namespace TpsShooter.Services.Input
{
    public interface IInputService
    {
        Vector2 MoveAxis { get; }
        Vector2 LookAxis { get; }
        bool IsFiring { get; }
        bool IsAiming { get; }
        bool IsRunning { get; }
        bool IsCrouching { get; }
        bool IsRollTriggered { get; }
        
        // Новые события для инвентаря
        event Action<int> OnWeaponSelect; // Передаем индекс (0, 1, 2)
        event Action<int> OnWeaponScroll; // Передаем направление (+1 или -1)
        event Action OnDropWeapon;
        
        event Action OnJump;
        event Action OnReload;
        event Action OnMelee;
    }
}