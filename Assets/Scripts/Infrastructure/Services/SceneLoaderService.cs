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
            Gameplay.Tools.GameLogger.Log($"<color=yellow>[SceneLoader] Начинаю загрузку сцены: {sceneName}...</color>");
            

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            

            if (asyncLoad == null)
            {
                Gameplay.Tools.GameLogger.LogError($"[SceneLoader] Ошибка: Сцена '{sceneName}' не найдена! Убедись, что она добавлена в File -> Build Settings.");
                return; 
            }


            await asyncLoad.WithCancellation(ct);
            

            Time.timeScale = 1f; 
            
            Gameplay.Tools.GameLogger.Log($"<color=green>[SceneLoader] Сцена {sceneName} успешно загружена.</color>");
        }
    }
}