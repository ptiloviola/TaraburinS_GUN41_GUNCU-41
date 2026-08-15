using System;
using System.Collections;
using UnityEngine;
using Zenject;
using TpsShooter.Player;
using TpsShooter.Services.Progress;
using TpsShooter.Services.SceneManagement;

namespace TpsShooter.Environment
{
    public class LevelFlowManager : MonoBehaviour
    {
        [Header("Extraction Settings")]
        [Tooltip("Возможные точки появления эвакуации")]
        [SerializeField] private Transform[] _extractionSpawnLocations;

        // Событие для Презентера: передает номер СЛЕДУЮЩЕГО уровня
        public event Action<int> OnVictoryTransition;

        private ExtractionPoint _extractionPoint;
        private PlayerFacade _player;
        private EnemyWaveSpawner _waveSpawner;
        private GameProgressService _progressService;
        private SceneLoaderService _sceneLoader;

        [Inject]
        public void Construct(
            PlayerFacade player,
            EnemyWaveSpawner waveSpawner,
            GameProgressService progressService,
            SceneLoaderService sceneLoader,
            ExtractionPoint extractionPoint) 
        {
            _player = player;
            _waveSpawner = waveSpawner;
            _progressService = progressService;
            _sceneLoader = sceneLoader;
            _extractionPoint = extractionPoint;
        }

        private void Start()
        {
            if (_extractionPoint != null)
            {
                _extractionPoint.gameObject.SetActive(false);
                _extractionPoint.OnPlayerExtracted += HandleVictory;
                _extractionPoint.OnTimeExpired += HandleDefeat;
            }

            _player.Health.OnDeath += HandleDefeat;
            _waveSpawner.OnAllEnemiesDefeated += ActivateExtraction;
        }

        private void ActivateExtraction()
        {
            if (_extractionPoint == null || _extractionSpawnLocations.Length == 0) return;

            Transform randomLocation = _extractionSpawnLocations[UnityEngine.Random.Range(0, _extractionSpawnLocations.Length)];
            _extractionPoint.transform.position = randomLocation.position;
            _extractionPoint.transform.rotation = randomLocation.rotation;

            _extractionPoint.Activate();
        }

        private void HandleVictory()
        {
            _progressService.NextLevel();
            int nextLevel = _progressService.CurrentLevel;
            
            // Вызываем событие, чтобы Презентер показал UI
            OnVictoryTransition?.Invoke(nextLevel);
            
            // Запускаем корутину с задержкой перед сменой сцены
            StartCoroutine(VictoryTransitionRoutine());
        }

        private IEnumerator VictoryTransitionRoutine()
        {
            // Ждем 3 секунды, чтобы игрок успел прочитать текст
            yield return new WaitForSeconds(3f);
            _sceneLoader.ReloadCurrentScene(); 
        }

        private void HandleDefeat()
        {
            _sceneLoader.SetPause(true);
            _progressService.ResetProgress();
            _sceneLoader.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (_extractionPoint != null)
            {
                _extractionPoint.OnPlayerExtracted -= HandleVictory;
                _extractionPoint.OnTimeExpired -= HandleDefeat;
            }

            if (_player != null && _player.Health != null)
                _player.Health.OnDeath -= HandleDefeat;

            if (_waveSpawner != null)
                _waveSpawner.OnAllEnemiesDefeated -= ActivateExtraction;
        }
    }
}