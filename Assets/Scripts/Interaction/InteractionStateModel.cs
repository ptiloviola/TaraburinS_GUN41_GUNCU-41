namespace Gameplay.Interaction
{
    public enum InteractionMode
    {
        Normal,   // Обычный режим (можно выделять башни)
        Building  // Режим стройки (выделение отключено)
    }

    /// <summary>
    /// Глобальное состояние взаимодействия игрока.
    /// Позволяет системам знать текущий режим, не ссылаясь друг на друга.
    /// </summary>
    public class InteractionStateModel
    {
        public InteractionMode CurrentMode { get; set; } = InteractionMode.Normal;
    }
}