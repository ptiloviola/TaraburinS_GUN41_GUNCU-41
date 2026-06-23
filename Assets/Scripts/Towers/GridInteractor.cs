using Gameplay.Grid;
using UnityEngine;
using Zenject;
using Gameplay.Towers.Behaviors;
using Gameplay.Towers.Visuals;
using Gameplay.Economy;
using Gameplay.Towers.Data;
using System;
using UnityEngine.EventSystems; // Обязательно для работы с интерфейсом!

namespace Gameplay.Towers
{
    // Оставляем IInitializable для спавна курсоров при старте
    public class GridInteractor : ITickable, IInitializable 
    {
        private readonly IGridService _gridService;
        private readonly GridGenerator _gridGenerator;
        private readonly Camera _mainCamera;
        private readonly Settings _settings;
        private readonly IInstantiator _instantiator;
        private readonly BankService _bankService; // Ссылка на наш кошелек

        // НОВОЕ: Наш каталог всех башен
        private readonly TowerRegistry _towerRegistry;
        // НОВОЕ: Данные башни, которую игрок собирается построить прямо сейчас
        private TowerShopData _selectedTowerData;

        // Храним экземпляры обоих курсоров
        private GameObject _validCursorInstance;
        private GameObject _invalidCursorInstance;
        
        // Ссылка на текущий активный курсор, чтобы не делать лишних GetActive()
        private GameObject _currentActiveCursor;
        // НОВОЕ: Ссылка на инстанс радиуса
        private GameObject _radiusIndicatorInstance;

        // НОВОЕ: Событие, которое сообщит UI, что выбор сброшен
        public event Action OnTowerDeselected;

        [System.Serializable]
        public class Settings
        {
            public LayerMask GridLayerMask;

            [Header("Курсоры")]
            // ТЕПЕРЬ ДВА ПРЕФАБА: Один зеленый, другой красный
            public GameObject ValidCursorPrefab;
            public GameObject InvalidCursorPrefab;
            // Смещение по высоте над сеткой, чтобы избежать мерцания (Z-fighting)
            // НОВОЕ: Поле для префаба радиуса
            [Tooltip("Префаб полупрозрачного круга")]
            public GameObject RadiusIndicatorPrefab;
            public float HeightOffset = 0.05f; 
        }

        public GridInteractor(
            IGridService gridService, 
            GridGenerator gridGenerator, 
            Settings settings,
            IInstantiator instantiator,
            BankService bankService,
            TowerRegistry towerRegistry)
        {
            _gridService = gridService;
            _gridGenerator = gridGenerator;
            _settings = settings;
            _instantiator = instantiator;
            _bankService = bankService;
            _towerRegistry = towerRegistry;
            _mainCamera = Camera.main;
        }

        public void Initialize()
        {
            // Создаем оба курсора сразу
            _validCursorInstance = _instantiator.InstantiatePrefab(_settings.ValidCursorPrefab);
            _invalidCursorInstance = _instantiator.InstantiatePrefab(_settings.InvalidCursorPrefab);
            
            _radiusIndicatorInstance = _instantiator.InstantiatePrefab(_settings.RadiusIndicatorPrefab);
            // Изначально скрываем радиус
            _radiusIndicatorInstance.SetActive(false); 
            // Изначально скрываем оба
            _validCursorInstance.SetActive(false);
            _invalidCursorInstance.SetActive(false);
            
            
            Debug.Log("<color=cyan>[GridInteractor] Профессиональные курсоры созданы!</color>");
            
            // МЫ УДАЛИЛИ ХАК. Теперь _selectedTowerData изначально null.
            // И благодаря твоей проверке в Tick() (if (_selectedTowerData == null) return;) 
            // строитель просто будет спать, пока игрок не кликнет по кнопке.
            
            // ВРЕМЕННЫЙ ХАК ДО ПОЯВЛЕНИЯ UI: 
            // Берем первую башню из каталога по умолчанию, чтобы было что строить
            // if (_towerRegistry.Towers.Count > 0)
            // {
            //     SelectTower(_towerRegistry.Towers[0].TowerId);
            // }
            // else
            // {
            //     Debug.LogError("[GridInteractor] В каталоге нет башен!");
            // }
        }

