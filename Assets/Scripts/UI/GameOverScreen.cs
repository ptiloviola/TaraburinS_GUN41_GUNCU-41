using MeatMushrooms.Player.Components;
using UnityEngine;


namespace MeatMushrooms.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private GameObject _deathPanel;

        private void OnEnable() => _player.OnDeath += ShowGameOver;
        private void OnDisable() => _player.OnDeath -= ShowGameOver;

        private void ShowGameOver()
    {
        Debug.Log("<color=cyan>[GameOverScreen]</color> Сигнал о смерти получен менеджером!");
        
        if (_deathPanel == null)
        {
            Debug.LogError("<color=red>[GameOverScreen]</color> ОШИБКА: Панель смерти не назначена в Инспекторе!");
            return;
        }

        _deathPanel.SetActive(true);
        Debug.Log($"<color=cyan>[GameOverScreen]</color> Панель активирована. Ее состояние: {_deathPanel.activeInHierarchy}");
        
        Time.timeScale = 0f; 
    }
    }
}
