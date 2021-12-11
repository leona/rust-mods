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
    [Info("RateLimit", "Leon", "1.0.0")]
    [Description("RateLimit")]
    class RateLimit : CovalencePlugin
    {
        #region Definitions

        Dictionary<string, Dictionary<string, long>> limits = new Dictionary<string, Dictionary<string, long>>();
        Dictionary<string, long> deaths = new Dictionary<string, long>();

        #endregion Definitions
        
        long timestamp() => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

        // Allowed once in a period of time
        [HookMethod("inTime")]
        public long inTime(string key, string playerId, int seconds) {
            var _timestamp = timestamp();

            if (!limits.ContainsKey(playerId)) {
                limits[playerId] = new Dictionary<string, long>();
            }

            if (!limits[playerId].ContainsKey(key)) {
                limits[playerId][key] = _timestamp;
                return (long) 0;
            }

            if (limits[playerId][key] > _timestamp - (long) seconds) {
                return limits[playerId][key] - (_timestamp - (long) seconds);
            }

            limits[playerId][key] = _timestamp;
            return (long) 0;
        }

        // Allowed once per life
        [HookMethod("inLife")]
        public bool inLife(string key, string playerId) {
            if (!limits.ContainsKey(playerId)) {
                limits[playerId] = new Dictionary<string, long>() {
                    { key, timestamp() }
                };

                return false;
            }

            if (!deaths.ContainsKey(playerId) || deaths[playerId] < limits[playerId][key]) {
                return true;
            }       

            return false;
        }

        // Allowed for first X seconds of life
        [HookMethod("afterLife")]
        public bool afterLife(string playerId, int seconds) {
            if (!deaths.ContainsKey(playerId)) {
                deaths[playerId] = timestamp();
                return false;
            }

            if (deaths[playerId] < timestamp() - (long) seconds) {
                return true;
            }       

            return false;
        }

        [HookMethod("blockPlayer")]
        public void blockPlayer(string playerId) {
            deaths[playerId] = 0;
        }
        
        void OnPlayerConnected(BasePlayer player)   {
            if (!deaths.ContainsKey(player.IPlayer.Id)) {
                deaths[player.IPlayer.Id] = timestamp();
            }
        }

        object OnPlayerDeath(BasePlayer player, HitInfo info) {
            deaths[player.IPlayer.Id] = timestamp();
            return null;
        }
    }
}