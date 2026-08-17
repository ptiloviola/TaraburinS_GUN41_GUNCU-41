using UnityEngine;

namespace TpsShooter.UI.Settings
{
    public class SettingsModel
    {
        private const string MasterKey = "Settings_MasterVolume";
        private const string MusicKey = "Settings_MusicVolume";
        private const string SFXKey = "Settings_SFXVolume";

        // По умолчанию громкость максимальная (1.0)
        public float MasterVolume 
        { 
            get => PlayerPrefs.GetFloat(MasterKey, 1f); 
            set => PlayerPrefs.SetFloat(MasterKey, value); 
        }

        public float MusicVolume 
        { 
            get => PlayerPrefs.GetFloat(MusicKey, 1f); 
            set => PlayerPrefs.SetFloat(MusicKey, value); 
        }

        public float SFXVolume 
        { 
            get => PlayerPrefs.GetFloat(SFXKey, 1f); 
            set => PlayerPrefs.SetFloat(SFXKey, value); 
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }
    }
}