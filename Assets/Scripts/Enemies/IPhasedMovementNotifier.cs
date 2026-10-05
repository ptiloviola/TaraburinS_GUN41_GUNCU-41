using System;

namespace Gameplay.Enemies
{
    public interface IPhasedMovementNotifier
    {
        event Action OnMovementPhaseStarted;
        event Action OnPausePhaseStarted;
    }
}