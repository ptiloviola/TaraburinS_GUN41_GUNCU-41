using System;

namespace Gameplay.Enemies.Movement
{
    public interface IPhasedMovementNotifier
    {
        event Action OnMovementPhaseStarted;
        event Action OnPausePhaseStarted;
    }
}