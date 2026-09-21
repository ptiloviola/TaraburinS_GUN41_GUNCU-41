using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;

namespace Gameplay.Infrastructure.Services
{
    public class SceneLoaderService : ISceneLoaderService
    {
        public async UniTask LoadSceneAsync(string sceneName, CancellationToken ct = default)
        {
            Debug.Log($"<color=yellow>[SceneLoader] Начинаю загрузку сцены: {sceneName}...</color>");
            

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            

            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneLoader] Ошибка: Сцена '{sceneName}' не найдена! Убедись, что она добавлена в File -> Build Settings.");
                return; 
            }


            await asyncLoad.WithCancellation(ct);
            

            Time.timeScale = 1f; 
            
            Debug.Log($"<color=green>[SceneLoader] Сцена {sceneName} успешно загружена.</color>");
        }
    }
}