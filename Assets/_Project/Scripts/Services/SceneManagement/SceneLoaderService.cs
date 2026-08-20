using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace TpsShooter.Services.SceneManagement
{
    public class SceneLoaderService
    {
        public async UniTaskVoid LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            
            await SceneManager.LoadSceneAsync(sceneName).ToUniTask();
        }

        public async UniTaskVoid ReloadCurrentScene()
        {
            Time.timeScale = 1f;
            
            await SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name).ToUniTask();
        }

        public void SetPause(bool isPaused)
        {
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }
}