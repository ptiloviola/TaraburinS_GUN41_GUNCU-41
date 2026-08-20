using System;
using UnityEngine;
using Zenject;
using TpsShooter.Player;
using TpsShooter.Services.Progress;
using TpsShooter.Services.SceneManagement;
using TpsShooter.Audio;
using Cysharp.Threading.Tasks;

namespace TpsShooter.Environment
{
    public class LevelFlowManager : MonoBehaviour
    {
        [Header("Extraction Settings")]
        [Tooltip("Возможные точки появления эвакуации")]
        [SerializeField] private Transform[] _extractionSpawnLocations;
        [Tooltip("Задержка перед перезагрузкой уровня после победы")]
        [SerializeField] private float _victoryTransitionDelay = 3f;

        public event Action<int> OnVictoryTransition;

        private ExtractionPoint _extractionPoint;
        private PlayerFacade _player;
        private EnemyWaveSpawner _waveSpawner;
        private GameProgressService _progressService;
        private SceneLoaderService _sceneLoader;
        
        private IAudioService _audioService;

        [Inject]
        public void Construct(
            PlayerFacade player,
            EnemyWaveSpawner waveSpawner,
            GameProgressService progressService,
            SceneLoaderService sceneLoader,
            ExtractionPoint extractionPoint,
            IAudioService audioService)
        {
            _player = player;
            _waveSpawner = waveSpawner;
            _progressService = progressService;
            _sceneLoader = sceneLoader;
            _extractionPoint = extractionPoint;
            _audioService = audioService;
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

            _audioService?.StartDynamicMusic("Music_Calm", "Music_Combat");
            
            _audioService?.SetCombatMusicState(false);
        }

        private void ActivateExtraction()
        {
            if (_extractionPoint == null || _extractionSpawnLocations.Length == 0) return;

            Transform randomLocation = _extractionSpawnLocations[UnityEngine.Random.Range(0, _extractionSpawnLocations.Length)];
            _extractionPoint.transform.position = randomLocation.position;
            _extractionPoint.transform.rotation = randomLocation.rotation;

            _audioService?.SetExtractionMusicState();

            _extractionPoint.Activate();
        }

        private void HandleVictory()
        {
            _audioService?.PlaySFX("Player_Joy", _player.transform.position);

            _progressService.NextLevel();
            int nextLevel = _progressService.CurrentLevel;
            
            OnVictoryTransition?.Invoke(nextLevel);
            VictoryTransitionAsync().Forget();
        }

        private async UniTaskVoid VictoryTransitionAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_victoryTransitionDelay));
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