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



namespace Oxide.Plugins
{
    [Info("Server", "Leon", "1.0.0")]
    [Description("General server commands")]
    class Server : CovalencePlugin
    {
        #region Definitions

        [PluginReference]
        private Plugin PlayerDatabase, AutoTeams, RateLimit;

        string[] commands = {
            "Hold E and press R or Type /. in chat for the server menu"
        };

        #endregion Definitions

        void OnPlayerInput(BasePlayer player, InputState state)
        {
            if(state.IsDown(BUTTON.USE) && state.WasJustPressed(BUTTON.RELOAD)) {
                SendCommand(player.Connection, new string[] { "/." }, true);
            }
        }
        // Help command
        [Command("help", "h", "commands", "kit", "kits", "info")]
        private void helpCommand(IPlayer player, string command, string[] args) {
            var commandsString = String.Join("\n", commands.ToArray());
            player.Reply($"Help Menu\n\nCommands\n{commandsString}");
        }


        [Command("stop")]
        void stopSpectating(IPlayer player, string command, string[] args) {
            // Restore player to normal mode
            var basePlayer = player.Object as BasePlayer;
            player.Command("camoffset", "0,1,0");
            basePlayer.SetParent(null);
            basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.Spectating, false);
            basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.ThirdPersonViewmode, false);
            basePlayer.gameObject.SetLayerRecursive(17);
            basePlayer.metabolism.Reset();
            basePlayer.InvokeRepeating("InventoryUpdate", 1f, 0.1f * UnityEngine.Random.Range(0.99f, 1.01f));

