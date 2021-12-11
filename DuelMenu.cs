using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using Network;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Oxide.Plugins
{
    [Info("Leaderboard", "Leon", "1.0.0")]
    [Description("Leaderboard menu")]
    class Leaderboard : CovalencePlugin
    {
        [PluginReference]
        private Plugin AutoTeams, PlayerDatabase, MostWanted, Menu;

        [Command("1v1")]
        private void leaderboardCmd(IPlayer player, string command, string[] args) {
            displayInterface(player);
        }

        void displayInterface(IPlayer player) {
            Puts("Showing leaderboard interface");
            JArray scores = GetScoreboard();
            
             Menu.Call("display", "1v1", player.Id, new JArray() {
                new JObject() {
                    { "type", "buttonCollection"},
                    {
                        "parent",
                        new JObject() {
                            { "colour", "navButtonColour" },
                            { "background", "navButtonBackground" },
                            {
                                "transform", 
                                new JObject() { 
                                    { "width", 0.27f },
                                    { "height", 0.07f },
                                    { "top", 0.6f },
                                    { "left", 0.51 }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  "Spectate Duels" },
                                { "command",  "/auth" }
                            },
                        }
                    }
                },                       
                new JObject() {
                    { "type", "table"},
                    { "data", scores },
                    { "transform",  new JObject() { 
                            { "width", 0.6f },
                            { "height", 0.5f },
                            { "top", 0.2911f },
                            { "left", 0.2f }
                        }
                    }
                }
            });
        }

        public JArray GetScoreboard() {
            //var regularPlayers = players.Connected.Count(p => !p.IsAdmin && !p.HasPermission(permHide));
            JArray data = new JArray();
            var iteration = 0;
            var stats = (Dictionary<string, long>) MostWanted.Call("getPlayerStats");
            
            foreach(var _player in players.Connected) {
                iteration++;
                if (iteration > 14 ) {
                    break;
                }
                var basePlayer = _player.Object as BasePlayer;
                var _kills = PlayerDatabase.Call("GetPlayerData", _player.Id, "kills");
                var _deaths = PlayerDatabase.Call("GetPlayerData", _player.Id, "deaths");
                int kills, deaths;

                if (_kills == null) {
                    kills = 0;
                } else {
                    kills = Convert.ToInt32(_kills);
                }

                if (_deaths == null) {
                    deaths = 0;
                } else {
                    deaths = Convert.ToInt32(_deaths);
                }

                var clanTag = (string) AutoTeams.Call<string>("getTeamNameFor", _player.Id);

                JObject playerStat = new JObject();
                playerStat["Rank"] = iteration;
                playerStat["Name"] = basePlayer.displayName;
                playerStat["Team"] = clanTag;
                playerStat["KDR"] = kills == 0 && deaths == 0 ? 0 : Math.Round((float)kills / (float)deaths, 3);
                playerStat["Kills"] = kills;
                playerStat["Deaths"] = deaths;
                playerStat["Objective Time"] = 0;

                                
                if (stats.ContainsKey(_player.Id)) {
                    var score = (int) stats[_player.Id];
                    Puts($"Found player score stats {score}");
                    playerStat["Objective Time"] = score;
                } else {
                    Puts($"No stats found. {stats.Count()}");
                    playerStat["Objective Time"] = 0;
                }

                data.Add(playerStat);
            }
    
            JArray sortedData = new JArray(data.OrderBy(obj => (int)obj["Objective Time"]).Reverse().Take(14));

            for(var i = 0; i < sortedData.Count; i++) {
                sortedData[i]["Rank"] = i + 1;
            }
            
            return sortedData;
        }  
    }
}