        // Метод, который позже будет вызывать UI-панель при клике на кнопку
        public void SelectTower(string towerId)
        {
            _selectedTowerData = _towerRegistry.GetTowerById(towerId);
            
            if (_selectedTowerData != null)
                Debug.Log($"<color=cyan>[GridInteractor] Выбрана башня для постройки: {_selectedTowerData.TowerConfig.DisplayName} (Цена: {_selectedTowerData.Cost})</color>");
                if (_selectedTowerData.TowerConfig != null)
                {
                    // В Unity масштаб 1 означает диаметр 1 метр. 
                    // Радиус (Range) нужно умножить на 2, чтобы получить диаметр круга.
                    TowerLevelData baseLevel = _selectedTowerData.TowerConfig.Levels[0]; // Берем характеристики первого уровня
                    float targetDiameter = GetTowerRange(baseLevel) * 2f;
                    Debug.Log($"[GridInteractor] Боевые характеристики: Урон = {baseLevel.Attack.Damage}, Радиус = {baseLevel.Attack.Range}");
                    // Меняем только X и Z. Ось Y (толщину блина) оставляем маленькой.
                    _radiusIndicatorInstance.transform.localScale = new Vector3(targetDiameter, 0.01f, targetDiameter);
                }
            else
                Debug.LogError($"[GridInteractor] Башня с ID {towerId} не найдена в каталоге!");
        }

        // НОВЫЙ МЕТОД: Очистка выбора
        public void DeselectTower()
        {
            _selectedTowerData = null;
            HideAllCursors();
            
            // Запускаем событие для всех подписчиков (например, для UI-панели)
            OnTowerDeselected?.Invoke(); 
            
            Debug.Log("<color=cyan>[GridInteractor] Режим строительства отменен.</color>");
        }

        public void Tick()
        {
            // НОВОЕ: Проверяем нажатие Правой кнопки мыши (1) или Escape для отмены
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (_selectedTowerData != null)
                {
                    DeselectTower();
                }
            }
            // Если ничего не выбрано - даже не пытаемся обрабатывать клики
            if (_selectedTowerData == null) return;
            // 3. НОВОЕ: Защита от "сквозного клика" через UI
            // Если мышка сейчас находится над любым элементом Canvas
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                HideAllCursors(); // Прячем зеленую/красную подсветку ячейки
                return;           // Прерываем Tick, чтобы физический луч не пускался
            }