            // Restore player to previous state
            basePlayer.StartSleeping();
            var heldEntity = basePlayer.GetActiveItem()?.GetHeldEntity() as HeldEntity;
            heldEntity?.SetHeld(true);
        }

        [Command("on")]
        void cmdOn(BasePlayer player, string cmd, string[] args)
        {
            Puts("Creating foundation ");
            BuildingBlock block = (BuildingBlock)GameManager.server.CreateEntity("assets/prefabs/building core/foundation/foundation.prefab");
            if (block == null) return;            block.transform.position = player.transform.position;

            Puts("Can create");
            block.transform.rotation = player.transform.rotation;
            block.gameObject.SetActive(true);
            block.blockDefinition = PrefabAttribute.server.Find<Construction>(block.prefabID);
            block.Spawn();
            block.SetGrade(BuildingGrade.Enum.Stone);
            block.SetHealthToMax();
            block.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
        }
        
        [Command("off")]
        void cmdOff(BasePlayer player, string cmd, string[] args)
        {
          //  player.SetPlayerFlag(BasePlayer.PlayerFlags.HasBuildingPrivilege, false);
       //   player.SetPlayerFlag(BasePlayer.PlayerFlags.InBuildingPrivilege, true);

        }

        /*
        BGRADE TODO
        private static void SetGrade(BuildingBlock block, BuildingGrade.Enum level)
        {
            block.SetGrade(level);
            block.health = block.MaxHealth();
            block.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
        }
        */
        
        void spawnVehicle(string name, IPlayer player) {
            var cooldown = (long) RateLimit.Call("inTime", $"spawn-{name}", player.Id, 60 * 2);

            if (cooldown > 0) {
                player.Reply($"You must wait {cooldown} seconds before doing that again.");
                return;
            }

            spawn(player, name);
            giveItem(player, "lowgradefuel", 500);
            player.Reply($"Spawned {name}");
        }

        [Command("mini")]
        private void miniCommand(IPlayer player, string command, string[] args) {
            spawnVehicle("minicopter", player);
        }

        [Command("car")]
        private void carCommand(IPlayer player, string command, string[] args) {
            spawnVehicle("sedan", player);
        }

        [Command("boat")]
        private void boatCommand(IPlayer player, string command, string[] args) {
            spawnVehicle("rhib", player);
        }

        [Command("drone")]
        private void droneCommand(IPlayer player, string command, string[] args) {
            spawn(player, "drone.deployed");
        }

        [Command("day"), Permission("server.admin")]
        private void dayCommand(IPlayer player, string command, string[] args) {
            setTimeOfDay(8);
        }

        public void giveItem(IPlayer player, string name, int amount = 1) {
            var basePlayer = player.Object as BasePlayer;

            var item = ItemManager.CreateByName(name, amount);
            if (item == null) {return;}
            basePlayer.inventory.GiveItem(item);
        }

        public void SendCommand(Connection conn, string[] args, bool isChat)
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

        private void spawn(IPlayer player, string spawnable) {
            var position = player.Position();
            var basePlayer = player.Object as BasePlayer;
            float lookRotation = basePlayer.eyes?.rotation.eulerAngles.y ?? 0;
            int playerDirection = (Convert.ToInt16((lookRotation - 5) / 10 + 0.5) * 10);

            ConsoleSystem.Run(ConsoleSystem.Option.Server.Quiet(), $"spawn {spawnable} {basePlayer.transform.position.x + 5f},{basePlayer.transform.position.y + 5f},{basePlayer.transform.position.z}");
        }

        // Welcome new users
        private void OnUserConnected(IPlayer player) {
            var hasReceivedWelcome = PlayerDatabase.Call("GetPlayerData", player.Id, "welcome");
            
            if (hasReceivedWelcome == null) {
                Puts("New player joined");
                server.Broadcast($"{player.Name} has joined for the first time");
                PlayerDatabase.Call("SetPlayerData", player.Id, "welcome", true);
                SendCommand((player.Object as BasePlayer).Connection, new string[] { "/." }, true);
            }

            player.Reply("Welcome. Hold USE (E) and press RELOAD (R) or type /. in the chat to open the server menu. Be sure to join the discord at www.e2.ie/discord");
        }

        // Disable NPCs attacking
        bool CanNpcAttack(BaseNpc npc, BaseEntity target) {
            return false;
        }

        private void OnServerInitialized() {
            disableAllNativeEvents();
        }
        
        void OnPluginLoaded(Plugin plugin) {
            disableAllNativeEvents();
            
            // Setup permissions
            server.Command("oxide.grant group default discordauth.auth");
            server.Command("oxide.grant group admin bigbrother.admin");
            server.Command("oxide.grant group admin disableloot.admin");
            server.Command("oxide.grant group admin markers.admin");
            server.Command("oxide.grant group default randomrespawner.use");
            server.Command("oxide.grant group admin server.admin");
            server.Command("oxide.grant group admin randomrespawner.use");
            server.Command("oxide.grant group admin autokits.admin");
            server.Command("oxide.grant group admin vanish.allow");
            server.Command("oxide.grant group admin duel.admin");

            server.Command("oxide.grant group admin copypaste.copy");
            server.Command("oxide.grant group admin copypaste.list");
            server.Command("oxide.grant group admin copypaste.paste");
            server.Command("oxide.grant group admin copypaste.pasteback");
            server.Command("oxide.grant group admin copypaste.undo");

            // Disable NPCs
            server.Command("ai.move false");
            server.Command("ai.think false");

            // Set owners
            server.Command("ownerid 76561198027571209");
            server.Command("ownerid 76561199113618466");

            // Disable decay
            server.Command("decay.scale 0");
            server.Command("decay.upkeep False");
        }

        [Command("steamid")]
        void getSteamId(IPlayer player, string command, string[] args) {
            Puts($"Steam id: {player.Id}");
            player.Reply(player.Id);
        }

        public static void setTimeOfDay(int hour) {
            ConVar.Env.time = hour;
        }

        // Only allow building walls
        string[] validBuildables = { "wall.external.high.wood", "wall.external.high.stone",  "drone.deployed" };

        object CanBuild(Planner plan, Construction prefab, Construction.Target target)
        {
            BasePlayer player = plan.GetOwnerPlayer();
            if (!player) return null;
            
            if (permission.UserHasPermission(player.userID.ToString(), "server.admin")) {
                return null;
            }

            if (validBuildables.Any(prefab.fullName.Contains)) {
                return null;
            }
   
            Puts($"Attempting to build: {prefab.fullName}");
            player.IPlayer.Reply("You cannot build on this server");
            return false;
        }

        // Disable mark hostile
        object OnEntityMarkHostile(BaseCombatEntity entity, float duration) {
            return true;
        }

        void OnPlayerDisconnected(BasePlayer player, string reason) {            
            server.Broadcast($"{player.IPlayer.Name} has left the server");
        }

        // Full vitals on spawn
        void OnPlayerRespawned(BasePlayer player) {
            player.health = 100f;
            player.metabolism.hydration.value = 250;
            player.metabolism.calories.value = 500;
            player.SendNetworkUpdate();
        }

        // Disable friendly fire
        object OnPlayerAttack(BasePlayer player, HitInfo info) {
            if (info?.HitEntity == null || player == null)
                return null;
            IPlayer attacker = player.IPlayer;
            if (!(info.HitEntity is BasePlayer))
                return null;
            BasePlayer victimBP = info.HitEntity as BasePlayer;
            IPlayer victim = victimBP.IPlayer;
            if (attacker == null || victim == null || attacker.Id == victim.Id)
                return null;


            var isTeamMate = (bool) AutoTeams.Call("isTeamMate", attacker.Id, victim.Id);

            if (isTeamMate == true) {
                return true;
            }
        
            return null;
        }

        // Disable heli, chinook, bradley and crates
        void disableAllNativeEvents() {
            Puts("Disabling helis");
            
            var eventPrefabs = UnityEngine.Object.FindObjectsOfType<TriggeredEventPrefab>();
            for (int i = 0; i < eventPrefabs.Length; i++) {
                var eve = eventPrefabs[i];
                if ((eve?.targetPrefab?.resourcePath?.Contains("heli") ?? false))
                {
                    UnityEngine.Object.Destroy(eve);
                    Puts("Disabled default Helicopter spawning.");
                    break;
                }
            }

            for (int i = 0; i < eventPrefabs.Length; i++) {
                var eve = eventPrefabs[i];
                if ((eve?.targetPrefab?.resourcePath?.Contains("ch47") ?? false))
                {
                    UnityEngine.Object.Destroy(eve);
                    Puts("Disabled default Chinook spawning.");
                    break;
                }
            }

            var safeZones = UnityEngine.Object.FindObjectsOfType<TriggerSafeZone>();

            for (int i = 0; i < safeZones.Length; i++) {
                var eve = safeZones[i];
                UnityEngine.Object.Destroy(eve);
            }
        }
    }
}
