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
    [Info("Duel", "Leon", "1.0.0")]
    [Description("Duel")]
    class Duel : CovalencePlugin
    {

        [PluginReference]
        private Plugin CopyPaste, AutoKits;
        
        float duelExpireTime = 20f;

        public static List<Vector3> spawns = new List<Vector3>() {
            new Vector3(7.519409f, 302.050f, 10.4855f),
            new Vector3(-4.163967f, 302.20f, -11.87619f),
            new Vector3(16.52677f, 306.5526f, 4.26803f)
        };

        public class Config {
            public bool duelInProgress = false;
        }

        public static Config config = new Config();
        
        class DuelContract {
            public List<IPlayer> players = new List<IPlayer>();
            public IPlayer initiator;
            public IPlayer opponent;
            public IPlayer winner;
            public bool accepted = false;
            public Vector3 originalInitiatorLocation;
            public Vector3 originalOpponentLocation;
        }
        
        class Duels {
            public List<DuelContract> queue = new List<DuelContract>();
            public List<ulong> spawnProtected = new List<ulong>();
            public DuelContract current;

            public void Add(DuelContract item) {
                queue.Add(item);
            }

            public DuelContract Find(IPlayer player) {
                foreach (var item in queue) {
                    List<IPlayer> result = item.players.ToArray().Where((a) => a.Id == player.Id).ToList(); 

                    if (result.Count > 0) {
                        return item;
                    }
                }

                return null;
            }

            public void RemoveQueueItem(DuelContract contract) {
                var item = queue.SingleOrDefault(x => x.initiator.Id == contract.initiator.Id);

                if (item != null)
                    queue.Remove(item);
            }
        }

        Duels duels = new Duels();

        [Command("setuparena"), Permission("duel.admin")]
        private void setupArenaCommand(IPlayer player, string command, string[] args) {
            setupDuelArena();
        }

        [Command("spectate")]
        private void spectateCommand(IPlayer player, string command, string[] args) {
            var basePlayer = player.Object as BasePlayer;
            teleportProtect(basePlayer);
            AutoKits.Call("clearAndBlock", player);

            InvokeHandler.Instance.Invoke(() => {
                basePlayer.Teleport(spawns[2]);
            }, 0.2f);
        }

        [Command("1v1")]
        private void duelCommand(IPlayer player, string command, string[] args) {
            if (args.Count() == 0) {
                player.Reply("No steam ID provided");
                return;
            }

            var id = Convert.ToUInt64(args[0]);

            if (id == (player.Object as BasePlayer).userID) {
                player.Reply("Cannot challenge yourself");
                return;
            }
            
            var opponent = BasePlayer.FindByID(id);

            if (opponent == null || !opponent.IsConnected) {
                player.Reply("Opponent does not exist or is not online.");
                return;
            }

            var existingContract = duels.Find(player);

            if (existingContract != null) {
                player.Reply("You already have an open 1v1 request");
                return;
            }

            duels.Add(new DuelContract() {
                players = new List<IPlayer>() { player, opponent.IPlayer },
                initiator = player,
                opponent = opponent.IPlayer,
            });

            player.Reply($"You have requested a 1v1 with {opponent.IPlayer.Name}");
            opponent.IPlayer.Reply($"You have been challenged to a 1v1 by: {player.Name} - Type /accept to begin in 5 seconds");
            
            InvokeHandler.Instance.Invoke(() => {
                var contract = duels.Find(player);

                if (contract.accepted == false) {
                    Puts("Removing expired contract");
                    player.Reply("Your 1v1 request has expired");
                    opponent.IPlayer.Reply("You did not respond to the 1v1 request in time");
                    duels.RemoveQueueItem(contract);
                } else {
                    Puts("Contract was accepted");
                }
            }, duelExpireTime);
        }

        [Command("accept")]
        private void acceptDuelCommand(IPlayer player, string command, string[] args) {
            var contract = duels.Find(player);
            
            if (contract == null) {
                player.Reply("1v1 request not found or expired.");
                return;
            }

            if (config.duelInProgress) {
                contract.initiator.Reply("1v1 already in progress");
                contract.opponent.Reply("1v1 already in progress");
                duels.RemoveQueueItem(contract);
                return;
            }

            contract.initiator.Reply("Starting 1v1 in 5 seconds");
            contract.opponent.Reply("Starting 1v1 in 5 seconds");

            var initBasePlayer = (contract.initiator.Object as BasePlayer);
            var opponentBasePlayer = (contract.opponent.Object as BasePlayer);
            
            contract.accepted = true;

            InvokeHandler.Instance.Invoke(() => {
                if (!opponentBasePlayer.IsAlive() || !initBasePlayer.IsAlive()) {
                    contract.initiator.Reply("Cannot start 1v1 as one of the players is not alive");
                    contract.opponent.Reply("Cannot start 1v1 as one of the players is not alive");
                    duels.RemoveQueueItem(contract);
                    return;
                }

                contract.initiator.Reply("Starting now!");
                contract.opponent.Reply("Starting now!");
                startDuel(contract);
            }, 5f);
        }

        object OnPlayerDeath(BasePlayer player, HitInfo info) {
            BasePlayer killer = info.Initiator?.ToPlayer();

            if (duels.current != null) {
                if (player.IPlayer.Id == duels.current.initiator.Id) {
                    endDuel(duels.current.opponent, duels.current.initiator);
                }

                if (player.IPlayer.Id == duels.current.opponent.Id) {
                    endDuel(duels.current.initiator, duels.current.opponent);
                }
            }

            return null;
        }

        private object OnEntityTakeDamage(BasePlayer player, HitInfo info)
        {
            if (duels.spawnProtected.Contains(player.userID)) {
                Puts($"Spawn protecting {player.userID}");
                return true;
            }

            return null;
        }

        private void endDuel(IPlayer winner, IPlayer loser) {
            duels.current.winner = winner;
            var duel = duels.current;
            duels.current = null;
            duels.RemoveQueueItem(duel);

            var basePlayerInitiator = duel.initiator.Object as BasePlayer;
            var basePlayerOpponent = duel.opponent.Object as BasePlayer;

            server.Broadcast($"{winner.Name} has won a 1v1 against {loser.Name}");

            InvokeHandler.Instance.Invoke(() => {
                basePlayerInitiator.Teleport(duel.originalInitiatorLocation);
                basePlayerOpponent.Teleport(duel.originalOpponentLocation);
            }, 2f);
        }

        private void startDuel(DuelContract contract) {
            duels.current = contract;
            duels.current.originalInitiatorLocation = getPlayerPositionVector(duels.current.initiator);
            duels.current.originalOpponentLocation =  getPlayerPositionVector(duels.current.opponent);

            var basePlayerInitiator = contract.initiator.Object as BasePlayer;
            var basePlayerOpponent = contract.opponent.Object as BasePlayer;

            teleportProtect(basePlayerInitiator);
            teleportProtect(basePlayerOpponent);

            InvokeHandler.Instance.Invoke(() => {
                basePlayerInitiator.Teleport(spawns[0]);
                basePlayerOpponent.Teleport(spawns[1]);
            }, 0.2f);

        }

        void teleportProtect(BasePlayer player) {
            duels.spawnProtected.Add(player.userID);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Sleeping, true);

            InvokeHandler.Instance.Invoke(() => {
                recurseCheckProtection(player);
            }, 4f);
        }

        void recurseCheckProtection(BasePlayer player, float interval = 1f, int iteration = 0, int maxIteration = 10) {
            if (player.IsOnGround() || iteration > 10) {
                Puts("Player is on ground. removing godmode");
                InvokeHandler.Instance.Invoke(() => {
                    duels.spawnProtected.Remove(player.userID);
                    player.SetPlayerFlag(BasePlayer.PlayerFlags.Sleeping, false);
                }, 2f);
            } else {
                Puts("Player not on ground. Checking again.");
                
                InvokeHandler.Instance.Invoke(() => {
                    recurseCheckProtection(player, interval, iteration + 1);
                }, interval);
            }
        }

        [Command("pos")]
        private void posCommand(IPlayer player, string command, string[] args) {
            var position = player.Position();

            player.Reply($"X: {position.X} Y: {position.Y} Z: {position.Z}");
        }

        private void OnServerInitialized(bool initial) {
            if (initial) {
                InvokeHandler.Instance.Invoke(() => {
                    setupDuelArena();
                }, 120f);
            }
        }

        private void setupDuelArena() {
            var filename = "arena";
            var _args = new List<string>{ "stability", "false", "autoheight", "false", "inventories", "false", "vending", "false", "entityowner", "false" };
            float rotation = 0f;
            var position = new Vector3(0, 300, 0);
            var pasted = CopyPaste.Call("TryPasteFromVector3", position, rotation, filename, _args.ToArray());

            if (pasted.GetType() != typeof(bool)) {
                Puts("Failed to paste");
            } else {
                Puts("Pasted");
            }
        }
/*
        Vector3 mapSize() {
            List<Vector3> list = Pool.GetList<Vector3>();
            float mapSizeX = TerrainMeta.Size.x / 2;
            float mapSizeZ = TerrainMeta.Size.z / 2;
            Vector3 randomPos = Vector3.zero;


            for (int i = 0; i < attempts; i++) {
                randomPos.x = UnityEngine.Random.Range(-mapSizeX, mapSizeX);
                randomPos.z = UnityEngine.Random.Range(-mapSizeZ, mapSizeZ);
            }
        }*/

        Vector3 getPlayerPositionVector(IPlayer player) {
            var playerPosition = player.Position();
            if (playerPosition == null) return new Vector3();
            return new Vector3(playerPosition.X, playerPosition.Y, playerPosition.Z);
        }
    }
}