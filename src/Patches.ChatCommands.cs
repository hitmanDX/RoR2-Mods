using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace DXsRoR2Mods {
    partial class Patches {

        public static Dictionary<string, Command> commands = new Dictionary<string, Command>() {
            { "/load", new Command() { names = "load", desc = "", action = load_command } },
            { "/restart", new Command() { names = "restart", desc = "", action = restart_command } },
            { "/dice", new Command() { names = "dice", desc = "", action = dice_command } },
        };

        public class Command {
            public string names;
            public string desc;
            public Action<String[]> action;
        }

        static bool ChatCommands_PreFix(ConCommandArgs args) {
            string[] cmdArgs = args[0].Split(' ');
            string cmd = cmdArgs[0];
            if (commands.ContainsKey(cmd)) {
                commands[cmd].action(cmdArgs);
                return false;
            }
            return true;
        }


        private static void load_command(String[] args) {
            //TODO use Console.LoadConfig instead...
            console.SubmitCmd(null, "exec " + args[1], false);
        }

        private static void restart_command(String[] args) {
            if (NetworkServer.active) {
                typeof(Run).GetMethod("CCRunEnd", PrivateStatic).Invoke(null, new object[] { null });
            }
        }

        private static void dice_command(String[] args) {
            Log("dice command called.");
        }

    }
}
