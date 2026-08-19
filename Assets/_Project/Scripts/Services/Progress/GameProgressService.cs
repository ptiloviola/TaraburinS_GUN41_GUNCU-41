using UnityEngine;

namespace TpsShooter.Services.Progress
{
    public class GameProgressService
    {
        private const string HighScoreKey = "HighScore";

        public int CurrentLevel { get; private set; } = 1;
        public int HighScore { get; private set; }

        public GameProgressService()
        {
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 1);
        }

        public void NextLevel()
        {
            CurrentLevel++;
            if (CurrentLevel > HighScore)
            {
                HighScore = CurrentLevel;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }
            DevLogger.Log($"<color=cyan>[Progress]</color> Переход на уровень {CurrentLevel}. Рекорд: {HighScore}");
        }

        public void ResetProgress()
        {
            CurrentLevel = 1;
            DevLogger.Log("<color=cyan>[Progress]</color> Прогресс сброшен до 1 уровня.");
        }
    }
}