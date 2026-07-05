using UnityEngine;

namespace MeatMushrooms.Core
{
    public static class GameSession
    {
        public static int CurrentLevel = 1;

        public static void CompleteLevel()
        {
            CurrentLevel++;
            
            int highScore = PlayerPrefs.GetInt("HighScore", 1);
            if (CurrentLevel > highScore)
            {
                PlayerPrefs.SetInt("HighScore", CurrentLevel);
                PlayerPrefs.Save();
            }
        }

        public static void Reset()
        {
            CurrentLevel = 1;
        }
    }
}