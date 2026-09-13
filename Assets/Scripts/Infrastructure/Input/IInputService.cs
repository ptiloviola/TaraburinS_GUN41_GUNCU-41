using UnityEngine;
using System;

namespace Gameplay.Infrastructure.Input
{
    public interface IInputService
    {
        // Существующие свойства для взаимодействия
        Vector2 PointerPosition { get; }
        event Action OnPrimaryAction;
        event Action OnCancelAction;
        event Action OnPauseAction;
        bool IsPrimaryActionDown { get; }
        bool IsCancelActionDown { get; }
        

        // НОВЫЕ СВОЙСТВА ДЛЯ КАМЕРЫ
        Vector2 PanDelta { get; } // Вектор движения (WASD/Стрелки)
        float ZoomDelta { get; }  // Прокрутка колесика (Scroll)

        bool IsDragPanning { get; } // Зажато ли колесико
        Vector2 PointerDelta { get; } // Смещение мыши за кадр
    }
}