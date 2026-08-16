using System;
using UnityEngine;

namespace TpsShooter.Player.Core
{
    public class PlayerAnimationEvents : MonoBehaviour
    {
        public event Action OnMeleeStrike;
        public event Action OnFootstep;

        public void TriggerMeleeStrike()
        {
            OnMeleeStrike?.Invoke();
        }

        public void TriggerFootstep()
        {
            OnFootstep?.Invoke();
        }
    }
}