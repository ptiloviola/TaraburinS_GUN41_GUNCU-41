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
            
            
            await SceneManager.LoadSceneAsync(sceneName).WithCancellation(ct);
            
            Time.timeScale = 1f; 
            
            Debug.Log($"<color=green>[SceneLoader] Сцена {sceneName} успешно загружена.</color>");
        }
    }
}