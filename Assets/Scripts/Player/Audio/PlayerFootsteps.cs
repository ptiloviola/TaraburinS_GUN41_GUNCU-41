using UnityEngine;
using Infrastructure.Interfaces;
namespace Player.Audio
{
    public class PlayerFootsteps
    {
        private readonly AudioSource _footstepAudio;
        private readonly CharacterController _controller;
        private readonly IInputProvider _input;

        public PlayerFootsteps(AudioSource footstepAudio, CharacterController controller, IInputProvider input)
        {
            _footstepAudio = footstepAudio;
            _controller = controller;
            _input = input;
        }

        // Вызывается каждый кадр из PlayerController
        public void Tick()
        {
            // Логика определения шагов теперь инкапсулирована здесь
            bool isWalking = _input.MoveInput.sqrMagnitude > 0.01f && _controller.isGrounded;

            if (isWalking)
            {
                if (!_footstepAudio.isPlaying)
                {
                    _footstepAudio.Play();
                }
            }
            else
            {
                if (_footstepAudio.isPlaying)
                {
                    _footstepAudio.Pause();
                }
            }
        }
    }
}