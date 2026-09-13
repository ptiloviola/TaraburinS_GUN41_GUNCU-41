using System;

namespace Gameplay.Infrastructure.Services
{
    public interface IPauseService
    {
        bool IsPaused { get; }
        void PauseGame();
        void ResumeGame();
        void TogglePause();
    }
}