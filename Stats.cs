using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Stats", "Leon", "1.0.0")]
    [Description("Stats")]
    class Stats : CovalencePlugin
    {
        #region Definitions

        [PluginReference]
        Plugin PlayerDatabase;

        #endregion Definitions

        object OnPlayerDeath(BasePlayer player, HitInfo info) {
            BasePlayer killer = info.Initiator?.ToPlayer();

            
            if (killer != null && killer != player && player.IPlayer != null) {
                incrementPlayerStat(killer.IPlayer.Id, "kills");
            } 

            if (player.IPlayer != null) {
                incrementPlayerStat(player.IPlayer.Id, "deaths");
            }

            // TODO cooldown for kits in a life
           // PlayerDatabase.Call("SetPlayerData", player.IPlayer.Id, "last_death", DateTimeOffset.Now.ToUnixTimeSeconds());
            return null;
        }

        void OnPlayerDisconnected(BasePlayer player, string reason) {
            resetPlayerStats(player.IPlayer.Id);
        }

        void OnPlayerConnected(BasePlayer player) {
            resetPlayerStats(player.IPlayer.Id);
        }

        void resetPlayerStats(string id) {
            PlayerDatabase.Call("SetPlayerData", id, "kills", 0);
            PlayerDatabase.Call("SetPlayerData", id, "deaths", 0);
        }

        void incrementPlayerStat(string id, string key) {
            Puts($"Increment: {key} for player: {id}");

            var currentValue = PlayerDatabase.Call("GetPlayerData", id, key);
            
            int newValue;

            if (currentValue == null) {
                newValue = 1;
                currentValue = 0;
            } else {
                newValue = Convert.ToInt32(currentValue);
                newValue++;
            }

            Puts($"Current: {currentValue} New: {newValue}");
            PlayerDatabase.Call("SetPlayerData", id, key, newValue);
        }
    }
}
