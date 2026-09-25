using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Modifiers.Data;

namespace Gameplay.Campaign.Data
{
    [Serializable]
    public class EventChoice
    {
        public string ChoiceText;
        public List<ItemConfig> RewardsOrCurses;
    }

    [CreateAssetMenu(fileName = "NewEventConfig", menuName = "TD/Campaign/Event Config")]
    public class EventConfig : ScriptableObject
    {
        [Header("Визуал на Карте")]
        public Sprite MapIcon;

        public string EventName;
        [TextArea] public string EventDescription;
        public List<EventChoice> Choices = new List<EventChoice>();
    }
}