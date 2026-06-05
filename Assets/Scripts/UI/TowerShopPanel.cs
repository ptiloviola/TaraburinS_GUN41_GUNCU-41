using UnityEngine;
using Zenject;
using Gameplay.Towers.Data;
using Gameplay.Towers; // Для доступа к GridInteractor
using System.Collections.Generic; // Нужно для списков


namespace Gameplay.UI
{
    public class TowerShopPanel : MonoBehaviour
    {
        [Header("Настройки UI")]
        [SerializeField] private TowerButtonView _buttonPrefab;
        [SerializeField] private Transform _buttonsContainer; // Куда спавнить кнопки

        private TowerRegistry _towerRegistry;
        private GridInteractor _gridInteractor;

        // НОВОЕ: Храним все кнопки, чтобы управлять их подсветкой
        private List<TowerButtonView> _spawnedButtons = new List<TowerButtonView>();
        // НОВОЕ: Запоминаем, какая башня выбрана сейчас
        private string _currentSelectedId = null;

        // Магия Zenject: он сам найдет этот метод и передаст нужные зависимости
        // при старте сцены, так как этот скрипт висит на объекте в иерархии!
        [Inject]
        public void Construct(TowerRegistry towerRegistry, GridInteractor gridInteractor)
        {
            _towerRegistry = towerRegistry;
            _gridInteractor = gridInteractor;
        }

        private void Start()
        {
            GenerateShopButtons();
            // НОВОЕ: Подписываемся на событие сброса выбора из строителя
            _gridInteractor.OnTowerDeselected += HandleDeselectFromGrid;

            // МЫ УДАЛИЛИ автоматический клик.
            // Кнопки сами при инициализации вызывают SetSelected(false), 
            // так что они все будут серыми (неактивными) при старте сцены.

            // // Автоматически выделяем первую кнопку визуально (т.к. строитель ее тоже берет первой)
            // if (_spawnedButtons.Count > 0)
            // {
            //     OnTowerButtonClicked(_spawnedButtons[0].TowerId);
            // }
        }

        private void OnDestroy()
        {
            // ОБЯЗАТЕЛЬНО: Отписываемся при уничтожении объекта, чтобы избежать утечек памяти
            if (_gridInteractor != null)
            {
                _gridInteractor.OnTowerDeselected -= HandleDeselectFromGrid;
            }
        }

        private void GenerateShopButtons()
        {
            // Защита от пустых данных
            if (_towerRegistry == null || _towerRegistry.Towers.Count == 0)
            {
                Debug.LogWarning("[TowerShopPanel] Каталог башен пуст или не найден!");
                return;
            }

            // Проходимся по всем башням в нашем каталоге
            foreach (TowerShopData towerData in _towerRegistry.Towers)
            {
                // Создаем кнопку в контейнере
                TowerButtonView newButton = Instantiate(_buttonPrefab, _buttonsContainer);
                
                // Передаем в кнопку данные и метод, который нужно вызвать при клике
                newButton.Init(towerData, OnTowerButtonClicked);
                _spawnedButtons.Add(newButton); // Сохраняем кнопку в список
            }
            
            Debug.Log($"<color=cyan>[TowerShopPanel] Сгенерировано {_towerRegistry.Towers.Count} кнопок магазина.</color>");
        }

        // Метод срабатывает, когда GridInteractor сбрасывает выбор (по Esc или Правому клику)
        private void HandleDeselectFromGrid()
        {
            _currentSelectedId = null;
            
            // Гасим подсветку у всех кнопок
            foreach (var btn in _spawnedButtons)
            {
                btn.SetSelected(false);
            }
        }

        // Этот метод срабатывает, когда Игрок жмет на ЛЮБУЮ кнопку башни
        private void OnTowerButtonClicked(string clickedTowerId)
        {
            // НОВОЕ: Если игрок кликнул на УЖЕ выбранную кнопку — отменяем выбор
            if (_currentSelectedId == clickedTowerId)
            {
                _gridInteractor.DeselectTower();
                return;
            }

            _currentSelectedId = clickedTowerId;
            
            Debug.Log($"<color=yellow>[TowerShopPanel] Игрок выбрал башню: {clickedTowerId}</color>");
            
            // Передаем команду нашему строителю!
            _gridInteractor.SelectTower(clickedTowerId);
            // Обновляем визуальное выделение всех кнопок
            foreach (var btn in _spawnedButtons)
            {
                // Если ID совпадает с нажатым - подсвечиваем, иначе гасим
                btn.SetSelected(btn.TowerId == clickedTowerId); 
            }
        }
    }
}
