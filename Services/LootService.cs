using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using ProjectWarnerShared.Entities;
using ProjectWarnerShared.Entities.Items;

namespace WarnerEngine.Services
{
    public sealed class LootService : Service
    {
        public enum LootClass { LowLevelEnemy, MidLevelEnemy }
        private static readonly Dictionary<LootClass, (string, float)[]> LOOT_CLASS_DROPS = new Dictionary<LootClass, (string, float)[]>()
        {
            { LootClass.LowLevelEnemy, new (string, float)[] {(Ribble.ENTITY_KEY_ONE, 0.5f), (Ribble.ENTITY_KEY_FIVE, 0.6f), (Ribble.ENTITY_KEY_TWENTY, 0.62f) }},
            { LootClass.MidLevelEnemy, new (string, float)[] {(Ribble.ENTITY_KEY_ONE, 0.2f), (Ribble.ENTITY_KEY_FIVE, 0.7f), (Ribble.ENTITY_KEY_TWENTY, 0.75f) }}
        };

        public LootService() { }

        public void DropLoot(LootClass Class, Vector3 Position, int NumberOfRolls = 1)
        {
            (string, float)[] chances = LOOT_CLASS_DROPS[Class];
            for (int i = 0; i < NumberOfRolls; i++) {
                float diceRoll = (float)GameService.GetService<StateService>().GetGlobalRandom().NextDouble();
                foreach ((string key, float chance) in chances)
                {
                    if (diceRoll <= chance)
                    {
                        GameService.GetService<SceneService>().CurrentScene.AddEntity(EntitySpawner.CreateEntityByKey(key, Position, new string[] { }));
                        break;
                    }
                }
            }
        }

        public override Type GetBackingInterfaceType()
        {
            return typeof(LootService);
        }
    }
}
