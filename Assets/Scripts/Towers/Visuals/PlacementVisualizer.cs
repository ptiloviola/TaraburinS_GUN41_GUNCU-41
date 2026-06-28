using Gameplay.Towers.Data;
using UnityEngine;

namespace Gameplay.Towers.Visuals
{
    public class PlacementVisualizer
    {
        private readonly GameObject _validCursor;
        private readonly GameObject _invalidCursor;
        private readonly GameObject _radiusIndicator;
        private readonly float _heightOffset;

        private GameObject _currentActiveCursor;

        public PlacementVisualizer(GameObject validCursor, GameObject invalidCursor, GameObject radiusIndicator, float heightOffset)
        {
            _validCursor = validCursor;
            _invalidCursor = invalidCursor;
            _radiusIndicator = radiusIndicator;
            _heightOffset = heightOffset;
        }

        public void SetSelectedTower(TowerShopData shopData)
        {
            if (shopData?.TowerConfig == null) return;

            // Расчет диаметра круга для визуала радиуса
            TowerLevelData baseLevel = shopData.TowerConfig.Levels[0];
            float range = baseLevel.Attack != null ? baseLevel.Attack.Range : 
                          baseLevel.Barracks != null ? baseLevel.Barracks.RallyPointRadius : 0f;
            
            float targetDiameter = range * 2f;
            _radiusIndicator.transform.localScale = new Vector3(targetDiameter, 0.01f, targetDiameter);
        }

        public void UpdateVisuals(Vector3 blockPosition, float blockMaxY, bool isValid)
        {
            GameObject targetCursor = isValid ? _validCursor : _invalidCursor;
            GameObject cursorToHide = isValid ? _invalidCursor : _validCursor;

            if (_currentActiveCursor != targetCursor)
            {
                cursorToHide.SetActive(false);
                targetCursor.SetActive(true);
                _currentActiveCursor = targetCursor;
            }

            Vector3 targetPosition = new Vector3(blockPosition.x, blockMaxY + _heightOffset, blockPosition.z);
            _currentActiveCursor.transform.position = targetPosition;

            if (!_radiusIndicator.activeSelf) _radiusIndicator.SetActive(true);
            _radiusIndicator.transform.position = targetPosition + Vector3.down * (_heightOffset * 0.5f);
        }

        public void Hide()
        {
            if (_currentActiveCursor != null)
            {
                _currentActiveCursor.SetActive(false);
                _currentActiveCursor = null;
            }
            _radiusIndicator.SetActive(false);
        }
    }
}