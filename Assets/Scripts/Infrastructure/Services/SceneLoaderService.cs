using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Infrastructure.Services
{
    public class SceneLoaderService : ISceneLoaderService
    {
        public async UniTask LoadSceneAsync(string sceneName)
        {
            Debug.Log($"<color=yellow>[SceneLoader] Начинаю загрузку сцены: {sceneName}...</color>");
            
            // Запускаем асинхронную загрузку сцены Unity
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            // Ждем завершения через UniTask
            await asyncLoad.ToUniTask();
            
            // Если игра была на паузе при выходе в меню — обязательно снимаем
            Time.timeScale = 1f; 
            
            Debug.Log($"<color=green>[SceneLoader] Сцена {sceneName} успешно загружена.</color>");
        }
    }
}