using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using UnityEngine;

namespace Gameplay.Towers.Visuals
{
    public class PlacementVisualizer
    {
        private readonly GameObject _validCursor;
        private readonly GameObject _invalidCursor;
        private readonly GameObject _radiusIndicator;
        private readonly GameObject _minRadiusIndicator; 
        private readonly float _heightOffset;

        private GameObject _currentActiveCursor;
        private float _currentMinRange; 

        public PlacementVisualizer(GameObject validCursor, GameObject invalidCursor, GameObject radiusIndicator, GameObject minRadiusIndicator, float heightOffset)
        {
            _validCursor = validCursor;
            _invalidCursor = invalidCursor;
            _radiusIndicator = radiusIndicator;
            _minRadiusIndicator = minRadiusIndicator;
            _heightOffset = heightOffset;
        }

        public void SetSelectedTower(TowerConfig config)
        {
            if (config == null || config.Levels == null || config.Levels.Count == 0) return;

            TowerLevelData baseLevel = config.Levels[0];
            
            var attack = baseLevel.GetModule<AttackStats>();
            var barracks = baseLevel.GetModule<BarracksModuleDescriptor>();

            float range = attack != null ? attack.Range : 
                          barracks != null ? barracks.RallyPointRadius : 0f;
            
            _currentMinRange = attack != null ? attack.MinRange : 0f;
            
            float targetDiameter = range * 2f;
            _radiusIndicator.transform.localScale = new Vector3(targetDiameter, 0.01f, targetDiameter);

            if (_currentMinRange > 0)
            {
                _minRadiusIndicator.SetActive(true);
                _minRadiusIndicator.transform.localScale = new Vector3(_currentMinRange * 2f, 0.015f, _currentMinRange * 2f);
            }
            else
            {
                _minRadiusIndicator.SetActive(false);
            }
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
            
            if (_currentMinRange > 0)
            {
                if (!_minRadiusIndicator.activeSelf) _minRadiusIndicator.SetActive(true);
                _minRadiusIndicator.transform.position = targetPosition + Vector3.down * (_heightOffset * 0.4f);
            }
        }

        public void Hide()
        {
            if (_currentActiveCursor != null)
            {
                _currentActiveCursor.SetActive(false);
                _currentActiveCursor = null;
            }
            _radiusIndicator.SetActive(false);
            _minRadiusIndicator.SetActive(false); 
        }
    }
}