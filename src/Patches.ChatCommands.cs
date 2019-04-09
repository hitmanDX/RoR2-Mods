using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace DXsRoR2Mods {
    partial class Patches {

        public static Dictionary<string, Command> commands = new Dictionary<string, Command>() {
            { "/load", new Command() { name = "load", desc = "Load a config file for quick custom game settings.", action = load_command } },
            { "/restart", new Command() { name = "restart", desc = "Takes you back to character select screen.", action = restart_command } },
            { "/roll", new Command() { name = "roll", desc = "Rolls a number between 0 - 100.", action = dice_command } },
            { "/dice", new Command() { name = "dice", desc = "See /roll", action = dice_command } },
        };

        public class Command {
            public string name;
            public string desc;
            public Action<NetworkUser, String[]> action;
        }

        static bool ChatCommands_PreFix(ConCommandArgs args) {
            string[] cmdArgs = args[0].Split(' ');
            string cmd = cmdArgs[0];
            if (commands.ContainsKey(cmd)) {
                commands[cmd].action(args.sender, cmdArgs);
                return false;
            }
            return true;
        }


        private static void load_command(NetworkUser sender, String[] args) {
            var LoadConfig = typeof(RoR2.Console).GetMethod("LoadConfig", PrivateStatic);
            string config = (string) LoadConfig.Invoke(null, new object[] { args[1] });
            if (config == null) {
                Log("Failed to load config!");
                return;
            }
            
            RoR2.Console.instance.SubmitCmd(sender, config, false);
            Log(String.Format("Loaded config: {0}", args[1]));
        }

        private static void restart_command(NetworkUser sender, String[] args) {
            if (NetworkServer.active) {
                typeof(Run).GetMethod("CCRunEnd", PrivateStatic).Invoke(null, new object[] { null });
            }
        }

        private static void dice_command(NetworkUser sender, String[] args) {
            try {
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage() {
                    baseToken = "DX_DICE",
                    subjectNetworkUser = sender,
                    paramTokens = new string[] {
                        Run.instance.runRNG.RangeInt(0, 100).ToString()
                    },
                });
            } catch (Exception ex) {
                Log(ex.Message + " - " + ex.StackTrace);
            }
        }
        

    }
}
