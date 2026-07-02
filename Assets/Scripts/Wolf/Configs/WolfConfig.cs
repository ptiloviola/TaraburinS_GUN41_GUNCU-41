using System;
using UnityEngine;

namespace MeatMushrooms.Wolf.Configs
{
    [CreateAssetMenu(fileName = "WolfConfig", menuName = "MeatMushrooms/Wolf Config")]
    public class WolfConfig : ScriptableObject
    {
        [Header("Базовые системы")]
        public StatsConfig Stats;
        public LocomotionConfig Locomotion;
        public SocialConfig Social;

        [Header("Настройки ИИ (Utility Scores)")]
        public WanderConfig Wander;
        public HuntConfig Hunt;
        public EatConfig Eat;
        public IdleConfig Idle;
    }

    #region Базовые системы (Физиология и Движение)
    
    [Serializable]
    public class StatsConfig
    {
        public float MaxHealth = 100f;
        public float MaxHunger = 100f;
        public float HungerDepletionRate = 1f;
    }

    [Serializable]
    public class LocomotionConfig
    {
        public float WalkSpeed = 1.5f;
        public float RunSpeed = 3.5f;
        public float TurnSmoothSpeed = 2f;
        public float MaxTurnAngle = 90f;
        public float StoppingDistance = 0.5f;
        [Tooltip("Насколько сильно волк тормозит при крутом повороте (0 - не тормозит, 0.8 - почти останавливается)")]
        [Range(0f, 0.9f)] 
        public float TurnPenalty = 0.6f; // ДОБАВИЛИ
    }

    [Serializable]
    public class SocialConfig
    {
        [Tooltip("При каком голоде волк начинает рычать на своих")]
        public float AggroHungerThreshold = 10f;
        public float Cooldown = 3f;
        public float StunDuration = 1f;
    }
    
    #endregion

    #region Настройки ИИ (States)

    [Serializable]
    public class WanderConfig
    {
        [Header("Оценка (Score)")]
        public float BaseScore = 20f;
        public float HungerMultiplier = 0.4f;

        [Header("Поведение")]
        public float WanderRadius = 15f;
        public float MinWaitTime = 1f;
        public float MaxWaitTime = 3f;
        public float ObstacleWaitTime = 1f;
    }

    [Serializable]
    public class HuntConfig
    {
        [Header("Условия")]
        public float MinHungerToHunt = 30f;
        public float DistanceToSwitchToEat = 2.5f;

        [Header("Оценка (Score)")]
        public float BaseScore = 50f;
        public float HungerMultiplier = 0.4f;
    }

    [Serializable]
    public class EatConfig
    {
        [Header("Условия")]
        public float MaxDistanceToEat = 2.5f; // Синхронизируем с HuntConfig

        [Header("Оценка (Score)")]
        public float BaseScore = 100f; // Абсолютный приоритет, если еда в радиусе

        [Header("Поведение")]
        public float BiteInterval = 1f;
        public float BiteDamage = 25f;
        public float RotationSpeed = 5f;
    }

    [Serializable]
    public class IdleConfig
    {
        [Header("Оценка (Score)")]
        public float BaseScore = 40f;
        public float HungerPenaltyMultiplier = 0.5f;
        [Tooltip("Бонус к оценке, чтобы волк не просыпался от малейшего голода")]
        public float StickyRestScoreBonus = 30f;

        [Header("Поведение в покое (Стояние / Отряхивание)")]
        public float MinIdleTime = 2f;
        public float MaxIdleTime = 6f;
        public float ShakeProbability = 0.3f; // 30% шанс отряхнуться
        public float ShakeDuration = 2f;      // Время анимации отряхивания

        [Header("Глубокий отдых (Пищевая кома)")]
        [Tooltip("Сколько раз волк должен поесть, чтобы захотеть спать")]
        public int MealsToSleep = 2; 
        [Tooltip("За какое время (в секундах) нужно съесть эти порции")]
        public float MealTimeWindow = 40f; 
        public float RestDuration = 20f;
        public float RetreatRadius = 5f;
    }

    #endregion
}