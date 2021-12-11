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
using Rust;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Oxide.Plugins
{
    [Info("AutoRestart", "Leon", "1.0.0")]
    [Description("AutoRestart")]
    class AutoRestart : CovalencePlugin
    {

        public class Time {
            public int hour;
            public int minute = 0;
        }

        float checkInterval = 60f;

        public List<Time> restartSchedule = new List<Time>() {
            new Time() {
                hour = 3,
                minute = 30 // Must be more than 5 minutes past the hour.
            }
        };

        void OnPluginLoaded(Plugin plugin) {
            InvokeHandler.Instance.InvokeRepeating(() => checkSchedule(), 0f, checkInterval); 
        }

        void notify(string message) {
            server.Broadcast(message);
            Puts(message);
        }

        void checkSchedule() {
            Puts($"Checking for restarts {DateTime.Now.Hour}:{DateTime.Now.Minute}");

            Time currentTime = new Time() {
                minute = DateTime.Now.Minute,
                hour = DateTime.Now.Hour
            };

            foreach(var schedule in restartSchedule) {
                if (schedule.hour == currentTime.hour && schedule.minute == currentTime.minute) {
                    restart();
                }
            }
        }

        void restart() {
            server.Command("restart");
        }
    }
}
