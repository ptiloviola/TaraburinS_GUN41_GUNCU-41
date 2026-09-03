using UnityEngine;
using Zenject;
using Gameplay.Core;

namespace Gameplay.Enemies.Visuals
{
    [RequireComponent(typeof(DamageReceiver))]
    public class FloatingTextSpawner : MonoBehaviour
    {
        [SerializeField] private DamageVisualSettings _settings;
        [SerializeField] private Transform _spawnPoint; // Можно привязать пустой объект над головой врага

        private DamageReceiver _receiver;
        private FloatingText.Pool _textPool;

        [Inject]
        public void Construct(FloatingText.Pool textPool)
        {
            _textPool = textPool;
        }

        private void Awake()
        {
            _receiver = GetComponent<DamageReceiver>();
        }

        private void OnEnable() => _receiver.OnHitReceived += SpawnText;
        private void OnDisable() => _receiver.OnHitReceived -= SpawnText;

        private void SpawnText(DamagePayload payload)
        {
            if (_settings == null || _textPool == null) return;

            var floatingText = _textPool.Spawn();
            Vector3 pos = _spawnPoint != null ? _spawnPoint.position : transform.position + Vector3.up;
            
            floatingText.Init(pos, payload.Amount, _settings.GetColor(payload.Type), _textPool);
        }
    }
}