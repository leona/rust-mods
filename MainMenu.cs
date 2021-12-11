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
    class MainMenu : CovalencePlugin
    {
        [PluginReference]
        private Plugin IInterface, Menu, AutoTeams, MostWanted;

        [Command(".")]
        private void mainMenuCommand(IPlayer player, string command, string[] args) {
            displayInterface(player);
        }

        void displayInterface(IPlayer player) {    
            var playersOnline = players.Connected.Count();
            var maxPlayers = 60;
            var currentTeam = (string) AutoTeams.Call<string>("getTeamNameFor", player.Id);
            var currentBounty = (string) MostWanted.Call<string>("currentBountyName");

             Menu.Call("display", "main", player.Id, new JArray() {
                new JObject() {
                    { "type", "label" },
                    { "text", "Players online" },
                    { "size", 10 },
                    { "transform",  new JObject() { 
                            { "width", 0.1f },
                            { "height", 0.07f },
                            { "top", 0.264f },
                            { "right", 0.18f }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", $"{playersOnline}/{maxPlayers}" },
                    { "size", 14 },
                    { "transform",  new JObject() { 
                            { "width", 0.1f },
                            { "height", 0.07f },
                            { "top", 0.29f },
                            { "right", 0.18f }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", $"Current team: {currentTeam}" },
                    { "size", 14 },
                    { "transform",  new JObject() { 
                            { "width", 0.1f },
                            { "height", 0.07f },
                            { "top", 0.275f },
                            { "right", 0.30f }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", $"Current bounty: {currentBounty}" },
                    { "size", 14 },
                    { "transform",  new JObject() { 
                            { "width", 0.1f },
                            { "height", 0.07f },
                            { "top", 0.275f },
                            { "right", 0.42f }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", "EU1.E2.IE" },
                    { "size", 22 },
                    { "colour", (string) IInterface.Call("Colour", "5297d8", 0.5f) },
                    { "transform",  new JObject() { 
                            { "width", 0.3f },
                            { "height", 0.07f },
                            { "top", 0.278f },
                            { "left", 0.1f }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", "PVP Tag Mode BETA" },
                    { "size", 19 },
                    { "colour", "titleColour" },
                    { "transform",  new JObject() { 
                            { "width", 0.3f },
                            { "height", 0.07f },
                            { "top", 0.278f },
                            { "left", 0.22f }
                        }
                    }
                },
               new JObject() {
                    { "type", "panel"},
                    { "colour", (string) IInterface.Call("Colour", "ffffff", 0.2f) },
                    { "transform",  new JObject() { 
                            { "width", 0.575f },
                            { "height", 0.42f },
                            { "top", 0.35f },
                            { "left", 0.212f }
                        }
                    }
               },
               new JObject() {
                    { "type", "panel"},
                    { "colour", (string) IInterface.Call("Colour", "000000", 0.6f) },
                    { "transform",  new JObject() { 
                            { "width", 0.0001f },
                            { "height", 0.4f },
                            { "top", 0.36f },
                            { "left", 0.5f }
                        }
                    }
               },
                new JObject() {
                    { "type", "label"},
                    { "text", "Discord" },
                    { "size", 20 },
                    { "colour", "titleColour" },
                    { "transform",  new JObject() { 
                            { "width", 0.1f },
                            { "height", 0.07f },
                            { "top", 0.36f },
                            { "left", 0.52 }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", "www.e2.ie/discord" },
                    { "size", 20 },
                    { "colour", "titleColour" },
                    { "transform",  new JObject() { 
                            { "width", 0.1f },
                            { "height", 0.07f },
                            { "top", 0.40f },
                            { "left", 0.52f }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", "Vote" },
                    { "size", 20 },
                    { "colour", "titleColour" },
                    { "transform",  new JObject() { 
                            { "width", 0.1f },
                            { "height", 0.07f },
                            { "top", 0.36f },
                            { "left", 0.675f }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", "www.e2.ie/vote" },
                    { "size", 20 },
                    { "colour", "titleColour" },
                    { "transform",  new JObject() { 
                            { "width", 0.1f },
                            { "height", 0.07f },
                            { "top", 0.40f },
                            { "left", 0.675f }
                        }
                    }
                },
                new JObject() {
                    { "type", "table"},
                    { "align", "left" },
                    { "increment", 0 },
                    { "pageSize", 14 },
                    { "cmd", "" },
                    {
                        "data",
                        new JArray() {
                            new JObject() { { "How it works", "- 8 teams max 6 per team"} },
                            new JObject() { { "How it works", "- Auto assigned teams when you join. Change teams above."} },
                            new JObject() { { "How it works", "- Find the target on the map and kill them"} },
                            new JObject() { { "How it works", "- If you kill the target, you become the target."} },
                            new JObject() { { "How it works", "- The longer you are the target the more points you get."} },
                            new JObject() { { "How it works", "- When you become the target you will receive high tier weapons." } },
                            new JObject() { { "How it works", "- No building/heli/crates/safezone/night"} },
                            new JObject() { { "How it works", "- Daily map cycle"} },
                            new JObject() { { "How it works", "- Random events happen"} },
                            new JObject() { { "How it works", "- Have fun and join the discord!"} },
                        }
                    },
                    { "transform",  new JObject() { 
                            { "width", 0.3f },
                            { "height", 0.3f },
                            { "top", 0.36f },
                            { "left", 0.225}
                        }
                    }
                },
               new JObject() {
                    { "type", "buttonCollection"},
                    {
                        "parent",
                        new JObject() {
                            { "colour", "buttonColour" },
                            { "background", "buttonBackground" },
                            { "padding", 0.005f },
                            {
                                "transform", 
                                new JObject() { 
                                    { "width", 0.27f },
                                    { "height", 0.07f },
                                    { "top", 0.50f },
                                    { "left", 0.51 }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  "Toggle Killfeed" },
                                { "command",  "/feed" },
                            },
                            new JObject() {
                                { "text",  "Spawn Drone" },
                                { "command",  "/drone" }
                            },
                            new JObject() {
                                { "text",  "Save current loadout" },
                                { "command",  "/loadoutsave" },
                            }
                        }
                    }
               },
               new JObject() {
                    { "type", "buttonCollection"},
                    {
                        "parent",
                        new JObject() {
                            { "colour", "buttonColour" },
                            { "background", "buttonBackground" },
                            { "padding", 0.005f },
                            {
                                "transform", 
                                new JObject() { 
                                    { "width", 0.27f },
                                    { "height", 0.07f },
                                    { "top", 0.579f },
                                    { "left", 0.51 }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  "Discord auth" },
                                { "command",  "/auth" }
                            },
                            new JObject() {
                                { "text",  "Check Vote" },
                                { "command",  "/vote" }
                            },
                            new JObject() {
                                { "text",  "Claim Vote Reward" },
                                { "command",  "/reward 0" }
                            },
                        }
                    }
               },
               new JObject() {
                    { "type", "buttonCollection"},
                    {
                        "parent",
                        new JObject() {
                            { "colour", "buttonColour" },
                            { "background", "buttonBackground" },
                            { "padding", 0.005f },
                            {
                                "transform", 
                                new JObject() { 
                                    { "width", 0.27f },
                                    { "height", 0.07f },
                                    { "top", 0.659f },
                                    { "left", 0.51 }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  "Spectate 1v1s" },
                                { "command",  "/spectate" }
                            },
                            new JObject() {
                                { "text",  "N/A" },
                                { "command",  "" }
                            },
                            new JObject() {
                                { "text",  "N/A" },
                                { "command",  "" }
                            },
                        }
                    }
               }
               // Kill streaks 
               // tanks, supply drop, m2, targetted rocket
            });
        }
    }
}
