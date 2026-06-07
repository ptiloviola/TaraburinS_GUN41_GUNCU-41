using UnityEngine;

namespace Gameplay.Towers.Visuals
{
    public class TowerRadiusVisualizer : MonoBehaviour
    {
        [SerializeField] private GameObject _currentRadiusObj; // Обычный радиус
        [SerializeField] private GameObject _upgradedRadiusObj; // Зеленый радиус превью

        private void Start()
        {
            HidePreview();
        }

        public void ShowPreview(float currentRadius, float upgradedRadius)
        {
            Debug.Log($"[Visualizer] Пытаюсь нарисовать: Текущий = {currentRadius}, Новый = {upgradedRadius}");
            // Управляем текущим радиусом
            if (_currentRadiusObj != null)
            {
                bool showCurrent = currentRadius > 0;
                _currentRadiusObj.SetActive(showCurrent);
                if (showCurrent)
                    _currentRadiusObj.transform.localScale = new Vector3(currentRadius * 2, 0.01f, currentRadius * 2);
            }

            // Управляем будущим радиусом
            if (_upgradedRadiusObj != null)
            {
                bool showUpgraded = upgradedRadius > 0;
                _upgradedRadiusObj.SetActive(showUpgraded); // Вот тут теперь происходит ВЫКЛЮЧЕНИЕ
                if (showUpgraded)
                    _upgradedRadiusObj.transform.localScale = new Vector3(upgradedRadius * 2, 0.01f, upgradedRadius * 2);
            }
        }

        public void HidePreview()
        {
            if (_currentRadiusObj != null) _currentRadiusObj.SetActive(false);
            if (_upgradedRadiusObj != null) _upgradedRadiusObj.SetActive(false);
        }
    }
}