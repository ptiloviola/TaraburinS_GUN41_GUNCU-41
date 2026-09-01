using Infrastructure.Signals;
using UnityEngine;

namespace Gameplay.Enemies.Data.Death
{
    [CreateAssetMenu(fileName = "SplitterDeath", menuName = "TD/Enemies/Death/Splitter")]
    public class SplitterDeathConfig : DeathBehaviorConfig
    {
        [Header("Настройки распада")]
        public string SegmentEnemyId;
        public int SegmentCount = 4;
        public float SpawnRadius = 1.0f;

        public override void Execute(EnemyFacade facade)
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                // Генерируем случайную точку вокруг умирающего босса
                Vector2 randomCircle = Random.insideUnitCircle * SpawnRadius;
                Vector3 spawnPos = facade.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                // Кричим Режиссеру: "Срочно заспавни сегмент вот тут!"
                facade.SignalBus.Fire(new SignalSpawnEnemyRequest
                {
                    EnemyId = SegmentEnemyId,
                    Position = spawnPos,
                    TargetBaseId = "" // Режиссер сам направит их на ближайшую базу
                });
            }
        }
    }
}