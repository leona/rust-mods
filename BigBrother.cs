using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using Network;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;


// TODO
namespace Oxide.Plugins
{
    [Info("BigBrother", "Leon", "1.0.0")]
    [Description("BigBrother twitch integration")]
    class BigBrother : CovalencePlugin
    {
        #region Definitions

        // from https://wiki.facepunch.com/rust/Other_Cinematic_Commands
        // https://quietusplus.github.io/Rust-Easy_Config/docs/Rust-Variables.html
/*
        new Dictionary<string, Action<string, string>> ConsoleCommands = new Dictionary<string, Action<string, string>> {
            // Z/C - changes zoom level // DO WHILE MOVING FORWARD OR BACK
            { "debugcamera", (a, b) => "debugcamera"  },
            { "client.camoffset, "\"(0, 0, 0)\"" },
            { "client.camoffset_relative", "1" },
            { "spectate", (a, b) => $"spectate \"{a, b}\"" },
            { "respawn", (a, b) => "respawn" },
            { "spawn", (a, b) => $"spawn \"{a}\"" },
            { "camlerp", (a, b) => $"camlerp {a}" },
            { "camlerptilt", (a, b) => $"camlerptilt {a == true ? "0" : "1"}" },
            { "camzoomlerp", (a, b) => $"camzoomlerp {a}" },
            { "debugcamera_unfreeze", (a, b) => "debugcamera_unfreeze" },
            { "debugcamera_save", (a, b) => $"debugcamera_save {a}"},
            { "camparent", (a, b) => "bind <key> +debugcamera_targetbind" },
            { "dof", (a, b) => $"dof {a == true ? "0" : "1"}" },
            { "dof_mode", (a, b) => $"dof_mode {a}" }, // 0 = auto, 1 = manual, 2 = target.
            { "dof_aper", (a, b) => $"dof_aper {a}" },
            { "dof_blur", (a, b) => $"dof_blur {a}" },
            { "dof_focus_lookingat", (a, b) => "dof_focus_lookingat" }, // target focus
            { "dof_focus_target", (a, b) => $"dof_focus_target {a}" }, // Sets the focus target to a specified entity ID
            { "dof_focus_dist", (a, b) => $"dof_focus_dist {a}" }, // set focus manually
            { "dof_focus_time", (a, b) => $"dof_focus_time {a}" }, // time it takes, smootherning focus
            { "dof_nudge", (a, b) => $"dof_nudge {a}" }, //  Incrementally modify the focus distance by a specified amount. Both positive + and negative - values are accepted
            { "playerseed", (seed, playerId) => $"playerseed {seed} {playerId}"}, // change player appearance
            { "playerseed_shuffle", (playerId, b) => $"playerseed {playerId}"}, // shuffle player appearance
            { "admintime", (a, b) => $"admintime {a}" }, // 24 hour value for client time
            { "adminclouds", (a, b) => $"adminclouds {a == true ? "1" : "0"}" },
            { "adminfog", (a, b) => $"adminfog {a == true ? "1" : "0"}" },
            { "adminwind", (a, b) => $"adminwind {a == true ? "1" : "0"}" },
            { "adminrain", (a, b) => $"adminrain {a == true ? "1" : "0"}" },
            { "env.cloudmovement", (a, b) => $"env.cloudmovement {a == true ? "1" : "0"}" },
            { "env.cloudrotation", (a, b) => $"env.cloudrotation {a}" }, // 0 - 360 rotation value
            { "lodbias", (a, b) => $"lodbias {a}" }, // Increases level of details especially at distance
            { "specnet", (a, b) => $"specnet {a == true ? "true" : "false"}" },
            { "fps.limit", (a, b) => $"fps.limit {a}" },
            { "censornudity", (a, b) => $"censornudity {a == true ? "true" : "false"}" },
            { "tree.quality", (a, b) => $"tree.quality {a}" }, // 0 - 1000 render distance
            { "grass.distance", (a, b) => $"grass.distance {a}" } // 100 - 200 render distance
        };
*/

        [PluginReference]
        private Plugin Server;

        #endregion Definitions
        
        private void sendCommand(IPlayer player, string command) {
            Puts($"Running command: {command}");
            string[] commands = command.Split(' ');

            SendRawCommand((player.Object as BasePlayer).Connection, commands, false); // Breaks if things between "" have spaces
        }

        private void shakyLowAngleView() {
            // shake camera, shift focus, view from low angle following player
        }
        private void cycleCloseTeam() {
            // cycles through a team if they're in close proximity to eachother.
        }

        private void freeMode() {
            // debugcamera 
        }

        private void receivedRconVoteCommand() {
            // When an rcon message is received for a vote option, do an action.
        }

        private void messagePlayerPhrase() {
            // Message a player a phrase from a set of choices like "Get good" that external sources(twitch, discord) can send the player.
        }

        // Toggle between first/third person mode
        private void toggleViewMode(BasePlayer player) {
            var isThirdPerson = player.HasPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode);

            if (isThirdPerson == true) {
                // Apply first person settings
                sendCommand(player.IPlayer, "dof 0");
            } else {
                // Apply third person settings
                sendCommand(player.IPlayer, "dof 1");
            }

