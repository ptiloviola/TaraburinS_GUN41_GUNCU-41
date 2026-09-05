using UnityEngine;

namespace Gameplay.Towers.Visuals
{
    public class TowerRadiusVisualizer : MonoBehaviour
    {
        [SerializeField] private GameObject _currentRadiusObj; 
        [SerializeField] private GameObject _upgradedRadiusObj; 
        [SerializeField] private GameObject _minRadiusObj; // НОВОЕ: Объект для мертвой зоны

        private void Start()
        {
            HidePreview();
        }

        // minRadius сделан опциональным, чтобы не переписывать вызовы в UI там, где он не нужен
        public void ShowPreview(float currentRadius, float upgradedRadius, float minRadius = 0f)
        {
            if (_currentRadiusObj != null)
            {
                bool showCurrent = currentRadius > 0;
                _currentRadiusObj.SetActive(showCurrent);
                if (showCurrent)
                    _currentRadiusObj.transform.localScale = new Vector3(currentRadius * 2, 0.01f, currentRadius * 2);
            }

            if (_upgradedRadiusObj != null)
            {
                bool showUpgraded = upgradedRadius > 0;
                _upgradedRadiusObj.SetActive(showUpgraded);
                if (showUpgraded)
                    _upgradedRadiusObj.transform.localScale = new Vector3(upgradedRadius * 2, 0.01f, upgradedRadius * 2);
            }

            // Управляем мертвой зоной (включается только если MinRange > 0)
            if (_minRadiusObj != null)
            {
                bool showMin = minRadius > 0;
                _minRadiusObj.SetActive(showMin);
                if (showMin)
                {
                    // Делаем чуть выше (0.015f вместо 0.01f), чтобы не было Z-Fighting (мерцания текстур) с основным кругом
                    _minRadiusObj.transform.localScale = new Vector3(minRadius * 2, 0.015f, minRadius * 2);
                }
            }
        }

        public void HidePreview()
        {
            if (_currentRadiusObj != null) _currentRadiusObj.SetActive(false);
            if (_upgradedRadiusObj != null) _upgradedRadiusObj.SetActive(false);
            if (_minRadiusObj != null) _minRadiusObj.SetActive(false);
        }
    }
}