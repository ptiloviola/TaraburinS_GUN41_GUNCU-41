using System;
using UnityEngine;

namespace TpsShooter.Core
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public event Action OnMeleeStrike;
        public event Action OnFootstep;

        public void TriggerMeleeStrike() => OnMeleeStrike?.Invoke();
        public void TriggerFootstep() => OnFootstep?.Invoke();
    }
}