            player.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, !isThirdPerson);
        }

        private void spawn(IPlayer player, string command, string[] args) {
           // if (permission.UserHasPermission(player.userID.ToString(), "AdminPlugin.spawn"))
            //{
                var position = player.Position();
                var basePlayer = player.Object as BasePlayer;
                float lookRotation = basePlayer.eyes?.rotation.eulerAngles.y ?? 0;
                int playerDirection = (Convert.ToInt16((lookRotation - 5) / 10 + 0.5) * 10);

                ConsoleSystem.Run(ConsoleSystem.Option.Server.Quiet(), $"spawn sedan {basePlayer.transform.position.x + 5f},{basePlayer.transform.position.y + 5f},{basePlayer.transform.position.z}");
                player.Reply("Sedan Spawned");
           // }
            //else
            //{
             //   PrintToChat(player, "You Do Not Have The Permissions To Run This Command");
            //}
        }

        public IPlayer getRandomPlayerAlive() {
            for (int i = 0; i < 10; i++) {
                var random = players.Connected.ToArray().GetRandom();
            
                if (!random.IsSleeping) {
                    return random;
                }
            }
            return null;
        }

        [Command("spec"), Permission("bigbrother.admin")]
        private void spectatePlayerCommand(IPlayer player, string command, string[] args) {
            var randomPlayer = getRandomPlayerAlive();
            spectatePlayer2(player, randomPlayer);
        }

        void spectatePlayer(IPlayer player, IPlayer target) {
            var basePlayer = player.Object as BasePlayer;
            var targetBasePlayer = target.Object as BasePlayer;
            // Prep player for spectate mode
            var heldEntity = basePlayer.GetActiveItem()?.GetHeldEntity() as HeldEntity;
            heldEntity?.SetHeld(false);

            // Put player in spectate mode
            basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Spectating, true);
            basePlayer.gameObject.SetLayerRecursive(10);
            basePlayer.CancelInvoke("MetabolismUpdate");
            basePlayer.CancelInvoke("InventoryUpdate");
            basePlayer.ClearEntityQueue();
            basePlayer.SendEntitySnapshot(targetBasePlayer);
            basePlayer.gameObject.Identity();
            basePlayer.SetParent(targetBasePlayer);
            basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, true);
            SendRawCommand(basePlayer.Connection, new string[] { "client.camoffset", "\"(0.0, 3, 0.0)\"" }, false);

            // Notify player and store target name
            player.Reply("Spectating");
        }

        [Command("stopspectate"), Permission("bigbrother.admin")]
        void stopSpectating(IPlayer player) {
            var basePlayer = player.Object as BasePlayer;

            // Restore player to normal mode
            player.Command("camoffset", "0,1,0");
            basePlayer.SetParent(null);
            basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Spectating, false);
            basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, false);
            basePlayer.gameObject.SetLayerRecursive(17);
            basePlayer.metabolism.Reset();
            basePlayer.InvokeRepeating("InventoryUpdate", 1f, 0.1f * UnityEngine.Random.Range(0.99f, 1.01f));

            // Restore player to previous state
            basePlayer.StartSleeping();
            HeldEntity heldEntity = basePlayer.GetActiveItem()?.GetHeldEntity() as HeldEntity;
            heldEntity?.SetHeld(true);
        }

        void spectatePlayer2(IPlayer player, IPlayer target) {
            var basePlayer = (player.Object as BasePlayer);
            var conn = basePlayer.Connection;
            Puts($"Spectating: {target.Id}");
            SendRawCommand(conn, new string[] { "spectate", target.Id }, false);
            SendRawCommand(conn, new string[] { "client.camoffset_relative", "1" }, false);
            SendRawCommand(conn, new string[] { "client.camoffset", "\"(0.0, 1.3, -5.0)\"" }, false);
           // basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, true);
        }

        private void setupClient(IPlayer player, string command, string[] args) {
            Puts("Setting up client");
            //sendCommand(player, "spawn sedan");
            var basePlayer = (player.Object as BasePlayer);

            SendRawCommand(basePlayer.Connection, new string[] { "spectate" }, false);
            SendRawCommand(basePlayer.Connection, new string[] { "client.camoffset", "\"(0.0, 0.0, -5.0)\"" }, false);
            basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, true);

            return;
            sendCommand(player, "specnet true");
            sendCommand(player, "fps.limit 30");
            sendCommand(player, "censornudity true");
            sendCommand(player, "specnet true");
            sendCommand(player, "lodbias 5");
            sendCommand(player, "tree.quality 1000");
            sendCommand(player, "grass.distance 200");
            sendCommand(player, "dof 1");
            sendCommand(player, "dof_mode 0");
            sendCommand(player, "dof_aper 3");
            sendCommand(player, "dof_blur 5");
        }

/*
        void receiveEvent(string name) {
            if (name == "set_night") {
                Server.setTimeOfDay(21);
            }

            if (name == "spawn_bear") {
                setView("far_spectator");
                spawn("bear");
            }
        }*/

        private void testCommand(IPlayer player, string command, string[] args) {
            var basePlayer = player.Object as BasePlayer;
            
            Server.Call("setTimeOfDay", 0);
           // Interface.SendCommand(basePlayer.Connection, commands, false);
           
        }

        public static void SendRawCommand(Connection conn, string[] args, bool isChat)
        {
            if (!Net.sv.IsConnected())
                return;

            var command = string.Empty;
            var argsLength = args.Length;
            for (var i = 0; i < argsLength; i++)
                command += $"{args[i]} ";
            
            if (isChat)
                command = $"chat.say {command.QuoteSafe()}";
            
            Net.sv.write.Start();
            Net.sv.write.PacketID(Message.Type.ConsoleCommand);
            Net.sv.write.String(command);
            Net.sv.write.Send(new SendInfo(conn));
        }

    }
}
