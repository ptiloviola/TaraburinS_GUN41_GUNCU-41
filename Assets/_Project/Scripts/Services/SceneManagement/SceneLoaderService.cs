using UnityEngine;
using UnityEngine.SceneManagement;

namespace TpsShooter.Services.SceneManagement
{
    public class SceneLoaderService
    {
        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f; // Обязательно сбрасываем паузу перед загрузкой
            SceneManager.LoadScene(sceneName);
        }

        public void ReloadCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void SetPause(bool isPaused)
        {
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }
}