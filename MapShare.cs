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
    [Info("MapShare", "Leon", "1.0.0")]
    [Description("Duel")]
    class MapShare : CovalencePlugin
    {

        [PluginReference]
        private Plugin DiscordCore, RustMapApi;
        bool newSave = false;
        string notificationChannel = "wipes-and-maps";
        
        
        string wipeMessage(object param1) {
            return $"**EU1.E2.IE** - New map: {param1}";
        }

        private void OnServerInitialized(bool initial) {
            Puts("Map share init");
            
            if (initial) {
                Puts("Is initial. Invoking gen&upload");
                newSave = false;

                InvokeHandler.Instance.Invoke(() => {
                    generateAndUpload();
                }, 60);
            }
        }

        void OnNewSave(string filename) {
            newSave = true;
        }

        void OnPluginLoaded(Plugin plugin) {
        }

        void generateAndUpload() {
            Puts("Generating and uploading map to discord");
            server.Command("rma_regenerate");
            server.Command("rma_upload default 2000 1 1");
        }

        void OnRustFullMapUploaded(Hash<string, object> response) {
            bool success = (bool)response["Success"];

            if (!success)
            {
                PrintError($"An error occured uploading the image \n\n{JsonConvert.SerializeObject(response)}");
                return;
            }

            Hash<string, object> data = response["Data"] as Hash<string, object>;
            DiscordCore.Call("SendMessageToChannel", notificationChannel, wipeMessage(data?["Link"]));
            Puts($"Map link generated: {data?["Link"]}");
        }
    }
}