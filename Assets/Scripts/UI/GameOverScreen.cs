using MeatMushrooms.Player;
using UnityEngine;
using Zenject;

namespace MeatMushrooms.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        [Inject] private PlayerRegistry _playerRegistry; // Получаем доступ к реестру
        [SerializeField] private GameObject _deathPanel;

        private void Start()
        {
            // Подписываемся на появление Шапочки
            _playerRegistry.OnPlayerSpawned += HookUpPlayerEvents;
            
            // Если Шапочка УЖЕ заспавнилась к этому моменту (страховка)
            if (_playerRegistry.Player != null)
            {
                HookUpPlayerEvents();
            }
        }

        private void HookUpPlayerEvents()
        {
            // Теперь смело подписываемся на ее смерть
            _playerRegistry.Player.OnDeath += ShowGameOver;
        }

        private void ShowGameOver()
        {
            if (_deathPanel != null)
            {
                _deathPanel.SetActive(true);
                Time.timeScale = 0f; // Замораживаем игру
            }
        }

        private void OnDestroy()
        {
            // Отписываемся, чтобы избежать утечек памяти
            if (_playerRegistry != null)
            {
                _playerRegistry.OnPlayerSpawned -= HookUpPlayerEvents;
                if (_playerRegistry.Player != null)
                {
                    _playerRegistry.Player.OnDeath -= ShowGameOver;
                }
            }
        }
    }
}