using UnityEngine;
using System;

namespace Gameplay.Infrastructure.Input
{
    public interface IInputService
    {

        Vector2 PointerPosition { get; }
        event Action OnPrimaryAction;
        event Action OnCancelAction;
        event Action OnPauseAction;
        bool IsPrimaryActionDown { get; }
        bool IsCancelActionDown { get; }
        


        Vector2 PanDelta { get; }
        float ZoomDelta { get; }

        bool IsDragPanning { get; }
        Vector2 PointerDelta { get; }
    }
}