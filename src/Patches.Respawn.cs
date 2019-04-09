using RoR2;
using RoR2.ConVar;
using System.Timers;
using UnityEngine.Networking;

namespace DXsRoR2Mods {
    partial class Patches {

        static void Respawn_Init() {
            createConVar<IntConVar>("i_resapwn_delay", ConVarFlags.ExecuteOnServer, "0", "Enables respawning if at least 1 play is alive, 0 is disabled.");
        }

        static void Respawn_PostFix(CharacterMaster __instance) {
            int respawnDelay = (conVars["i_resapwn_delay"] as IntConVar).value;
            if (respawnDelay > 0) {
                var timer = new Timer(respawnDelay * 1000);
                timer.AutoReset = false;
                timer.Elapsed += (s, e) => {
                    if (NetworkServer.active && __instance.preventGameOver == false) {
                        __instance.Invoke("RespawnExtraLife", 2f);
                        __instance.Invoke("PlayExtraLifeSFX", 1f);
                        __instance.preventGameOver = true;
                        typeof(CharacterMaster).GetField("preventRespawnUntilNextStageServer", PrivateInstanced).SetValue(__instance, false);
                        typeof(CharacterMaster).GetMethod("ResetLifeStopwatch", PrivateInstanced).Invoke(__instance, null);
                    }
                };
                timer.Start();
            }
        }

    }
}
