using UnityEngine;

namespace TpsShooter.Audio
{
    public interface IAudioService
    {
        void PlaySFX(string soundId, Vector3 position);
        void PlayUI(string soundId);
        void PlayMusic(string soundId);
        
        // Задел на будущее для здоровья и настроек
        void SetLowpassFilter(bool isActive);
        void SetGroupVolume(AudioGroup group, float volume);
    }
}