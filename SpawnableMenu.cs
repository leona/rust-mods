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
    [Info("SpawnableMenu", "Leon", "1.0.0")]
    [Description("Menu for spawnable items")]
    class SpawnableMenu : CovalencePlugin
    {

        [PluginReference]
        private Plugin Menu, IInterface;

        [Command("spawnables")]
        private void loadoutsCmd(IPlayer player, string command, string[] args) {
            displayInterface(player);
        }

        void displayInterface(IPlayer player) {
            Puts("Showing leaderboard interface");

             Menu.Call("display", "spawnables", player.Id, new JArray() {
                new JObject() {
                    { "type", "label"},
                    { "text", "Loadouts" },
                    { "transform",  new JObject() { 
                            { "width", 0.6f },
                            { "height", 0.07f },
                            { "top", 0.3f },
                            { "left", 0.2f }
                        }
                    }
                },
                new JObject() {
                    { "type", "label"},
                    { "text", "Vehicles" },
                    { "transform",  new JObject() { 
                            { "width", 0.6f },
                            { "height", 0.07f },
                            { "top", 0.507f },
                            { "left", 0.2f }
                        }
                    }
                },
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
                                    { "width", 0.4f },
                                    { "height", 0.15f },
                                    { "top", 0.36f },
                                    { "left", 0.3f }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  "AK" },
                                { "command",  "/loadout ak" },
                            },
                            new JObject() {
                                { "text",  "LR" },
                                { "command",  "/loadout lr" },
                            },
                            new JObject() {
                                { "text",  "MP5" },
                                { "command",  "/loadout mp5" },
                            },
                            new JObject() {
                                { "text",  "BOLT" },
                                { "command",  "/loadout bolt" },
                            },
                        }
                    }
                },
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
                                    { "width", 0.4f },
                                    { "height", 0.15f },
                                    { "top", 0.57f },
                                    { "left", 0.3f }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  "Mini" },
                                { "command",  "/mini" },
                            },
                            new JObject() {
                                { "text",  "Car" },
                                { "command",  "/car" },
                            },
                            new JObject() {
                                { "text",  "Boat" },
                                { "command",  "/boat" },
                            },
                        }
                    }
                }
            });
        }
    }
}
