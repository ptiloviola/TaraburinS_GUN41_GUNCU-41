using System;
using UnityEngine;
using TpsShooter.Core;

namespace TpsShooter.Audio
{
    public class FootstepAudioSystem : IDisposable
    {
        private readonly IAudioService _audioService;
        private readonly Transform _rootTransform;
        private readonly CharacterAnimationEvents _animEvents;
        private readonly FootstepConfig _config;

        public FootstepAudioSystem(
            IAudioService audioService, 
            Transform rootTransform, 
            CharacterAnimationEvents animEvents,
            FootstepConfig config)
        {
            _audioService = audioService;
            _rootTransform = rootTransform;
            _animEvents = animEvents;
            _config = config;

            if (_animEvents != null)
                _animEvents.OnFootstep += PlayFootstep;
        }

        private void PlayFootstep()
        {
            string soundId = _config.DefaultStep;
            Vector3 rayStart = _rootTransform.position + Vector3.up * _config.RaycastOffset;
            
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, _config.RaycastDistance, _config.GroundMask))
            {
                if (hit.collider.sharedMaterial != null)
                {
                    string matName = hit.collider.sharedMaterial.name.ToLower();
                    if (matName.Contains("metal")) soundId = _config.MetalStep;
                    else if (matName.Contains("dirt") || matName.Contains("grass")) soundId = _config.DirtStep;
                    else if (matName.Contains("wood")) soundId = _config.WoodStep;
                }
            }

            _audioService?.PlaySFX(soundId, _rootTransform.position);
        }

        public void Dispose()
        {
            if (_animEvents != null)
                _animEvents.OnFootstep -= PlayFootstep;
        }
    }
}