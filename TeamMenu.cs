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
    [Info("TeamMenu", "Leon", "1.0.0")]
    [Description("Menu for teams")]
    class TeamMenu : CovalencePlugin
    {
        [PluginReference]
        private Plugin Menu, AutoTeams;

        [Command("teams")]
        private void teamCmd(IPlayer player, string command, string[] args) {
            displayInterface(player);
        }

        void displayInterface(IPlayer player) {
            var counts = AutoTeams.Call<Dictionary<string,string>>("getAllTeamsCount");

             Menu.Call("display", "team", player.Id, new JArray() {
                new JObject() {
                    { "type", "label"},
                    { "text", "Select a team to join" },
                    { "transform",  new JObject() { 
                            { "width", 0.6f },
                            { "height", 0.07f },
                            { "top", 0.3f },
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
                            { "padding", 0.001f },
                            {
                                "transform", 
                                new JObject() { 
                                    { "width", 0.4f },
                                    { "height", 0.15f },
                                    { "top", 0.4f },
                                    { "left", 0.3f }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  $"alpha {counts["alpha"]}" },
                                { "command",  "/join alpha" },
                            },
                            new JObject() {
                                { "text",  $"bravo {counts["bravo"]}" },
                                { "command",  "/join bravo" },
                            },
                            new JObject() {
                                { "text",  $"charlie {counts["charlie"]}" },
                                { "command",  "/join charlie" },
                            },
                            new JObject() {
                                { "text",  $"delta {counts["delta"]}" },
                                { "command",  "/join delta" },
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
                                    { "top", 0.553f },
                                    { "left", 0.3f }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  $"echo {counts["echo"]}" },
                                { "command",  "/join echo" },
                            },
                            new JObject() {
                                { "text",  $"foxtrot {counts["foxtrot"]}" },
                                { "command",  "/join foxtrot" },
                            },
                            new JObject() {
                                { "text",  $"golf {counts["golf"]}" },
                                { "command",  "/join golf" },
                            },
                            new JObject() {
                                { "text",  $"hotel {counts["hotel"]}" },
                                { "command",  "/join hotel" },
                            },
                        }
                    }
                }
            });
        }
    }
}
