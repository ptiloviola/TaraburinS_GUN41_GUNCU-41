using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace TpsShooter.Audio
{
    public enum AudioGroup
    {
        SFX,
        Music,
        Music_Calm,   // Подгруппа для спокойной музыки
        Music_Combat, // Подгруппа для боевой музыки
        UI,
        Ambient
    }

    [Serializable]
    public class SoundRecord
    {
        [Tooltip("Уникальный ID (например: Player_Step_Concrete)")]
        public string ID;
        
        [Tooltip("Массив клипов (выбирается случайно, если их несколько)")]
        public AudioClip[] Clips;
        
        public AudioGroup Group = AudioGroup.SFX;
        
        [Range(0f, 1f)] public float Volume = 1f;
        
        [Header("3D Settings")]
        [Range(0f, 1f)] public float SpatialBlend = 1f; // 1 = полное 3D
        public float MinDistance = 5f;
        public float MaxDistance = 50f;
    }

    [CreateAssetMenu(fileName = "AudioConfig", menuName = "TpsShooter/Audio/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        public AudioMixer MainMixer;
        
        [Header("Mixer Snapshots")]
        public AudioMixerSnapshot ExplorationSnapshot;
        public AudioMixerSnapshot CombatSnapshot;
        public AudioMixerSnapshot ExtractionSnapshot;

        public List<SoundRecord> Sounds = new List<SoundRecord>();

        private Dictionary<string, SoundRecord> _soundDictionary;

        public void Initialize()
        {
            _soundDictionary = new Dictionary<string, SoundRecord>();
            foreach (var record in Sounds)
            {
                if (!string.IsNullOrEmpty(record.ID) && !_soundDictionary.ContainsKey(record.ID))
                {
                    _soundDictionary.Add(record.ID, record);
                }
            }
        }

        public SoundRecord GetRecord(string id)
        {
            if (_soundDictionary == null) Initialize();
            
            if (_soundDictionary.TryGetValue(id, out SoundRecord record))
            {
                return record;
            }
            
            Debug.LogWarning($"[AudioConfig] Звук с ID '{id}' не найден!");
            return null;
        }
    }
}