using UnityEngine;
using System;

namespace Gameplay.Infrastructure.Input
{
    /// <summary>
    /// Единая точка доступа к вводу игрока.
    /// Изолирует игровую логику от конкретных устройств (мышь, тачскрин, геймпад).
    /// </summary>
    public interface IInputService
    {
        // Текущая позиция курсора на экране
        Vector2 PointerPosition { get; }
        
        // Реактивные события (для UI и строгих стейт-машин)
        event Action OnPrimaryAction;   // Например, клик ЛКМ или тап по экрану
        event Action OnCancelAction;    // Например, клик ПКМ или кнопка Esc
        
        // Флаги состояния (для проверок внутри метода Tick)
        bool IsPrimaryActionDown { get; }
        bool IsCancelActionDown { get; }
    }
}