using UnityEngine;

namespace MeatMushrooms.Core
{
    public static class GameSession
    {
        public static int CurrentLevel = 1;

        public static void CompleteLevel()
        {
            CurrentLevel++;
            
            // Читаем старый рекорд, сравниваем с новым
            int highScore = PlayerPrefs.GetInt("HighScore", 1);
            if (CurrentLevel > highScore)
            {
                PlayerPrefs.SetInt("HighScore", CurrentLevel);
                PlayerPrefs.Save(); // Записываем на диск
            }
        }

        public static void Reset()
        {
            CurrentLevel = 1;
        }
    }
}