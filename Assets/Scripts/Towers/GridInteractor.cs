using Gameplay.Grid;
using UnityEngine;
using Zenject;
using Gameplay.Towers.Behaviors;
using Gameplay.Towers.Visuals;

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

        // Храним экземпляры обоих курсоров
        private GameObject _validCursorInstance;
        private GameObject _invalidCursorInstance;
        
        // Ссылка на текущий активный курсор, чтобы не делать лишних GetActive()
        private GameObject _currentActiveCursor;

        [System.Serializable]
        public class Settings
        {
            public LayerMask GridLayerMask;
            public GameObject DummyTowerPrefab;
            [Header("Курсоры")]
            // ТЕПЕРЬ ДВА ПРЕФАБА: Один зеленый, другой красный
            public GameObject ValidCursorPrefab;
            public GameObject InvalidCursorPrefab;
            // Смещение по высоте над сеткой, чтобы избежать мерцания (Z-fighting)
            public float HeightOffset = 0.05f; 
        }

        public GridInteractor(
            IGridService gridService, 
            GridGenerator gridGenerator, 
            Settings settings,
            IInstantiator instantiator)
        {
            _gridService = gridService;
            _gridGenerator = gridGenerator;
            _settings = settings;
            _instantiator = instantiator;
            _mainCamera = Camera.main;
        }

        public void Initialize()
        {
            // Создаем оба курсора сразу
            _validCursorInstance = _instantiator.InstantiatePrefab(_settings.ValidCursorPrefab);
            _invalidCursorInstance = _instantiator.InstantiatePrefab(_settings.InvalidCursorPrefab);
            
            // Изначально скрываем оба
            _validCursorInstance.SetActive(false);
            _invalidCursorInstance.SetActive(false);
            
            Debug.Log("<color=cyan>[GridInteractor] Профессиональные курсоры созданы!</color>");
        }

        public void Tick()
        {
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
                bool canBuild = _gridService.CanBuildAt(gridPos);

                // 1. Показываем нужный курсор и перемещаем его
                UpdateCursor(hit.collider, canBuild);

                // 2. Обработка клика
                if (Input.GetMouseButtonDown(0))
                {
                    if (canBuild)
                    {
                        BuildDummyTower(gridPos, hit.collider);
                        // Сразу после постройки ячейка занята, переключаемся на красный курсор
                        UpdateCursor(hit.collider, false); 
                    }
                    else
                    {
                        Debug.LogWarning($"[GridInteractor] ОТКАЗ! Нельзя строить на {gridPos}.");
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
        }

        private void BuildDummyTower(Vector2Int gridPos, Collider gridBlockCollider)
        {
            GridNode node = _gridService.GetNode(gridPos);
            node.IsOccupied = true;

            Vector3 spawnPosition = new Vector3(
                gridBlockCollider.transform.position.x, 
                gridBlockCollider.bounds.max.y, 
                gridBlockCollider.transform.position.z
            );

            // Спавним и сохраняем ссылку на созданный объект
            GameObject towerGo = _instantiator.InstantiatePrefab(_settings.DummyTowerPrefab, spawnPosition, Quaternion.identity, null);
            
            // ВЫВОДИМ В КОНСОЛЬ ПОЛНУЮ ИНФОРМАЦИЮ
            Debug.Log($"<color=orange>[GridInteractor] Создан GameObject: {towerGo.name}. Ищем компоненты...</color>");
            
            var facade = towerGo.GetComponent<TowerFacade>();
            var attack = towerGo.GetComponentInChildren<AttackBehavior>();
            var visuals = towerGo.GetComponentInChildren<ProceduralTowerVisuals>();

            Debug.Log($"[GridInteractor] Результаты поиска: Facade = {facade != null}, Attack = {attack != null}, Visuals = {visuals != null}");

            if (visuals == null)
            {
                Debug.LogError("<color=red>[GridInteractor] КРИТИКА: На созданном объекте физически отсутствует компонент ProceduralTowerVisuals! Мы спавним не тот префаб!</color>");
            }

            Debug.Log($"<color=green>[GridInteractor] УСПЕХ! Башня построена на {gridPos}!</color>");

            

            _instantiator.InstantiatePrefab(_settings.DummyTowerPrefab, spawnPosition, Quaternion.identity, null);
            Debug.Log($"<color=green>[GridInteractor] УСПЕХ! Башня построена на {gridPos}!</color>");
        }
        
        private void HideAllCursors()
        {
            if (_currentActiveCursor != null)
            {
                _currentActiveCursor.SetActive(false);
                _currentActiveCursor = null;
            }
        }
    }
}