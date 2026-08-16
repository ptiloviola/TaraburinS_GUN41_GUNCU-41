using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;
using Random = UnityEngine.Random;

namespace TpsShooter.Audio
{
    public class GlobalAudioService : IAudioService, IInitializable, IDisposable
    {
        private readonly AudioConfig _config;
        private Transform _poolRoot;
        private List<AudioSource> _sfxPool;
        private int _poolSize = 20;

        // Кэшируем группы микшера, чтобы не искать их каждый раз по строкам
        private Dictionary<AudioGroup, AudioMixerGroup> _mixerGroups;

        public GlobalAudioService(AudioConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            _config.Initialize();
            
            // Создаем родительский объект для пула, который не будет уничтожаться при смене сцен
            GameObject rootObject = new GameObject("[GlobalAudioService]");
            GameObject.DontDestroyOnLoad(rootObject);
            _poolRoot = rootObject.transform;

            InitializeMixerGroups();
            InitializePool();
        }

        private void InitializeMixerGroups()
        {
            _mixerGroups = new Dictionary<AudioGroup, AudioMixerGroup>();
            if (_config.MainMixer == null) return;

            // Ищем группы в микшере (имена должны совпадать с enum!)
            foreach (AudioGroup group in Enum.GetValues(typeof(AudioGroup)))
            {
                AudioMixerGroup[] foundGroups = _config.MainMixer.FindMatchingGroups(group.ToString());
                if (foundGroups.Length > 0)
                {
                    _mixerGroups[group] = foundGroups[0];
                }
            }
        }

        private void InitializePool()
        {
            _sfxPool = new List<AudioSource>();
            for (int i = 0; i < _poolSize; i++)
            {
                _sfxPool.Add(CreateNewAudioSource($"SFX_Source_{i}"));
            }
        }

        private AudioSource CreateNewAudioSource(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_poolRoot);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            // Правильный спад звука
            source.rolloffMode = AudioRolloffMode.Logarithmic; 
            return source;
        }

        private AudioSource GetFreeSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying) return source;
            }
            
            // Если все заняты, создаем новый и добавляем в пул (динамическое расширение)
            AudioSource newSource = CreateNewAudioSource($"SFX_Source_{_sfxPool.Count}");
            _sfxPool.Add(newSource);
            return newSource;
        }

        public void PlaySFX(string soundId, Vector3 position)
        {
            SoundRecord record = _config.GetRecord(soundId);
            if (record == null || record.Clips.Length == 0) return;

            AudioSource source = GetFreeSource();
            
            // Настройка 3D
            source.transform.position = position;
            source.spatialBlend = record.SpatialBlend;
            source.minDistance = record.MinDistance;
            source.maxDistance = record.MaxDistance;
            
            // Важно для выстрелов: легкая рандомизация питча (±10%)
            source.pitch = Random.Range(0.9f, 1.1f);
            
            PlayRecordOnSource(record, source);
        }

        public void PlayUI(string soundId)
        {
            SoundRecord record = _config.GetRecord(soundId);
            if (record == null || record.Clips.Length == 0) return;

            AudioSource source = GetFreeSource();
            
            // UI всегда играет в 2D (в голове)
            source.spatialBlend = 0f;
            source.pitch = 1f;
            
            PlayRecordOnSource(record, source);
        }

        public void PlayMusic(string soundId)
        {
            // Для музыки обычно нужен отдельный закрепленный AudioSource с Fade In/Out
            // Пока используем пул в 2D режиме без изменения питча
            SoundRecord record = _config.GetRecord(soundId);
            if (record == null || record.Clips.Length == 0) return;

            AudioSource source = GetFreeSource();
            source.spatialBlend = 0f;
            source.pitch = 1f;
            
            PlayRecordOnSource(record, source);
        }

        private void PlayRecordOnSource(SoundRecord record, AudioSource source)
        {
            source.clip = record.Clips[Random.Range(0, record.Clips.Length)];
            source.volume = record.Volume;
            
            if (_mixerGroups.TryGetValue(record.Group, out AudioMixerGroup mixerGroup))
            {
                source.outputAudioMixerGroup = mixerGroup;
            }

            source.Play();
        }

        public void SetLowpassFilter(bool isActive)
        {
            // Здесь будем управлять Snapshots микшера
        }

        public void SetGroupVolume(AudioGroup group, float volume)
        {
            // Здесь будет конвертация в децибелы
        }

        public void Dispose()
        {
            // Очистка при выходе из игры
        }
    }
}