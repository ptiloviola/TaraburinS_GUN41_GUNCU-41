using System;
using UnityEngine;

namespace TpsShooter.Core
{
    // Теперь это универсальный класс для игрока и для любых врагов
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public event Action OnMeleeStrike;
        public event Action OnFootstep;

        public void TriggerMeleeStrike() => OnMeleeStrike?.Invoke();
        public void TriggerFootstep() => OnFootstep?.Invoke();
    }
}