            // 4. Если мышка над свободной зоной - пускаем луч и разрешаем строить
            HandleMouseInteraction();
        }

        private void HandleMouseInteraction()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _settings.GridLayerMask))
            {
                int gridX = Mathf.RoundToInt(hit.transform.position.x / _gridGenerator.Spacing);
                int gridZ = Mathf.RoundToInt(hit.transform.position.z / _gridGenerator.Spacing);
                Vector2Int gridPos = new Vector2Int(gridX, gridZ);

                // Запрашиваем состояние ячейки
                bool isCellFree = _gridService.CanBuildAt(gridPos);

                // БЕРЕМ ЦЕНУ ИЗ ВЫБРАННОЙ БАШНИ
                bool hasEnoughMoney = _bankService.CurrentBalance >= _selectedTowerData.Cost;
                // Разрешаем строить ТОЛЬКО если есть и место, и деньги
                bool canBuild = isCellFree && hasEnoughMoney;

                // 1. Показываем нужный курсор и перемещаем его
                UpdateCursor(hit.collider, canBuild);

                // 2. Обработка клика
                if (Input.GetMouseButtonDown(0))
                {
                    if (canBuild)
                    {
                        // Пытаемся списать деньги. Если SpendMoney вернул true - строим!
                        if (_bankService.SpendMoney(_selectedTowerData.Cost))
                        {
                            BuildTower(gridPos, hit.collider);
                            // Сразу после постройки ячейка занята, переключаемся на красный курсор
                            UpdateCursor(hit.collider, false); 
                        }
                        
                    }
                    else
                    {
                        // Подробные логи, чтобы понимать, почему не можем построить
                        if (!isCellFree)
                        {
                            Debug.LogWarning($"[GridInteractor] ОТКАЗ! Нельзя строить на {gridPos} - место занято или это дорога.");
                        }
                        else if (!hasEnoughMoney)
                        {
                            Debug.LogWarning($"[GridInteractor] ОТКАЗ! Не хватает денег. Нужно: {_selectedTowerData.Cost}, Баланс: {_bankService.CurrentBalance}");
                        }
                    }
                }
            }
            else
            {
                // Если мышка ушла с сетки - прячем оба курсора
                HideAllCursors();
            }
        }

        private void UpdateCursor(Collider gridBlockCollider, bool isValid)
        {
            // Определяем, какой курсор нам нужен
            GameObject targetCursor = isValid ? _validCursorInstance : _invalidCursorInstance;
            GameObject cursorToHide = isValid ? _invalidCursorInstance : _validCursorInstance;

            // Если нужный курсор еще не активен - переключаем их
            if (_currentActiveCursor != targetCursor)
            {
                cursorToHide.SetActive(false);
                targetCursor.SetActive(true);
                _currentActiveCursor = targetCursor;
            }

            // Перемещаем текущий активный курсор
            Vector3 targetPosition = new Vector3(
                gridBlockCollider.transform.position.x, 
                gridBlockCollider.bounds.max.y + _settings.HeightOffset, // Используем смещение из настроек
                gridBlockCollider.transform.position.z
            );
            
            _currentActiveCursor.transform.position = targetPosition;
            // НОВОЕ: Перемещаем и показываем радиус атаки вместе с курсором
            if (!_radiusIndicatorInstance.activeSelf)
            {
                _radiusIndicatorInstance.SetActive(true);
            }
            // Радиус рисуем чуть ниже курсора, чтобы он лежал прямо на земле
            _radiusIndicatorInstance.transform.position = targetPosition + Vector3.down * (_settings.HeightOffset * 0.5f);
        }

        private void BuildTower(Vector2Int gridPos, Collider gridBlockCollider)
        {
            GridNode node = _gridService.GetNode(gridPos);
            node.IsOccupied = true;

            Vector3 spawnPosition = new Vector3(
                gridBlockCollider.transform.position.x, 
                gridBlockCollider.bounds.max.y, 
                gridBlockCollider.transform.position.z
            );

            // Спавним и сохраняем ссылку на созданный объект
            GameObject towerGo = _instantiator.InstantiatePrefab(_selectedTowerData.Prefab, spawnPosition, Quaternion.identity, null);
            
            // НОВОЕ: Ищем наш "паспорт" и заполняем его!
            if (towerGo.TryGetComponent(out TowerFacade towerFacade))
            {
                // Передаем боевой конфиг и координаты клетки
                towerFacade.Initialize(_selectedTowerData.TowerConfig, gridPos);
            }
            else
            {
                Debug.LogWarning($"[GridInteractor] На префабе {_selectedTowerData.Prefab.name} нет скрипта TowerFacade!");
            }
            
            // var facade = towerGo.GetComponent<TowerFacade>();
            // var attack = towerGo.GetComponentInChildren<AttackBehavior>();
            // var visuals = towerGo.GetComponentInChildren<ProceduralTowerVisuals>();

            // Debug.Log($"[GridInteractor] Результаты поиска: Facade = {facade != null}, Attack = {attack != null}, Visuals = {visuals != null}");

            // if (visuals == null)
            // {
            //     Debug.LogError("<color=red>[GridInteractor] КРИТИКА: На созданном объекте физически отсутствует компонент ProceduralTowerVisuals! Мы спавним не тот префаб!</color>");
            // }
            // ВЫВОДИМ В КОНСОЛЬ ПОЛНУЮ ИНФОРМАЦИЮ
            Debug.Log($"<color=green>[GridInteractor] УСПЕХ! Построена {_selectedTowerData.TowerConfig.DisplayName} на {gridPos} за {_selectedTowerData.Cost} монет.</color>");

        }
        
        private void HideAllCursors()
        {
            if (_currentActiveCursor != null)
            {
                _currentActiveCursor.SetActive(false);
                _currentActiveCursor = null;
            }

            // НОВОЕ: Прячем радиус, когда мышка уходит с сетки или сбрасывается выбор
            if (_radiusIndicatorInstance != null)
            {
                _radiusIndicatorInstance.SetActive(false);
            }
        }

        private float GetTowerRange(TowerLevelData levelData)
        {
             if (levelData.Attack != null && levelData.Attack.Range > 0)
             {
                 return levelData.Attack.Range;
             }

             if (levelData.Aura != null && levelData.Aura.Radius > 0)
             {
                 return levelData.Aura.Radius;
             }
             // НОВОЕ: Если это казарма, показываем радиус сбора!
             if (levelData.Barracks != null && levelData.Barracks.RallyPointRadius > 0) 
             {
                return levelData.Barracks.RallyPointRadius;
             }

             // Если башня вообще без радиуса (например, добывает деньги)
             return 0f; // Если данных нет, возвращаем 0
        }
    }
}