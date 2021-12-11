using Newtonsoft.Json.Linq;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using Unity.Jobs;

namespace Oxide.Plugins
{
    [Info("WaterLevels", "Leon", "1.0.0")]
    [Description("Raise water levels event")]
    class WaterLevels : CovalencePlugin
    {
        #region Definitions

        float raiseSpeed = 0.1f;
        float maxRaise = 10f;
        float raiseIncrementAmount = 0.01f;
        int recoverAfter = 120;
        int startDelay = 3;

        #endregion Definitions

        [Command("water"), Permission("nx.admin")]
        private void waterEventCommand(IPlayer player, string command, string[] args) {
            Puts("Starting waterEvent");
            server.Broadcast($"Water levels rising in {startDelay} seconds");

            timer.In(startDelay, () => {
                server.Broadcast("Water levels are now rising!");

                recurseOceanRaise(0f, maxRaise, raiseIncrementAmount, 0f, () => {
                    server.Broadcast($"Water levels have stopped rising. Recovering in {recoverAfter} seconds.");

                    timer.In(recoverAfter, () => {
                        server.Broadcast("Water levels returning to normal");
                        recurseOceanRaise(maxRaise, 0f, raiseIncrementAmount, maxRaise);
                    });
                });
            });
        }

        private void recurseOceanRaise(float min, float max, float increment, float iteration, Action callback = null) {
            if ((min < max && iteration > max) || (min > max && iteration < max)) {
                if (callback != null) {
                    callback();
                }
                return;
            }

            timer.In(raiseSpeed, () => {
                setOceanLevel(iteration);
                float _iteration;

                if (min < max) {
                    _iteration = iteration + raiseIncrementAmount;
                } else {
                    _iteration = iteration - raiseIncrementAmount;
                }

                recurseOceanRaise(min, max, increment, _iteration, callback);
            });
        }

        private void setOceanLevel(float level) {
            var stringLevel = level.ToString("0.0000");
            server.Command($"oceanlevel {stringLevel}");
        }
    }
}
