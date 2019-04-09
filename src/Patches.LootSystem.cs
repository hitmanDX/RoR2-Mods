using RoR2;
using RoR2.ConVar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DXsRoR2Mods {
    partial class Patches {

        static void PopulateLoot_Init() {
            createConVar<BoolConVar>("b_use_custom_loot", ConVarFlags.ExecuteOnServer, "0", "Enables custom looting settings.");
            createConVar<FloatConVar>("f_additional_player_multiplier", ConVarFlags.ExecuteOnServer, "0.5", "Scale loot based on number of players.");
            createConVar<IntConVar>("i_add_amount_2_base", ConVarFlags.ExecuteOnServer, "0", "Adds x amount of money to start off with scaled with per level.");

            createConVar<FloatConVar>("f_loot_hiding_rate", ConVarFlags.ExecuteOnServer, "0.20", "Percecent at which to hide loot in multi shop.");

            createConVar<FloatConVar>("f_double_loot_rate", ConVarFlags.ExecuteOnServer, "0", "Percecent at getting double drops.");
            createConVar<FloatConVar>("f_basic_loot_rate", ConVarFlags.ExecuteOnServer, "0", "Percecent at which basic mobs drop loot.");
            createConVar<FloatConVar>("f_basic_tier1_weight", ConVarFlags.ExecuteOnServer, "0.80", "Weight at which basic mobs drop tier1.");
            createConVar<FloatConVar>("f_basic_tier2_weight", ConVarFlags.ExecuteOnServer, "0.20", "Weight at which basic mobs drop tier2.");
            createConVar<FloatConVar>("f_basic_tier3_weight", ConVarFlags.ExecuteOnServer, "0.01", "Weight at which basic mobs drop tier3.");
            createConVar<FloatConVar>("f_basic_lunar_weight", ConVarFlags.ExecuteOnServer, "0.02", "Weight at which basic mobs drop lunar items.");
            createConVar<FloatConVar>("f_basic_equipment_weight", ConVarFlags.ExecuteOnServer, "0.05", "Weight at which basic mobs drop equipment.");
            createConVar<FloatConVar>("f_elite_loot_rate", ConVarFlags.ExecuteOnServer, "0", "Percecent at which elite mobs drop loot.");
            createConVar<FloatConVar>("f_elite_tier2_weight", ConVarFlags.ExecuteOnServer, "0.90", "Weight at which elite mobs drop tier2.");
            createConVar<FloatConVar>("f_elite_tier3_weight", ConVarFlags.ExecuteOnServer, "0.02", "Weight at which elite mobs drop tier3.");
            createConVar<FloatConVar>("f_elite_lunar_weight", ConVarFlags.ExecuteOnServer, "0.010", "Weight at which elite mobs drop lunar items.");
            createConVar<FloatConVar>("f_elite_equipment_weight", ConVarFlags.ExecuteOnServer, "0.05", "Weight at which elite mobs drop equipment.");
            createConVar<FloatConVar>("f_boss_loot_rate", ConVarFlags.ExecuteOnServer, "0", "Percecent at which boss mobs drop loot.");
            createConVar<FloatConVar>("f_boss_tier2_weight", ConVarFlags.ExecuteOnServer, "0.90", "Weight at which boss mobs drop tier2.");
            createConVar<FloatConVar>("f_boss_tier3_weight", ConVarFlags.ExecuteOnServer, "0.02", "Weight at which boss mobs drop tier3.");
            createConVar<FloatConVar>("f_boss_lunar_weight", ConVarFlags.ExecuteOnServer, "0.20", "Weight at which boss mobs drop lunar items.");
            createConVar<FloatConVar>("f_boss_equipment_weight", ConVarFlags.ExecuteOnServer, "0.05", "Weight at which boss mobs drop equipment.");
            createConVar<FloatConVar>("f_elite_boss_loot_rate", ConVarFlags.ExecuteOnServer, "0", "Percecent at which boss mobs drop loot.");
            createConVar<FloatConVar>("f_elite_boss_tier2_weight", ConVarFlags.ExecuteOnServer, "0.40", "Weight at which elite boss mobs drop tier2.");
            createConVar<FloatConVar>("f_elite_boss_tier3_weight", ConVarFlags.ExecuteOnServer, "0.20", "Weight at which elite boss mobs drop tier3.");
            createConVar<FloatConVar>("f_elite_boss_lunar_weight", ConVarFlags.ExecuteOnServer, "0.20", "Weight at which elite boss mobs drop lunar items.");
            createConVar<FloatConVar>("f_elite_boss_equipment_weight", ConVarFlags.ExecuteOnServer, "0.05", "Weight at which elite boss mobs drop equipment.");
        }

        static void PopulateLoot_PreFix(SceneDirector __instance) {
            if ((conVars["b_use_custom_loot"] as BoolConVar).value) {
                var interactableCredit = typeof(SceneDirector).GetField("interactableCredit", PrivateInstanced);
                int playerCount = Run.instance.participatingPlayerCount;
                int baseLevel = (int)((double)((int)interactableCredit.GetValue(__instance)) / (0.5 + playerCount * 0.5));
                float playerScale = (conVars["f_additional_player_multiplier"] as FloatConVar).value;
                baseLevel += (conVars["i_add_amount_2_base"] as IntConVar).value;

                interactableCredit.SetValue(__instance, (int)(playerCount * playerScale * baseLevel));
            }
        }

        static void HideLoot_PreFix(ref bool newHidden) {
            newHidden = (double)Run.instance.treasureRng.nextNormalizedFloat < (conVars["f_loot_hiding_rate"] as FloatConVar).value;
        }


        static void DeathLoot_PostFix(DeathRewards __instance) {
            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
            Vector3 direction = __instance.transform.position + Vector3.up * 1.5f;
            Vector3 velocity = Vector3.up * 20f + __instance.transform.forward * 2f;
            PickupIndex itemToDrop = PickupIndex.none;
            CharacterBody actor = (CharacterBody)typeof(DeathRewards).GetField("characterBody", PrivateInstanced).GetValue(__instance);

            if (actor.isPlayerControlled || actor.isLocalPlayer)
                return;

            bool isBasicMob = !actor.isElite && !actor.isBoss;
            bool isEliteMob = actor.isElite && !actor.isBoss;
            bool isBasicBoss = !actor.isElite && actor.isBoss;
            bool isEliteBoss = actor.isElite && actor.isBoss;

            if (isBasicMob && (conVars["f_basic_loot_rate"] as FloatConVar).value < Run.instance.treasureRng.nextNormalizedFloat) {
                weightedSelection.AddChoice(Run.instance.availableTier1DropList, (conVars["f_basic_tier1_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, (conVars["f_basic_tier2_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, (conVars["f_basic_tier3_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableLunarDropList, (conVars["f_basic_lunar_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, (conVars["f_basic_equipment_weight"] as FloatConVar).value);
            } else if (isEliteMob && (conVars["f_elite_loot_rate"] as FloatConVar).value < Run.instance.treasureRng.nextNormalizedFloat) {
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, (conVars["f_elite_tier2_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, (conVars["f_elite_tier3_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableLunarDropList, (conVars["f_elite_lunar_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, (conVars["f_elite_equipment_weight"] as FloatConVar).value);
            } else if (isBasicBoss && (conVars["f_boss_loot_rate"] as FloatConVar).value < Run.instance.treasureRng.nextNormalizedFloat) {
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, (conVars["f_boss_tier2_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, (conVars["f_boss_tier3_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableLunarDropList, (conVars["f_boss_lunar_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, (conVars["f_boss_equipment_weight"] as FloatConVar).value);
            } else if (isEliteBoss && (conVars["f_elite_boss_loot_rate"] as FloatConVar).value < Run.instance.treasureRng.nextNormalizedFloat) {
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, (conVars["f_elite_boss_tier2_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, (conVars["f_elite_boss_tier3_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableLunarDropList, (conVars["f_elite_boss_lunar_weight"] as FloatConVar).value);
                weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, (conVars["f_elite_boss_equipment_weight"] as FloatConVar).value);
            }

            var dropList = weightedSelection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
            if (dropList == null || dropList.Count <= 0)
                return;

            itemToDrop = dropList[Run.instance.treasureRng.RangeInt(0, dropList.Count)];

            if (itemToDrop != PickupIndex.none) {
                PickupDropletController.CreatePickupDroplet(itemToDrop, direction, velocity);
                if (Run.instance.treasureRng.nextNormalizedFloat < (conVars["f_double_loot_rate"] as FloatConVar).value) {
                    PickupDropletController.CreatePickupDroplet(itemToDrop, direction, velocity);
                }
            }
        }

    }
}
