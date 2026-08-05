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

        bool IsWeaponToggleTriggered { get; }
        
        event Action OnJump;
        event Action OnReload;
        event Action OnMelee;
    }
}