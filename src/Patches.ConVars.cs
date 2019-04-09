using RoR2;
using RoR2.ConVar;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DXsRoR2Mods {
    partial class Patches {
        private static RoR2.Console console;

        public static Dictionary<string, BaseConVar> conVars = new Dictionary<string, BaseConVar>();

        static void createConVar<T>(string name, ConVarFlags flags, string defaultValue, string help) where T : BaseConVar {
            T t = (T)typeof(T).GetConstructor(new Type[] { typeof(string), typeof(ConVarFlags), typeof(string), typeof(string) }).Invoke(new object[] { name, flags, defaultValue, help });
            conVars.Add(name, t);
        }

        static void ConVars_Init() {
            createConVar<BoolConVar>("b_stage1_pod", ConVarFlags.ExecuteOnServer, "1", "Whether or not to use the pod when spawning on the first stage.");
        }

        static void ConVars_PostFix(RoR2.Console __instance) {
            console = __instance;
            try {
                var RegisterConVarInternal = typeof(RoR2.Console).GetMethod("RegisterConVarInternal", PrivateInstanced);
                foreach (var convar in conVars.Values) {
                    RegisterConVarInternal.Invoke(__instance, new object[] {
                        convar
                    });
                }
            } catch (Exception ex) {
                Debug.Log("ConVars_PostFix Error:  " + ex.Message + " - " + ex.StackTrace);
            }

            if ((conVars["b_stage1_pod"] as BoolConVar).value) {

            }
        }
    }
}
