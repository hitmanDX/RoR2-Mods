using Harmony;
using RoR2;
using RoR2.ConVar;
using RoR2.Mods;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DXsRoR2Mods {
    partial class Patches {

        const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
        const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        const BindingFlags PrivateInstanced = BindingFlags.NonPublic | BindingFlags.Instance;
        const BindingFlags PublicInstanced = BindingFlags.Public | BindingFlags.Instance;

        public static Run run;

        [ModEntry("DX's RoR Mods", "1.0.0", "DX")]
        public static void Init() {
            CreateHook(typeof(Run), "Start", PrivateInstanced, "RunStart");
            CreateHook(typeof(Run), "Start", PrivateInstanced, "RunDestroy");

            CreateHook(typeof(Chat), "CCSay", PrivateStatic, "ChatCommands");
            
            CreateHook(typeof(SceneDirector), "PopulateScene", PrivateInstanced, "PopulateLoot");
            CreateHook(typeof(ShopTerminalBehavior), "SetPickupIndex", PublicInstanced, "HideLoot");
            CreateHook(typeof(DeathRewards), "OnKilled", PrivateInstanced, "DeathLoot");

            CreateHook(typeof(CharacterMaster), "OnBodyDeath", PublicInstanced, "Respawn");



            //Run this last...
            CreateHook(typeof(RoR2.Console), "InitConVars", PrivateInstanced, "ConVars");
        }
        

        public static void CreateHook(Type targetClass, string targetMethod, BindingFlags bindingAttr, string patchName) {
            var harmony = HarmonyInstance.Create(String.Format("dev.dx.{0}-{1}", targetClass.Name, targetMethod));

            var method = targetClass.GetMethod(targetMethod, bindingAttr);

            try {
                typeof(Patches).GetMethod(patchName + "_Init", PrivateStatic).Invoke(null, null);
            } catch (Exception ex) {
                Debug.Log("CreateHook Error:  " + ex.Message + " - " + ex.StackTrace);
            }

            HarmonyMethod prefix = getPatchMethod(patchName + "_PreFix");
            HarmonyMethod postfix = getPatchMethod(patchName + "_PostFix");
            HarmonyMethod transpiler = getPatchMethod(patchName + "_Transpile");
            
            harmony.Patch(method, prefix, postfix, transpiler);
        }

        private static HarmonyMethod getPatchMethod(string patchMethod) {
            try {
                return new HarmonyMethod(typeof(Patches).GetMethod(patchMethod, PrivateStatic));
            } catch {
                return null;
            }
        }


        public static void Log(string msg) {
            Chat.AddMessage(msg);
        }

    }
}
