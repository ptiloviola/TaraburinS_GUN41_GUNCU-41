using UnityEngine;
using System;
namespace Infrastructure.Interfaces
{
    public interface IInputProvider
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        
        event Action OnFireStarted;
        event Action OnReloadStarted;
        event Action<bool> OnAimChanged;
    }
}