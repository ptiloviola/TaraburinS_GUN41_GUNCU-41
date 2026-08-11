using System;
using UnityEngine;

namespace TpsShooter.Player.Core
{
    // Этот скрипт автоматически повесится на объект с Аниматором через Фасад
    public class PlayerAnimationEvents : MonoBehaviour
    {
        public event Action OnMeleeStrike;

        // ЭТОТ метод мы выберем в окне Animation в Unity
        public void TriggerMeleeStrike()
        {
            OnMeleeStrike?.Invoke();
        }
    }
}