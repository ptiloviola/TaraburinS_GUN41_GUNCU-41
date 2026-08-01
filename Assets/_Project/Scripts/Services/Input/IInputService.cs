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
        
        event Action OnJump;
        event Action OnReload;
        event Action OnMelee;
    }
}