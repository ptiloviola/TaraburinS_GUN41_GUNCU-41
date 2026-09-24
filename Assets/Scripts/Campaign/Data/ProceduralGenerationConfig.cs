using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Levels.Data;
using Gameplay.Campaign.Services;
using Zenject;

namespace Gameplay.Campaign.Data
{
    [Serializable]
    public class TierConfig
    {
        [Tooltip("Минимальная глубина, на которой работает этот тир")]
        public int MinDepth;
        [Tooltip("Максимальная глубина, на которой работает этот тир")]
        public int MaxDepth;

        [Header("Веса вероятностей (Сумма не обязательна 100)")]
        public float CombatWeight = 60f;
        public float ShopWeight = 20f;
        public float EventWeight = 20f;

        [Header("Пулы контента")]
        public List<LevelBlueprintConfig> CombatPool;
        public List<ShopConfig> ShopPool;
        public List<EventConfig> EventPool;
    }

    [CreateAssetMenu(fileName = "ProceduralGenerationConfig", menuName = "TD/Campaign/Procedural Config")]
    public class ProceduralGenerationConfig : RunModeConfig
    {
        public int MaxDepth = 5; 
        public LevelBlueprintConfig BossLevel; 
        
        public List<TierConfig> Tiers = new List<TierConfig>();
        public override void InstallModeBindings(DiContainer container)
        {
            container.BindInstance(this).AsSingle();
            container.Bind<IRunDirectorService>().To<ProceduralRunDirector>().AsSingle();
        }
    }
}