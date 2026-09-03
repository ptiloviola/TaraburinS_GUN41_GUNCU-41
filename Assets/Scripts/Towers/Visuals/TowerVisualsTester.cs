using UnityEngine;

namespace Gameplay.Towers.Visuals
{
    public class TowerVisualsTester : MonoBehaviour
    {
        private TowerVisualsHub _hub;

        private void Awake()
        {
            _hub = GetComponent<TowerVisualsHub>();
        }
        // Пока оставляем пустым, чтобы ушла ошибка компиляции.
    }
}