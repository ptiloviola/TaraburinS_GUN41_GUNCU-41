using UnityEngine;

namespace TpsShooter.Audio
{
    public interface IAudioService
    {
        void PlaySFX(string soundId, Vector3 position);
        void PlayUI(string soundId);
        void PlayMusic(string soundId);

        void StartDynamicMusic(string calmId, string combatId);

        void SetCombatMusicState(bool isCombat);

        void SetExtractionMusicState();
    }
}