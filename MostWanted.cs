using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using Oxide.Game.Rust;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Most Wanted", "Leon", "1.0.0")]
    [Description("Most wanted game mode")]
    class MostWanted : CovalencePlugin
    {
        [PluginReference]
        private Plugin Markers;

        public IPlayer _mostWanted;

        private Timer updateMostWantedTimer;


        private ConfigData configData;
        private DynamicConfigFile configFileData;

        private void SaveData() => configFileData.WriteObject(configData);
        void OnServerShutdown() => SaveData();
        void OnServerSave() => SaveData();
        
        private void LoadData()
        {
            configFileData = Interface.Oxide.DataFileSystem.GetFile("MostWanted/data");

            try {
                configData = configFileData.ReadObject<ConfigData>();
            } catch {
                configData = new ConfigData();
            }
        }

        class ConfigData {
            [JsonProperty]
            public string targetPlayerId = null;
        }

        private class SharedStore {
            public List<ObjectiveTaken> timestamps = new List<ObjectiveTaken>();
        }

        private static SharedStore shared = new SharedStore();
        
        private class ObjectiveTaken {
            public long timestamp;
            public string playerId;
        }

        private IPlayer mostWanted {
            get { return _mostWanted; }
            set {
                if (value == _mostWanted ) {
                    Puts("This player is already the most wanted");
                    return;
                }

                if (value == null) {
                    _mostWanted = null;
                    configData.targetPlayerId = null;
                    return;
                }

                shared.timestamps.Add(new ObjectiveTaken {
                    timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    playerId = value.Id
                });

                // Give bounty kit
                if (mostWanted == null || value.Id != mostWanted.Id) {
                    timer.In(4, () => {
                        // Errors out sometimes if not enough players
                        giveBountyKit(value);
                    });
                }

                _mostWanted = value;
                configData.targetPlayerId = value.Id;
                SaveData();
                updateMostWanted();

                Puts($"New most wanted player: {value.Name} - number of timestamps: {shared.timestamps.Count()}");
                server.Broadcast($"New mosted wanted player is: {value.Name}");
                value.Reply("You're now the bounty. Check your inventory.");
            }
        }

        void OnPlayerRespawned(BasePlayer player) {
            timer.In(4, () => {
                Puts("Giving kit");
                if (player == null || player.IPlayer == null || player.IPlayer.Id == null) {
                    return;
                }
                if (mostWanted == null || player.IPlayer.Id == mostWanted.Id) {
                    Puts("GIVING KIT");
                    giveBountyKit(player.IPlayer);
                } else {
                    Puts($"NOT BNOUNTY. {player.IPlayer.Id} - {mostWanted.Id}");
                }
            });
        }

        void giveBountyKit(IPlayer player) {
            giveItem(player, "lmg.m249", 1);
            giveItem(player, "ammo.rifle", 300);
        }

        // TODO: Cache this
        private Dictionary<string, long> getPlayerStats() {
            int index = -1;
            var userScores = new Dictionary<string, long>();

            if (shared.timestamps == null) {
                Puts("shared timestamps null");
                return null;
            }
            
            Puts("Getting player stats. Timestamps: ", shared.timestamps.ToString());

            foreach(var point in shared.timestamps) {
                index = index + 1;
                Puts($"LOOPING TIMESTAMPS: {point.playerId} - {point.timestamp}");
                if (index == 0) {
                    continue;
                }

                var lastPoint = shared.timestamps[index - 1];
                var duration = point.timestamp - lastPoint.timestamp;
                Puts($"Got duration: {duration} - LAST {lastPoint.timestamp}");
                
                if (userScores.ContainsKey(point.playerId)) {
                    userScores[point.playerId] += duration;
                } else {
                    userScores[point.playerId] = duration;
                }
            }

            Puts("Got player stats", userScores);

            return userScores;
        }

        void cleanTimestamps() {
            Puts("Cleaning timestamps");
            var currentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            shared.timestamps = shared.timestamps.Where(x => x.timestamp > currentTimestamp - (60 * 10)).ToList();
        }

        void OnPluginLoaded(Plugin plugin) {
            LoadData();
            Puts("LOADED");
            updateMostWantedTimer = timer.Every(2, () => updateMostWanted());

            if (configData.targetPlayerId != null) {
                Puts("Config exists");
                var player = RustCore.FindPlayerByIdString(configData.targetPlayerId);

                if (player != null && player.IsConnected && player.IsAlive()) {
                    Puts("Setting _mostwanted");
                    _mostWanted = player.IPlayer;
                    return;
                }
            }

            updateMostWantedValue();
        }

        void OnPluginUnloaded(Plugin name) {
           updateMostWantedTimer.Destroy();   
           SaveData();   
        }

        public void giveItem(IPlayer player, string name, int amount = 1) {
            var basePlayer = player.Object as BasePlayer;

            var item = ItemManager.CreateByName(name, amount);
            if (item == null) {return;}
            basePlayer.inventory.GiveItem(item);
        }

        private void OnServerInitialized() {
            Puts("Most wanted plugin loaded");
            timer.Every(60 * 10, () => cleanTimestamps());
        }

        void updateMostWanted() {
            if (mostWanted == null) {
                //Puts("No most wanted player yet");
                return;
            }
            
            var basePlayer = mostWanted.Object as BasePlayer;

            if (!basePlayer.IsAlive() || !basePlayer.IsConnected) {
                return;
            }

            Markers.Call("markAndDome", "most-wanted", mostWanted, 3f, "red");
        }

        [HookMethod("currentBountyName")]
        string currentBountyName() {
            return mostWanted?.Name;
        }

        public IPlayer getRandomPlayerAlive(string ignoreId = null) {
            var validPlayers = players.Connected.ToArray().Where((a) => a.Id != ignoreId && !a.IsSleeping).ToArray();
            return validPlayers.GetRandom();
        }

        void updateMostWantedValue(string ignoredId = null) {
            var _mostWanted = (IPlayer) getRandomPlayerAlive(ignoredId);

            if (_mostWanted != null) {
                mostWanted = _mostWanted;
            }
        }

        void OnPlayerDisconnected(BasePlayer player, string reason) {            
            if (mostWanted == null || player.IPlayer.Id == mostWanted.Id) {
                updateMostWantedValue();
                Puts($"Most wanted player disconnected");
            }
        }

        void OnPlayerConnected(BasePlayer player) {
            if (mostWanted == null) {
                mostWanted = player.IPlayer;
            }
        }

        object OnPlayerDeath(BasePlayer player, HitInfo info) {
            BasePlayer killer = info.Initiator?.ToPlayer();
            Puts("ON PLAYER DEATH");

            if (mostWanted == null) {
                Puts("Current most wanted is null");
                updateMostWantedValue();
                return null;
            }

            if (player.IPlayer != null) {
                if (player.IPlayer.Id == mostWanted.Id) {
                    if (killer.IPlayer.Id == mostWanted.Id) {
                        updateMostWantedValue(mostWanted.Id);
                        return null;
                    }

                    if (killer.IPlayer != null) {
                        Puts("Setting killer as new most wanted");
                        mostWanted = killer.IPlayer;
                    } else {
                        Puts("Choosing random most wanted");
                        updateMostWantedValue();
                    }
                }
            }

            return null;
        }
    }
}
