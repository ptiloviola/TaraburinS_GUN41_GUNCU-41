using System;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Services;
using Gameplay.UI.Views;

namespace Gameplay.UI.Presenters
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly ISceneLoaderService _sceneLoader;

        // Избавляемся от магических строк. 
        // В будущем можно вынести в глобальный класс констант SceneNames
        private const string GameplaySceneName = "LevelScene";

        public MainMenuPresenter(MainMenuView view, ISceneLoaderService sceneLoader)
        {
            _view = view;
            _sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            _view.OnPlayClicked += HandlePlayClicked;
            _view.OnLoadSaveClicked += HandleLoadSaveClicked;
            _view.OnSettingsClicked += HandleSettingsClicked;
        }

        public void Dispose()
        {
            _view.OnPlayClicked -= HandlePlayClicked;
            _view.OnLoadSaveClicked -= HandleLoadSaveClicked;
            _view.OnSettingsClicked -= HandleSettingsClicked;
        }

        private void HandlePlayClicked()
        {
            // Блокируем кнопки, чтобы нетерпеливый игрок не запустил загрузку 5 раз
            _view.SetInteractable(false);
            
            // Запускаем асинхронную загрузку. 
            // Используем Forget(), так как не ждем возврата значения в этот метод
            _sceneLoader.LoadSceneAsync(GameplaySceneName).Forget();
        }

        private void HandleLoadSaveClicked()
        {
            // TODO: Вызов подсистемы загрузки сохранений
            Debug.Log("<color=yellow>[MainMenuPresenter] ЗАГЛУШКА: Открытие окна загрузки сохранений...</color>");
        }

        private void HandleSettingsClicked()
        {
            // TODO: Вызов подсистемы настроек
            Debug.Log("<color=yellow>[MainMenuPresenter] ЗАГЛУШКА: Открытие окна настроек...</color>");
        }
    }
}