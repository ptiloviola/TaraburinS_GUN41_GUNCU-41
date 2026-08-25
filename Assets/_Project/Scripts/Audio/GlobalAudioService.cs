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
        private int _combatantsCount = 0;

        private Dictionary<AudioGroup, AudioMixerGroup> _mixerGroups;

        private AudioSource _calmMusicSource;
        private AudioSource _combatMusicSource;

        public GlobalAudioService(AudioConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            _config.Initialize();
            
            GameObject rootObject = new GameObject("[GlobalAudioService]");
            GameObject.DontDestroyOnLoad(rootObject);
            _poolRoot = rootObject.transform;

            InitializeMixerGroups();
            InitializePool();
            InitializeMusicSources();
        }

        private void InitializeMixerGroups()
        {
            _mixerGroups = new Dictionary<AudioGroup, AudioMixerGroup>();
            if (_config.MainMixer == null) return;

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

        private void InitializeMusicSources()
        {
            _calmMusicSource = CreateNewAudioSource("Music_Calm_Source");
            _calmMusicSource.loop = true;
            _calmMusicSource.spatialBlend = 0f;

            _combatMusicSource = CreateNewAudioSource("Music_Combat_Source");
            _combatMusicSource.loop = true;
            _combatMusicSource.spatialBlend = 0f;
        }

        private AudioSource CreateNewAudioSource(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_poolRoot);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.rolloffMode = AudioRolloffMode.Logarithmic; 
            return source;
        }

        private AudioSource GetFreeSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying) return source;
            }
            
            AudioSource newSource = CreateNewAudioSource($"SFX_Source_{_sfxPool.Count}");
            _sfxPool.Add(newSource);
            return newSource;
        }

        public void PlaySFX(string soundId, Vector3 position)
        {
            SoundRecord record = _config.GetRecord(soundId);
            if (record == null || record.Clips.Length == 0) return;

            AudioSource source = GetFreeSource();
            
            source.transform.position = position;
            source.spatialBlend = record.SpatialBlend;
            source.minDistance = record.MinDistance;
            source.maxDistance = record.MaxDistance;
            
            source.pitch = Random.Range(0.9f, 1.1f);
            
            PlayRecordOnSource(record, source);
        }

        public void PlayUI(string soundId)
        {
            SoundRecord record = _config.GetRecord(soundId);
            if (record == null || record.Clips.Length == 0) return;

            AudioSource source = GetFreeSource();
            
            source.spatialBlend = 0f;
            source.pitch = 1f;
            
            PlayRecordOnSource(record, source);
        }

        public void PlayMusic(string soundId) { }

        public void StartDynamicMusic(string calmId, string combatId)
        {
            SoundRecord calmRecord = _config.GetRecord(calmId);
            SoundRecord combatRecord = _config.GetRecord(combatId);

            if (calmRecord != null && calmRecord.Clips.Length > 0)
            {
                _calmMusicSource.clip = calmRecord.Clips[0];
                _calmMusicSource.volume = calmRecord.Volume;
                if (_mixerGroups.TryGetValue(calmRecord.Group, out AudioMixerGroup calmGroup))
                    _calmMusicSource.outputAudioMixerGroup = calmGroup;
                _calmMusicSource.Play();
            }

            if (combatRecord != null && combatRecord.Clips.Length > 0)
            {
                _combatMusicSource.clip = combatRecord.Clips[0];
                _combatMusicSource.volume = combatRecord.Volume;
                if (_mixerGroups.TryGetValue(combatRecord.Group, out AudioMixerGroup combatGroup))
                    _combatMusicSource.outputAudioMixerGroup = combatGroup;
                _combatMusicSource.Play();
            }
        }

        public void SetCombatMusicState(bool isCombat)
        {
            if (isCombat && _config.CombatSnapshot != null)
            {
                _config.CombatSnapshot.TransitionTo(2f);
            }
            else if (!isCombat && _config.ExplorationSnapshot != null)
            {
                _config.ExplorationSnapshot.TransitionTo(4f);
            }
        }

        public void SetExtractionMusicState()
        {
            if (_config.ExtractionSnapshot != null)
            {
                _config.ExtractionSnapshot.TransitionTo(1f); 
            }
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

        public void AddCombatant()
        {
            _combatantsCount++;
            if (_combatantsCount == 1) SetCombatMusicState(true);
        }

        public void RemoveCombatant()
        {
            _combatantsCount--;
            if (_combatantsCount <= 0)
            {
                _combatantsCount = 0;
                SetCombatMusicState(false);
            }
        }



        public void SetLowpassFilter(bool isActive) { }
        public void SetGroupVolume(AudioGroup group, float volume) { }
        public void Dispose() { }


    }
}