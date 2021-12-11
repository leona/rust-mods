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
    [Info("Menu", "Leon", "1.0.0")]
    [Description("Server menu")]
    class Menu : CovalencePlugin
    {

        [PluginReference]
        private Plugin IInterface;

        [HookMethod("display")]
        public void display(string selected, string playerId, JArray other = null) {
            Puts("Showing menu interface");

            var collection = new JArray() {
               new JObject() {
                    { "type", "panel"},
                    { "colour", (string) IInterface.Call("Colour", "ffffff", 0.4f) },
                    { "transform",  new JObject() { 
                            { "width", 1f },
                            { "height", 1f },
                            { "top", 0f },
                            { "left", 0f }
                        }
                    }
               },
               new JObject() {
                    { "type", "panel"},
                    { "transform",  new JObject() { 
                            { "width", 0.6f },
                            { "height", 0.6f },
                            { "top", 0.2f },
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
                                    { "width", 0.6f },
                                    { "height", 0.07f },
                                    { "top", 0.2f },
                                    { "left", 0.2f }
                                }
                            }
                        }
                    },
                    {
                        "collection",
                        new JArray() { 
                            new JObject() {
                                { "text",  "Main" },
                                { "command",  "/." },
                                { "background",  selected == "main" ? "navButtonBackgroundSelected": "navButtonBackground" }
                            },
                            new JObject() {
                                { "text",  "Switch Team" },
                                { "command",  "/teams" },
                                { "background",  selected == "team" ? "navButtonBackgroundSelected" : "navButtonBackground" }
                            },
                            new JObject() {
                                { "text",  "Scoreboard" },
                                { "command",  "/score 0" },
                                { "background",  selected == "scoreboard" ? "navButtonBackgroundSelected" : "navButtonBackground" }
                            },
                            new JObject() {
                                { "text",  "Loadouts/Vehicles" },
                                { "command",  "/spawnables" },
                                { "background",  selected == "spawnables" ? "navButtonBackgroundSelected" : "navButtonBackground" }
                            },
                        }
                    }
               }
           };
           
            if (other != null) {
               collection.Merge(other);
            }

            IInterface.Call("display", playerId, collection);
        }
    }
}
