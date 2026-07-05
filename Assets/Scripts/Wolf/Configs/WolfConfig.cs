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
        public PerceptionConfig Perception;

        [Header("Настройки ИИ (Utility Scores)")]
        public WanderConfig Wander;
        public HuntConfig Hunt;
        public EatConfig Eat;
        public IdleConfig Idle;
        public HowlConfig Howl;
        
        public InvestigateConfig Investigate;
        public ChaseConfig Chase;
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
        public float TurnPenalty = 0.6f;
    }

    [Serializable]
    public class SocialConfig
    {
        [Tooltip("При каком голоде волк начинает рычать на своих")]
        public float AggroHungerThreshold = 10f;
        public float Cooldown = 3f;
        public float StunDuration = 1f;
    }

    [Serializable]
    public class PerceptionConfig
    {
        [Header("Зрение")]
        public float SightDistance = 12f;
        [Range(10f, 180f)]
        public float SightAngle = 90f; 
        public LayerMask ObstacleMask; 
        public LayerMask PlayerMask;   

        [Header("Слух и Подозрение")]
        public float SuspicionBuildRate = 20f; 
        public float SuspicionDecayRate = 10f; 
        
        [Header("Модификаторы голода и отвлечения")]
        [Tooltip("Порог голода, после которого волк становится бдительнее")]
        public float HungerThreshold = 50f;
        [Tooltip("Во сколько раз быстрее копится подозрение у голодного волка")]
        public float HungrySuspicionMultiplier = 1.5f;
        [Tooltip("Штраф к слуху, когда волк занят едой (от 0 до 1)")]
        public float EatingDistractionMultiplier = 0.2f; 
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
        public float MaxDistanceToEat = 2.5f;

        [Header("Оценка (Score)")]
        public float BaseScore = 100f;

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
        public float ShakeProbability = 0.3f;
        public float ShakeDuration = 2f;

        [Header("Глубокий отдых (Пищевая кома)")]
        [Tooltip("Сколько раз волк должен поесть, чтобы захотеть спать")]
        public int MealsToSleep = 2; 
        [Tooltip("За какое время (в секундах) нужно съесть эти порции")]
        public float MealTimeWindow = 40f; 
        public float RestDuration = 20f;
        public float RetreatRadius = 5f;
    }

    [Serializable]
    public class HowlConfig
    {
        [Header("Условия")]
        [Tooltip("Слишком голодные волки не воют")]
        public float MaxHungerToHowl = 40f; 
        
        [Header("Оценка (Score)")]
        public float SpontaneousScore = 60f;
        public float JoinHowlScore = 80f;

        [Header("Поведение")]
        public float HowlDuration = 5f;
        public float HearRadius = 15f;
        public float Cooldown = 30f;
        
        [Tooltip("Шанс завыть самому (проверяется каждую секунду)")]
        [Range(0f, 1f)] 
        public float SpontaneousChance = 0.05f;
    }

    [Serializable]
    public class InvestigateConfig
    {
        [Header("Условия (Ступени подозрения)")]
        [Tooltip("1 ступень (замирает и слушает)")]
        public float NoticeThreshold = 30f; 
        
        [Tooltip("2 ступень (идет к источнику шума)")]
        public float MoveThreshold = 60f;   
        
        public float BaseScore = 65f;

        [Header("Поведение")]
        [Tooltip("Скорость ходьбы к источнику")]
        public float TrotSpeed = 2.5f;
        
        [Tooltip("Сколько секунд осматривается на месте")]
        public float LookAroundTime = 4f;
    }

    [Serializable]
    public class ChaseConfig
    {
        [Header("Погоня и Атака")]
        public float ChaseSpeed = 6f;
        public float AttackDistance = 1.2f;
        
        [Header("Стайный зов")]
        public float AlertRadius = 25f;
        public float HowlDuration = 2f;
    }
    

    #endregion
}